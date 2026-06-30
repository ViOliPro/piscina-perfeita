using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Dtos.Response;

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
        return await _context.Piscinas.Select(u => new PiscinaResponseDto
        {
            Id = u.Id,
            Nome = u.Nome,
            VolumeLitros = u.VolumeLitros,
            ProfundidadeMedia = u.ProfundidadeMedia,
            CreatedAt = u.CreatedAt,
            UsuarioPiscina = u.Usuario != null ? new UsuarioPiscinaResponseDto
            {
                Id = u.Usuario.Id,
                Nome = u.Usuario.Nome,
            } : null,
            AnalisePiscina = u.Analises != null ? u.Analises.Select(a => new AnalisePiscinaResponseDto
            {
                Id = a.Id,
                DataAnalise = a.DataAnalise
            }).ToList() : new List<AnalisePiscinaResponseDto>(),
            Estoques = u.Estoques != null ? u.Estoques.Select(e => new EstoquePiscinaResponseDto
            {
                Id = e.Id,
                Nome = e.Produto.Nome
            }).ToList() : new List<EstoquePiscinaResponseDto>(),
            MovimentacoesEstoques = u.MovimentacoesEstoques != null ? u.MovimentacoesEstoques.Select(m => new MovimentacaoEstoquePiscinaResponsetDto
            {
                Id = m.Id,
                Nome = m.TipoMovimentacao.ToString(),
                DataMovimentacao = m.DataMovimentacao,
            }).ToList() : new List<MovimentacaoEstoquePiscinaResponsetDto>(),
        }).ToListAsync();
    }

    public async Task<PiscinaResponseDto?> GetById(Guid id)
    {
        var piscinaDto = await _context.Piscinas
            .Where(p => p.Id == id)
            .Select(u => new PiscinaResponseDto
            {
                Id = u.Id,
                Nome = u.Nome,
                VolumeLitros = u.VolumeLitros,
                ProfundidadeMedia = u.ProfundidadeMedia,
                CreatedAt = u.CreatedAt,
                UsuarioPiscina = u.Usuario != null ? new UsuarioPiscinaResponseDto
                {
                    Id = u.Usuario.Id,
                    Nome = u.Usuario.Nome
                } : null,
                AnalisePiscina = u.Analises != null ? u.Analises.Select(a => new AnalisePiscinaResponseDto
                {
                    Id = a.Id,
                    DataAnalise = a.DataAnalise
                }).ToList() : new List<AnalisePiscinaResponseDto>(),
                Estoques = u.Estoques != null ? u.Estoques.Select(e => new EstoquePiscinaResponseDto
                {
                    Id = e.Id,
                    Nome = e.Produto.Nome
                }).ToList() : new List<EstoquePiscinaResponseDto>(),
                MovimentacoesEstoques = u.MovimentacoesEstoques != null ? u.MovimentacoesEstoques.Select(m => new MovimentacaoEstoquePiscinaResponsetDto
                {
                    Id = m.Id,
                    Nome = m.TipoMovimentacao.ToString(),
                    DataMovimentacao = m.DataMovimentacao,
                }).ToList() : new List<MovimentacaoEstoquePiscinaResponsetDto>(),
            }).FirstOrDefaultAsync();

        return piscinaDto == null ? null : piscinaDto;
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
