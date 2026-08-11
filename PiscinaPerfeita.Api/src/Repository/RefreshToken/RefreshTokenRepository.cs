using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly Data.PiscinaPerfeitaContext _context;

    public RefreshTokenRepository(Data.PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task Create(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByHash(string tokenHash)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    public async Task Revoke(Guid id)
    {
        var token = await _context.RefreshTokens.FindAsync(id);
        if (token is null)
            return;

        token.RevogadoEm = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    // Ainda não usado (fica pronto pra detecção de reuso, que combinamos
    // deixar pra depois) — revoga toda a cadeia de sessões de um usuário.
    public async Task RevokeAllByUsuario(Guid usuarioId)
    {
        var ativos = await _context
            .RefreshTokens.Where(t => t.UsuarioId == usuarioId && t.RevogadoEm == null)
            .ToListAsync();

        foreach (var token in ativos)
            token.RevogadoEm = DateTime.UtcNow;

        if (ativos.Count > 0)
            await _context.SaveChangesAsync();
    }
}
