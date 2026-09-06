using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Piscinas
{
    public interface IPiscinaRepository
    {
        Task<List<PiscinaResponseDto>> Show();

        Task<PiscinaResponseDto?> GetById(Guid id);

        Task<PiscinaDashboardResponseDto?> GetDashboard(
            Guid piscinaId,
            DateTimeOffset inicio,
            DateTimeOffset fim,
            int limitAnalises,
            int limitMovimentacoes
        );

        Task Create(Piscina piscina);

        Task Update(Guid id, Piscina piscina);

        Task Delete(Guid id);
    }
}
