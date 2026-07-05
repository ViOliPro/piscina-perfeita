using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Service.Estoques;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EstoquesController : ControllerBase
    {
        private readonly IEstoqueService _estoquesService;

        public EstoquesController(IEstoqueService estoquesService)
        {
            _estoquesService =
                estoquesService ?? throw new ArgumentNullException(nameof(estoquesService));
        }

        // 1. GET: api/clientes (Retorna todos os registros do banco)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstoqueResponseDto>>> Get()
        {
            var estoques = await _estoquesService.Show();
            return Ok(estoques);
        }

        // 2. GET: api/clientes/id (Retorna o registro com id)
        [HttpGet("{id}")]
        public async Task<ActionResult<EstoqueResponseDto>> GetById(Guid id)
        {
            try
            {
                var estoques = await _estoquesService.GetById(id);
                return Ok(estoques);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // 3. POST: api/clientes (Insere um dado novo que aparecerá no pgAdmin)
        [HttpPost]
        public async Task<ActionResult<EstoqueResponseDto>> Create(EstoqueRequestDto dto)
        {
            try
            {
                var user = await _estoquesService.Create(dto);

                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EstoqueResponseDto>> Update(Guid id, EstoqueRequestDto dto)
        {
            try
            {
                var estoque = await _estoquesService.Update(id, dto);
                return CreatedAtAction(nameof(GetById), new { id = estoque.Id }, estoque);
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
                await _estoquesService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
