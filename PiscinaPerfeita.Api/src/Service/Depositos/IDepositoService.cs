using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Depositos
{
    public interface IDepositoService
    {
        Task<List<DepositoResponseDto>> Show();
        Task<DepositoResponseDto> GetById(Guid id);
        Task<DepositoResponseDto> Create(DepositoRequestDto dto);
        Task<DepositoResponseDto> Update(Guid id, DepositoRequestDto dto);
        Task Delete(Guid id);
    }
}
