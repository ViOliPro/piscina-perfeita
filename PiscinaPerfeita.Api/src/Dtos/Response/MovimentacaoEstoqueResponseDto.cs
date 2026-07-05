using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public partial class MovimentacaoEstoqueResponseDto
    {
        public Guid Id { get; set; }

        public Tipo TipoMovimentacao { get; set; }

        public decimal? Quantidade { get; set; }

        public DateTimeOffset? DataMovimentacao { get; set; }

        // Produto movimentado
        public NomeIdDto? Piscina { get; set; }

        // Produto que realizou a movimentação
        public NomeIdDto? Produto { get; set; }

        // Usuario que realizou a movimentação
        public NomeIdDto? Usuario { get; set; }
    }
}
