using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public partial class MovimentacaoEstoqueResponseDto
    {
        public Guid Id { get; set; }

        public Tipo TipoMovimentacao { get; set; }

        public decimal? Quantidade { get; set; }

        public string? UnidadeLancamento { get; set; }

        public DateTimeOffset? DataMovimentacao { get; set; }

        // Piscina em que o produto foi/será utilizado
        public NomeIdDto? Piscina { get; set; }

        // Produto movimentado
        public NomeIdDto? Produto { get; set; }

        // Depósito de onde saiu / para onde entrou o produto
        public NomeIdDto? Deposito { get; set; }

        // Usuario que realizou a movimentação
        public NomeIdDto? Usuario { get; set; }
    }
}
