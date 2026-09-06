using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Piscinas
{
    public interface IPiscinaService
    {
        Task<List<PiscinaResponseDto>> Show();
        Task<PiscinaResponseDto> GetById(Guid id);
        Task<PiscinaDashboardResponseDto> GetDashboard(
            Guid piscinaId,
            DateTimeOffset? inicio,
            DateTimeOffset? fim,
            int limitAnalises = 10,
            int limitMovimentacoes = 10
        );
        Task<PiscinaResponseDto> Create(PiscinaRequestDto dto);
        Task<PiscinaResponseDto> Update(Guid id, PiscinaRequestDto dto);
        Task Delete(Guid id);
    }
}
