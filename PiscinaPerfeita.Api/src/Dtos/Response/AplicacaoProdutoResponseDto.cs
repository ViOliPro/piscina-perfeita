using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class AplicacaoProdutoResponseDto
    {
        public Guid Id { get; set; }
        public NomeIdDto? Piscina { get; set; }
        public NomeIdDto? Produto { get; set; }
        public NomeIdDto? Deposito { get; set; }
        public NomeIdDto? Usuario { get; set; }
        public Guid? AnaliseId { get; set; }
        public Guid? MovimentacaoEstoqueId { get; set; }
        public decimal Quantidade { get; set; }
        public string UnidadeLancamento { get; set; } = string.Empty;
        public DateTimeOffset DataAplicacao { get; set; }
        public string? Observacoes { get; set; }
    }
}
