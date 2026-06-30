using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Piscinas
{
    public interface IPiscinaService
    {
        Task<List<PiscinaResponseDto>> Show();
        Task<PiscinaResponseDto> GetById(Guid id);
        Task<PiscinaResponseDto> Create(PiscinaRequestDto dto);
        Task<PiscinaResponseDto> Update(Guid id, PiscinaRequestDto dto);
        Task Delete(Guid id);

    }
}
