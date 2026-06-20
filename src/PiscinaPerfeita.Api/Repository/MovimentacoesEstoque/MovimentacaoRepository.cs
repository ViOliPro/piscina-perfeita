using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.MovimentacoesEstoque;

public class MovimentacaoRepository : IMovimentacaoRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public MovimentacaoRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Implementação dos métodos do repositório
    // Exemplo de método para obter todas as movimentações de estoque
    public async Task<List<MovimentacaoEstoque>> Show()
    {
        return await _context.MovimentacoesEstoques.ToListAsync();
    }


    // Exemplo de método para obter uma movimentação de estoque por ID
    public async Task<MovimentacaoEstoque> GetById(Guid id)
    {
        var movimentacao = await _context.MovimentacoesEstoques.FirstOrDefaultAsync(m => m.Id == id);
        if(movimentacao == null)
            throw new KeyNotFoundException($"Movimentação com ID {id} não encontrada.");

        return movimentacao;
    }


    // Exemplo de método para criar uma nova movimentação de estoque
    public async Task Create(MovimentacaoEstoque movimentacao)
    {
        _context.MovimentacoesEstoques.Add(movimentacao);
        await _context.SaveChangesAsync();
    }


    // Exemplo de método para atualizar uma movimentação de estoque existente
    public async Task Update(Guid id, MovimentacaoEstoque movimentacao)
    {
        var mov = await _context.MovimentacoesEstoques.FindAsync(id);
        if (mov == null)
            throw new KeyNotFoundException($"Movimentação com ID {id} não encontrada.");

        mov.Id = movimentacao.Id;
        mov.PiscinaId = movimentacao.PiscinaId;
        mov.ProdutoId = movimentacao.ProdutoId;
        mov.TipoMovimentacao = movimentacao.TipoMovimentacao;
        mov.Quantidade = movimentacao.Quantidade;
        mov.Valor = movimentacao.Valor;
        mov.DataMovimentacao = movimentacao.DataMovimentacao;
        mov.Piscina = movimentacao.Piscina;
        mov.Produto = movimentacao.Produto;

        await _context.SaveChangesAsync();

    }

    // Exemplo de método para excluir uma movimentação de estoque
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
