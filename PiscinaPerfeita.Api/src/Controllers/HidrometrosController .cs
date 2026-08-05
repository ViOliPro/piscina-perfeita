using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Authorization;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.Hidrometros;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.UserOrSuper)]
    public class HidrometrosController : ControllerBase
    {
        private readonly IHidrometroService _hidrometrosService;

        public HidrometrosController(IHidrometroService hidrometrosService)
        {
            _hidrometrosService =
                hidrometrosService ?? throw new ArgumentNullException(nameof(hidrometrosService));
        }

        // 1. GET: api/clientes (Retorna todos os registros do banco)
        [HttpGet]
        [Authorize(Policy = Policies.Listar)]
        public async Task<ActionResult<IEnumerable<HidrometroResponseDto>>> Get()
        {
            try
            {
                var hidrometros = await _hidrometrosService.Show();
                return Ok(hidrometros);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 2. GET: api/clientes/id (Retorna o registro com id)
        [HttpGet("{id}")]
        [Authorize(Policy = Policies.Listar)]
        public async Task<ActionResult<HidrometroResponseDto>> GetById(Guid id)
        {
            try
            {
                var hidrometros = await _hidrometrosService.GetById(id);
                return Ok(hidrometros);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // 3. POST: api/hidrometros
        [HttpPost]
        [Authorize(Policy = Policies.Cadastrar)]
        public async Task<ActionResult<HidrometroResponseDto>> Create(HidrometroRequestDto dto)
        {
            try
            {
                var hidrometro = await _hidrometrosService.Create(dto);

                return CreatedAtAction(nameof(GetById), new { id = hidrometro.Id }, hidrometro);
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
        public async Task<ActionResult> Update(Guid id, HidrometroRequestDto dto)
        {
            try
            {
                var hidrometros = await _hidrometrosService.Update(id, dto);
                return CreatedAtAction(nameof(GetById), new { id = hidrometros.Id }, hidrometros);
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
                await _hidrometrosService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
