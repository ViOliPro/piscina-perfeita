using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.UsuariosLocal
{
    public interface IUsuarioLocalService
    {
        Task<List<UsuarioLocalResponseDto>> Show();
        Task<UsuarioLocalResponseDto> GetById(Guid id);
        Task<UsuarioLocalResponseDto> Create(UsuarioLocalRequestDto dto);
        Task<UsuarioLocalResponseDto> Update(Guid id, UsuarioLocalRequestDto dto);
        Task Delete(Guid id);
    }
}
