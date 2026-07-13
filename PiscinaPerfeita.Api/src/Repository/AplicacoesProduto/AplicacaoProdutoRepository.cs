using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.AplicacoesProduto;

public class AplicacaoProdutoRepository : IAplicacaoProdutoRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public AplicacaoProdutoRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private static readonly System.Linq.Expressions.Expression<
        Func<AplicacaoProduto, AplicacaoProdutoResponseDto>
    > Projecao = a => new AplicacaoProdutoResponseDto
    {
        Id = a.Id,
        Piscina = new NomeIdDto(a.PiscinaId, a.Piscina.Nome),
        Produto = new NomeIdDto(a.ProdutoId, a.Produto.Nome),
        Deposito = new NomeIdDto(a.DepositoId, a.Deposito.Nome),
        Usuario = new NomeIdDto(a.UsuarioId, a.Usuario.Nome),
        AnaliseId = a.AnaliseId,
        MovimentacaoEstoqueId = a.MovimentacaoEstoqueId,
        Quantidade = a.Quantidade,
        UnidadeLancamento = a.UnidadeLancamento,
        DataAplicacao = a.DataAplicacao,
        Observacoes = a.Observacoes,
    };

    public async Task<List<AplicacaoProdutoResponseDto>> Show()
    {
        return await _context
            .Set<AplicacaoProduto>()
            .Select(Projecao)
            .OrderByDescending(a => a.DataAplicacao)
            .ToListAsync();
    }

    public async Task<AplicacaoProdutoResponseDto?> GetById(Guid id)
    {
        return await _context
            .Set<AplicacaoProduto>()
            .Where(a => a.Id == id)
            .Select(Projecao)
            .FirstOrDefaultAsync();
    }

    public async Task Create(
        AplicacaoProduto aplicacao,
        MovimentacaoEstoque movimentacao,
        Guid estoqueId,
        decimal novaQuantidadeEstoque
    )
    {
        var estoque = await _context.Estoques.FindAsync(estoqueId);
        if (estoque == null)
            throw new KeyNotFoundException($"Estoque com ID {estoqueId} não encontrado.");

        estoque.QuantidadeAtual = novaQuantidadeEstoque;

        _context.MovimentacoesEstoques.Add(movimentacao);
        // A FK só existe de fato no banco depois do SaveChanges, mas o EF
        // Core já resolve esse vínculo em memória via a navegação abaixo —
        // não precisamos de dois SaveChanges para "descobrir" o Id gerado.
        aplicacao.MovimentacaoEstoque = movimentacao;

        _context.AplicacoesProduto.Add(aplicacao);

        await _context.SaveChangesAsync();
    }
}
