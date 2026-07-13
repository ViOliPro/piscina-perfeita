using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.Depositos;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepositosController : ControllerBase
    {
        private readonly IDepositoService _depositoService;

        public DepositosController(IDepositoService depositoService)
        {
            _depositoService = depositoService ?? throw new ArgumentNullException(nameof(depositoService));
        }

        // 1. GET: api/clientes (Retorna todos os registros do banco)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnaliseResponseDto>>> Get()
        {
            try
            {
                var data = await _depositoService.Show();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 2. GET: api/clientes/id (Retorna o registro com id)
        [HttpGet("{id}")]
        public async Task<ActionResult<AnaliseResponseDto>> GetById(Guid id)
        {
            try
            {
                var data = await _depositoService.GetById(id);
                return Ok(data);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // 3. POST: api/analises
        [HttpPost]
        public async Task<ActionResult<DepositoResponseDto>> Create(DepositoRequestDto dto)
        {
            try
            {
                var user = await _depositoService.Create(dto);

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
        public async Task<ActionResult> Update(Guid id, DepositoRequestDto dto)
        {
            try
            {
                var query = await _depositoService.Update(id, dto);
                return CreatedAtAction(nameof(GetById), new { id = query.Id }, query); ;
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _depositoService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }
    }
}


