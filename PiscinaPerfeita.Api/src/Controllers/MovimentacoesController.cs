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
            _movimentacoesService =
                movimentacoesService
                ?? throw new ArgumentNullException(nameof(movimentacoesService));
        }

        // GET: api/movimentacoes
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
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/movimentacoes/{id}
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

        // POST: api/movimentacoes
        // Cria a movimentação e já atualiza o saldo do Estoque na mesma
        // operação (ver MovimentacaoService.Create).
        [HttpPost]
        public async Task<ActionResult<MovimentacaoEstoqueResponseDto>> Create(
            MovimentacaoEstoqueRequestDto dto
        )
        {
            try
            {
                var mov = await _movimentacoesService.Create(dto);
                return CreatedAtAction(nameof(GetById), new { id = mov.Id }, mov);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Ex.: estoque insuficiente para a saída, ou depósito sem
                // saldo cadastrado — 409 Conflict é mais preciso que 400
                // aqui, pois o problema é o ESTADO atual dos dados, não a
                // requisição em si.
                return Conflict(new { message = ex.Message });
            }
        }

        // PUT: api/movimentacoes/{id}
        // NOTA: só corrige campos de registro — não recalcula o efeito no
        // Estoque (ver comentário em MovimentacaoService.Update).
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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

        // POST: api/movimentacoes/contagem-inventario
        //
        // Feature de contagem física ("Ajuste de Inventário"): recebe a
        // contagem de vários produtos de um Depósito de uma vez, calcula a
        // diferença contra o estoque lógico e gera uma MovimentacaoEstoque
        // do tipo AjusteInventario para cada divergência encontrada.
        [HttpPost("contagem-inventario")]
        public async Task<ActionResult<IEnumerable<ContagemInventarioResultadoDto>>> ContagemInventario(
            ContagemInventarioRequestDto dto
        )
        {
            try
            {
                var resultado = await _movimentacoesService.RegistrarContagemInventario(dto);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
