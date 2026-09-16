using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Authorization;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Service.Audit;

namespace PiscinaPerfeita.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = Policies.UserOrSuper)]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _audit;
    private readonly IAuthenticatedUser _user;

    public AuditLogsController(IAuditService audit, IAuthenticatedUser user)
    {
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _user = user ?? throw new ArgumentNullException(nameof(user));
    }

    /// <summary>
    /// Lista eventos de auditoria do Local ativo (ou filtro livre para SuperAdmin).
    /// GET /api/audit-logs?de=&amp;ate=&amp;action=&amp;entityType=&amp;usuarioId=&amp;limit=50
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Policies.Listar)]
    public async Task<ActionResult<IEnumerable<AuditLogResponseDto>>> List(
        [FromQuery] DateTimeOffset? de = null,
        [FromQuery] DateTimeOffset? ate = null,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] int limit = 50,
        CancellationToken ct = default
    )
    {
        Guid? localFilter;

        if (_user.IsSuperAdmin())
        {
            var lid = _user.GetLocalId();
            localFilter = lid == Guid.Empty ? null : lid;
        }
        else
        {
            var lid = _user.GetLocalId();
            if (lid == Guid.Empty)
                return Forbid();
            localFilter = lid;
        }

        var items = await _audit.ListAsync(
            localFilter,
            usuarioId,
            action,
            entityType,
            de,
            ate,
            limit,
            ct
        );

        return Ok(items);
    }
}
