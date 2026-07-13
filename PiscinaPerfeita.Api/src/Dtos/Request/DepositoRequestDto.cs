using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    public class DepositoRequestDto
    {
        [Required(ErrorMessage = "Nome é obrigatorio")]
        public string Nome { get; set; } = string.Empty;
        public string? Observacao { get; set; }
        public Guid LocalId { get; set; }

    }
}
