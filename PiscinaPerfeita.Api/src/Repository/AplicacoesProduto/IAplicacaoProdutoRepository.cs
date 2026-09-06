using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository.AplicacoesProduto
{
    public interface IAplicacaoProdutoRepository
    {
        Task<List<AplicacaoProdutoResponseDto>> Show(
            DateTimeOffset? dataInicio = null,
            DateTimeOffset? dataFim = null,
            Guid? piscinaId = null,
            int? limit = null
        );

        Task<AplicacaoProdutoResponseDto?> GetById(Guid id);

        /// <summary>
        /// Linhas brutas para agregação de uso de produtos (conversão de
        /// unidade é feita no Service com ConversorUnidade).
        /// </summary>
        Task<List<AplicacaoUsoRaw>> ListarParaUsoProdutos(
            Guid piscinaId,
            DateTimeOffset inicio,
            DateTimeOffset fim
        );

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

    /// <summary>Projeção mínima para agregar uso por produto no Service.</summary>
    public class AplicacaoUsoRaw
    {
        public Guid ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string UnidadeMedidaProduto { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public string UnidadeLancamento { get; set; } = string.Empty;
    }
}
