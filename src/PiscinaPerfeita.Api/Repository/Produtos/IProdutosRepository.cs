using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Produtos
{
    public interface IProdutoRepository
    {

        Task<List<Produto>> Show();

        Task<Produto> GetById(Guid id);

        Task Create(Produto produto);

        Task Update(Guid id, Produto produto);

        Task Delete(Guid id);
    }
}
