
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models;

[Table("MovimentacoesEstoque")]
public partial class MovimentacaoEstoque
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid PiscinaId { get; set; }

    public Guid ProdutoId { get; set; }

    public string TipoMovimentacao { get; set; } = string.Empty;

    public decimal? Quantidade { get; set; }

    public decimal? Valor { get; set; }

    public DateTimeOffset? DataMovimentacao { get; set; }

    public virtual Piscina Piscina { get; set; } = null!;

    public virtual Produto Produto { get; set; } = null!;
}
