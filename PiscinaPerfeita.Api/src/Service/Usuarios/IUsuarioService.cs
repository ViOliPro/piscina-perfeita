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

        // Fluxo completo de "esqueci minha senha": gera o token E dispara o
        // e-mail via Resend. Antes o controller chamava PasswordResetToken()
        // diretamente (que só gera o token) porque este método nem existia
        // na interface — o e-mail nunca era enviado.
        Task EsqueciSenha(EsqueciSenhaRequestDto dto);

        Task<PasswordResetToken?> GetPasswordResetTokenByHash(string tokenHash);

        Task<ConviteResponseDto> CriarConvite(ConviteRequestDto dto);
        Task CompletarConvite(CompletarConviteRequestDto dto);

        // Google Auth
        Task<Usuario?> ObterOuVincularPorEmailGoogleAsync(string email);

        // Usado pelo AutenticarAsync pra decidir entre "acesso bloqueado" e
        // "tem convite, redireciona pra completar cadastro".
        Task<bool> ExisteConviteAtivoAsync(string email);
        Task<Usuario?> CompletarConviteGoogleAsync(string email, string nome, string? cpf);
    }
}
