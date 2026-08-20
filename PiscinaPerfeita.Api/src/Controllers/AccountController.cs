using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Service.Account;
using PiscinaPerfeita.Api.Service.Account.Google;
using PiscinaPerfeita.Api.Service.Email;
using PiscinaPerfeita.Api.Service.Usuarios;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private const string RefreshCookieName = "pp_refresh";

        private void SetRefreshCookie(string rawToken) =>
            Response.Cookies.Append(
                RefreshCookieName,
                rawToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/api/account",
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                }
            );

        private readonly IAccountService _accountService;
        private readonly IAuthenticatedUser _authenticatedUser;
        private readonly IUsuarioService _usuarioService;
        private readonly IGoogleAuthService _googleAuthService;

        public AccountController(
            IAccountService accountService,
            IAuthenticatedUser authenticatedUser,
            IUsuarioService usuarioService,
            IGoogleAuthService googleAuthService
        )
        {
            _accountService =
                accountService ?? throw new ArgumentNullException(nameof(accountService));
            _authenticatedUser =
                authenticatedUser ?? throw new ArgumentNullException(nameof(authenticatedUser));
            _usuarioService =
                usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
            _googleAuthService =
                googleAuthService ?? throw new ArgumentNullException(nameof(googleAuthService));
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
                SetRefreshCookie(res.RefreshToken);
                return Ok(res.Response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Ocorreu um erro ao processar o login." });
            }
        }

        // Novo JWT se o usuario mudar de local
        [HttpPost("SwitchLocal")]
        [Authorize]
        public async Task<ActionResult<AccountResponseDto>> SwitchLocal(Guid? newLocalId)
        {
            try
            {
                // Id sempre vem do token (claims), nunca do corpo da
                // requisição — evita trocar o local de outro usuário.
                var userId = _authenticatedUser.GetUserId();
                var res = await _accountService.SwitchLocal(userId, newLocalId);
                SetRefreshCookie(res.RefreshToken);
                return Ok(res.Response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Ocorreu um erro ao processar o login." });
            }
        }

        // POST /api/auth/esqueci-senha
        [HttpPost("esqueci-senha")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-sensitive")]
        public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaRequestDto dto)
        {
            try
            {
                await _usuarioService.EsqueciSenha(dto);
            }
            catch (EmailDeliveryException)
            {
                // Falha no Resend não deve virar erro pro usuário — só significa
                // que o e-mail pode não ter chegado; a resposta continua neutra.
            }

            return Ok(new { message = "Se o e-mail existir, você receberá um link em instantes." });
        }

        // POST /api/auth/redefinir-senha
        [HttpPost("redefinir-senha")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-sensitive")]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequestDto dto)
        {
            try
            {
                await _usuarioService.UpdatePasswordResetToken(dto);
                return Ok(new { message = "Senha redefinida com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                // Token inválido/expirado/já usado ou senha fora dos requisitos.
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Ocorreu um erro ao redefinir a senha." });
            }
        }

        // POST /api/account/completar-convite
        [HttpPost("completar-convite")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-sensitive")]
        public async Task<IActionResult> CompletarConvite([FromBody] CompletarConviteRequestDto dto)
        {
            try
            {
                await _usuarioService.CompletarConvite(dto);
                return Ok(new { message = "Cadastro concluído. Faça login para continuar." });
            }
            catch (InvalidOperationException ex)
            {
                // Convite inválido/expirado/já usado, e-mail já cadastrado
                // (corrida) ou senha fora dos requisitos.
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Ocorreu um erro ao concluir o cadastro." });
            }
        }

        [HttpPost("google")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-sensitive")]
        public async Task<ActionResult<AccountResponseDto>> GoogleLogin(
            [FromBody] GoogleLoginRequest request
        )
        {
            var resultado = await _googleAuthService.AutenticarAsync(request.IdToken);
            if (!resultado.Sucesso)
                return Unauthorized(
                    new { message = resultado.Mensagem, erro = resultado.Erro.ToString() }
                );
            SetRefreshCookie(resultado.RefreshToken!);

            return Ok(
                new AccountResponseDto
                {
                    AccessToken = resultado.Token!,
                    TokenType = "Bearer",
                    expiresIn = 3600,
                    User = resultado.User!,
                }
            );
        }

        [HttpPost("google/completar-convite")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-sensitive")]
        public async Task<ActionResult<AccountResponseDto>> GoogleCompletarConvite(
            [FromBody] CompletarConviteGoogleRequest request
        )
        {
            var resultado = await _googleAuthService.CompletarCadastroAsync(
                request.IdToken,
                request.Cpf,
                request.AceiteTermos
            );
            if (!resultado.Sucesso)
            {
                // AceiteTermosPendente é erro de validação do formulário, não de
                // sessão — 401 aqui faria o front tentar refresh de token à toa
                // (não há sessão nenhuma neste endpoint anônimo) e mostrar
                // "Sessão expirada" em vez da mensagem real.
                if (resultado.Erro == AuthErro.AceiteTermosPendente)
                    return BadRequest(
                        new { message = resultado.Mensagem, erro = resultado.Erro.ToString() }
                    );

                return Unauthorized(
                    new { message = resultado.Mensagem, erro = resultado.Erro.ToString() }
                );
            }

            SetRefreshCookie(resultado.RefreshToken!);
            return Ok(
                new AccountResponseDto
                {
                    AccessToken = resultado.Token!,
                    TokenType = "Bearer",
                    expiresIn = 3600,
                    User = resultado.User!,
                }
            );
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-sensitive")]
        public async Task<ActionResult<AccountResponseDto>> Refresh()
        {
            if (
                !Request.Cookies.TryGetValue(RefreshCookieName, out var rawToken)
                || string.IsNullOrEmpty(rawToken)
            )
                return Unauthorized(new { message = "Sessão expirada. Faça login novamente." });

            try
            {
                var resultado = await _accountService.Refresh(rawToken);
                SetRefreshCookie(resultado.RefreshToken);
                return Ok(resultado.Response);
            }
            catch (UnauthorizedAccessException ex)
            {
                Response.Cookies.Delete(
                    RefreshCookieName,
                    new CookieOptions { Path = "/api/account" }
                );
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            if (
                Request.Cookies.TryGetValue(RefreshCookieName, out var rawToken)
                && !string.IsNullOrEmpty(rawToken)
            )
                await _accountService.RevogarRefreshToken(rawToken);

            Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/account" });
            return NoContent();
        }
    }
}
