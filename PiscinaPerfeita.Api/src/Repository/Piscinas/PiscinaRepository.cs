using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Piscinas;

public class PiscinaRepository : IPiscinaRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public PiscinaRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<PiscinaResponseDto>> Show()
    {
        return await _context
            .Piscinas.AsNoTracking()
            .Select(u => new PiscinaResponseDto
            {
                Id = u.Id,
                Nome = u.Nome,
                VolumeLitros = u.VolumeLitros,
                ProfundidadeMedia = u.ProfundidadeMedia,
                CreatedAt = u.CreatedAt,
                UsuarioPiscina =
                    u.Usuario != null ? new NomeIdDto(u.Usuario.Id, u.Usuario.Nome) : null,
            })
            .ToListAsync();
    }

    public async Task<PiscinaResponseDto?> GetById(Guid id)
    {
        return await _context
            .Piscinas.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(u => new PiscinaResponseDto
            {
                Id = u.Id,
                Nome = u.Nome,
                VolumeLitros = u.VolumeLitros,
                ProfundidadeMedia = u.ProfundidadeMedia,
                CreatedAt = u.CreatedAt,
                UsuarioPiscina =
                    u.Usuario != null ? new NomeIdDto(u.Usuario.Id, u.Usuario.Nome) : null,
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PiscinaDashboardResponseDto?> GetDashboard(
        Guid piscinaId,
        DateTimeOffset inicio,
        DateTimeOffset fim,
        int limitAnalises,
        int limitMovimentacoes
    )
    {
        var piscina = await _context
            .Piscinas.AsNoTracking()
            .Where(p => p.Id == piscinaId)
            .Select(u => new PiscinaResumoDto
            {
                Id = u.Id,
                Nome = u.Nome,
                VolumeLitros = u.VolumeLitros,
                ProfundidadeMedia = u.ProfundidadeMedia,
                UsuarioPiscina =
                    u.Usuario != null ? new NomeIdDto(u.Usuario.Id, u.Usuario.Nome) : null,
            })
            .FirstOrDefaultAsync();

        if (piscina == null)
            return null;

        var analisesQuery = _context
            .Analises.AsNoTracking()
            .Where(a =>
                a.PiscinaId == piscinaId && a.DataAnalise >= inicio && a.DataAnalise <= fim
            );

        var movQuery = _context
            .MovimentacoesEstoques.AsNoTracking()
            .Where(m =>
                m.PiscinaId == piscinaId
                && m.DataMovimentacao >= inicio
                && m.DataMovimentacao <= fim
            );

        // Contagem de aplicações: preferir tabela AplicacoesProduto;
        // fallback para movimentações Tipo = Aplicacao.
        var aplicacoesCount = await _context
            .Set<AplicacaoProduto>()
            .AsNoTracking()
            .CountAsync(ap =>
                ap.PiscinaId == piscinaId && ap.DataAplicacao >= inicio && ap.DataAplicacao <= fim
            );

        var contagens = new ContagensDto
        {
            Analises = await analisesQuery.CountAsync(),
            Movimentacoes = await movQuery.CountAsync(),
            Aplicacoes = aplicacoesCount,
        };

        var ultimasAnalises = await analisesQuery
            .OrderByDescending(a => a.DataAnalise)
            .Take(limitAnalises)
            .Select(a => new AnaliseResumoDto
            {
                Id = a.Id,
                DataAnalise = a.DataAnalise,
                Ph = a.Ph,
                CloroLivre = a.CloroLivre,
                Alcalinidade = a.Alcalinidade,
                Temperatura = a.Temperatura,
            })
            .ToListAsync();

        var ultimasMovimentacoes = await movQuery
            .OrderByDescending(m => m.DataMovimentacao)
            .Take(limitMovimentacoes)
            .Select(m => new MovimentacaoResumoDto
            {
                Id = m.Id,
                Tipo = m.TipoMovimentacao.ToString(),
                DataMovimentacao = m.DataMovimentacao,
                ProdutoNome = m.Produto != null ? m.Produto.Nome : null,
            })
            .ToListAsync();

        // Produtos utilizados no período (agregado simples via AplicacaoProduto)
        var produtosUtilizados = await _context
            .Set<AplicacaoProduto>()
            .AsNoTracking()
            .Where(ap =>
                ap.PiscinaId == piscinaId && ap.DataAplicacao >= inicio && ap.DataAplicacao <= fim
            )
            .GroupBy(ap => new
            {
                ap.ProdutoId,
                Nome = ap.Produto.Nome,
                Unidade = ap.Produto.UnidadeMedida,
            })
            .Select(g => new ProdutoUsoResumoDto
            {
                ProdutoId = g.Key.ProdutoId,
                Nome = g.Key.Nome,
                Quantidade = g.Sum(x => x.Quantidade),
                Unidade = g.Key.Unidade ?? "",
                Ocorrencias = g.Count(),
            })
            .OrderByDescending(p => p.Quantidade)
            .Take(20)
            .ToListAsync();

        return new PiscinaDashboardResponseDto
        {
            Piscina = piscina,
            Periodo = new PeriodoDto { Inicio = inicio, Fim = fim },
            Contagens = contagens,
            UltimasAnalises = ultimasAnalises,
            UltimasMovimentacoes = ultimasMovimentacoes,
            ProdutosUtilizados = produtosUtilizados,
        };
    }

    public async Task Create(Piscina piscina)
    {
        _context.Piscinas.Add(piscina);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Guid id, Piscina piscina)
    {
        var piscinaToUpdate = await _context.Piscinas.FindAsync(id);
        if (piscinaToUpdate == null)
            throw new KeyNotFoundException($"Piscina com ID {id} não encontrada.");

        piscinaToUpdate.Nome = piscina.Nome;
        piscinaToUpdate.VolumeLitros = piscina.VolumeLitros;
        piscinaToUpdate.ProfundidadeMedia = piscina.ProfundidadeMedia;
        piscinaToUpdate.UsuarioId = piscina.UsuarioId;

        await _context.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        var piscina = await _context.Piscinas.FirstOrDefaultAsync(p => p.Id == id);
        if (piscina == null)
        {
            throw new KeyNotFoundException($"Piscina com ID {id} não encontrada.");
        }

        _context.Remove(piscina);
        await _context.SaveChangesAsync();
    }
}
