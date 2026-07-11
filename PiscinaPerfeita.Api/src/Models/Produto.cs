using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PiscinaPerfeita.Api.Models.Interfaces;

namespace PiscinaPerfeita.Api.Models;

[Table("Produtos", Schema = "piscina-perfeita")]
public partial class Produto : IBelongsToLocal
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }
    public Guid LocalId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string UnidadeMedida { get; set; } = string.Empty;

    public virtual ICollection<Estoque> Estoques { get; set; } = [];
    public virtual ICollection<MovimentacaoEstoque> MovimentacoesEstoques { get; set; } = [];

    [ForeignKey(nameof(LocalId))]
    public virtual Local Local { get; set; } = null!;
}
