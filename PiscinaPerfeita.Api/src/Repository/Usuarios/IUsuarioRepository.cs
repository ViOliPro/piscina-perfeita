using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Usuarios
{
    public interface IUsuarioRepository
    {
        Task<List<UsuarioResponseDto>> Show();

        Task<UsuarioResponseDto?> GetByIdDto(Guid id);

        //Se o usuario logado for um usuario
        //A lista de usuario lista apenas Usuarios daquele Local
        // Não lista nenhum usuario com a Role SuperAdmin
        Task<List<UsuarioResponseDto>> FilterRoleUsuario(Guid localId);

        Task Create(Usuario usuario);

        Task Update(Guid id, Usuario usuario);

        Task UpdateUltimoLocal(Guid id, Guid localId);

        Task Delete(Guid id);

        Task<Usuario?> GetByEmail(string email);

        Task<Usuario?> GetNameById(Guid id);

        Task<Usuario?> GetPasswordById(Guid id);
        Task<Usuario?> GetById(Guid id);
    }
}
