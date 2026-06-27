using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.MovimentacoesEstoque
{
    public interface IMovimentacaoRepository
    {

        Task<List<MovimentacaoEstoque>> Show();

        Task<MovimentacaoEstoque> GetById(Guid id);

        Task Create(MovimentacaoEstoque movimentacao);

        Task Update(Guid id, MovimentacaoEstoque movimentacao);

        Task Delete(Guid id);
    }
}
