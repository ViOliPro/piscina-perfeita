using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Account
{
    public record LoginResult(AccountResponseDto Response, string RefreshToken);

    public interface IAccountService
    {
        Task<LoginResult> Login(AccountRequestDto request);
        Task<LoginResult> SwitchLocal(Guid userId, Guid? newLocalId);
        Task<LoginResult> Refresh(string rawRefreshToken);
        Task RevogarRefreshToken(string rawRefreshToken);
    };
}
