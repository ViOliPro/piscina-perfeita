using Microsoft.AspNetCore.Authorization;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Authorization
{
    // SuperAdmin sempre passa, independente de perfil

    public class PerfilHandler : AuthorizationHandler<PerfilRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PerfilRequirement requirement
        )
        {
            if (context.User.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var perfilClaim = context.User.FindFirst("perfil")?.Value;
            if (
                perfilClaim is not null
                && Enum.TryParse<Perfil>(perfilClaim, out var perfil)
                && requirement.PerfisPermitidos.Contains(perfil)
            )
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
