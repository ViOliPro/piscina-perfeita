using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.UsuariosLocal;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosLocaisController : ControllerBase
    {
        private readonly IUsuarioLocalService _usuariosLocaisService;

        public UsuariosLocaisController(IUsuarioLocalService usuariosLocaisService)
        {
            _usuariosLocaisService =
                usuariosLocaisService
                ?? throw new ArgumentNullException(nameof(usuariosLocaisService));
        }

        // GET: api/usuarioslocais  (somente SuperAdmin)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioLocalResponseDto>>> Get()
        {
            try
            {
                var vinculos = await _usuariosLocaisService.Show();
                return Ok(vinculos);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // GET: api/usuarioslocais/meus
        // Locais vinculados ao usuário autenticado — alimenta o seletor "Trocar Local".
        [HttpGet("meus")]
        public async Task<ActionResult<IEnumerable<UsuarioLocalResponseDto>>> GetMeus()
        {
            try
            {
                var meusLocais = await _usuariosLocaisService.GetMeusLocais();
                return Ok(meusLocais);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // GET: api/usuarioslocais/usuario/{usuarioId}
        // Locais vinculados a um usuário específico — usado na tela de administração.
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<UsuarioLocalResponseDto>>> GetByUsuario(
            Guid usuarioId
        )
        {
            try
            {
                var vinculos = await _usuariosLocaisService.GetByUsuario(usuarioId);
                return Ok(vinculos);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // GET: api/usuarioslocais/{id}  (somente SuperAdmin)
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioLocalResponseDto>> GetById(Guid id)
        {
            try
            {
                var vinculo = await _usuariosLocaisService.GetById(id);
                return Ok(vinculo);
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

        // POST: api/usuarioslocais  (somente SuperAdmin)
        // É o passo de "vincular": liga um usuário (já criado) a um Local (já criado).
        [HttpPost]
        public async Task<ActionResult<UsuarioLocalResponseDto>> Create(UsuarioLocalRequestDto dto)
        {
            try
            {
                var vinculo = await _usuariosLocaisService.Create(dto);
                return CreatedAtAction(nameof(GetById), new { id = vinculo.Id }, vinculo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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

        // PUT: api/usuarioslocais/{id}  (somente SuperAdmin)
        // Também usado para oficializar o vínculo pendente (LocalId nulo)
        // criado automaticamente junto do usuário.
        [HttpPut("{id}")]
        public async Task<ActionResult<UsuarioLocalResponseDto>> Update(
            Guid id,
            UsuarioLocalRequestDto dto
        )
        {
            try
            {
                var vinculo = await _usuariosLocaisService.Update(id, dto);
                return CreatedAtAction(nameof(GetById), new { id = vinculo.Id }, vinculo);
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

        // DELETE: api/usuarioslocais/{id}  (somente SuperAdmin)
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _usuariosLocaisService.Delete(id);
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
