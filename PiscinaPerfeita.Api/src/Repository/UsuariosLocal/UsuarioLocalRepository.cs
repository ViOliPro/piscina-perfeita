using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.UsuariosLocal;

public class UsuarioLocalRepository : IUsuarioLocalRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public UsuarioLocalRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<UsuarioLocalResponseDto>> Show()
    {
        return await _context
            .UsuariosLocal.Select(u => new UsuarioLocalResponseDto
            {
                Id = u.Id,
                UsuarioId = u.UsuarioId,
                LocalId = u.LocalId ?? null,
                LocalNome = u.Local != null ? u.Local.Nome : null,
                Perfil = u.Perfil,
                CreatedAt = u.CreatedAt,
                Ativo = u.Ativo,
            })
            .ToListAsync();
    }

    public async Task<UsuarioLocalResponseDto?> GetById(Guid id)
    {
        var usuarioDto = await _context
            .UsuariosLocal.Where(u => u.Id == id)
            .Select(u => new UsuarioLocalResponseDto
            {
                Id = u.Id,
                UsuarioId = u.UsuarioId,
                LocalId = u.LocalId ?? null,
                LocalNome = u.Local != null ? u.Local.Nome : null,
                Perfil = u.Perfil,
                CreatedAt = u.CreatedAt,
                Ativo = u.Ativo,
            })
            .FirstOrDefaultAsync();

        return usuarioDto ?? null;
    }

    public async Task<UsuarioLocal?> Vinculo(Guid userId, Guid newLocalId)
    {
        return await _context.UsuariosLocal.FirstOrDefaultAsync(ul =>
            ul.UsuarioId == userId && ul.LocalId == newLocalId && ul.Ativo
        );
    }

    public async Task<int> QtdUserByLocal(Guid userId)
    {
        var query = _context.UsuariosLocal.Where(u => u.UsuarioId == userId);

        return await query.CountAsync();
    }

    public async Task<UsuarioLocalResponseDto?> GetByUserId(Guid userId)
    {
        var usuarioDto = await _context
            .UsuariosLocal.Where(u => u.UsuarioId == userId)
            .Select(u => new UsuarioLocalResponseDto
            {
                Id = u.Id,
                UsuarioId = u.UsuarioId,
                LocalId = u.LocalId ?? null,
                Perfil = u.Perfil,
            })
            .FirstOrDefaultAsync();

        return usuarioDto ?? null;
    }

    public async Task<List<UsuarioLocalResponseDto>> GetAllByUserId(Guid userId)
    {
        return await _context
            .UsuariosLocal.Where(u => u.UsuarioId == userId && u.Ativo)
            .Select(u => new UsuarioLocalResponseDto
            {
                Id = u.Id,
                UsuarioId = u.UsuarioId,
                LocalId = u.LocalId ?? null,
                LocalNome = u.Local != null ? u.Local.Nome : null,
                Perfil = u.Perfil,
                CreatedAt = u.CreatedAt,
                Ativo = u.Ativo,
            })
            .OrderBy(u => u.LocalNome)
            .ToListAsync();
    }

    public async Task Create(UsuarioLocal usuarioLocal)
    {
        _context.UsuariosLocal.Add(usuarioLocal);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Guid id, UsuarioLocal usuarioLocal)
    {
        var user = await _context.UsuariosLocal.FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException($"Usuário com ID {id} não encontrado.");

        user.Id = id;
        user.UsuarioId = usuarioLocal.UsuarioId;
        user.LocalId = usuarioLocal.LocalId;
        user.Perfil = usuarioLocal.Perfil;

        await _context.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        var user = await _context.UsuariosLocal.FirstOrDefaultAsync(m => m.Id == id);
        if (user == null)
        {
            throw new KeyNotFoundException($"Usuário com ID {id} não encontrado.");
        }

        _context.Remove(user);
        await _context.SaveChangesAsync();
    }
}
