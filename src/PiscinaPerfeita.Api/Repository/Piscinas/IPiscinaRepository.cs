using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Repository.Piscinas
{
    public interface IPiscinaRepository
    {

        Task<List<PiscinaResponseDto>> Show();

        Task<PiscinaResponseDto?> GetById(Guid id);

        Task Create(Piscina piscina);

        Task Update(Guid id, Piscina piscina);

        Task Delete(Guid id);
    }
}
