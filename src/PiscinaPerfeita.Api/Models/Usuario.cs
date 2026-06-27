using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models
{
    [Table("Usuarios", Schema = "piscina-perfeita")]
    public partial class Usuario
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        public string Nome { get; set; } = null!;
        public string? Email { get; set; }
        public string? SenhaHash { get; set; }
        public Role Role { get; set; }

        [Column("CreatedAt")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public virtual ICollection<Piscina> Piscinas { get; set; } = new List<Piscina>();
    }
    public enum Role
    {
        Admin = 0,
        User = 1
    }
}