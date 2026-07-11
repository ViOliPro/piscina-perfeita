namespace PiscinaPerfeita.Api.Dtos.Response;

public partial class EstoqueResponseDto
{
    public Guid Id { get; set; }
    public ProdutoEstoque? Produto { get; set; }
    public decimal? QuantidadeAtual { get; set; }
    public decimal? QuantidadeMinima { get; set; }
}

public class ProdutoEstoque
{
    public Guid Id { set; get; }
    public string Nome { set; get; } = string.Empty;
    public string UnidadeMedida { get; set; } = string.Empty;
}
