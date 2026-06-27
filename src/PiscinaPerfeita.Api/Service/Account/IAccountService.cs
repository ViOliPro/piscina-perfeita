using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Dtos.Request;

namespace PiscinaPerfeita.Api.Service.Account
{
    public interface IAccountService
    {
        Task<AccountResponseDto> Login(AccountRequestDto request);
    }
}
