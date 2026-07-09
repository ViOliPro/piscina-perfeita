using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Repository.UsuariosLocal;

namespace PiscinaPerfeita.Api.Service.Usuarios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuariosRepository;
        private readonly IUsuarioLocalRepository _usuariosLocalRepository;
        private readonly IAuthenticatedUser _user;

        public UsuarioService(
            IUsuarioRepository usuariosRepository,
            IAuthenticatedUser user,
            IUsuarioLocalRepository usuariosLocalRepository
        )
        {
            _usuariosRepository =
                usuariosRepository ?? throw new ArgumentNullException(nameof(usuariosRepository));
            _usuariosLocalRepository =
                usuariosLocalRepository
                ?? throw new ArgumentNullException(nameof(usuariosLocalRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        public async Task<List<UsuarioResponseDto>> Show()
        {
            return await _usuariosRepository.Show();
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
            CurrentUser usuarioLocalLogado
        )
        {
            if (usuarioCriado.Role == Role.SuperAdmin)
            {
                var NovoUsuarioLocal = new UsuarioLocal
                {
                    UsuarioId = usuarioCriado.Id,
                    LocalId = dto.LocalId ?? null,
                    Perfil = dto.Perfil ?? Perfil.Administrador,
                };

                await _usuariosLocalRepository.Create(NovoUsuarioLocal);
                return;
            }

            if (usuarioLocalLogado == null)
                throw new InvalidOperationException("Usuário não pertence a nenhum local.");

            var usuarioLocal = new UsuarioLocal
            {
                UsuarioId = usuarioCriado.Id,
                LocalId = usuarioCriado.LocalId ?? usuarioLocalLogado.LocalId,
                Perfil = dto.Perfil ?? Perfil.Visualizador,
            };

            await _usuariosLocalRepository.Create(usuarioLocal);
        }
    }
}
