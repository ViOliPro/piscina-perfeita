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

        public Guid? LocalId { get; set; } = null;

        public bool Ativo { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Perfil Perfil { get; set; } = Perfil.Visualizador;

        /// <summary>
        /// Marca o Administrador ORIGINAL/"Pai" de um Local — quem criou o
        /// Local (self-onboarding) ou foi vinculado diretamente a ele por um
        /// SuperAdmin no cadastro. Regra de negócio: SOMENTE o SuperAdmin
        /// pode alterar ou remover privilégios/vínculos de um Administrador
        /// marcado como Pai; um Administrador comum não pode rebaixar,
        /// desvincular ou apagar esse vínculo (nem o dele próprio), mesmo
        /// dentro do próprio tenant. Vínculos criados depois (Admin Filho,
        /// Operador, Visualizador) nascem sempre com false.
        /// </summary>
        public bool EhAdministradorPai { get; set; } = false;

        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; } = null!;

        [ForeignKey(nameof(LocalId))]
        public virtual Local Local { get; set; } = null!;
    }

    public enum Perfil
    {
        Administrador = 0,
        Operador = 1,
        Visualizador = 2,
    }
}
