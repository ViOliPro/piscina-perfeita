using PiscinaPerfeita.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request
{

    // Um único cabeçalho para uma nota de entrada/compra ou uma conferência de inventário.
    public class MovimentacaoLoteInventarioRequestDto
    {
        [Required] public Guid DepositoId { get; set; }
        [Required] public Tipo TipoMovimentacao { get; set; }
        public DateTimeOffset? DataMovimentacao { get; set; }
        [Required, MinLength(1)] public List<MovimentacaoLoteInventarioItemRequestDto> Itens { get; set; } = [];
    }

    public class MovimentacaoLoteInventarioItemRequestDto
    {
        [Required] public Guid ProdutoId { get; set; }
        [Range(typeof(decimal), "0", "999999999")] public decimal Quantidade { get; set; }
        // Vazio significa a unidade base do produto. Em ajuste, a quantidade é o saldo físico contado.
        public string? UnidadeLancamento { get; set; }
    }

}
