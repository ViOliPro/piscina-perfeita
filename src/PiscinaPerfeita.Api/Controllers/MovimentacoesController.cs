using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.MovimentacoesEstoque;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var movimentacoes = await _movimentacoesService.Show();
            return Ok(movimentacoes);
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
            var user = await _movimentacoesService.Create(dto);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MovimentacaoEstoqueResponseDto>> Update(Guid id, MovimentacaoEstoqueRequestDto dto)
        {
            try
            {
                await _movimentacoesService.Update(id, dto);
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
