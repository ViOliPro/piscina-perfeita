using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Locais;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Repository.UsuariosLocal;

namespace PiscinaPerfeita.Api.Service.UsuariosLocal
{
    public class UsuarioLocalService : IUsuarioLocalService
    {
        private readonly IUsuarioLocalRepository _usuariosLocalRepository;
        private readonly ILocalRepository _locaisRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAuthenticatedUser _user;

        public UsuarioLocalService(
            IUsuarioLocalRepository usuariosRepository,
            ILocalRepository locaisRepository,
            IUsuarioRepository usuarioRepository,
            IAuthenticatedUser user
        )
        {
            _usuariosLocalRepository =
                usuariosRepository ?? throw new ArgumentNullException(nameof(usuariosRepository));
            _locaisRepository =
                locaisRepository ?? throw new ArgumentNullException(nameof(locaisRepository));
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        public async Task<List<UsuarioLocalResponseDto>> Show()
        {
            await GarantirSuperAdmin();
            return await _usuariosLocalRepository.Show();
        }

        public async Task<UsuarioLocalResponseDto> GetById(Guid id)
        {
            await GarantirSuperAdmin();

            var usuarioDb = await _usuariosLocalRepository.GetById(id);
            if (usuarioDb == null)
            {
                throw new KeyNotFoundException($"Usuario com id {id} não encontrado");
            }

            return usuarioDb;
        }

        public async Task<List<UsuarioLocalResponseDto>> GetMeusLocais()
        {
            var userId = _user.GetUserId();
            return await _usuariosLocalRepository.GetAllByUserId(userId);
        }

        public async Task<List<UsuarioLocalResponseDto>> GetByUsuario(Guid usuarioId)
        {
            // Um usuário pode sempre consultar os próprios vínculos; consultar
            // os de outra pessoa é uma ação administrativa (tela de gestão de
            // usuários), restrita a SuperAdmin.
            if (usuarioId != _user.GetUserId())
                await GarantirSuperAdmin();

            return await _usuariosLocalRepository.GetAllByUserId(usuarioId);
        }

        // Vincular/desvincular um usuário a um Local é uma ação administrativa.
        // CORRIGIDO: antes disso era restrito a SuperAdmin (GarantirSuperAdmin),
        // o que contrariava a regra de negócio — "O Administrador Pai pode
        // alterar, gerenciar ou remover privilégios e vínculos de seus
        // usuários filhos". Agora um Administrador comum também pode criar
        // vínculos, mas só dentro do próprio Local ativo (nunca em outro
        // tenant) e nunca marcando o novo vínculo como Administrador Pai.
        public async Task<UsuarioLocalResponseDto> Create(UsuarioLocalRequestDto dto)
        {
            var localId = await GarantirPodeGerenciarNoLocal(dto.LocalId);

            if (localId.HasValue)
                await GarantirLocalExiste(localId.Value);

            var vinculosExistentes = await _usuariosLocalRepository.GetAllByUserId(dto.UsuarioId);
            if (localId.HasValue && vinculosExistentes.Any(v => v.LocalId == localId))
                throw new InvalidOperationException("Este usuário já está cadastrado neste local");

            var newUser = new UsuarioLocal
            {
                UsuarioId = dto.UsuarioId,
                LocalId = localId,
                Perfil = dto.Perfil,
                // EhAdministradorPai nunca vem do request (não existe no
                // DTO): só nasce true nos fluxos internos de onboarding do
                // SuperAdmin/LocalService — nunca por essa rota.
                EhAdministradorPai = false,
            };

            await _usuariosLocalRepository.Create(newUser);

            return new UsuarioLocalResponseDto
            {
                Id = newUser.Id,
                UsuarioId = newUser.UsuarioId,
                LocalId = newUser.LocalId,
                Perfil = newUser.Perfil,
                CreatedAt = newUser.CreatedAt,
                Ativo = newUser.Ativo,
                EhAdministradorPai = newUser.EhAdministradorPai,
            };
        }

        // Também usado para o passo de "vincular": edita o vínculo pendente
        // (criado com LocalId nulo junto do usuário) e define o LocalId,
        // oficializando a ligação usuário ↔ local.
        public async Task<UsuarioLocalResponseDto> Update(Guid id, UsuarioLocalRequestDto dto)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException($"O id informado não pode ser vazio {nameof(id)} .");
            }

            var usuario = await GarantirPodeGerenciarVinculo(id);

