using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Analises;

public class PiscinaRepository : IPiscinaRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public PiscinaRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<Analise>> Show()
    {
        return await _context.Analises.ToListAsync();
    }

    public async Task<Analise> GetById(Guid id)
    {
        var analise = await _context.Analises.FirstOrDefaultAsync(a => a.Id == id);
        if(analise == null)
            throw new KeyNotFoundException($"Análise com ID {id} não encontrada.");

        return analise;
    }

    public async Task Create(Analise analise)
    {
        _context.Analises.Add(analise);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Guid id, Analise analise)
    {
        var analiseToUpdate = await _context.Analises.FindAsync(id);
        if (analiseToUpdate == null)
            throw new KeyNotFoundException($"Análise com ID {id} não encontrada.");

        analiseToUpdate.Ph = analise.Ph;
        analiseToUpdate.CloroLivre = analise.CloroLivre;
        analiseToUpdate.Alcalinidade = analise.Alcalinidade;
        analiseToUpdate.Temperatura = analise.Temperatura;
        analiseToUpdate.Observacoes = analise.Observacoes;

        await _context.SaveChangesAsync();

    }

    public async Task Delete(Guid id)
    {
        var analise = await _context.Analises.FirstOrDefaultAsync(a => a.Id == id);
        if (analise == null)
        {
            throw new KeyNotFoundException($"Análise com ID {id} não encontrada.");
        }

        _context.Remove(analise);
        await _context.SaveChangesAsync();

    }



}
