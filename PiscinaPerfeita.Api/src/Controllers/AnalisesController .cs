using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Authorization;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.Analises;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.UserOrSuper)]
    public class AnalisesController : ControllerBase
    {
        private readonly IAnaliseService _analisesService;

        public AnalisesController(IAnaliseService analisesService)
        {
            _analisesService =
                analisesService ?? throw new ArgumentNullException(nameof(analisesService));
        }

        // GET: api/analises/qualidade-agua
        // Precisa estar declarado antes de GetById({id}) — "qualidade-agua"
        // não é um Guid, mas o roteamento do ASP.NET Core resolve por
        // especificidade de rota, então a ordem de declaração aqui não
        // importa de fato; mantido no topo só por legibilidade.
        [HttpGet("qualidade-agua")]
        [Authorize(Policy = Policies.Listar)]
        public async Task<ActionResult<QualidadeAguaResponseDto>> QualidadeAgua(
            [FromQuery] Guid piscinaId,
            [FromQuery] DateTimeOffset? inicio,
            [FromQuery] DateTimeOffset? fim
        )
        {
            try
            {
                var resultado = await _analisesService.ObterQualidadeAgua(piscinaId, inicio, fim);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/analises
        [HttpGet]
        [Authorize(Policy = Policies.Listar)]
        public async Task<ActionResult<IEnumerable<AnaliseResponseDto>>> Get(
            [FromQuery] DateTimeOffset? dataInicio = null,
            [FromQuery] DateTimeOffset? dataFim = null,
            [FromQuery] Guid? piscinaId = null,
            [FromQuery] int? limit = null
        )
        {
            try
            {
                var analises = await _analisesService.Show(dataInicio, dataFim, piscinaId, limit);
                return Ok(analises);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2. GET: api/clientes/id (Retorna o registro com id)
        [HttpGet("{id}")]
        [Authorize(Policy = Policies.Listar)]
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
        [Authorize(Policy = Policies.Cadastrar)]
        public async Task<ActionResult<AnaliseResponseDto>> Create(AnaliseRequestDto dto)
        {
            try
            {
                var user = await _analisesService.Create(dto);

                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Policies.Editar)]
        public async Task<ActionResult> Update(Guid id, AnaliseRequestDto dto)
        {
            try
            {
                var analises = await _analisesService.Update(id, dto);
                return CreatedAtAction(nameof(GetById), new { id = analises.Id }, analises);
                ;
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.Deletar)]
        public async Task<ActionResult> Delete(Guid id)
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
