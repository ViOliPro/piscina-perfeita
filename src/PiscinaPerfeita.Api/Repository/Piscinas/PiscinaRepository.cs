using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Piscinas;

public class PiscinaRepository : IPiscinaRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public PiscinaRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<Piscina>> Show()
    {
        return await _context.Piscinas.ToListAsync();
    }

    public async Task<Piscina> GetById(Guid id)
    {
        var piscina = await _context.Piscinas.FirstOrDefaultAsync(p => p.Id == id);
        if(piscina == null)
            throw new KeyNotFoundException($"Piscina com ID {id} não encontrada.");

        return piscina;
    }

    public async Task Create(Piscina piscina)
    {
        _context.Piscinas.Add(piscina);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Guid id, Piscina piscina)
    {
        var piscinaToUpdate = await _context.Piscinas.FindAsync(id);
        if (piscinaToUpdate == null)
            throw new KeyNotFoundException($"Piscina com ID {id} não encontrada.");

        piscinaToUpdate.Nome = piscina.Nome;
        piscinaToUpdate.VolumeLitros = piscina.VolumeLitros;
        piscinaToUpdate.ProfundidadeMedia = piscina.ProfundidadeMedia;

        await _context.SaveChangesAsync();

    }

    public async Task Delete(Guid id)
    {
        var piscina = await _context.Piscinas.FirstOrDefaultAsync(p => p.Id == id);
        if (piscina == null)
        {
            throw new KeyNotFoundException($"Piscina com ID {id} não encontrada.");
        }

        _context.Remove(piscina);
        await _context.SaveChangesAsync();

    }



}
