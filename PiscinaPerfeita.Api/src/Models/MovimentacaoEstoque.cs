using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PiscinaPerfeita.Api.Models.Interfaces;

namespace PiscinaPerfeita.Api.Models
{
    [Table("MovimentacoesEstoque", Schema = "piscina-perfeita")]
    public partial class MovimentacaoEstoque : IBelongsToLocal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        public Guid PiscinaId { get; set; }

        public Guid ProdutoId { get; set; }

        public Guid UsuarioId { get; set; }

        public Guid LocalId { get; set; }

        public Tipo TipoMovimentacao { get; set; }

        public decimal? Quantidade { get; set; }

        public DateTimeOffset DataMovimentacao { get; set; } = DateTimeOffset.UtcNow;

        [ForeignKey("PiscinaId")]
        public virtual Piscina Piscina { get; set; } = null!;

        [ForeignKey("ProdutoId")]
        public virtual Produto Produto { get; set; } = null!;

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuarios { get; set; } = null!;

        [ForeignKey(nameof(LocalId))]
        public virtual Local? Local { get; set; }
    }

    public enum Tipo
    {
        Entrada = 0,
        Saida = 1,
    }
}
