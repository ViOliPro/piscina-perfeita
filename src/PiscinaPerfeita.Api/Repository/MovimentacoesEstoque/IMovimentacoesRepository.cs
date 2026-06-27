using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.Movimentacoes
{
    public interface IMovimentacoesRepository
    {

        Task<List<MovimentacoesEstoque>> Show();

        Task<MovimentacoesEstoque> GetById(Guid id);

        Task Create(MovimentacoesEstoque movimentacao);

        Task Update(Guid id, MovimentacoesEstoque movimentacao);

        Task Delete(Guid id);
    }
}
