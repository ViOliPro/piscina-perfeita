using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Usuarios
{
    public interface IUsuarioRepository
    {

        Task<List<Usuario>> Show();

        Task<Usuario> GetById(Guid id);

        Task Create(Usuario usuario);

        Task Update(Guid id, Usuario usuario);

        Task Delete(Guid id);
    }
}
