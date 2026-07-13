using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Depositos
{
    public interface IDepositoRepository
    {
        Task<List<DepositoResponseDto>> Show();
        Task<DepositoResponseDto?> GetById(Guid id);
        Task Create(Deposito deposito);
        Task Update(Guid id, Deposito deposito);
        Task Delete(Guid id);
    }
}
