using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models;

[Table("Estoques", Schema = "piscina-perfeita")]
public partial class Estoque
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    public Guid PiscinaId { get; set; }

    public Guid ProdutoId { get; set; }

    public decimal? QuantidadeAtual { get; set; }

    [ForeignKey("PiscinaId")]
    public virtual Piscina Piscina { get; set; } = null!;

    [ForeignKey("ProdutoId")]
    public virtual Produto Produto { get; set; } = null!;
}