            var localId = await GarantirPodeGerenciarNoLocal(dto.LocalId);
            if (localId.HasValue)
                await GarantirLocalExiste(localId.Value);

            var perfilMudou = usuario.Perfil != dto.Perfil;

            var newUser = new UsuarioLocal
            {
                UsuarioId = dto.UsuarioId,
                LocalId = localId,
                Perfil = dto.Perfil,
                EhAdministradorPai = usuario.EhAdministradorPai,
            };

            await _usuariosLocalRepository.Update(id, newUser);

            // Mudou o Perfil (nível de permissão) desse vínculo — invalida
            // imediatamente qualquer token já emitido pra esse usuário, forçando
            // relogin. Sem isso, um rebaixamento (ex: Operador virando
            // Visualizador) só teria efeito quando o token expirasse (até 1h) ou
            // o usuário trocasse de Local manualmente.

            if(perfilMudou)
                await _usuarioRepository.RotateSecurityStamp(dto.UsuarioId);

            return new UsuarioLocalResponseDto
            {
                Id = id,
                UsuarioId = newUser.UsuarioId,
                LocalId = newUser.LocalId,
                Perfil = newUser.Perfil,
                CreatedAt = newUser.CreatedAt,
                Ativo = newUser.Ativo,
                EhAdministradorPai = newUser.EhAdministradorPai,
            };
        }

        public async Task Delete(Guid id)
        {
            var usuario = await GarantirPodeGerenciarVinculo(id);

            await _usuariosLocalRepository.Delete(id);
            await _usuarioRepository.RotateSecurityStamp(usuario.UsuarioId);
        }

        // SuperAdmin sempre pode. Um Administrador comum só pode gerenciar
        // vínculos DENTRO do seu próprio Local ativo — nunca em outro
        // LocalId (mesmo que informado no dto) e nunca vínculos pendentes
        // (LocalId nulo) de outro Local, que são exclusivos do onboarding
        // conduzido pelo SuperAdmin.
        private async Task<Guid?> GarantirPodeGerenciarNoLocal(Guid? localIdDoDto)
        {
            var usuarioLogado = await _user.GetCurrentUser();
            if (usuarioLogado.Role == Role.SuperAdmin)
                return localIdDoDto;

            if (usuarioLogado.Perfil != Perfil.Administrador)
                throw new UnauthorizedAccessException(
                    "Somente um SuperAdmin ou um Administrador pode gerenciar vínculos entre usuários e locais."
                );

            if (usuarioLogado.LocalId == null || usuarioLogado.LocalId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Selecione um Local ativo para gerenciar vínculos de usuários."
                );

            // Ignora silenciosamente um LocalId diferente vindo do dto — o
            // Administrador só pode agir sobre o próprio Local ativo.
            return usuarioLogado.LocalId;
        }

        // Garante que o vínculo (id) existe, pertence ao Local ativo do
        // Administrador logado (SuperAdmin ignora essa restrição) e não é o
        // vínculo do Administrador Pai — que só o SuperAdmin pode alterar ou
        // remover.
        private async Task<UsuarioLocalResponseDto> GarantirPodeGerenciarVinculo(Guid id)
        {
            var usuario = await _usuariosLocalRepository.GetById(id);
            if (usuario == null)
                throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");

            var usuarioLogado = await _user.GetCurrentUser();
            if (usuarioLogado.Role == Role.SuperAdmin)
                return usuario;

            if (
                usuarioLogado.Perfil != Perfil.Administrador
                || usuarioLogado.LocalId == null
                || usuario.LocalId != usuarioLogado.LocalId
            )
                // 404 (não 403): não confirma a existência do vínculo fora
                // do tenant do Administrador logado.
                throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");

            if (usuario.EhAdministradorPai)
                throw new UnauthorizedAccessException(
                    "Somente o SuperAdmin pode alterar ou remover o vínculo do Administrador responsável por este Local."
                );

            return usuario;
        }

        private async Task GarantirSuperAdmin()
        {
            var usuarioLogado = await _user.GetCurrentUser();
            if (usuarioLogado.Role != Role.SuperAdmin)
                throw new UnauthorizedAccessException(
                    "Somente um SuperAdmin pode executar esta ação."
                );
        }

        private async Task GarantirLocalExiste(Guid localId)
        {
            var local = await _locaisRepository.GetById(localId);
            if (local == null)
                throw new KeyNotFoundException($"Local com id {localId} não encontrado.");
        }
    }
}
