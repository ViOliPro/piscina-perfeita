using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Locais
{
    public interface ILocalService
    {
        Task<List<LocalResponseDto>> Show();
        Task<LocalResponseDto> GetById(Guid id);
        Task<LocalResponseDto> Create(LocalRequestDto dto);
        Task<LocalResponseDto> Update(Guid id, LocalRequestDto dto);
        Task Delete(Guid id);
    }
}
