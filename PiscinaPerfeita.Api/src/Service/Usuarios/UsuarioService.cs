using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Locais;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Repository.UsuariosLocal;

namespace PiscinaPerfeita.Api.Service.Usuarios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuariosRepository;
        private readonly IUsuarioLocalRepository _usuariosLocalRepository;
        private readonly ILocalRepository _locaisRepository;
        private readonly IAuthenticatedUser _user;

        public UsuarioService(
            IUsuarioRepository usuariosRepository,
            IAuthenticatedUser user,
            IUsuarioLocalRepository usuariosLocalRepository,
            ILocalRepository locaisRepository
        )
        {
            _usuariosRepository =
                usuariosRepository ?? throw new ArgumentNullException(nameof(usuariosRepository));
            _usuariosLocalRepository =
                usuariosLocalRepository
                ?? throw new ArgumentNullException(nameof(usuariosLocalRepository));
            _locaisRepository =
                locaisRepository ?? throw new ArgumentNullException(nameof(locaisRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        public async Task<List<UsuarioResponseDto>> Show()
        {
            try
            {
                var usuarioLogado = _user.IsSuperAdmin();
                if (usuarioLogado)
                    return await _usuariosRepository.Show();

                return await _usuariosRepository.FilterRoleUsuario();
            }
            catch
            {
                throw new Exception("Erro com a requisicao");
            }
        }

        public async Task<UsuarioResponseDto> GetById(Guid id)
        {
            var usuarioDb = await _usuariosRepository.GetById(id);
            if (usuarioDb == null)
            {
                throw new KeyNotFoundException($"Usuario com id {id} não encontrado");
            }

            return usuarioDb;
        }

        public async Task<UsuarioResponseDto> Create(UsuarioRequestDto dto)
        {
            var userByEmail = await _usuariosRepository.GetByEmail(dto.Email);
            if (userByEmail != null)
                throw new InvalidOperationException("Existe um usuario com o email cadastrado");

            var usuarioLogado = await _user.GetCurrentUser();

            //Somente um usuario com role SuperAdmin pode cadastrar outro usuario com role SuperAdmin
            await ValidarPermissaoCadastro(usuarioLogado, dto);

            // Criando um novo usuário com os dados fornecidos no DTO
            var newUsuario = await CriarUsuario(dto);
            await _usuariosRepository.Create(newUsuario);

            await CriarUsuarioLocal(newUsuario, dto, usuarioLogado);

            return new UsuarioResponseDto
            {
                Id = newUsuario.Id,
                Nome = newUsuario.Nome,
                Email = newUsuario.Email ?? string.Empty,
                Cpf = newUsuario.Cpf ?? string.Empty,
                Role = newUsuario.Role,
                CreatedAt = newUsuario.CreatedAt,
                LocalId = newUsuario.LocalId,
                Perfil = dto.Perfil ?? Perfil.Administrador,
            };
        }

        public async Task<UsuarioResponseDto> Update(Guid id, UsuarioRequestUpdateDto dto)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException($"O id informado não pode ser vazio {nameof(id)} .");
            }

            var usuario = await _usuariosRepository.GetById(id);
            if (usuario == null)
            {
                throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");
            }

            var getPassword = await _usuariosRepository.GetPasswordById(id);

            // Mesma regra do Create: sem essa checagem, qualquer usuário autenticado
            // podia fazer PUT no próprio id com Role=SuperAdmin e se auto-promover.
            var usuarioLogado = await _user.GetCurrentUser();
            if (dto.Role.HasValue)
            {
                await ValidarPermissaoCadastro(
                    usuarioLogado,
                    new UsuarioRequestDto { Role = dto.Role.Value }
                );
            }

            var usuarioDb = new Usuario
            {
                Id = id,
                Nome = !string.IsNullOrEmpty(dto.Nome) ? dto.Nome : usuario.Nome,
                Email = !string.IsNullOrEmpty(dto.Email) ? dto.Email : usuario.Email,
                SenhaHash = !string.IsNullOrEmpty(dto.SenhaHash)
                    ? BCrypt.Net.BCrypt.HashPassword(dto.SenhaHash)
                    : getPassword?.SenhaHash,
                Role = dto.Role ?? usuario.Role,
            };

            await _usuariosRepository.Update(id, usuarioDb);

            return new UsuarioResponseDto
            {
                Id = usuarioDb.Id,
                Nome = usuarioDb.Nome,
                Email = usuarioDb.Email,
                CreatedAt = usuarioDb.CreatedAt,
                Role = usuarioDb.Role,
            };
        }

        public async Task Delete(Guid id)
        {
            var usuarioDb = await _usuariosRepository.GetById(id);
            if (usuarioDb == null)
            {
                throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");
            }

            await _usuariosRepository.Delete(id);
        }

        private async Task ValidarPermissaoCadastro(
            CurrentUser usuarioLogado,
            UsuarioRequestDto dto
        )
        {
            // Somente um usuario com role SuperAdmin pode cadastrar outro usuario com role SuperAdmin
            if (usuarioLogado.Role != Role.SuperAdmin && dto.Role == Role.SuperAdmin)
                throw new InvalidOperationException(
                    "Você não tem permissão para esse cadastro, contate um administrador"
                );
        }

        private async Task<Usuario> CriarUsuario(UsuarioRequestDto dto)
        {
            return new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Cpf = dto.Cpf,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.SenhaHash),
                Role = dto.Role,
            };
        }

        private async Task CriarUsuarioLocal(
            Usuario usuarioCriado,
            UsuarioRequestDto dto,
            CurrentUser usuarioLogado
        )
        {
            // Um SuperAdmin cria cada entidade (Local, Usuário, etc.) de forma
            // independente e faz o vínculo entre elas depois — por isso o
            // UsuarioLocal nasce "pendente" (LocalId nulo), a menos que o
            // SuperAdmin já informe um LocalId de propósito no cadastro.
            // Isso vale mesmo que o SuperAdmin esteja no momento com um Local
            // ativo (trocado via SwitchLocal): ele administra TODOS os
            // locais, então não deve herdar automaticamente o local em que
            // está navegando no momento.
            if (usuarioLogado.Role == Role.SuperAdmin)
            {
                if (dto.LocalId.HasValue)
                    await GarantirLocalExiste(dto.LocalId.Value);

                var novoUsuarioLocal = new UsuarioLocal
                {
                    UsuarioId = usuarioCriado.Id,
                    LocalId = dto.LocalId ?? null,
                    Perfil = dto.Perfil ?? Perfil.Administrador,
                };

                await _usuariosLocalRepository.Create(novoUsuarioLocal);
                return;
            }

            // Um Administrador comum só gerencia usuários dentro do seu
            // próprio Local — o novo usuário herda automaticamente o Local
            // de quem o está criando (nunca de um LocalId arbitrário vindo
            // do DTO, para não vazar usuários entre locais diferentes).
            if (usuarioLogado.LocalId == null)
                throw new InvalidOperationException(
                    "Você precisa estar vinculado a um Local para cadastrar usuários."
                );

            var usuarioLocal = new UsuarioLocal
            {
                UsuarioId = usuarioCriado.Id,
                LocalId = usuarioLogado.LocalId,
                Perfil = dto.Perfil ?? Perfil.Visualizador,
            };

            await _usuariosLocalRepository.Create(usuarioLocal);
        }

        private async Task GarantirLocalExiste(Guid localId)
        {
            var local = await _locaisRepository.GetById(localId);
            if (local == null)
                throw new KeyNotFoundException($"Local com id {localId} não encontrado.");
        }
    }
}
