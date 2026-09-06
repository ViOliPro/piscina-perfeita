using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class UsoProdutoItemDto
    {
        public Guid ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        /// <summary>Unidade base do produto (ex.: L, kg) — quantidades já convertidas.</summary>
        public string Unidade { get; set; } = string.Empty;
        public decimal QuantidadeTotal { get; set; }
        public int QuantidadeAplicacoes { get; set; }
    }

    public class UsoProdutosResponseDto
    {
        public NomeIdDto Piscina { get; set; } = null!;
        public PeriodoDto Periodo { get; set; } = new();
        /// <summary>Ordenado por QuantidadeTotal desc.</summary>
        public List<UsoProdutoItemDto> Itens { get; set; } = [];
        public string TextoResumo { get; set; } = string.Empty;
    }
}
