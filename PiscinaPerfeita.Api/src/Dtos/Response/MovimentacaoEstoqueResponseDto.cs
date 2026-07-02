using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{

    public partial class MovimentacaoEstoqueResponseDto
    {
        public Guid Id { get; set; }

        public Tipo TipoMovimentacao { get; set; }

        public decimal? Quantidade { get; set; }

        public string Valor { get; set; } = string.Empty;

        public DateTimeOffset? DataMovimentacao { get; set; }

        // Produto movimentado
        public NomeIdDto? Piscina { get; set; }


        // Usuario que realizou a movimentação
        public NomeIdDto? Produto { get; set; }
    }


}
