using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Estoques
{
    public interface IEstoqueRepository
    {

        Task<List<Estoque>> Show();

        Task<Estoque> GetById(Guid id);

        Task Create(Estoque estoque);

        Task Update(Guid id, Estoque estoque);

        Task Delete(Guid id);
    }
}
