using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiscinaPerfeita.Api.Models;

/// <summary>
/// Registro de auditoria append-only.
/// NÃO implementa IBelongsToLocal: login sem Local, SuperAdmin e
/// consultas de audit usam filtro explícito por LocalId.
/// </summary>
[Table("AuditLogs", Schema = "piscina-perfeita")]
public class AuditLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Tenant. Null em ações globais (login sem local, SuperAdmin).</summary>
    public Guid? LocalId { get; set; }

    public Guid? UsuarioId { get; set; }

    [MaxLength(256)]
    public string? UsuarioEmail { get; set; }

    /// <summary>Ex.: Auth.LoginSuccess, Analise.Create, AplicacaoProduto.Create</summary>
    [Required]
    [MaxLength(64)]
    public string Action { get; set; } = null!;

    [MaxLength(64)]
    public string? EntityType { get; set; }

    public Guid? EntityId { get; set; }

    [MaxLength(16)]
    public string? HttpMethod { get; set; }

    [MaxLength(512)]
    public string? Path { get; set; }

    [MaxLength(64)]
    public string? IpAddress { get; set; }

    [MaxLength(512)]
    public string? UserAgent { get; set; }

    public int? StatusCode { get; set; }

    public bool Success { get; set; } = true;

    [MaxLength(512)]
    public string? Summary { get; set; }

    /// <summary>JSON serializado (jsonb no Postgres). Sem senhas/tokens.</summary>
    public string? Payload { get; set; }

    public Guid? CorrelationId { get; set; }
}
