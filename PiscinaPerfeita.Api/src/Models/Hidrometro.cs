using PiscinaPerfeita.Api.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models
{
    public class Hidrometro:IBelongsToLocal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        public Guid LocalId { get; set; }
        public float ? Consumo { get; set; }
        public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;

        [ForeignKey(nameof(LocalId))]
        public virtual Local Local { get; set; } = null!;

    }
}
