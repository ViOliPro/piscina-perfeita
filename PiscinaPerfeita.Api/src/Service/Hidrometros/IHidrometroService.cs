using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Hidrometros
{
    public interface IHidrometroService
    {
        Task<List<HidrometroResponseDto>> Show();
        Task<HidrometroResponseDto> GetById(Guid id);
        Task<HidrometroResponseDto> Create(HidrometroRequestDto dto);
        Task<HidrometroResponseDto> Update(Guid id, HidrometroRequestDto dto);
        Task Delete(Guid id);
    }
}
