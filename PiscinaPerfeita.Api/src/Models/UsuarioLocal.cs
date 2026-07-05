using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models
{
    [Table("UsuariosLocais", Schema = "piscina-perfeita")]
    public class UsuarioLocal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        public Guid UsuarioId { get; set; }

        public Guid LocalId { get; set; }

        public bool Ativo { get; set; } = false;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Perfil Perfil { get; set; } = Perfil.Visualizador;


        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; } = null!;

        [ForeignKey(nameof(LocalId))]
        public virtual Local Local { get; set; } = null!;
    }


    public enum Perfil
    {
        Administrador = 0,
        Operador = 1,
        Visualizador = 2
    }
}
