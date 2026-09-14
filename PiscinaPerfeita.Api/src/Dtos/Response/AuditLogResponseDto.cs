namespace PiscinaPerfeita.Api.Dtos.Response;

public class AuditLogResponseDto
{
    public Guid Id { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public Guid? LocalId { get; set; }
    public Guid? UsuarioId { get; set; }
    public string? UsuarioEmail { get; set; }
    public string Action { get; set; } = null!;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? HttpMethod { get; set; }
    public string? Path { get; set; }
    public string? IpAddress { get; set; }
    public bool Success { get; set; }
    public string? Summary { get; set; }
    public string? Payload { get; set; }
    public Guid? CorrelationId { get; set; }
}
