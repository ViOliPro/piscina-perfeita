using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Analises
{
    public interface IAnaliseRepository
    {

        Task<List<Analise>> Show();

        Task<Analise> GetById(Guid id);

        Task Create(Analise analise);

        Task Update(Guid id, Analise analise);

        Task Delete(Guid id);
    }
}
