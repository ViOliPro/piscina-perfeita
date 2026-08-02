using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Service.Usuarios
{
    public interface IUsuarioService
    {
        Task<List<UsuarioResponseDto>> Show();
        Task<UsuarioResponseDto> GetById(Guid id);
        Task<UsuarioResponseDto> Create(UsuarioRequestDto dto);
        Task<UsuarioResponseDto> Update(Guid id, UsuarioRequestUpdateDto dto);
        Task Delete(Guid id);
        Task<UsuarioResponseDto?> GetMeuPerfil();
        Task<Usuario?> GetUsuarioByEmail(string email);
        Task<UsuarioResponseDto> UpdateMyProfileAsync(UsuarioRequestUpdateDto dto);
        Task<string?> PasswordResetToken(string tokenHash);
        Task UpdatePasswordResetToken(RedefinirSenhaRequestDto token);

        Task<PasswordResetToken?> GetPasswordResetTokenByHash(string tokenHash);
    }
}
