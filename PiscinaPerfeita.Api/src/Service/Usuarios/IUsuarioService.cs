using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Usuarios
{
    public interface IUsuarioService
    {
        Task<List<UsuarioResponseDto>> Show();
        Task<UsuarioResponseDto> GetById(Guid id);
        Task<UsuarioResponseDto> Create(UsuarioRequestDto dto);
        Task<UsuarioResponseDto> Update(Guid id, UsuarioRequestDto dto);
        Task Delete(Guid id);

    }
}
