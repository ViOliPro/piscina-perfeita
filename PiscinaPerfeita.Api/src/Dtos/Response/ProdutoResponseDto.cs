
using System.Text.Json.Serialization;

namespace PiscinaPerfeita.Api.Dtos.Response;

public class ProdutoResponseDto
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string UnidadeMedida { get; set; } = string.Empty;


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual ICollection<EstoqueProdutoResponseDto> Estoques { get; set; } = new List<EstoqueProdutoResponseDto>();


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual ICollection<MovimentacaoEstoqueProdutoResponseDto> MovimentacoesEstoques { get; set; } = new List<MovimentacaoEstoqueProdutoResponseDto>();
}

public class EstoqueProdutoResponseDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;

}

public class MovimentacaoEstoqueProdutoResponseDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTimeOffset DataMovimentacao { get; set; }

}
