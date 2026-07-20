using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.Usuarios;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Estava faltando: este controller ficava 100% público (lista, cria, edita e apaga
                // usuário sem autenticação nenhuma). Todos os métodos do serviço já assumiam um
                // usuário autenticado (via IAuthenticatedUser) — só faltava essa trava na entrada.
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuariosService;

        public UsuariosController(IUsuarioService usuariosService)
        {
            _usuariosService =
                usuariosService ?? throw new ArgumentNullException(nameof(usuariosService));
        }

        // 1. GET: api/clientes (Retorna todos os registros do banco)
        [HttpGet]
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

        [HttpPut("{id}")]
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

        // Restrito a SuperAdmin: o serviço ainda não valida se o usuário logado
        // pertence ao mesmo Local do usuário sendo apagado, então liberar isso
        // pra qualquer usuário autenticado permitiria um Administrador apagar
        // usuários de outros Locais (IDOR). Enquanto esse escopo não é
        // implementado no service, mantemos essa ação restrita ao papel mais
        // privilegiado.
        [Authorize(Roles = "SuperAdmin")]
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
    }
}
