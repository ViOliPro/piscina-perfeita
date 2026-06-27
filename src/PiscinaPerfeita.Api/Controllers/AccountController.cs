using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.Account;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        // 1. GET: api/clientes (Retorna todos os registros do banco)
        [HttpGet]
        public async Task<ActionResult<ProdutoResponseDto>> Login(AccountRequestDto req)
        {
            var res = await _accountService.Login(req);
            return Ok(res);
        }

    }
}
