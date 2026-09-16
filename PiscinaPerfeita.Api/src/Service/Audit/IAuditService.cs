using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Audit;

public interface IAuditService
{
    /// <summary>
    /// Grava um evento de auditoria. Falhas de escrita são engolidas e
    /// logadas (negócio não quebra por audit no MVP).
    /// </summary>
    Task WriteAsync(AuditEntry entry, CancellationToken ct = default);

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
