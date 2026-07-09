using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.UsuariosLocal;

namespace PiscinaPerfeita.Api.Service.UsuariosLocal
{
    public class UsuarioLocalService : IUsuarioLocalService
    {
        private readonly IUsuarioLocalRepository _usuariosLocalRepository;
        private readonly IAuthenticatedUser _user;

        public UsuarioLocalService(
            IUsuarioLocalRepository usuariosRepository,
            IAuthenticatedUser user
        )
        {
            _usuariosLocalRepository =
                usuariosRepository ?? throw new ArgumentNullException(nameof(usuariosRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        public async Task<List<UsuarioLocalResponseDto>> Show()
        {
            return await _usuariosLocalRepository.Show();
        }

        public async Task<UsuarioLocalResponseDto> GetById(Guid id)
        {
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
            return await _usuariosLocalRepository.GetAllByUserId(usuarioId);
        }

        public async Task<UsuarioLocalResponseDto> Create(UsuarioLocalRequestDto dto)
        {
            // NOTA: a versão anterior ignorava dto.UsuarioId e sempre vinculava
            // o usuário logado ao invés do usuário informado no DTO — corrigido aqui.
            var vinculosExistentes = await _usuariosLocalRepository.GetAllByUserId(dto.UsuarioId);
            if (vinculosExistentes.Any(v => v.LocalId == dto.LocalId))
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

        public async Task<UsuarioLocalResponseDto> Update(Guid id, UsuarioLocalRequestDto dto)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException($"O id informado não pode ser vazio {nameof(id)} .");
            }

            var usuario = await _usuariosLocalRepository.GetById(id);
            if (usuario == null)
            {
                throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");
            }

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
            var usuarioDb = await _usuariosLocalRepository.GetById(id);
            if (usuarioDb == null)
            {
                throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");
            }

            await _usuariosLocalRepository.Delete(id);
        }
    }
}
