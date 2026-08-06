namespace PiscinaPerfeita.Api.Dtos.Response;

public class HidrometroDashboardResponseDto
{
    public decimal? UltimaLeitura { get; init; }
    public DateTimeOffset? DataUltimaLeitura { get; init; }
    public decimal? UltimoConsumo { get; init; }
    public decimal? ConsumoMedio { get; init; }
    public decimal? ConsumoMes { get; init; }
    public int? DiasSemLeitura { get; init; }
    public string? PeriodoUltimoConsumo { get; init; }
    public string? PeriodoMedia { get; init; }
    public string? MesReferencia { get; init; }
}
