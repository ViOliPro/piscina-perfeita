using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Repository.Usuarios;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public UsuarioRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<UsuarioResponseDto>> Show()
    {
        return await _context.Usuarios.Select(u => new UsuarioResponseDto
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            CreatedAt = u.CreatedAt,
            Piscinas = u.Piscinas.Where(p => p.UsuarioId == u.Id)
            .Select(p => new UsuarioPiscinaResponseDto
            {
                Id = p.Id,
                Nome = p.Nome
            }).ToList()
        }).ToListAsync();
    }

    public async Task<UsuarioResponseDto?> GetById(Guid id)
    {
        var usuarioDto = await _context.Usuarios
            .Where(u => u.Id == id)
            .Select(u => new UsuarioResponseDto
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                CreatedAt = u.CreatedAt,
                Piscinas = u.Piscinas.Where(p => p.UsuarioId == id)
                            .Select(p => new UsuarioPiscinaResponseDto
                            {
                                Id = p.Id,
                                Nome = p.Nome
                            }).ToList()
            }).FirstOrDefaultAsync();

        return usuarioDto ?? null;
    }

    public async Task Create(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Guid id, Usuario usuario)
    {
        var user = await _context.Usuarios.FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException($"Usuário com ID {id} não encontrado.");

        user.Nome = usuario.Nome;
        user.Email = usuario.Email;
        user.SenhaHash = usuario.SenhaHash;

        await _context.SaveChangesAsync();

    }

    public async Task Delete(Guid id)
    {
        var user = await _context.Usuarios.FirstOrDefaultAsync(m => m.Id == id);
        if (user == null)
        {
            throw new KeyNotFoundException($"Usuário com ID {id} não encontrado.");
        }

        _context.Remove(user);
        await _context.SaveChangesAsync();

    }



}
