namespace PiscinaPerfeita.Api.Dtos.Request
{
    public class RedefinirSenhaRequestDto
    {
        public string Token { get; set; } = string.Empty;
        public string NovaSenha { get; set; } = string.Empty;
    }
}
