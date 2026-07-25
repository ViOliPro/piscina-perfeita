using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PiscinaPerfeita.Api.Authorization;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.Locais;

namespace PiscinaPerfeita.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.UserOrSuper)]
    public class LocaisController : ControllerBase
    {
        private readonly ILocalService _locaisService;

        public LocaisController(ILocalService locaisService)
        {
            _locaisService =
                locaisService ?? throw new ArgumentNullException(nameof(locaisService));
        }

        // GET: api/locais
        // SuperAdmin vê todos os Locais; demais usuários veem só os que estão vinculados.
        [HttpGet]
        [Authorize(Policy = Policies.GerenciarLocal)]
        public async Task<ActionResult<IEnumerable<LocalResponseDto>>> Get()
        {
            try
            {
                var locais = await _locaisService.Show();
                return Ok(locais);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/locais/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = Policies.GerenciarLocal)]
        public async Task<ActionResult<LocalResponseDto>> GetById(Guid id)
        {
            try
            {
                var local = await _locaisService.GetById(id);
                return Ok(local);
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

        // POST: api/locais  (somente SuperAdmin)
        [HttpPost]
        [Authorize(Policy = Policies.GerenciarLocal)]
        public async Task<ActionResult<LocalResponseDto>> Create(LocalRequestDto dto)
        {
            try
            {
                var local = await _locaisService.Create(dto);
                return CreatedAtAction(nameof(GetById), new { id = local.Id }, local);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/locais/{id}  (somente SuperAdmin)
        [HttpPut("{id}")]
        [Authorize(Policy = Policies.GerenciarLocal)]
        public async Task<ActionResult<LocalResponseDto>> Update(Guid id, LocalRequestDto dto)
        {
            try
            {
                var local = await _locaisService.Update(id, dto);
                return CreatedAtAction(nameof(GetById), new { id = local.Id }, local);
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

        // DELETE: api/locais/{id}  (somente SuperAdmin)
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.GerenciarLocal)]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _locaisService.Delete(id);
                return NoContent();
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
    }
}
