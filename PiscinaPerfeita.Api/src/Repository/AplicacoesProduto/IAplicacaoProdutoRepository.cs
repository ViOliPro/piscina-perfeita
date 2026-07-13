using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.AplicacoesProduto
{
    public interface IAplicacaoProdutoRepository
    {
        Task<List<AplicacaoProdutoResponseDto>> Show();

        Task<AplicacaoProdutoResponseDto?> GetById(Guid id);

        // Grava a AplicacaoProduto, a MovimentacaoEstoque gerada por ela e a
        // atualização do saldo do Estoque — tudo em uma única transação
        // implícita do EF Core (um SaveChangesAsync). É a garantia de que
        // "salvar a aplicação" e "atualizar o estoque" nunca ficam
        // dessincronizados entre si.
        Task Create(
            AplicacaoProduto aplicacao,
            MovimentacaoEstoque movimentacao,
            Guid estoqueId,
            decimal novaQuantidadeEstoque
        );
    }
}
