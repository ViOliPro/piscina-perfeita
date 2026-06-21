using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Analises
{
    public interface IProdutosService
    {
        Task<List<AnaliseResponseDto>> Show();
        Task<AnaliseResponseDto> GetById(Guid id);
        Task<AnaliseResponseDto> Create(AnaliseRequestDto dto);
        Task<AnaliseResponseDto> Update(Guid id, AnaliseRequestDto dto);
        Task Delete(Guid id);

    }
}
