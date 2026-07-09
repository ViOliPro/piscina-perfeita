using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Account
{
    public interface IAccountService
    {
        Task<AccountResponseDto> Login(AccountRequestDto request);
        Task<AccountResponseDto> SwitchLocal(Guid userId, Guid newLocalId);
    };
}
