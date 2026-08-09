namespace PiscinaPerfeita.Api.Service.Account.Google
{
    // Auth/Google/IGoogleTokenValidator.cs
    public interface IGoogleTokenValidator
    {
        Task<GoogleTokenPayload?> ValidarAsync(string idToken);
    }

    public record GoogleTokenPayload(string Email, string Nome, bool EmailVerificado);
}
