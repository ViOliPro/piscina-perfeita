using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Locais;

public class LocalRepository : ILocalRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public LocalRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private static readonly Func<Local, LocalResponseDto> Projetar = l => new LocalResponseDto
    {
        Id = l.Id,
        Nome = l.Nome,
        Descricao = l.Descricao,
        Telefone = l.Telefone,
        Observacoes = l.Observacoes,
        Endereco = l.Endereco,
        Cidade = l.Cidade,
        Estado = l.Estado,
        Pais = l.Pais,
        Cep = l.Cep,
    };

    public async Task<List<LocalResponseDto>> Show()
    {
        var locais = await _context.Locais.OrderBy(l => l.Nome).ToListAsync();
        return locais.Select(Projetar).ToList();
    }

    public async Task<List<LocalResponseDto>> ShowByIds(IEnumerable<Guid> ids)
    {
        var locais = await _context
            .Locais.Where(l => ids.Contains(l.Id))
            .OrderBy(l => l.Nome)
            .ToListAsync();

        return locais.Select(Projetar).ToList();
    }

    public async Task<LocalResponseDto?> GetById(Guid id)
    {
        var local = await _context.Locais.FirstOrDefaultAsync(l => l.Id == id);
        return local == null ? null : Projetar(local);
    }

    public async Task Create(Local local)
    {
        _context.Locais.Add(local);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Guid id, Local local)
    {
        var localDb = await _context.Locais.FindAsync(id);
        if (localDb == null)
            throw new KeyNotFoundException($"Local com ID {id} não encontrado.");

        localDb.Nome = local.Nome;
        localDb.Descricao = local.Descricao;
        localDb.Telefone = local.Telefone;
        localDb.Observacoes = local.Observacoes;
        localDb.Endereco = local.Endereco;
        localDb.Cidade = local.Cidade;
        localDb.Estado = local.Estado;
        localDb.Pais = local.Pais;
        localDb.Cep = local.Cep;

        await _context.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        var local = await _context.Locais.FirstOrDefaultAsync(l => l.Id == id);
        if (local == null)
            throw new KeyNotFoundException($"Local com ID {id} não encontrado.");

        _context.Remove(local);
        await _context.SaveChangesAsync();
    }
}
