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

    public async Task<List<AplicacaoProdutoResponseDto>> Show(
        DateTimeOffset? dataInicio = null,
        DateTimeOffset? dataFim = null,
        Guid? piscinaId = null,
        int? limit = null
    )
    {
        var query = _context.Set<AplicacaoProduto>().AsNoTracking().AsQueryable();

        // Sem dataInicio: início do mês corrente (mesmo padrão de Análises/Movimentações).
        if (!dataInicio.HasValue)
        {
            var agora = DateTimeOffset.UtcNow;
            dataInicio = new DateTimeOffset(agora.Year, agora.Month, 1, 0, 0, 0, agora.Offset);
        }

        query = query.Where(a => a.DataAplicacao >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(a => a.DataAplicacao <= dataFim.Value);

        if (piscinaId.HasValue)
            query = query.Where(a => a.PiscinaId == piscinaId.Value);

        query = query.OrderByDescending(a => a.DataAplicacao);

        if (limit is > 0)
            query = query.Take(limit.Value);

        return await query.Select(Projecao).ToListAsync();
    }

    public async Task<AplicacaoProdutoResponseDto?> GetById(Guid id)
    {
        return await _context
            .Set<AplicacaoProduto>()
            .Where(a => a.Id == id)
            .Select(Projecao)
            .FirstOrDefaultAsync();
    }

    public async Task<List<AplicacaoUsoRaw>> ListarParaUsoProdutos(
        Guid piscinaId,
        DateTimeOffset inicio,
        DateTimeOffset fim
    )
    {
        return await _context
            .Set<AplicacaoProduto>()
            .AsNoTracking()
            .Where(a =>
                a.PiscinaId == piscinaId && a.DataAplicacao >= inicio && a.DataAplicacao <= fim
            )
            .Select(a => new AplicacaoUsoRaw
            {
                ProdutoId = a.ProdutoId,
                ProdutoNome = a.Produto.Nome,
                UnidadeMedidaProduto = a.Produto.UnidadeMedida,
                Quantidade = a.Quantidade,
                UnidadeLancamento = a.UnidadeLancamento,
            })
            .ToListAsync();
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
