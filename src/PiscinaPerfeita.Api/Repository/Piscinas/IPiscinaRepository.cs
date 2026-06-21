using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Piscinas
{
    public interface IPiscinaRepository
    {

        Task<List<Piscina>> Show();

        Task<Piscina> GetById(Guid id);

        Task Create(Piscina piscina);

        Task Update(Guid id, Piscina piscina);

        Task Delete(Guid id);
    }
}
