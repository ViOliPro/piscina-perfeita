using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Repository.Produtos
{
    public interface IProdutoRepository
    {

        Task<List<ProdutoResponseDto>> Show();

        Task<ProdutoResponseDto?> GetById(Guid id);

        Task Create(Produto produto);

        Task Update(Guid id, Produto produto);

        Task Delete(Guid id);
    }
}
