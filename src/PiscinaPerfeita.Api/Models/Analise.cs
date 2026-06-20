using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models
{
    [Table("Analises")]
    public class Analise
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required()]
        public Guid PiscinaId { get; set; }

        public DateTimeOffset DataAnalise { get; set; }

        public decimal? Ph { get; set; }

        public decimal? CloroLivre { get; set; }

        public decimal? Alcalinidade { get; set; }

        public List<decimal>? Temperatura { get; set; }

        public string? Observacoes { get; set; }

        [ForeignKey("PiscinaId")]
        public virtual Piscina Piscina { get; set; } = null!;
    }
}

