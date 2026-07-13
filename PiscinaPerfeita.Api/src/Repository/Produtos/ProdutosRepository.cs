using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Repository.Produtos;

public class ProdutoRepository : IProdutoRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public ProdutoRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Método para listar todos os produtos
    public async Task<List<ProdutoResponseDto>> Show()
    {
        return await _context.Produtos.Select(p => new ProdutoResponseDto
        {
            Id = p.Id,
            Nome = p.Nome,
            UnidadeMedida = p.UnidadeMedida,
            Fabricante = p.Fabricante,
            Marca = p.Marca,
            Observacoes = p.Observacoes,
            Categoria = p.Categoria,
            Estoques = p.Estoques.Select(e => new NomeIdDto(e.Id, e.Produto.Nome)).ToList(),

            MovimentacoesEstoques = p.MovimentacoesEstoques.Select(m => new MovimentacaoEstoqueProdutoResponseDto
            {
                Id = m.Id,
                Nome = m.Produto.Nome,
                DataMovimentacao = m.DataMovimentacao
            }).ToList() 

        }).ToListAsync();
    }

    // Método para obter um produto por ID
    public async Task<ProdutoResponseDto?> GetById(Guid id)
    {
        var produtoDto = await _context.Produtos
            .Where(p => p.Id == id)
            .Select(p => new ProdutoResponseDto
            {
                Id = p.Id,
                Nome = p.Nome,
                UnidadeMedida = p.UnidadeMedida,
                Fabricante = p.Fabricante,
                Marca = p.Marca,
                Observacoes = p.Observacoes,
                Categoria = p.Categoria,
                Estoques = p.Estoques.Select(e => new NomeIdDto(e.Id, e.Produto.Nome)).ToList(),

                MovimentacoesEstoques = p.MovimentacoesEstoques.Select(m => new MovimentacaoEstoqueProdutoResponseDto
                {
                    Id = m.Id,
                    Nome = m.Produto.Nome,
                    DataMovimentacao = m.DataMovimentacao
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return produtoDto == null ? null : produtoDto;
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
        produtoToUpdate.Fabricante = produto.Fabricante;
        produtoToUpdate.Marca = produto.Marca;
        produtoToUpdate.Observacoes = produto.Observacoes;
        produtoToUpdate.Categoria = produto.Categoria;

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
