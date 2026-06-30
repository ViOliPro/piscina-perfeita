using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Repository.Analises;

public class AnaliseRepository : IAnaliseRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public AnaliseRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<AnaliseResponseDto>> Show()
    {
        return await _context.Analises.Select(a => new AnaliseResponseDto
        {
            Id = a.Id,
            DataAnalise = a.DataAnalise,
            Ph = a.Ph,
            CloroLivre = a.CloroLivre,
            Alcalinidade = a.Alcalinidade,
            Temperatura = a.Temperatura,
            Observacoes = a.Observacoes,
            Piscina = a.Piscina != null ? new PiscinaOrigem
            {
                Id = a.Piscina.Id,
                Nome = a.Piscina.Nome
            } : null
        }).ToListAsync();
    }

    public async Task<AnaliseResponseDto?> GetById(Guid id)
    {
        var analise = await _context.Analises.Where(e => e.Id == id).Select(a => new AnaliseResponseDto
        {
            Id = a.Id,
            DataAnalise = a.DataAnalise,
            Ph = a.Ph,
            CloroLivre = a.CloroLivre,
            Alcalinidade = a.Alcalinidade,
            Temperatura = a.Temperatura,
            Observacoes = a.Observacoes,
            Piscina = a.Piscina != null ? new PiscinaOrigem
            {
                Id = a.Piscina.Id,
                Nome = a.Piscina.Nome
            } : null
        }).FirstOrDefaultAsync();

        return analise ?? null;
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
