using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request;

public class EstoqueRequestDto
{
    [Required(ErrorMessage = "O ID do produto é obrigatório.")]
    public Guid ProdutoId { get; set; }

    [Required(ErrorMessage = "O ID do local é obrigatório.")]
    public Guid LocalId { get; set; }

    [Range(0, 999999, ErrorMessage = "A quantidade não pode ser negativa.")]
    public decimal? QuantidadeAtual { get; set; }

    [Range(0, 999999, ErrorMessage = "A quantidade não pode ser negativa.")]
    public decimal? QuantidadeMinima { get; set; }
}
