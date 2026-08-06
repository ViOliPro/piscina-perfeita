using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Data;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Hidrometros;

public class HidrometroRepository : IHidrometroRepository
{
    private readonly PiscinaPerfeitaContext _context;

    public HidrometroRepository(PiscinaPerfeitaContext context) => _context = context;

    public Task<List<Hidrometro>> ListarOrdenadoAsync(CancellationToken cancellationToken = default) =>
        _context.Hidrometros
            .AsNoTracking()
            .OrderBy(item => item.DataLeitura)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);

    public Task<Hidrometro?> BuscarPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Hidrometros.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<Hidrometro?> BuscarAnteriorAsync(DateTimeOffset dataLeitura, Guid? ignorarId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Hidrometros.Where(item => item.DataLeitura < dataLeitura);
        if (ignorarId.HasValue) query = query.Where(item => item.Id != ignorarId.Value);
        return query.OrderByDescending(item => item.DataLeitura).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Hidrometro?> BuscarProximoAsync(DateTimeOffset dataLeitura, Guid? ignorarId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Hidrometros.Where(item => item.DataLeitura > dataLeitura);
        if (ignorarId.HasValue) query = query.Where(item => item.Id != ignorarId.Value);
        return query.OrderBy(item => item.DataLeitura).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ExisteNaDataAsync(DateTimeOffset dataLeitura, Guid? ignorarId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Hidrometros.Where(item => item.DataLeitura == dataLeitura);
        if (ignorarId.HasValue) query = query.Where(item => item.Id != ignorarId.Value);
        return query.AnyAsync(cancellationToken);
    }

    public async Task CriarAsync(Hidrometro hidrometro, CancellationToken cancellationToken = default)
    {
        await _context.Hidrometros.AddAsync(hidrometro, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Hidrometro hidrometro, CancellationToken cancellationToken = default)
    {
        _context.Hidrometros.Update(hidrometro);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExcluirAsync(Hidrometro hidrometro, CancellationToken cancellationToken = default)
    {
        _context.Hidrometros.Remove(hidrometro);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
