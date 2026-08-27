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

    private static readonly System.Linq.Expressions.Expression<
        Func<MovimentacaoEstoque, MovimentacaoEstoqueResponseDto>
    > Projecao = m => new MovimentacaoEstoqueResponseDto
    {
        Id = m.Id,
        TipoMovimentacao = m.TipoMovimentacao,
        Quantidade = m.Quantidade,
        UnidadeLancamento = m.UnidadeLancamento,
        DataMovimentacao = m.DataMovimentacao,
        Piscina =
            m.PiscinaId != null && m.Piscina != null
                ? new NomeIdDto(m.PiscinaId.Value, m.Piscina.Nome)
                : null,
        Produto = new NomeIdDto(m.ProdutoId, m.Produto.Nome),
        Deposito = m.Deposito != null ? new NomeIdDto(m.DepositoId, m.Deposito.Nome) : null,
        Usuario = new NomeIdDto(m.UsuarioId, m.Usuarios.Nome),
    };

    public async Task<List<MovimentacaoEstoqueResponseDto>> Show(
        DateTimeOffset? dataInicio = null,
        DateTimeOffset? dataFim = null,
        Guid? piscinaId = null
    )
    {
        var query = _context.MovimentacoesEstoques.AsNoTracking().AsQueryable();

        // Início do mês atual como padrão caso não receba parâmetro
        if (!dataInicio.HasValue)
        {
            var agora = DateTimeOffset.UtcNow;
            dataInicio = new DateTimeOffset(agora.Year, agora.Month, 1, 0, 0, 0, agora.Offset);
        }

        query = query.Where(a => a.DataMovimentacao >= dataInicio.Value);

        if (dataFim.HasValue)
        {
            query = query.Where(a => a.DataMovimentacao <= dataFim.Value);
        }

        if (piscinaId.HasValue)
        {
            query = query.Where(a => a.PiscinaId == piscinaId.Value);
        }

        query = query.OrderByDescending(d => d.DataMovimentacao);
        return await query.Select(Projecao).ToListAsync();
    }

    public async Task<MovimentacaoEstoqueResponseDto?> GetById(Guid id)
    {
        return await _context
            .MovimentacoesEstoques.Where(m => m.Id == id)
            .Select(Projecao)
            .FirstOrDefaultAsync();
    }

    public async Task Create(MovimentacaoEstoque movimentacao)
    {
        _context.MovimentacoesEstoques.Add(movimentacao);
        await _context.SaveChangesAsync();
    }

    public async Task CreateComAtualizacaoEstoque(
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

        // Uma única transação implícita do EF Core: grava a movimentação e
        // o novo saldo do estoque juntos, ou nenhum dos dois.
        await _context.SaveChangesAsync();
    }

    public async Task CreateLoteComAtualizacaoEstoque(
        IReadOnlyCollection<MovimentacaoEstoque> movimentacoes,
        IReadOnlyCollection<Estoque> estoquesNovos,
        IReadOnlyCollection<(Guid EstoqueId, decimal QuantidadeAtual)> estoquesAtualizados
    )
    {
        // Um único SaveChangesAsync() já grava tudo abaixo numa única
        // transação implícita do EF Core — não precisa (e não pode) abrir
        // uma transação manual aqui: o projeto tem EnableRetryOnFailure
        // ativo no Npgsql, e BeginTransactionAsync() direto (fora de
        // CreateExecutionStrategy().ExecuteAsync()) lança
        // "InvalidOperationException: The configured execution strategy
        // does not support user-initiated transactions" em runtime, toda
        // vez que este método fosse chamado.
        if (estoquesNovos.Count > 0)
            await _context.Estoques.AddRangeAsync(estoquesNovos);

        foreach (var (estoqueId, quantidadeAtual) in estoquesAtualizados)
        {
            var estoque =
                await _context.Estoques.FindAsync(estoqueId)
                ?? throw new KeyNotFoundException($"Estoque com ID {estoqueId} não encontrado.");
            estoque.QuantidadeAtual = quantidadeAtual;
        }

        await _context.MovimentacoesEstoques.AddRangeAsync(movimentacoes);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Guid id, MovimentacaoEstoque movimentacao)
    {
        var mov = await _context.MovimentacoesEstoques.FindAsync(id);
        if (mov == null)
            throw new KeyNotFoundException($"Movimentação com ID {id} não encontrada.");

        mov.PiscinaId = movimentacao.PiscinaId;
        mov.ProdutoId = movimentacao.ProdutoId;
        mov.DepositoId = movimentacao.DepositoId;
        mov.UsuarioId = movimentacao.UsuarioId;
        mov.TipoMovimentacao = movimentacao.TipoMovimentacao;
        mov.Quantidade = movimentacao.Quantidade;
        mov.UnidadeLancamento = movimentacao.UnidadeLancamento;
        mov.DataMovimentacao = movimentacao.DataMovimentacao;

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
