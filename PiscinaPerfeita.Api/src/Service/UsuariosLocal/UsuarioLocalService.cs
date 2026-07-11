using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Locais;
using PiscinaPerfeita.Api.Repository.UsuariosLocal;

namespace PiscinaPerfeita.Api.Service.UsuariosLocal
{
    public class UsuarioLocalService : IUsuarioLocalService
    {
        private readonly IUsuarioLocalRepository _usuariosLocalRepository;
        private readonly ILocalRepository _locaisRepository;
        private readonly IAuthenticatedUser _user;

        public UsuarioLocalService(
            IUsuarioLocalRepository usuariosRepository,
            ILocalRepository locaisRepository,
            IAuthenticatedUser user
        )
        {
            _usuariosLocalRepository =
                usuariosRepository ?? throw new ArgumentNullException(nameof(usuariosRepository));
            _locaisRepository =
                locaisRepository ?? throw new ArgumentNullException(nameof(locaisRepository));
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

        // Vincular/desvincular um usuário a um Local é uma ação administrativa
        // — é exatamente o passo de "vincular o Local ao usuário" que o
        // SuperAdmin faz depois de criar cada entidade de forma independente.
        public async Task<UsuarioLocalResponseDto> Create(UsuarioLocalRequestDto dto)
        {
            await GarantirSuperAdmin();

            if (dto.LocalId.HasValue)
                await GarantirLocalExiste(dto.LocalId.Value);

            var vinculosExistentes = await _usuariosLocalRepository.GetAllByUserId(dto.UsuarioId);
            if (dto.LocalId.HasValue && vinculosExistentes.Any(v => v.LocalId == dto.LocalId))
                throw new InvalidOperationException("Este usuário já está cadastrado neste local");

            var newUser = new UsuarioLocal
            {
                UsuarioId = dto.UsuarioId,
                LocalId = dto.LocalId,
                Perfil = dto.Perfil,
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
            };
        }

        // Também usado para o passo de "vincular": o SuperAdmin edita o
        // vínculo pendente (criado com LocalId nulo junto do usuário) e
        // define o LocalId, oficializando a ligação usuário ↔ local.
        public async Task<UsuarioLocalResponseDto> Update(Guid id, UsuarioLocalRequestDto dto)
        {
            await GarantirSuperAdmin();

            if (id == Guid.Empty)
            {
                throw new ArgumentException($"O id informado não pode ser vazio {nameof(id)} .");
            }

            var usuario = await _usuariosLocalRepository.GetById(id);
            if (usuario == null)
            {
                throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");
            }

            if (dto.LocalId.HasValue)
                await GarantirLocalExiste(dto.LocalId.Value);

            var newUser = new UsuarioLocal
            {
                UsuarioId = dto.UsuarioId,
                LocalId = dto.LocalId,
                Perfil = dto.Perfil,
            };

            await _usuariosLocalRepository.Update(id, newUser);

            return new UsuarioLocalResponseDto
            {
                Id = id,
                UsuarioId = newUser.UsuarioId,
                LocalId = newUser.LocalId,
                Perfil = newUser.Perfil,
                CreatedAt = newUser.CreatedAt,
                Ativo = newUser.Ativo,
            };
        }

        public async Task Delete(Guid id)
        {
            await GarantirSuperAdmin();

            var usuarioDb = await _usuariosLocalRepository.GetById(id);
            if (usuarioDb == null)
            {
                throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");
            }

            await _usuariosLocalRepository.Delete(id);
        }

        private async Task GarantirSuperAdmin()
        {
            var usuarioLogado = await _user.GetCurrentUser();
            if (usuarioLogado.Role != Role.SuperAdmin)
                throw new UnauthorizedAccessException(
                    "Somente um SuperAdmin pode gerenciar vínculos entre usuários e locais."
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
