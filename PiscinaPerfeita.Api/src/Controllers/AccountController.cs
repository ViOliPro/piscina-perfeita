using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Service.Account;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IAuthenticatedUser _authenticatedUser;

        public AccountController(
            IAccountService accountService,
            IAuthenticatedUser authenticatedUser
        )
        {
            _accountService =
                accountService ?? throw new ArgumentNullException(nameof(accountService));
            _authenticatedUser =
                authenticatedUser ?? throw new ArgumentNullException(nameof(authenticatedUser));
        }

        // Login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AccountResponseDto>> Login([FromBody] AccountRequestDto req)
        {
            try
            {
                var res = await _accountService.Login(req);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Novo JWT se o usuario mudar de local
        [HttpPost("SwitchLocal")]
        [Authorize]
        public async Task<ActionResult<AccountResponseDto>> SwitchLocal(Guid newLocalId)
        {
            try
            {
                // O usuário só pode trocar para um Local ao qual ele mesmo está
                // vinculado — por isso o Id vem do token (claims), nunca do
                // corpo da requisição. Antes este endpoint era [AllowAnonymous]
                // e aceitava um userId arbitrário no body, permitindo que
                // qualquer pessoa trocasse o local de qualquer usuário.
                var userId = _authenticatedUser.GetUserId();
                var res = await _accountService.SwitchLocal(userId, newLocalId);
                return Ok(res);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
