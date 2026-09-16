using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.AuditLogs;

namespace PiscinaPerfeita.Api.Service.Audit;

public class AuditService : IAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IAuditLogRepository _repo;
    private readonly IAuthenticatedUser _user;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditLogRepository repo,
        IAuthenticatedUser user,
        IHttpContextAccessor http,
        ILogger<AuditService> logger
    )
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task WriteAsync(AuditEntry entry, CancellationToken ct = default)
    {
        if (entry == null || string.IsNullOrWhiteSpace(entry.Action))
            return;

        try
        {
            var ctx = _http.HttpContext;
            var request = ctx?.Request;

            Guid? localId = entry.LocalId;
            Guid? usuarioId = entry.UsuarioId;
            string? email = entry.UsuarioEmail;

            try
            {
                if (!usuarioId.HasValue)
                {
                    var uid = _user.GetUserId();
                    if (uid != Guid.Empty)
                        usuarioId = uid;
                }

                if (!localId.HasValue)
                {
                    var lid = _user.GetLocalId();
                    if (lid != Guid.Empty)
                        localId = lid;
                }
            }
            catch
            {
                // Sem autenticação (ex.: login failure) — ok
            }

            string? ip = entry.IpAddress;
            if (string.IsNullOrEmpty(ip) && ctx != null)
            {
                ip =
                    ctx.Connection.RemoteIpAddress?.ToString()
                    ?? ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }

            string? path = entry.Path ?? request?.Path.Value;
            string? method = entry.HttpMethod ?? request?.Method;
            string? ua = entry.UserAgent;
            if (string.IsNullOrEmpty(ua) && request != null)
                ua = Truncate(request.Headers.UserAgent.ToString(), 512);

            string? payloadJson = null;
            if (entry.Payload != null)
            {
                payloadJson = entry.Payload is string s
                    ? s
                    : JsonSerializer.Serialize(entry.Payload, JsonOptions);
                if (payloadJson.Length > 8000)
                    payloadJson = payloadJson[..8000];
            }

            var log = new AuditLog
            {
                OccurredAt = DateTimeOffset.UtcNow,
                LocalId = localId,
                UsuarioId = usuarioId,
                UsuarioEmail = Truncate(email, 256),
                Action = Truncate(entry.Action, 64)!,
                EntityType = Truncate(entry.EntityType, 64),
                EntityId = entry.EntityId,
                HttpMethod = Truncate(method, 16),
                Path = Truncate(path, 512),
                IpAddress = Truncate(ip, 64),
                UserAgent = ua,
                StatusCode = entry.StatusCode,
                Success = entry.Success,
                Summary = Truncate(entry.Summary, 512),
                Payload = payloadJson,
                CorrelationId = entry.CorrelationId,
            };

            await _repo.AddAsync(log, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao gravar audit log Action={Action}", entry.Action);
        }
    }

    public Task<List<AuditLogResponseDto>> ListAsync(
        Guid? localId,
        Guid? usuarioId,
        string? action,
        string? entityType,
        DateTimeOffset? de,
        DateTimeOffset? ate,
        int limit,
        CancellationToken ct = default
    ) => _repo.ListAsync(localId, usuarioId, action, entityType, de, ate, limit, ct);

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        return value.Length <= max ? value : value[..max];
    }
}
