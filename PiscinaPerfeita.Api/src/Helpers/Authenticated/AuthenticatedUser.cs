using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Helpers.Authenticated
{
    public class CurrentUser
    {
        public Guid? UserId { get; set; } = null;
        public Guid? LocalId { get; set; } = null;
        public Role? Role { get; set; } = Models.Role.Usuario;
        public Perfil? Perfil { get; set; } = Models.Perfil.Visualizador;
    }

    public class AuthenticatedUser : IAuthenticatedUser
    {
        private readonly IHttpContextAccessor _accessor;

        public AuthenticatedUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        public Task<CurrentUser> GetCurrentUser()
        {
            var userId =
                _accessor
                    .HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?.Value
                ?? null;

            var localId = _accessor.HttpContext?.User?.FindFirst("local_id")?.Value ?? null;

            var roleClaim = _accessor
                .HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)
                ?.Value;
            Role? role = Models.Role.Usuario; // Default role
            if (!string.IsNullOrEmpty(roleClaim) && Enum.TryParse(roleClaim, out Role parsedRole))
            {
                role = parsedRole;
            }

            var perfilClaim = _accessor.HttpContext?.User?.FindFirst("perfil")?.Value;
            Perfil? perfil = Models.Perfil.Visualizador; // Default perfil
            if (
                !string.IsNullOrEmpty(perfilClaim)
                && Enum.TryParse(perfilClaim, out Perfil Visualizador)
            )
            {
                perfil = Visualizador;
            }

            return Task.FromResult(
                new CurrentUser
                {
                    UserId = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId),
                    LocalId = string.IsNullOrEmpty(localId) ? null : Guid.Parse(localId),
                    Role = role,
                    Perfil = perfil,
                }
            );
        }

        public Guid GetUserId()
        {
            var userId = _accessor
                .HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Usuário não autenticado no sistema.");

            return Guid.Parse(userId);
        }

        public Guid GetLocalId()
        {
            var localId = _accessor.HttpContext?.User?.FindFirst("local_id")?.Value;

            if (string.IsNullOrEmpty(localId))
                throw new UnauthorizedAccessException(
                    "Local do usuário não encontrado no sistema."
                );

            return Guid.Parse(localId);
        }
    }
}
