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
            if (_user.IsSuperAdmin())
                return await _usuariosRepository.Show();

            // CORRIGIDO: antes retornava TODOS os usuários com Role=Usuario do
            // sistema inteiro, de qualquer Local — um Administrador enxergava
            // usuários de outros tenants. Agora fica restrito ao Local ativo
            // do Administrador que está consultando.
            var localId = _user.GetLocalId();
            if (localId == Guid.Empty)
                throw new InvalidOperationException(
                    "Selecione um Local ativo para listar os usuários."
                );

            return await _usuariosRepository.FilterRoleUsuario(localId);
        }

        public async Task<UsuarioResponseDto> GetById(Guid id)
        {
            var usuarioDb = await _usuariosRepository.GetByIdDto(id);
            if (usuarioDb == null)
            {
                throw new KeyNotFoundException($"Usuario com id {id} não encontrado");
            }

            // CORRIGIDO: faltava checar se o usuário buscado pertence ao
            // mesmo tenant do Administrador logado — sem isso, qualquer
            // Administrador podia consultar dados de usuários de QUALQUER
            // outro Local só sabendo o Guid (IDOR/vazamento entre tenants).
            // Devolvemos 404 (não 403) para não confirmar nem a existência
            // do usuário fora do escopo do tenant atual. Leitura não altera
            // privilégio, então não bloqueia por causa do Administrador Pai.
            await GarantirUsuarioNoTenantAtual(id, protegerAdministradorPai: false);

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

            var usuario = await _usuariosRepository.GetByIdDto(id);
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

            // CORRIGIDO: mesmo IDOR do GetById — faltava garantir que o
            // usuário editado pertence ao tenant do Administrador logado.
            // A proteção extra do Administrador Pai só entra quando a
            // chamada tenta mexer em Role (privilégio); edição básica de
            // nome/email/senha continua permitida por outro Administrador
            // do mesmo tenant.
            await GarantirUsuarioNoTenantAtual(id, protegerAdministradorPai: dto.Role.HasValue);

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

            // CORRIGIDO: mesmo IDOR do GetById/Update — um Administrador
            // conseguia apagar QUALQUER usuário do sistema (de qualquer
            // Local) só sabendo o Guid, e nada impedia apagar o
            // Administrador Pai do próprio tenant. Excluir é sempre uma
            // ação sobre o vínculo, então aqui a proteção do Administrador
            // Pai vale sempre.
            await GarantirUsuarioNoTenantAtual(id, protegerAdministradorPai: true);

            await _usuariosRepository.Delete(id);
        }

        // Garante que o usuário-alvo (id) está vinculado, com vínculo ATIVO,
        // ao Local em que o Administrador logado está atuando no momento.
        // Quando protegerAdministradorPai=true, também bloqueia a ação se o
        // vínculo pertencer ao Administrador Pai (só o SuperAdmin pode
        // alterar privilégio/excluir esse vínculo). SuperAdmin sempre passa
        // sem restrição.
        private async Task GarantirUsuarioNoTenantAtual(
            Guid usuarioAlvoId,
            bool protegerAdministradorPai
        )
        {
            if (_user.IsSuperAdmin())
                return;

            var localAtivo = _user.GetLocalId();
            if (localAtivo == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Selecione um Local ativo para gerenciar usuários."
                );

            var vinculo = await _usuariosLocalRepository.Vinculo(usuarioAlvoId, localAtivo);

            // Não confirmamos a existência do usuário fora do tenant atual:
            // 404 igual ao caso de "não existe" (evita enumeração de Ids de
            // outros tenants).
            if (vinculo == null)
                throw new KeyNotFoundException($"Usuario com id {usuarioAlvoId} não encontrado");

            if (protegerAdministradorPai && vinculo.EhAdministradorPai)
                throw new UnauthorizedAccessException(
                    "Somente o SuperAdmin pode alterar ou remover o Administrador responsável por este Local."
                );
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

                var perfilAtribuido = dto.Perfil ?? Perfil.Administrador;
                var novoUsuarioLocal = new UsuarioLocal
                {
                    UsuarioId = usuarioCriado.Id,
                    LocalId = dto.LocalId ?? null,
                    Perfil = perfilAtribuido,
                    // O SuperAdmin pode "criar um Local e vinculá-lo
                    // diretamente a um Administrador" (regra de negócio) —
                    // quando isso acontece já no cadastro (LocalId + Perfil
                    // Administrador informados juntos), esse vínculo nasce
                    // como o Administrador Pai daquele Local.
                    EhAdministradorPai =
                        dto.LocalId.HasValue && perfilAtribuido == Perfil.Administrador,
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
                // Mesmo que o Administrador logado crie outro Admin (Admin
                // Filho — permitido pela regra de negócio), o novo vínculo
                // NUNCA nasce como Administrador Pai: só o SuperAdmin ou o
                // fluxo de auto-vínculo em LocalService.VincularCriadorAoNovoLocal
                // podem originar um Administrador Pai.
                EhAdministradorPai = false,
            };

            await _usuariosLocalRepository.Create(usuarioLocal);
        }

        private async Task GarantirLocalExiste(Guid localId)
        {
            var local = await _locaisRepository.GetById(localId);
            if (local == null)
                throw new KeyNotFoundException($"Local com id {localId} não encontrado.");
        }

        // Listar dados do perfil logado
        public async Task<UsuarioResponseDto?> GetMeuPerfil()
        {
            var usuarioLogado = await _user.GetCurrentUser();
            if (usuarioLogado == null)
                return null;

            var userId = usuarioLogado.UserId;
            if (userId == null)
                return null;

            var usuarioDb = await _usuariosRepository.GetById(userId.Value);

            if (usuarioDb == null)
                return null;

            return new UsuarioResponseDto
            {
                Id = usuarioDb.Id,
                Nome = usuarioDb.Nome,
                Email = usuarioDb.Email ?? string.Empty,
                Cpf = usuarioDb.Cpf ?? string.Empty,
                Role = usuarioDb.Role,
                CreatedAt = usuarioDb.CreatedAt,
                LocalId = usuarioDb.LocalId,
            };
        }

        // Atualizar dados do perfil logado
        public async Task<UsuarioResponseDto> UpdateMyProfileAsync(UsuarioRequestUpdateDto dto)
        {
            var usuarioLogado = await _user.GetCurrentUser();
            if (usuarioLogado == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            var userId = usuarioLogado.UserId;
            if (userId == null)
                throw new KeyNotFoundException("ID do usuário não encontrado.");

            var usuarioDb = await _usuariosRepository.GetById(userId.Value);
            if (usuarioDb == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            var emailJaExiste = await _usuariosRepository.GetByEmail(dto.Email ?? string.Empty);
            if (emailJaExiste != null && emailJaExiste.Id != usuarioDb.Id)
                throw new InvalidOperationException(
                    "Já existe um usuário com este e-mail cadastrado."
                );

            if (
                !string.IsNullOrWhiteSpace(dto.Email)
                && !dto.Email.Equals(usuarioDb.Email, StringComparison.OrdinalIgnoreCase)
            )
            {
                usuarioDb.SecurityStamp = Guid.NewGuid().ToString(); // Atualiza o SecurityStamp para invalidar tokens antigos
            }

            // Atualizar os dados do usuário
            var newUsuario = new Usuario
            {
                Nome = !string.IsNullOrEmpty(dto.Nome) ? dto.Nome : usuarioDb.Nome,
                Email = !string.IsNullOrEmpty(dto.Email) ? dto.Email : usuarioDb.Email,
                Cpf = !string.IsNullOrEmpty(dto.Cpf) ? dto.Cpf : usuarioDb.Cpf,
                Role = usuarioDb.Role, // O usuário não pode alterar sua própria role
                SecurityStamp = usuarioDb.SecurityStamp, // Mantém o SecurityStamp atual, a menos que o e-mail seja alterado
            };

            await _usuariosRepository.Update(userId.Value, newUsuario);

            return new UsuarioResponseDto
            {
                Id = usuarioDb.Id,
                Nome = usuarioDb.Nome,
                Email = usuarioDb.Email ?? string.Empty,
                Cpf = usuarioDb.Cpf ?? string.Empty,
                LocalId = usuarioDb.LocalId,
            };
        }
    }
}
