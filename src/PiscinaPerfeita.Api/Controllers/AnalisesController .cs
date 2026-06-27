using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.Analises;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalisesController : ControllerBase
    {
        private readonly IAnaliseService _analisesService;

        public AnalisesController(IAnaliseService analisesService)
        {
            _analisesService = analisesService ?? throw new ArgumentNullException(nameof(analisesService));
        }

        // 1. GET: api/clientes (Retorna todos os registros do banco)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnaliseResponseDto>>> Get()
        {
            var analises = await _analisesService.Show();
            return Ok(analises);
        }

        // 2. GET: api/clientes/id (Retorna o registro com id)
        [HttpGet("{id}")]
        public async Task<ActionResult<AnaliseResponseDto>> GetById(Guid id)
        {
            try
            {
                var analises = await _analisesService.GetById(id);
                return Ok(analises);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // 3. POST: api/analises
        [HttpPost]
        public async Task<ActionResult<AnaliseResponseDto>> Create(AnaliseRequestDto dto)
        {
            var user = await _analisesService.Create(dto);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AnaliseResponseDto>> Update(Guid id, AnaliseRequestDto dto)
        {
            try
            {
                await _analisesService.Update(id, dto);
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
                await _analisesService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }
    }
}
