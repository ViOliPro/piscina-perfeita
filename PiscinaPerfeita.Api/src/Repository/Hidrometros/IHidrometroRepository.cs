using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Hidrometros;

public interface IHidrometroRepository
{
    Task<List<Hidrometro>> ListarOrdenadoAsync(CancellationToken cancellationToken = default);
    Task<Hidrometro?> BuscarPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Hidrometro?> BuscarAnteriorAsync(DateTimeOffset dataLeitura, Guid? ignorarId = null, CancellationToken cancellationToken = default);
    Task<Hidrometro?> BuscarProximoAsync(DateTimeOffset dataLeitura, Guid? ignorarId = null, CancellationToken cancellationToken = default);
    Task<bool> ExisteNaDataAsync(DateTimeOffset dataLeitura, Guid? ignorarId = null, CancellationToken cancellationToken = default);
    Task CriarAsync(Hidrometro hidrometro, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Hidrometro hidrometro, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Hidrometro hidrometro, CancellationToken cancellationToken = default);
}
