using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Usuarios
{
    public interface IUsuarioRepository
    {
        Task<List<UsuarioResponseDto>> Show();

        Task<UsuarioResponseDto?> GetById(Guid id);

        //Se o usuario logado for um usuario
        //A lista de usuario lista apenas Usuarios daquele Local
        // Não lista nenhum usuario com a Role SuperAdmin
        Task<List<UsuarioResponseDto>> FilterRoleUsuario();

        Task Create(Usuario usuario);

        Task Update(Guid id, Usuario usuario);

        Task Delete(Guid id);

        Task<Usuario?> GetByEmail(string email);

        Task<Usuario?> GetNameById(Guid id);

        Task<Usuario?> GetPasswordById(Guid id);
    }
}
