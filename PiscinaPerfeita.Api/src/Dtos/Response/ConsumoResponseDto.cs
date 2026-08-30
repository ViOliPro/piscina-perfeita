using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class ConsumoProdutoDto
    {
        public Guid ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string UnidadeMedida { get; set; } = string.Empty;
        public decimal QuantidadeConsumida { get; set; }
    }

    public class ConsumoResponseDto
    {
        public NomeIdDto Deposito { get; set; } = null!;
        public PeriodoDto Periodo { get; set; } = new();

        // Ordenado por QuantidadeConsumida decrescente — o produto que
        // mais saiu no período já vem primeiro, sem o front precisar
        // reordenar pra montar o gráfico de barras.
        public List<ConsumoProdutoDto> Produtos { get; set; } = [];
    }
}
