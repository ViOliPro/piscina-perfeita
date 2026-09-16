using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.AuditLogs;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);

    Task<List<AuditLogResponseDto>> ListAsync(
        Guid? localId,
        Guid? usuarioId,
        string? action,
        string? entityType,
        DateTimeOffset? de,
        DateTimeOffset? ate,
        int limit,
        CancellationToken ct = default
    );
}
