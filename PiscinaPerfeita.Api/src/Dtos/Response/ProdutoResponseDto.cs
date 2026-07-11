using System.Text.Json.Serialization;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response;

public class ProdutoResponseDto
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string UnidadeMedida { get; set; } = string.Empty;

    public string? Fabricante { get; set; } = string.Empty;

    public string? Marca { get; set; } = string.Empty;

    public string? Observacoes { get; set; } = string.Empty;

    public string? Categoria { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual ICollection<NomeIdDto> Estoques { get; set; } = new List<NomeIdDto>();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual ICollection<MovimentacaoEstoqueProdutoResponseDto> MovimentacoesEstoques { get; set; } =
        new List<MovimentacaoEstoqueProdutoResponseDto>();
}

public class MovimentacaoEstoqueProdutoResponseDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTimeOffset DataMovimentacao { get; set; }
}
