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

        // Localiza o registro de Estoque (entidade rastreada pelo EF) de um
        // Produto dentro de um Depósito específico — usado internamente por
        // MovimentacaoService/AplicacaoProdutoService para dar baixa/alta no
        // saldo. Retorna null se o produto ainda não tem saldo cadastrado
        // nesse depósito.
        Task<Estoque?> GetEntidadeByProdutoEDeposito(Guid produtoId, Guid depositoId);
    }
}
