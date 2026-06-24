using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Repository.Piscinas;

public class PiscinaRepository : IPiscinaRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public PiscinaRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<PiscinaResponseDto>> Show()
    {
        return await _context.Piscinas.Select(u => new PiscinaResponseDto
        {
            Id = u.Id,
            Nome = u.Nome,
            VolumeLitros = u.VolumeLitros,
            ProfundidadeMedia = u.ProfundidadeMedia,
            CreatedAt = u.CreatedAt,
            UsuarioPiscina = u.Usuario != null ? new UsuarioPiscinaResponseDto
            {
                Id = u.Usuario.Id,
                Nome = u.Usuario.Nome
            } : null
        }).ToListAsync();
    }

    public async Task<PiscinaResponseDto?> GetById(Guid id)
    {
        var piscinaDto = await _context.Piscinas
            .Where(p => p.Id == id)
            .Select(u => new PiscinaResponseDto
            {
                Id = u.Id,
                Nome = u.Nome,
                VolumeLitros = u.VolumeLitros,
                ProfundidadeMedia = u.ProfundidadeMedia,
                CreatedAt = u.CreatedAt,
                UsuarioPiscina = u.Usuario != null ? new UsuarioPiscinaResponseDto
                {
                    Id = u.Usuario.Id,
                    Nome = u.Usuario.Nome
                } : null
            }).FirstOrDefaultAsync();

        return piscinaDto == null ? null : piscinaDto;
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
