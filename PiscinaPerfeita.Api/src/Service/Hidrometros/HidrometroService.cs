using System.Globalization;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Hidrometros;

namespace PiscinaPerfeita.Api.Service.Hidrometros;

public class HidrometroService : IHidrometroService
{
    private readonly IHidrometroRepository _repository;

    public HidrometroService(IHidrometroRepository repository) => _repository = repository;

    public async Task<List<HidrometroResponseDto>> ListarAsync(
        CancellationToken cancellationToken = default
    )
    {
        var leituras = await _repository.ListarOrdenadoAsync(cancellationToken);
        return MapearComConsumo(leituras).OrderByDescending(item => item.DataLeitura).ToList();
    }

    public async Task<HidrometroResponseDto> BuscarPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var leitura =
            await _repository.BuscarPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Leitura de hidrômetro não encontrada.");
        var leituras = await _repository.ListarOrdenadoAsync(cancellationToken);
        return MapearComConsumo(leituras).Single(item => item.Id == leitura.Id);
    }

    public async Task<HidrometroDashboardResponseDto> ObterDashboardAsync(
        CancellationToken cancellationToken = default
    )
    {
        var itens = MapearComConsumo(await _repository.ListarOrdenadoAsync(cancellationToken));
        var ultima = itens.LastOrDefault();
        var consumos = itens.Where(item => item.Consumo.HasValue).ToList();
        var agora = DateTimeOffset.UtcNow;
        var consumoMes = consumos
            .Where(item =>
                item.DataLeitura.Year == agora.Year && item.DataLeitura.Month == agora.Month
            )
            .Sum(item => item.Consumo!.Value);
        var cultura = CultureInfo.GetCultureInfo("pt-BR");

        return new HidrometroDashboardResponseDto
        {
            UltimaLeitura = ultima?.LeituraAtual,
            DataUltimaLeitura = ultima?.DataLeitura,
            UltimoConsumo = ultima?.Consumo,
            ConsumoMedio =
                consumos.Count == 0 ? null : consumos.Average(item => item.Consumo!.Value),
            ConsumoMes = consumos.Count == 0 ? null : consumoMes,
            DiasSemLeitura = ultima is null
                ? null
                : Math.Max(0, (int)Math.Floor((agora - ultima.DataLeitura).TotalDays)),
            PeriodoUltimoConsumo = ultima?.Consumo is null ? null : "Desde a leitura anterior",
            PeriodoMedia = consumos.Count == 0 ? null : $"Baseado em {consumos.Count} consumo(s)",
            MesReferencia = agora.ToString("MMMM 'de' yyyy", cultura),
        };
    }

    public async Task<HidrometroResponseDto> CriarAsync(
        HidrometroRequestDto dto,
        CancellationToken cancellationToken = default
    )
    {
        var dataLeitura = dto.DataLeitura.ToUniversalTime();
        ValidarData(dataLeitura);
        await ValidarSequenciaAsync(dto.LeituraAtual, dataLeitura, null, cancellationToken);

        var entidade = new Hidrometro
        {
            LeituraAtual = dto.LeituraAtual,
            DataLeitura = dataLeitura,
            Observacoes = LimparObservacoes(dto.Observacoes),
        };
        await _repository.CriarAsync(entidade, cancellationToken);
        return await BuscarPorIdAsync(entidade.Id, cancellationToken);
    }

    public async Task<HidrometroResponseDto> AtualizarAsync(
        Guid id,
        HidrometroRequestDto dto,
        CancellationToken cancellationToken = default
    )
    {
        var dataLeitura = dto.DataLeitura.ToUniversalTime();
        ValidarData(dataLeitura);

        var entidade =
            await _repository.BuscarPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Leitura de hidrômetro não encontrada.");

        await ValidarSequenciaAsync(dto.LeituraAtual, dataLeitura, id, cancellationToken);
        entidade.LeituraAtual = dto.LeituraAtual;
        entidade.DataLeitura = dataLeitura;
        entidade.Observacoes = LimparObservacoes(dto.Observacoes);
        await _repository.AtualizarAsync(entidade, cancellationToken);
        return await BuscarPorIdAsync(id, cancellationToken);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entidade =
            await _repository.BuscarPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Leitura de hidrômetro não encontrada.");
        await _repository.ExcluirAsync(entidade, cancellationToken);
    }

    private async Task ValidarSequenciaAsync(
        decimal leitura,
        DateTimeOffset data,
        Guid? ignorarId,
        CancellationToken cancellationToken
    )
    {
        if (leitura < 0)
            throw new ArgumentException("A leitura atual deve ser maior ou igual a zero.");

        if (await _repository.ExisteNaDataAsync(data, ignorarId, cancellationToken))
            throw new ArgumentException(
                "Já existe uma leitura registrada para esta mesma data e hora."
            );

        var anterior = await _repository.BuscarAnteriorAsync(data, ignorarId, cancellationToken);
        if (anterior is not null && leitura < anterior.LeituraAtual)
            throw new ArgumentException(
                $"A leitura deve ser maior ou igual à anterior ({anterior.LeituraAtual:N2} m³)."
            );

        var proxima = await _repository.BuscarProximoAsync(data, ignorarId, cancellationToken);
        if (proxima is not null && leitura > proxima.LeituraAtual)
            throw new ArgumentException(
                $"A leitura não pode ser maior que a próxima leitura já registrada ({proxima.LeituraAtual:N2} m³)."
            );
    }

    private static void ValidarData(DateTimeOffset data)
    {
        if (data > DateTimeOffset.UtcNow)
            throw new ArgumentException("A data e hora da leitura não podem estar no futuro.");
    }

    private static string? LimparObservacoes(string? observacoes) =>
        string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();

    private static List<HidrometroResponseDto> MapearComConsumo(IEnumerable<Hidrometro> leituras)
    {
        decimal? leituraAnterior = null;
        return leituras
            .OrderBy(item => item.DataLeitura)
            .ThenBy(item => item.Id)
            .Select(item =>
            {
                var consumo = leituraAnterior.HasValue
                    ? item.LeituraAtual - leituraAnterior.Value
                    : 0;
                leituraAnterior = item.LeituraAtual;
                return new HidrometroResponseDto
                {
                    Id = item.Id,
                    LeituraAtual = item.LeituraAtual,
                    Consumo = consumo,
                    DataLeitura = item.DataLeitura,
                    Observacoes = item.Observacoes,
                };
            })
            .ToList();
    }
}
