using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.Usuarios;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IEstoquesService _usuariosService;

        public UsuariosController(IEstoquesService usuariosService)
        {
            _usuariosService = usuariosService ?? throw new ArgumentNullException(nameof(usuariosService));
        }

        // 1. GET: api/clientes (Retorna todos os registros do banco)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> Get()
        {
            var usuarios = await _usuariosService.Show();
            return Ok(usuarios);
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
        }

        // 3. POST: api/clientes (Insere um dado novo que aparecerá no pgAdmin)
        [HttpPost]
        public async Task<ActionResult<UsuarioResponseDto>> Create(UsuarioRequestDto dto)
        {
            var user = await _usuariosService.Create(dto);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UsuarioResponseDto>> Update(Guid id, UsuarioRequestDto dto)
        {
            try
            {
                await _usuariosService.Update(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<UsuarioResponseDto>> Delete(Guid id)
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

        }
    }
}
