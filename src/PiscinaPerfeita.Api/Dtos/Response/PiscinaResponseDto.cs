using System.Text.Json.Serialization;

namespace PiscinaPerfeita.Api.Dtos.Response;

public class PiscinaResponseDto
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = null!;

    public decimal? VolumeLitros { get; set; }

    public decimal? ProfundidadeMedia { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UsuarioPiscinaResponseDto? UsuarioPiscina { get; set; } = new UsuarioPiscinaResponseDto();



    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual ICollection<AnalisePiscinaResponseDto> AnalisePiscina { get; set; } = new List<AnalisePiscinaResponseDto>();



    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual ICollection<EstoquePiscinaResponseDto> Estoques { get; set; } = new List<EstoquePiscinaResponseDto>();



    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual ICollection<MovimentacaoEstoquePiscinaResponsetDto> MovimentacoesEstoques { get; set; } = new List<MovimentacaoEstoquePiscinaResponsetDto>();
}

public class UsuarioPiscinaResponseDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
}

public class AnalisePiscinaResponseDto
{
    public Guid Id { get; set; }
    public DateTimeOffset DataAnalise { get; set; }
}

public class EstoquePiscinaResponseDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
}

public class MovimentacaoEstoquePiscinaResponsetDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public DateTimeOffset DataMovimentacao { get; set; }
}