using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Locais
{
    public interface ILocalRepository
    {
        Task<List<LocalResponseDto>> Show();

        Task<List<LocalResponseDto>> ShowByIds(IEnumerable<Guid> ids);

        Task<LocalResponseDto?> GetById(Guid id);

        Task Create(Local local);

        Task Update(Guid id, Local local);

        Task Delete(Guid id);
    }
}
