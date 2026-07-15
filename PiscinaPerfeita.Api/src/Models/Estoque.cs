using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PiscinaPerfeita.Api.Models.Interfaces;

namespace PiscinaPerfeita.Api.Models;

[Table("Estoques", Schema = "piscina-perfeita")]
public partial class Estoque : IBelongsToLocal
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    public Guid ProdutoId { get; set; }

    public Guid UsuarioId { get; set; }

    public Guid LocalId { get; set; }

    public Guid DepositoId { get; set; }

    public decimal? QuantidadeAtual { get; set; }

    public decimal? QuantidadeMinima { get; set; }

    public decimal? EstoqueIdeal { get; set; }

    [ForeignKey("ProdutoId")]
    public virtual Produto Produto { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    public virtual Usuario Usuario { get; set; } = null!;

    [ForeignKey(nameof(LocalId))]
    public virtual Local? Local { get; set; }

    [ForeignKey(nameof(DepositoId))]
    public virtual Deposito? Deposito { get; set; }
}
