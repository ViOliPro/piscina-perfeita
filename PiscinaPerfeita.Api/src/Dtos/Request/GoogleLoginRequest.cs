using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    public class GoogleLoginRequest
    {
        [Required(ErrorMessage = "O token do Google é obrigatório.")]
        public string IdToken { get; set; } = string.Empty;
    }
}
