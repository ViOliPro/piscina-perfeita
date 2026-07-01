using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Repository.Estoques
{
    public interface IEstoqueRepository
    {

        Task<List<EstoqueResponseDto>> Show();

        Task<EstoqueResponseDto?> GetById(Guid id);

        Task Create(Estoque estoque);

        Task Update(Guid id, Estoque estoque);

        Task Delete(Guid id);
    }
}
