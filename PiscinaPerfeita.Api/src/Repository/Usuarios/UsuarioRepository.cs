using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

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
        return await _context
            .Usuarios.Select(u => new UsuarioResponseDto
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email ?? string.Empty,
                Cpf = u.Cpf ?? string.Empty,
                CreatedAt = u.CreatedAt,
                Role = u.Role,
                Piscinas = u
                    .Piscinas.Where(p => p.UsuarioId == u.Id)
                    .Select(p => new NomeIdDto(p.Id, p.Nome))
                    .ToList(),
            })
            .ToListAsync();
    }

    public async Task<List<UsuarioResponseDto>> FilterRoleUsuario(Guid localId)
    {
        // CORRIGIDO: antes filtrava só por `u.Role == Role.Usuario`, ou seja,
        // qualquer Administrador via TODOS os usuários comuns do sistema,
        // de qualquer Local/tenant — vazamento direto entre tenants. Agora
        // exige também um vínculo ATIVO em UsuariosLocais para o Local do
        // Administrador que está fazendo a consulta.
        var usuarioDto = await _context
            .Usuarios.Where(u =>
                u.Role == Role.Usuario
                && u.UsuariosLocais.Any(ul => ul.LocalId == localId && ul.Ativo)
            )
            .Select(u => new UsuarioResponseDto
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email ?? string.Empty,
                Cpf = u.Cpf ?? string.Empty,
                CreatedAt = u.CreatedAt,
                Role = u.Role,
                // CORRIGIDO: este campo nunca era preenchido aqui — como o valor
                // default do enum Perfil é 0 (Administrador), todo usuário retornado
                // por este endpoint aparecia como "Administrador" no front, mesmo
                // sendo Operador ou Visualizador. Agora reflete o Perfil do vínculo
                // ativo deste usuário especificamente no Local consultado.
                Perfil = u
                    .UsuariosLocais.Where(ul => ul.LocalId == localId && ul.Ativo)
                    .Select(ul => ul.Perfil)
                    .FirstOrDefault(),
                Piscinas = u
                    .Piscinas.Where(p => p.UsuarioId == u.Id)
                    .Select(p => new NomeIdDto(p.Id, p.Nome))
                    .ToList(),
            })
            .ToListAsync();

        return usuarioDto;
    }

    public async Task<UsuarioResponseDto?> GetByIdDto(Guid id)
    {
        var usuarioDto = await _context
            .Usuarios.Where(u => u.Id == id)
            .Select(u => new UsuarioResponseDto
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email ?? string.Empty,
                Cpf = u.Cpf ?? string.Empty,
                CreatedAt = u.CreatedAt,
                Role = u.Role,
                Piscinas = u
                    .Piscinas.Where(p => p.UsuarioId == id)
                    .Select(p => new NomeIdDto(p.Id, p.Nome))
                    .ToList(),
            })
            .FirstOrDefaultAsync();

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
        user.Role = usuario.Role;
        // CORRIGIDO: SecurityStamp nunca era persistido aqui, então trocar de
        // e-mail (UpdateMyProfileAsync) ou redefinir senha (UpdatePasswordResetToken)
        // rotacionava o valor em memória mas o JWT antigo continuava válido
        // pra sempre — o middleware que compara security_stamp nunca via a
        // mudança. Callers que não querem rotacionar devem passar o stamp atual.
        user.SecurityStamp = usuario.SecurityStamp;

        await _context.SaveChangesAsync();
    }

    // Dedicado ao SwitchLocal: atualiza só o UltimoLocalId. Antes o SwitchLocal
    // reaproveitava o Update() genérico acima (pensado pra edição de perfil),
    // que (a) não toca em UltimoLocalId — então a escolha nunca era persistida
    // de verdade — e (b) zera o SenhaHash toda vez, porque o objeto Usuario
    // passado pra ele era montado do zero sem preencher esse campo.
    public async Task UpdateUltimoLocal(Guid id, Guid localId)
    {
        var user = await _context.Usuarios.FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException($"Usuário com ID {id} não encontrado.");

        user.UltimoLocalId = localId;

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

    public async Task<Usuario?> GetByEmail(string email)
    {
        var user = await _context
            .Usuarios.IgnoreQueryFilters()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();

        return user;
    }

    public async Task<Usuario?> GetNameById(Guid id)
    {
        var user = await _context.Usuarios.Where(u => u.Id == id).FirstOrDefaultAsync();

        return user;
    }

    public async Task<Usuario?> GetPasswordById(Guid id)
    {
        var user = await _context.Usuarios.Where(u => u.Id == id).FirstOrDefaultAsync();

        return user ?? null;
    }

    public async Task<Usuario?> GetById(Guid id)
    {
        var usuarioDto = await _context
            .Usuarios.Where(u => u.Id == id)
            .Select(u => new Usuario
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email ?? string.Empty,
                Cpf = u.Cpf ?? string.Empty,
                CreatedAt = u.CreatedAt,
                Role = u.Role,
                SecurityStamp = u.SecurityStamp,
            })
            .FirstOrDefaultAsync();

        return usuarioDto ?? null;
    }

    public async Task PasswordResetToken(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();
    }

    public async Task<PasswordResetToken?> GetPasswordResetToken(string tokenHash)
    {
        var token = await _context
            .PasswordResetTokens.IgnoreQueryFilters()
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        return token;
    }

    public async Task UpdatePasswordResetToken(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task CriarConvite(ConviteToken convite)
    {
        _context.ConviteTokens.Add(convite);
        await _context.SaveChangesAsync();
    }

    public async Task<ConviteToken?> GetConviteByHash(string tokenHash)
    {
        return await _context
            .ConviteTokens.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    public async Task UpdateConvite(ConviteToken convite)
    {
        _context.ConviteTokens.Update(convite);
        await _context.SaveChangesAsync();
    }
}
