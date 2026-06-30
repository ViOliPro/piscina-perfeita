using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Estoques
{
    public interface IEstoqueService
    {
        Task<List<EstoqueResponseDto>> Show();
        Task<EstoqueResponseDto> GetById(Guid id);
        Task<EstoqueResponseDto> Create(EstoqueRequestDto dto);
        Task<EstoqueResponseDto> Update(Guid id, EstoqueRequestDto dto);
        Task Delete(Guid id);

    }
}
