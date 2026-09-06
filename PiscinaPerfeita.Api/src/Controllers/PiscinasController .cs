using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Authorization;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.Piscinas;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.UserOrSuper)]
    public class PiscinasController : ControllerBase
    {
        private readonly IPiscinaService _piscinasService;

        public PiscinasController(IPiscinaService piscinasService)
        {
            _piscinasService =
                piscinasService ?? throw new ArgumentNullException(nameof(piscinasService));
        }

        // 1. GET: api/clientes (Retorna todos os registros do banco)
        [HttpGet]
        [Authorize(Policy = Policies.Listar)]
        public async Task<ActionResult<IEnumerable<PiscinaResponseDto>>> Get()
        {
            try
            {
                var piscinas = await _piscinasService.Show();
                return Ok(piscinas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/piscinas/{id}/dashboard — analítico sob demanda
        // Rota estática antes de {id} genérico para não conflitar com GetById.
        [HttpGet("{id:guid}/dashboard")]
        [Authorize(Policy = Policies.Listar)]
        public async Task<ActionResult<PiscinaDashboardResponseDto>> GetDashboard(
            Guid id,
            [FromQuery] DateTimeOffset? inicio = null,
            [FromQuery] DateTimeOffset? fim = null,
            [FromQuery] int limitAnalises = 10,
            [FromQuery] int limitMovimentacoes = 10
        )
        {
            try
            {
                var dashboard = await _piscinasService.GetDashboard(
                    id,
                    inicio,
                    fim,
                    limitAnalises,
                    limitMovimentacoes
                );
                return Ok(dashboard);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Policies.Listar)]
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
        [Authorize(Policy = Policies.Cadastrar)]
        public async Task<ActionResult<PiscinaResponseDto>> Create(PiscinaRequestDto dto)
        {
            try
            {
                var data = await _piscinasService.Create(dto);

                return Ok(data);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Policies.Editar)]
        public async Task<ActionResult<PiscinaResponseDto>> Update(Guid id, PiscinaRequestDto dto)
        {
            if (dto.UsuarioId == Guid.Empty)
            {
                return BadRequest(
                    new { message = "O ID do usuário é obrigatório e não pode ser vazio." }
                );
            }

            try
            {
                var data = await _piscinasService.Update(id, dto);

                return CreatedAtAction(nameof(GetById), new { id = data.Id }, data);
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

        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.Deletar)]
        public async Task<ActionResult> Delete(Guid id)
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
