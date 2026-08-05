using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Hidrometros;

public class HidrometroRepository : IHidrometroRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public HidrometroRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<HidrometroResponseDto>> Show(
        DateTimeOffset? dataInicio = null,
        DateTimeOffset? dataFim = null
    )
    {
        var query = _context.Hidrometros.AsNoTracking().AsQueryable();

        // Início do mês atual como padrão caso não receba parâmetro
        if (!dataInicio.HasValue)
        {
            var agora = DateTimeOffset.UtcNow;
            dataInicio = new DateTimeOffset(agora.Year, agora.Month, 1, 0, 0, 0, agora.Offset);
        }

        query = query.Where(a => a.CriadoEm >= dataInicio.Value);

        if (dataFim.HasValue)
        {
            query = query.Where(a => a.CriadoEm <= dataFim.Value);
        }

        return await query
            .OrderByDescending(a => a.CriadoEm)
            .Select(a => new HidrometroResponseDto
            {
                Id = a.Id,
                CriadoEm = a.CriadoEm,
                Consumo = a.Consumo ?? 0,
            })
            .ToListAsync();
    }

    public async Task<HidrometroResponseDto?> GetById(Guid id)
    {
        var hidrometro = await _context
            .Hidrometros.Where(e => e.Id == id)
            .Select(a => new HidrometroResponseDto
            {
                Id = a.Id,
                CriadoEm = a.CriadoEm,
                Consumo = a.Consumo ?? 0,
            })
            .FirstOrDefaultAsync();

        return hidrometro ?? null;
    }

    public async Task Create(Hidrometro hidrometro)
    {
        _context.Hidrometros.Add(hidrometro);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Guid id, Hidrometro hidro)
    {
        var hidrometroToUpdate = await _context.Hidrometros.FindAsync(id);
        if (hidrometroToUpdate == null)
            throw new KeyNotFoundException($"Hidrometro com ID {id} não encontrado.");

        hidrometroToUpdate.Consumo = hidro.Consumo;
        hidrometroToUpdate.CriadoEm = hidro.CriadoEm;

        await _context.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        var hidrometro = await _context.Hidrometros.FirstOrDefaultAsync(a => a.Id == id);
        if (hidrometro == null)
        {
            throw new KeyNotFoundException($"Hidrometro com ID {id} não encontrado.");
        }

        _context.Remove(hidrometro);
        await _context.SaveChangesAsync();
    }
}
