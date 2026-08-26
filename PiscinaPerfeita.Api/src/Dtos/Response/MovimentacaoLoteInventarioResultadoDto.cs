
namespace PiscinaPerfeita.Api.Dtos.Response
{

    public class MovimentacaoLoteInventarioResultadoDto
    {
        public Guid ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public decimal QuantidadeAnterior { get; set; }
        public decimal QuantidadeMovimentada { get; set; }
        public decimal QuantidadeAtual { get; set; }
        public Guid? MovimentacaoEstoqueId { get; set; }
    }

}
