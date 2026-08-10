using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    public class CompletarConviteGoogleRequest
    {
        [Required(ErrorMessage = "O token do Google é obrigatório.")]
        public string IdToken { get; set; } = string.Empty;

        // Opcional por enquanto — vira obrigatório quando pagamento/assinatura entrar.
        public string? Cpf { get; set; }
    }
}
