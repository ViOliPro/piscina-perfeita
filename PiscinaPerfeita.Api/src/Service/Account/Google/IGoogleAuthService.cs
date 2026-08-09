namespace PiscinaPerfeita.Api.Service.Account.Google
{
    // Auth/Google/IGoogleAuthService.cs
    public interface IGoogleAuthService
    {
        Task<AuthResult> AutenticarAsync(string idToken);
        Task<AuthResult> CompletarCadastroAsync(string idToken, string? cpf);
    }

    public enum AuthErro
    {
        TokenInvalido,
        AcessoNaoLiberado,
        ConvitePendente,
    }

    public record AuthResult
    {
        public bool Sucesso { get; init; }
        public string? Token { get; init; }
        public string? Mensagem { get; init; }
        public AuthErro? Erro { get; init; }

        public static AuthResult Ok(string token) => new() { Sucesso = true, Token = token };

        public static AuthResult Falha(AuthErro erro, string mensagem) =>
            new()
            {
                Sucesso = false,
                Erro = erro,
                Mensagem = mensagem,
            };
    }
}
