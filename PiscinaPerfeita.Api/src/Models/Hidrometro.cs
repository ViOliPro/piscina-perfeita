using PiscinaPerfeita.Api.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models;

public class Hidrometro : IBelongsToLocal
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid LocalId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal LeituraAtual { get; set; }

    public DateTimeOffset DataLeitura { get; set; }

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    [ForeignKey(nameof(LocalId))]
    public virtual Local Local { get; set; } = null!;
}
