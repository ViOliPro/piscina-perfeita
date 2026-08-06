using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Hidrometros;

public interface IHidrometroService
{
    Task<List<HidrometroResponseDto>> ListarAsync(CancellationToken cancellationToken = default);
    Task<HidrometroResponseDto> BuscarPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<HidrometroDashboardResponseDto> ObterDashboardAsync(CancellationToken cancellationToken = default);
    Task<HidrometroResponseDto> CriarAsync(HidrometroRequestDto dto, CancellationToken cancellationToken = default);
    Task<HidrometroResponseDto> AtualizarAsync(Guid id, HidrometroRequestDto dto, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
}
