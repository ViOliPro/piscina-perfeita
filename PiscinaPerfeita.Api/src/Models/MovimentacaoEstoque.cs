
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models
{
    [Table("MovimentacoesEstoque", Schema = "piscina-perfeita")]
    public partial class MovimentacaoEstoque
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        public Guid PiscinaId { get; set; }

        public Guid ProdutoId { get; set; }

        public Guid UsuarioId { get; set; }

        public Tipo TipoMovimentacao { get; set; }

        public decimal? Quantidade { get; set; }

        public string Valor { get; set; } = string.Empty;

        public DateTimeOffset DataMovimentacao { get; set; } = DateTimeOffset.UtcNow;

        [ForeignKey("PiscinaId")]
        public virtual Piscina Piscina { get; set; } = null!;

        [ForeignKey("ProdutoId")]
        public virtual Produto Produto { get; set; } = null!;

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuarios { get; set; } = null!;
    }

    public enum Tipo
    {
        Entrada = 0,
        Saida = 1
    }
}
