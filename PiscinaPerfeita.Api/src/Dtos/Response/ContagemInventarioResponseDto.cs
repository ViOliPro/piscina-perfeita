namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class ContagemInventarioResultadoDto
    {
        public Guid ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public decimal QuantidadeAnterior { get; set; }
        public decimal QuantidadeContada { get; set; }
        public decimal Diferenca { get; set; }

        // Só é gerada uma MovimentacaoEstoque quando há diferença real
        // (Diferenca != 0) — contagens que batem com o estoque lógico não
        // geram ruído no histórico de movimentações.
        public Guid? MovimentacaoEstoqueId { get; set; }
    }
}
