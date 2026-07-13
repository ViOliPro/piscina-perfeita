using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PiscinaPerfeita.Api.Models.Interfaces;

namespace PiscinaPerfeita.Api.Models
{
    [Table("Depositos", Schema ="piscina-perfeita")]
    public class Deposito : IBelongsToLocal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Observacao { get; set; } = null;
        public Guid LocalId { get; set; }

        [ForeignKey(nameof(LocalId))]
        public virtual Local Local { get; set; } = null!;
    }
}
