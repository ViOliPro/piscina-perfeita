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

        // Locais vinculados ao usuário atualmente autenticado — usado pelo
        // seletor "Trocar Local" no front.
        Task<List<UsuarioLocalResponseDto>> GetMeusLocais();

        // Locais vinculados a um usuário específico — usado nas telas de
        // administração de usuários.
        Task<List<UsuarioLocalResponseDto>> GetByUsuario(Guid usuarioId);
    }
}
