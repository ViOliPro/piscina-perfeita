using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Dtos.Response;
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
    public async Task<List<MovimentacaoEstoqueResponseDto>> Show()
    {
        return await _context
            .MovimentacoesEstoques.Select(m => new MovimentacaoEstoqueResponseDto
            {
                Id = m.Id,
                TipoMovimentacao = (Dtos.Response.Tipo)m.TipoMovimentacao,
                Quantidade = m.Quantidade,
                DataMovimentacao = m.DataMovimentacao,
                Piscina = new NomeIdDto(m.PiscinaId, m.Piscina.Nome),
                Produto = new NomeIdDto(m.ProdutoId, m.Produto.Nome),
                Usuario = new NomeIdDto(m.UsuarioId, m.Usuarios.Nome),
            })
            .ToListAsync();
    }

    // Exemplo de método para obter uma movimentação de estoque por ID
    public async Task<MovimentacaoEstoqueResponseDto?> GetById(Guid id)
    {
        var movimentacao = await _context
            .MovimentacoesEstoques.Where(m => m.Id == id)
            .Select(m => new MovimentacaoEstoqueResponseDto
            {
                Id = m.Id,
                TipoMovimentacao = (Dtos.Response.Tipo)m.TipoMovimentacao,
                Quantidade = m.Quantidade,
                DataMovimentacao = m.DataMovimentacao,
                Piscina = new NomeIdDto(m.PiscinaId, m.Piscina.Nome),
                Produto = new NomeIdDto(m.ProdutoId, m.Produto.Nome),
                Usuario = new NomeIdDto(m.UsuarioId, m.Usuarios.Nome),
            })
            .FirstOrDefaultAsync();

        return movimentacao ?? null;
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
        mov.UsuarioId = movimentacao.UsuarioId;
        mov.TipoMovimentacao = movimentacao.TipoMovimentacao;
        mov.Quantidade = movimentacao.Quantidade;
        mov.DataMovimentacao = movimentacao.DataMovimentacao;

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
