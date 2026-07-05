namespace PiscinaPerfeita.Api.Helpers.Authenticated
{
    public class AuthenticatedUser : IAuthenticatedUser
    {
        private readonly IHttpContextAccessor _accessor;

        public AuthenticatedUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
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
