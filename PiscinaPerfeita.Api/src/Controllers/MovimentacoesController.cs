using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.MovimentacoesEstoque;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MovimentacoesController : ControllerBase
    {
        private readonly IMovimentacaoService _movimentacoesService;

        public MovimentacoesController(IMovimentacaoService movimentacoesService)
        {
            _movimentacoesService = movimentacoesService ?? throw new ArgumentNullException(nameof(movimentacoesService));
        }

        // 1. GET: api/clientes (Retorna todos os registros do banco)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimentacaoEstoqueResponseDto>>> Get()
        {
            try
            {
                var movimentacoes = await _movimentacoesService.Show();
                return Ok(movimentacoes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 2. GET: api/clientes/id (Retorna o registro com id)
        [HttpGet("{id}")]
        public async Task<ActionResult<MovimentacaoEstoqueResponseDto>> GetById(Guid id)
        {
            try
            {
                var movimentacoes = await _movimentacoesService.GetById(id);
                return Ok(movimentacoes);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // 3. POST: api/clientes (Insere um dado novo que aparecerá no pgAdmin)
        [HttpPost]
        public async Task<ActionResult<MovimentacaoEstoqueResponseDto>> Create(MovimentacaoEstoqueRequestDto dto)
        {
            try
            {
                var user = await _movimentacoesService.Create(dto);

                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            } 
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, MovimentacaoEstoqueRequestDto dto)
        {
            try
            {
                var mov = await _movimentacoesService.Update(id, dto);
                return CreatedAtAction(nameof(GetById), new { id = mov.Id }, mov);
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
                await _movimentacoesService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }
    }
}
