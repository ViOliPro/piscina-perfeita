namespace PiscinaPerfeita.Api.Service.Audit;

/// <summary>
/// Entrada tipada para gravar um evento de auditoria.
/// LocalId / UsuarioId / IP / Path são enriquecidos pelo AuditService
/// a partir do usuário autenticado e do HttpContext quando omitidos.
/// </summary>
public class AuditEntry
{
    public string Action { get; set; } = null!;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public bool Success { get; set; } = true;
    public string? Summary { get; set; }

    /// <summary>Objeto serializado como JSON no Payload (sem segredos).</summary>
    public object? Payload { get; set; }

    public Guid? LocalId { get; set; }
    public Guid? UsuarioId { get; set; }
    public string? UsuarioEmail { get; set; }
    public string? HttpMethod { get; set; }
    public string? Path { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public int? StatusCode { get; set; }
    public Guid? CorrelationId { get; set; }
}

/// <summary>Constantes de Action — use estas strings para manter o catálogo estável.</summary>
public static class AuditActions
{
    public const string AuthLoginSuccess = "Auth.LoginSuccess";
    public const string AuthLoginFailure = "Auth.LoginFailure";
    public const string AuthLogout = "Auth.Logout";
    public const string AuthSwitchLocal = "Auth.SwitchLocal";
    public const string AuthAcceptTerms = "Auth.AcceptTerms";

    public const string AnaliseCreate = "Analise.Create";
    public const string AnaliseDelete = "Analise.Delete";

    public const string AplicacaoProdutoCreate = "AplicacaoProduto.Create";

    public const string MovimentacaoCreate = "Movimentacao.Create";
    public const string MovimentacaoInventario = "Movimentacao.Inventario";

    public const string PiscinaCreate = "Piscina.Create";
    public const string PiscinaUpdate = "Piscina.Update";
    public const string PiscinaDelete = "Piscina.Delete";

    public const string UsuarioCreate = "Usuario.Create";
    public const string UsuarioUpdate = "Usuario.Update";
    public const string UsuarioDelete = "Usuario.Delete";
    public const string UsuarioInvite = "Usuario.Invite";

    public const string LocalCreate = "Local.Create";
    public const string LocalUpdate = "Local.Update";
}
