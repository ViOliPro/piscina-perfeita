using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Hidrometros
{
    public interface IHidrometroRepository
    {
        Task<List<HidrometroResponseDto>> Show(
            DateTimeOffset? dataInicio = null,
            DateTimeOffset? dataFim = null
        );

        Task<HidrometroResponseDto?> GetById(Guid id);

        Task Create(Hidrometro hidrometro);

        Task Update(Guid id, Hidrometro hidrometro);

        Task Delete(Guid id);
    }
}
