using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models;

[Table("Estoques", Schema = "piscina-perfeita")]
public partial class Estoque
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    public Guid ProdutoId { get; set; }

    public Guid UsuarioId { get; set; }

    public decimal? QuantidadeAtual { get; set; }

    public decimal? QuantidadeMinima { get; set; }

    [ForeignKey("ProdutoId")]
    public virtual Produto Produto { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    public virtual Usuario Usuario { get; set; } = null!;
}
