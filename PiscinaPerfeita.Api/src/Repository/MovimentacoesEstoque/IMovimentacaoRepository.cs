using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.MovimentacoesEstoque
{
    public interface IMovimentacaoRepository
    {
        Task<List<MovimentacaoEstoqueResponseDto>> Show();

        Task<MovimentacaoEstoqueResponseDto?> GetById(Guid id);

        Task Create(MovimentacaoEstoque movimentacao);

        Task Update(Guid id, MovimentacaoEstoque movimentacao);

        Task Delete(Guid id);
    }
}
