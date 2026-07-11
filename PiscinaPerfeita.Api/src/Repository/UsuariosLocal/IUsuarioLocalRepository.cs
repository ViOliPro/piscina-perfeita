using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.UsuariosLocal
{
    public interface IUsuarioLocalRepository
    {
        Task<List<UsuarioLocalResponseDto>> Show();

        Task<UsuarioLocalResponseDto?> GetById(Guid id);

        Task<UsuarioLocalResponseDto?> GetByUserId(Guid userId);

        // Retorna TODOS os vínculos ativos do usuário (para o seletor de "Trocar Local")
        Task<List<UsuarioLocalResponseDto>> GetAllByUserId(Guid userId);

        Task<int> QtdUserByLocal(Guid id);

        Task Create(UsuarioLocal usuarioLocal);

        Task Update(Guid id, UsuarioLocal usuarioLocal);

        Task Delete(Guid id);

        Task<UsuarioLocal?> Vinculo(Guid userId, Guid newLocalId);
    }
}
