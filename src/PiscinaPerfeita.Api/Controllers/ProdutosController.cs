using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.Produtos;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoService _produtosService;

        public ProdutosController(IProdutoService produtosService)
        {
            _produtosService = produtosService ?? throw new ArgumentNullException(nameof(produtosService));
        }

        // 1. GET: api/clientes (Retorna todos os registros do banco)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoResponseDto>>> Get()
        {
            try
            {
                var produtos = await _produtosService.Show();
                return Ok(produtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 2. GET: api/clientes/id (Retorna o registro com id)
        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoResponseDto>> GetById(Guid id)
        {
            try
            {
                var produtos = await _produtosService.GetById(id);
                return Ok(produtos);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // 3. POST: api/clientes (Insere um dado novo que aparecerá no pgAdmin)
        [HttpPost]
        public async Task<ActionResult<ProdutoResponseDto>> Create(ProdutoRequestDto dto)
        {
            try
            {
                var user = await _produtosService.Create(dto);

                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProdutoResponseDto>> Update(Guid id, ProdutoRequestDto dto)
        {
            try
            {
                await _produtosService.Update(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ProdutoResponseDto>> Delete(Guid id)
        {
            try
            {
                await _produtosService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }
    }
}
