using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Authorization;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.Hidrometros;

namespace PiscinaPerfeita.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.UserOrSuper)]
public class HidrometrosController : ControllerBase
{
    private readonly IHidrometroService _service;

    public HidrometrosController(IHidrometroService service) => _service = service;

    [HttpGet]
    [Authorize(Policy = Policies.Listar)]
    public async Task<ActionResult<IEnumerable<HidrometroResponseDto>>> Get(CancellationToken cancellationToken) =>
        Ok(await _service.ListarAsync(cancellationToken));

    [HttpGet("dashboard")]
    [Authorize(Policy = Policies.Listar)]
    public async Task<ActionResult<HidrometroDashboardResponseDto>> Dashboard(CancellationToken cancellationToken) =>
        Ok(await _service.ObterDashboardAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Listar)]
    public async Task<ActionResult<HidrometroResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(await _service.BuscarPorIdAsync(id, cancellationToken)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPost]
    [Authorize(Policy = Policies.Cadastrar)]
    public async Task<ActionResult<HidrometroResponseDto>> Create(HidrometroRequestDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var leitura = await _service.CriarAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = leitura.Id }, leitura);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Editar)]
    public async Task<ActionResult<HidrometroResponseDto>> Update(Guid id, HidrometroRequestDto dto, CancellationToken cancellationToken)
    {
        try { return Ok(await _service.AtualizarAsync(id, dto, cancellationToken)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Deletar)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try { await _service.ExcluirAsync(id, cancellationToken); return NoContent(); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
