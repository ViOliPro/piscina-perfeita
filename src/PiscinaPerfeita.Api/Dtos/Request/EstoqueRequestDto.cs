using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request;

public class EstoqueRequestDto
{
    [Required(ErrorMessage = "O ID da piscina é obrigatório.")]
    public Guid PiscinaId { get; set; }

    [Required(ErrorMessage = "O ID do produto é obrigatório.")]
    public Guid ProdutoId { get; set; }

    [Required(ErrorMessage = "A quantidade atual é obrigatória.")]
    [Range(0, 999999, ErrorMessage = "A quantidade não pode ser negativa.")]
    public decimal? QuantidadeAtual { get; set; }
}
