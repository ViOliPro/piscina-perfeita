using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Movimentacoes;

public class MovimentacoesRepository : IMovimentacoesRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public MovimentacoesRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<MovimentacoesEstoque>> Show()
    {
        return await _context.MovimentacoesEstoques.ToListAsync();
    }

    public async Task<MovimentacoesEstoque> GetById(Guid id)
    {
        var movimentacao = await _context.MovimentacoesEstoques.FirstOrDefaultAsync(m => m.Id == id);
        if(movimentacao == null)
            throw new KeyNotFoundException($"Movimentação com ID {id} não encontrada.");

        return movimentacao;
    }

    public async Task Create(MovimentacoesEstoque movimentacao)
    {
        _context.MovimentacoesEstoques.Add(movimentacao);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Guid id, MovimentacoesEstoque movimentacao)
    {
        var mov = await _context.MovimentacoesEstoques.FindAsync(id);
        if (mov == null)
            throw new KeyNotFoundException($"Movimentação com ID {id} não encontrada.");

        mov.Nome = movimentacao.Nome;
        mov.Email = movimentacao.Email;
        mov.Senhahash = movimentacao.Senhahash;

        await _context.SaveChangesAsync();

    }

    public async Task Delete(Guid id)
    {
        var mov = await _context.MovimentacoesEstoques.FirstOrDefaultAsync(m => m.Id == id);
        if (mov == null)
        {
            throw new KeyNotFoundException($"Movimentação com ID {id} não encontrada.");
        }

        _context.Remove(mov);
        await _context.SaveChangesAsync();

    }



}
