
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models;

public partial class Usuario
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string Nome { get; set; } = null!;

    public string? Email { get; set; }

    public string? Senhahash { get; set; }
    public Guid PiscinaId { get; set; }

    public virtual Piscina Piscina { get; set; } = null!;

    public virtual ICollection<Piscina> Piscinas { get; set; } = new List<Piscina   >();
}
