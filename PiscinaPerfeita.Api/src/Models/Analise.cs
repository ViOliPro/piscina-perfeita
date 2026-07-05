using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models
{
    [Table("Analises", Schema = "piscina-perfeita")]
    public class Analise
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        public Guid PiscinaId { get; set; }

        public Guid UsuarioId { get; set; }

        public Guid LocalId { get; set; }

        public DateTimeOffset DataAnalise { get; set; } = DateTimeOffset.UtcNow;

        public decimal? Ph { get; set; }

        public decimal? CloroLivre { get; set; }

        public decimal? Alcalinidade { get; set; }

        public decimal? Temperatura { get; set; }

        public string? Observacoes { get; set; } = string.Empty;

        [ForeignKey("PiscinaId")]
        public virtual Piscina Piscina { get; set; } = null!;

        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario? Usuario { get; set; }

        [ForeignKey(nameof(LocalId))]
        public virtual Local? Local { get; set; }
    }
}
