namespace PiscinaPerfeita.Api.Dtos.Response;

public class HidrometroResponseDto
{
    public Guid Id { get; init; }
    public decimal LeituraAtual { get; init; }
    public decimal? Consumo { get; init; }
    public DateTimeOffset DataLeitura { get; init; }
    public string? Observacoes { get; init; }
}
