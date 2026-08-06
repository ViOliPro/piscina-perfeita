using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Authorization;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Service.Email;
using PiscinaPerfeita.Api.Service.Usuarios;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.UserOrSuper)]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuariosService;
        private readonly IAuthenticatedUser _authenticatedUser;

        public UsuariosController(
            IUsuarioService usuariosService,
            IAuthenticatedUser authenticatedUser
        )
        {
            _usuariosService =
                usuariosService ?? throw new ArgumentNullException(nameof(usuariosService));
            _authenticatedUser =
                authenticatedUser ?? throw new ArgumentNullException(nameof(authenticatedUser));
        }

        // 1. GET: api/clientes (Retorna todos os registros do banco)
        [HttpGet]
        [Authorize(Policy = Policies.Listar)]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> Get()
        {
            try
            {
                var usuarios = await _usuariosService.Show();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 2. GET: api/clientes/id (Retorna o registro com id)
        [HttpGet("{id}")]
        [Authorize(Policy = Policies.GerenciarUsuario)]
        public async Task<ActionResult<UsuarioResponseDto>> GetById(Guid id)
        {
            try
            {
                var usuarios = await _usuariosService.GetById(id);
                return Ok(usuarios);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // 3. POST: api/clientes (Insere um dado novo que aparecerá no pgAdmin)
        [HttpPost]
        [Authorize(Policy = Policies.GerenciarUsuario)]
        public async Task<ActionResult<UsuarioResponseDto>> Create(UsuarioRequestDto dto)
        {
            try
            {
                var user = await _usuariosService.Create(dto);

                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // POST: api/usuarios/convites — gera um convite por link em vez de
        // cadastrar o usuário direto (mesma regra de permissão do Create:
        // só um SuperAdmin pode convidar outro SuperAdmin; um Administrador
        // só convida gente pro seu próprio Local).
        [HttpPost("convites")]
        [Authorize(Policy = Policies.GerenciarUsuario)]
        public async Task<ActionResult<ConviteResponseDto>> CriarConvite(ConviteRequestDto dto)
        {
            try
            {
                var convite = await _usuariosService.CriarConvite(dto);
                return Ok(convite);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (EmailDeliveryException)
            {
                return StatusCode(
                    502,
                    new
                    {
                        message = "Convite salvo, mas não foi possível enviar o e-mail agora. Tente reenviar mais tarde.",
                    }
                );
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Policies.GerenciarUsuario)]
        public async Task<ActionResult> Update(Guid id, UsuarioRequestUpdateDto dto)
        {
            try
            {
                var userUpdated = await _usuariosService.Update(id, dto);

                return CreatedAtAction(nameof(GetById), new { id = userUpdated.Id }, userUpdated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // Restrito a Administrador (do próprio tenant) ou SuperAdmin. O
        // UsuarioService.Delete agora valida que o usuário-alvo pertence ao
        // mesmo Local do Administrador logado e bloqueia a exclusão do
        // Administrador Pai (só o SuperAdmin pode removê-lo) — antes isso
        // não existia e qualquer Administrador podia apagar usuários de
        // QUALQUER Local (IDOR).
        [Authorize(Policy = Policies.GerenciarUsuario)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _usuariosService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // 6. GET: api/usuarios/me (Retorna o perfil do usuário logado)
        [Authorize(Policy = Policies.VisualizarMeuPerfil)]
        [HttpGet("me")]
        public async Task<IActionResult> MeuPerfil()
        {
            try
            {
                var usuario = await _usuariosService.GetMeuPerfil();

                if (usuario is null)
                    return NotFound(new { message = "Usuário não encontrado." });

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 7. PUT: api/usuarios/me (Atualiza o perfil do usuário logado)
        [Authorize(Policy = Policies.AtualizarMeuPerfil)]
        [HttpPut("me")]
        public async Task<IActionResult> AtualizarMeuPerfil([FromBody] UsuarioRequestUpdateDto dto)
        {
            try
            {
                var usuarioAtualizado = await _usuariosService.UpdateMyProfileAsync(dto);

                return Ok(usuarioAtualizado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
