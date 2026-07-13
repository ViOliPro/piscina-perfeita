using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.AplicacoesProduto;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AplicacoesProdutoController : ControllerBase
    {
        private readonly IAplicacaoProdutoService _aplicacaoService;

        public AplicacoesProdutoController(IAplicacaoProdutoService aplicacaoService)
        {
            _aplicacaoService =
                aplicacaoService ?? throw new ArgumentNullException(nameof(aplicacaoService));
        }

        // GET: api/aplicacoesproduto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AplicacaoProdutoResponseDto>>> Get()
        {
            var aplicacoes = await _aplicacaoService.Show();
            return Ok(aplicacoes);
        }

        // GET: api/aplicacoesproduto/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AplicacaoProdutoResponseDto>> GetById(Guid id)
        {
            try
            {
                var aplicacao = await _aplicacaoService.GetById(id);
                return Ok(aplicacao);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/aplicacoesproduto
        //
        // Registra a aplicação de um produto numa piscina. Automaticamente,
        // na mesma operação: cria a MovimentacaoEstoque (Tipo=Aplicacao) e
        // dá baixa no Estoque do Depósito informado — considerando a
        // conversão de unidade se a aplicação foi lançada em uma unidade
        // diferente da unidade base do produto (ex.: produto em L,
        // aplicação em mL).
        [HttpPost]
        public async Task<ActionResult<AplicacaoProdutoResponseDto>> Create(
            AplicacaoProdutoRequestDto dto
        )
        {
            try
            {
                var aplicacao = await _aplicacaoService.Create(dto);
                return CreatedAtAction(nameof(GetById), new { id = aplicacao.Id }, aplicacao);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Ex.: estoque insuficiente, ou nenhum saldo cadastrado
                // ainda para este produto neste depósito.
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
