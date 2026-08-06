using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Models
{
    [Table("Usuarios", Schema = "piscina-perfeita")]
    public partial class Usuario
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        public Guid? LocalId { get; set; } = null;

        public Guid? UltimoLocalId { get; set; } = null;

        public string Nome { get; set; } = null!;

        [Column("Email")]
        public string? Email { get; set; }

        public string? Cpf { get; set; }

        public string? SenhaHash { get; set; }

        public Role Role { get; set; }

        // Campo para invalidar tokens antigos
        public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();

        [Column("CreatedAt")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public virtual ICollection<Piscina> Piscinas { get; set; } = [];

        // Vínculos reais de tenant (multi-Local). O par (LocalId/Local) acima
        // é legado/onboarding (ver UltimoLocalId e o fluxo de PrimeiroLocal);
        // quem manda no pertencimento a um tenant é sempre UsuariosLocais.
        public virtual ICollection<UsuarioLocal> UsuariosLocais { get; set; } = [];

        [ForeignKey(nameof(LocalId))]
        public virtual Local Local { get; set; } = null!;
    }
}
