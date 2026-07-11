using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Helpers.Authenticated;

public interface IAuthenticatedUser
{
    Guid GetUserId();
    Guid GetLocalId();
    Task<CurrentUser> GetCurrentUser();

    // Um SuperAdmin administra o CRUD completo da aplicação (incluindo os
    // próprios Locais) e por isso não precisa estar vinculado a um Local
    // específico. Usado para liberar o acesso mesmo sem local_id no token.
    bool IsSuperAdmin();
}
