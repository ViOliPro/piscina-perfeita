using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Produtos;

public class ProdutosRepository : IProdutoRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public ProdutosRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Método para listar todos os produtos
    public async Task<List<Produto>> Show()
    {
        return await _context.Produtos.ToListAsync();
    }

    // Método para obter um produto por ID
    public async Task<Produto> GetById(Guid id)
    {
        var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id);
        if(produto == null)
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");

        return produto;
    }

    // Método para criar um novo produto
    public async Task Create(Produto produto)
    {
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();
    }

    // Método para atualizar um produto existente
    public async Task Update(Guid id, Produto produto)
    {
        var produtoToUpdate = await _context.Produtos.FindAsync(id);
        if (produtoToUpdate == null)
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");

        produtoToUpdate.Nome = produto.Nome;
        produtoToUpdate.UnidadeMedida = produto.UnidadeMedida;

        await _context.SaveChangesAsync();

    }

    // Método para deletar um produto
    public async Task Delete(Guid id)
    {
        var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id);
        if (produto == null)
        {
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");
        }

        _context.Remove(produto);
        await _context.SaveChangesAsync();

    }



}
