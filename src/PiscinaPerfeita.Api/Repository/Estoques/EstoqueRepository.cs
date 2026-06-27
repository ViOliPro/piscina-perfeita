using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Estoques;

public class EstoqueRepository : IEstoqueRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public EstoqueRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<Estoque>> Show()
    {
        var query = _context.Estoques.Include(p => p.Piscina).Include(p => p.Produto);
        return await query.ToListAsync();
    }

    public async Task<Estoque> GetById(Guid id)
    {
        var estoque = await _context.Estoques.FirstOrDefaultAsync(e => e.Id == id);
        if(estoque == null)
            throw new KeyNotFoundException($"Estoque com ID {id} não encontrado.");

        return estoque;
    }

    public async Task Create(Estoque estoque)
    {
        _context.Estoques.Add(estoque);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Guid id, Estoque estoque)
    {
        var estoqueToUpdate = await _context.Estoques.FindAsync(id);
        if (estoqueToUpdate == null)
            throw new KeyNotFoundException($"Estoque com ID {id} não encontrado.");

        estoqueToUpdate.QuantidadeAtual = estoque.QuantidadeAtual;
        estoqueToUpdate.PiscinaId = estoque.PiscinaId;
        estoqueToUpdate.ProdutoId = estoque.ProdutoId;
        
        await _context.SaveChangesAsync();

    }

    public async Task Delete(Guid id)
    {
        var estoque = await _context.Estoques.FirstOrDefaultAsync(e => e.Id == id);
        if (estoque == null)
        {
            throw new KeyNotFoundException($"Estoque com ID {id} não encontrado.");
        }

        _context.Remove(estoque);
        await _context.SaveChangesAsync();

    }



}
