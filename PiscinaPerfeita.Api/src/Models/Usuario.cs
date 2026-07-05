using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models.Interfaces;

namespace PiscinaPerfeita.Api.Models
{
    [Table("Usuarios", Schema = "piscina-perfeita")]
    public partial class Usuario : IBelongsToLocal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        public Guid LocalId { get; set; }

        public string Nome { get; set; } = null!;

        [Column("Email")]
        public string? Email { get; set; }

        public string? SenhaHash { get; set; }

        public Role Role { get; set; }

        [Column("CreatedAt")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public virtual ICollection<Piscina> Piscinas { get; set; } = [];

        [ForeignKey(nameof(LocalId))]
        public virtual Local Local { get; set; } = null!;
    }
}
