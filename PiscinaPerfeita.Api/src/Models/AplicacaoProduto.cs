using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PiscinaPerfeita.Api.Models.Interfaces;

namespace PiscinaPerfeita.Api.Models
{
    // Registro de aplicação de um produto em uma piscina (ex.: "usei 500mL
    // de Algicida na Piscina X"). Ao ser criada, gera automaticamente uma
    // MovimentacaoEstoque (Tipo = Aplicacao) que dá baixa no Estoque do
    // Depósito de onde o produto saiu — ver AplicacaoProdutoService.
    [Table("AplicacoesProduto", Schema = "piscina-perfeita")]
    public class AplicacaoProduto : IBelongsToLocal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        public Guid LocalId { get; set; }

        public Guid PiscinaId { get; set; }

        public Guid ProdutoId { get; set; }

        // De qual Depósito o produto foi retirado — necessário porque um
        // mesmo Produto pode ter saldo em mais de um Depósito.
        public Guid DepositoId { get; set; }

        public Guid UsuarioId { get; set; }

        // Análise que motivou esta aplicação (ex.: pH baixo → aplicação de
        // Elevador de pH). Opcional.
        public Guid? AnaliseId { get; set; }

        // Movimentação de estoque gerada automaticamente para esta aplicação.
        public Guid? MovimentacaoEstoqueId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantidade { get; set; }

        // Unidade em que Quantidade foi informada (ex.: "mL", "g") — pode
        // divergir da UnidadeMedida "base" do Produto (ex.: produto em L,
        // aplicação em mL). Convertida automaticamente para dar baixa no
        // Estoque na unidade correta.
        public string UnidadeLancamento { get; set; } = string.Empty;

        public DateTimeOffset DataAplicacao { get; set; } = DateTimeOffset.UtcNow;

        [StringLength(500)]
        public string? Observacoes { get; set; }

        [ForeignKey(nameof(LocalId))]
        public virtual Local Local { get; set; } = null!;

        [ForeignKey(nameof(PiscinaId))]
        public virtual Piscina Piscina { get; set; } = null!;

        [ForeignKey(nameof(ProdutoId))]
        public virtual Produto Produto { get; set; } = null!;

        [ForeignKey(nameof(DepositoId))]
        public virtual Deposito Deposito { get; set; } = null!;

        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; } = null!;

        [ForeignKey(nameof(AnaliseId))]
        public virtual Analise? Analise { get; set; }

        [ForeignKey(nameof(MovimentacaoEstoqueId))]
        public virtual MovimentacaoEstoque? MovimentacaoEstoque { get; set; }
    }
}
