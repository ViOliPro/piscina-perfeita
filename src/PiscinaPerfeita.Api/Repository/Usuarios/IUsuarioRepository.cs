using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Repository.Usuarios
{
    public interface IUsuarioRepository
    {

        Task<List<UsuarioResponseDto>> Show();

        Task<UsuarioResponseDto?> GetById(Guid id);

        Task Create(Usuario usuario);

        Task Update(Guid id, Usuario usuario);

        Task Delete(Guid id);
    }
}
