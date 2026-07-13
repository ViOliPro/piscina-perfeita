using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Models.Interfaces;

namespace PiscinaPerfeita.Api.Models
{
    [Table("MovimentacoesEstoque", Schema = "piscina-perfeita")]
    public partial class MovimentacaoEstoque : IBelongsToLocal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        // Nullable de propósito: Compra/Perda/Descarte/AjusteInventario são
        // movimentações de depósito que nem sempre estão ligadas a uma
        // piscina específica. Entrada/Saida/Aplicacao continuam exigindo
        // PiscinaId — validado no MovimentacaoService, não aqui no model.
        public Guid? PiscinaId { get; set; }

        public Guid ProdutoId { get; set; }

        public Guid DepositoId { get; set; }

        public Guid UsuarioId { get; set; }

        public Guid LocalId { get; set; }

        public Tipo TipoMovimentacao { get; set; }

        public decimal? Quantidade { get; set; }

        // Unidade em que Quantidade foi lançada (pode diferir da unidade
        // "base" do produto — ver AplicacaoProduto/Helpers/Conversoes).
        public string? UnidadeLancamento { get; set; }

        public DateTimeOffset DataMovimentacao { get; set; } = DateTimeOffset.UtcNow;

        [ForeignKey("PiscinaId")]
        public virtual Piscina? Piscina { get; set; }

        [ForeignKey("ProdutoId")]
        public virtual Produto Produto { get; set; } = null!;

        [ForeignKey(nameof(DepositoId))]
        public virtual Deposito Deposito { get; set; } = null!;

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuarios { get; set; } = null!;

        [ForeignKey(nameof(LocalId))]
        public virtual Local? Local { get; set; }
    }
}
