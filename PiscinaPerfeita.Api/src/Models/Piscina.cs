using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PiscinaPerfeita.Api.Models.Interfaces;

namespace PiscinaPerfeita.Api.Models;

[Table("Piscinas", Schema = "piscina-perfeita")]
public partial class Piscina : IBelongsToLocal
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }
    public Guid LocalId { get; set; }

    public string Nome { get; set; } = null!;

    public decimal? VolumeLitros { get; set; }

    public decimal? ProfundidadeMedia { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual ICollection<Analise> Analises { get; set; } = [];

    public virtual ICollection<Estoque> Estoques { get; set; } = [];

    public virtual ICollection<MovimentacaoEstoque> MovimentacoesEstoques { get; set; } = [];

    [ForeignKey(nameof(UsuarioId))]
    public virtual Usuario Usuario { get; set; } = null!;

    [ForeignKey(nameof(LocalId))]
    public virtual Local Local { get; set; } = null!;
}
