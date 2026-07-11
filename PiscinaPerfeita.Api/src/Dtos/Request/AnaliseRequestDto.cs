using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    public class AnaliseRequestDto
    {
        [Required(ErrorMessage = "O ID da piscina é obrigatório.")]
        public Guid PiscinaId { get; set; }

        public Guid? UsuarioId { get; set; } = null;

        [Range(0, 14, ErrorMessage = "O PH deve estar entre 0 e 14.")]
        public decimal? Ph { get; set; }

        [Range(0, 20, ErrorMessage = "O cloro livre deve ser um valor positivo.")]
        public decimal? CloroLivre { get; set; }

        [Range(0, 500, ErrorMessage = "A alcalinidade deve ser um valor positivo.")]
        public decimal? Alcalinidade { get; set; }

        public decimal? Temperatura { get; set; }

        [StringLength(500, ErrorMessage = "As observações não podem passar de 500 caracteres.")]
        public string? Observacoes { get; set; }
        public DateTimeOffset? DataAnalise { get; set; }
    }
}
