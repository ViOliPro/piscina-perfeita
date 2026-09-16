using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Data;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.AuditLogs;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly PiscinaPerfeitaContext _context;

    public AuditLogRepository(PiscinaPerfeitaContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
    {
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<AuditLogResponseDto>> ListAsync(
        Guid? localId,
        Guid? usuarioId,
        string? action,
        string? entityType,
        DateTimeOffset? de,
        DateTimeOffset? ate,
        int limit,
        CancellationToken ct = default
    )
    {
        if (limit <= 0) limit = 50;
        if (limit > 200) limit = 200;

        var q = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (localId.HasValue)
            q = q.Where(a => a.LocalId == localId);

        if (usuarioId.HasValue)
            q = q.Where(a => a.UsuarioId == usuarioId);

        if (!string.IsNullOrWhiteSpace(action))
            q = q.Where(a => a.Action == action);

        if (!string.IsNullOrWhiteSpace(entityType))
            q = q.Where(a => a.EntityType == entityType);

        if (de.HasValue)
            q = q.Where(a => a.OccurredAt >= de);

        if (ate.HasValue)
            q = q.Where(a => a.OccurredAt <= ate);

        return await q
            .OrderByDescending(a => a.OccurredAt)
            .Take(limit)
            .Select(a => new AuditLogResponseDto
            {
                Id = a.Id,
                OccurredAt = a.OccurredAt,
                LocalId = a.LocalId,
                UsuarioId = a.UsuarioId,
                UsuarioEmail = a.UsuarioEmail,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                HttpMethod = a.HttpMethod,
                Path = a.Path,
                IpAddress = a.IpAddress,
                Success = a.Success,
                Summary = a.Summary,
                Payload = a.Payload,
                CorrelationId = a.CorrelationId,
            })
            .ToListAsync(ct);
    }
}
