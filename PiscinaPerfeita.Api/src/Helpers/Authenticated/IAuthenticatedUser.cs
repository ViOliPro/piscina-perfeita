using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Helpers.Authenticated;

public interface IAuthenticatedUser
{
    Guid GetUserId();
    Guid GetLocalId();
    Task<CurrentUser> GetCurrentUser();
}
