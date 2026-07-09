using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.UsuariosLocal
{
    public interface IUsuarioLocalRepository
    {
        Task<List<UsuarioLocalResponseDto>> Show();

        Task<UsuarioLocalResponseDto?> GetById(Guid id);

        Task<UsuarioLocalResponseDto?> GetByUserId(Guid userId);

        Task<int> QtdUserByLocal(Guid id);

        Task Create(UsuarioLocal usuarioLocal);

        Task Update(Guid id, UsuarioLocal usuarioLocal);

        Task Delete(Guid id);

        Task<UsuarioLocal?> Vinculo(Guid userId, Guid newLocalId);
    }
}
