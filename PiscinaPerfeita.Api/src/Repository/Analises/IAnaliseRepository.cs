using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Repository.Analises
{
    public interface IAnaliseRepository
    {

        Task<List<AnaliseResponseDto>> Show();

        Task<AnaliseResponseDto?> GetById(Guid id);

        Task Create(Analise analise);

        Task Update(Guid id, Analise analise);

        Task Delete(Guid id);
    }
}
