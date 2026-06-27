using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.Piscinas;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PiscinasController : ControllerBase
    {
        private readonly IPiscinaService _piscinasService;

        public PiscinasController(IPiscinaService piscinasService)
        {
            _piscinasService = piscinasService ?? throw new ArgumentNullException(nameof(piscinasService));
        }

        // 1. GET: api/clientes (Retorna todos os registros do banco)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PiscinaResponseDto>>> Get()
        {
            var piscinas = await _piscinasService.Show();
            return Ok(piscinas);
        }

        // 2. GET: api/clientes/id (Retorna o registro com id)
        [HttpGet("{id}")]
        public async Task<ActionResult<PiscinaResponseDto>> GetById(Guid id)
        {
            try
            {
                var piscinas = await _piscinasService.GetById(id);
                return Ok(piscinas);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // 3. POST: api/clientes (Insere um dado novo que aparecerá no pgAdmin)
        [HttpPost]
        public async Task<ActionResult<PiscinaResponseDto>> Create(PiscinaRequestDto dto)
        {
            var user = await _piscinasService.Create(dto);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PiscinaResponseDto>> Update(Guid id, PiscinaRequestDto dto)
        {
            try
            {
                await _piscinasService.Update(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<PiscinaResponseDto>> Delete(Guid id)
        {
            try
            {
                await _piscinasService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }
    }
}
