using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Service.Usuarios;
using PiscinaPerfeita.Api.Services;

namespace PiscinaPerfeita.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public AuthController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
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
                    new { message = "Se o e-mail existir, você receberá um link em instantes." }
                );

            var linkPasswordResetToken = _usuarioService.PasswordResetToken(dto.Email);

            return Ok(new { message = "Se o e-mail existir, você receberá um link em instantes." });
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
