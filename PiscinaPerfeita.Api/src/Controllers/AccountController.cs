using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Service.Account;
using PiscinaPerfeita.Api.Service.Email;
using PiscinaPerfeita.Api.Service.Usuarios;


namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IAuthenticatedUser _authenticatedUser;
        private readonly IUsuarioService _usuarioService;

        public AccountController(
            IAccountService accountService,
            IAuthenticatedUser authenticatedUser,
            IUsuarioService usuarioService
        )
        {
            _accountService =
                accountService ?? throw new ArgumentNullException(nameof(accountService));
            _authenticatedUser =
                authenticatedUser ?? throw new ArgumentNullException(nameof(authenticatedUser));
            _usuarioService =
                usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        }

        // Login
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
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
        public async Task<ActionResult<AccountResponseDto>> SwitchLocal(Guid? newLocalId)
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

        // POST /api/auth/esqueci-senha
        [HttpPost("esqueci-senha")]
        [AllowAnonymous]
        public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaRequestDto dto)
        {
            // Sempre responde 200 igual, exista ou não o e-mail —
            // impede que alguém descubra quais e-mails estão cadastrados.
            try
            {
                var usuario = await _usuarioService.GetUsuarioByEmail(dto.Email);
                if (usuario is null)
                    return Ok(
                        new { message = "Usuario Is null Se o e-mail existir, você receberá um link em instantes." }
                    );

                var linkPasswordResetToken = _usuarioService.PasswordResetToken(dto.Email);

                return Ok(
                    new { message = "Se o e-mail existir, você receberá um link em instantes." }
                );
            }
            catch (EmailDeliveryException)
            {
                return StatusCode(
                    200,
                    new { error = "Se o e-mail existir, você receberá um link em instantes." }
                );
            }
        }

        // POST /api/auth/redefinir-senha
        [HttpPost("redefinir-senha")]
        [AllowAnonymous]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequestDto dto)
        {
            try
            {
                var resetToken = await _usuarioService.GetPasswordResetTokenByHash(dto.Token);

                if (
                    resetToken is null
                    || resetToken.UsadoEm != null
                    || resetToken.ExpiraEm < DateTime.UtcNow
                )
                    return BadRequest(new { error = "Link inválido ou expirado." });

                return Ok(new { message = "Senha redefinida com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ocorreu um erro ao redefinir a senha." });
            }
        }
    }
}
