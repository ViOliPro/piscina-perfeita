using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Usuarios;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public UsuarioRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<Usuario>> Show()
    {
        return await _context.Usuarios.ToListAsync();
    }

    public async Task<Usuario> GetById(Guid id)
    {
        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        if(user == null)
            throw new KeyNotFoundException($"Usuário com ID {id} não encontrado.");

        return user;
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
        user.Senhahash = usuario.Senhahash;

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
