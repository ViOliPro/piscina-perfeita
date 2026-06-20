using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    public class AnaliseRequestDto
    {
        [Required(ErrorMessage = "O ID da piscina é obrigatório.")]
        public Guid PiscinaId { get; set; }

        [Required(ErrorMessage = "O nível de PH é obrigatório.")]
        [Range(0, 14, ErrorMessage = "O PH deve estar entre 0 e 14.")]
        public decimal? Ph { get; set; }

        [Required(ErrorMessage = "O nível de cloro livre é obrigatório.")]
        [Range(0, 20, ErrorMessage = "O cloro livre deve ser um valor positivo.")]
        public decimal? CloroLivre { get; set; }

        [Required(ErrorMessage = "A alcalinidade é obrigatória.")]
        [Range(0, 500, ErrorMessage = "A alcalinidade deve ser um valor positivo.")]
        public decimal? Alcalinidade { get; set; }

        // Ajustado para List<decimal> para bater com a sua Model
        public List<decimal>? Temperatura { get; set; }

        [StringLength(500, ErrorMessage = "As observações não podem passar de 500 caracteres.")]
        public string? Observacoes { get; set; }
    }
}
