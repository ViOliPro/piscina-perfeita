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

        // Grava a movimentação E aplica a nova quantidade no Estoque em UMA
        // ÚNICA chamada a SaveChangesAsync — ou os dois persistem, ou
        // nenhum. É assim que a feature "atualizar estoque com base nas
        // movimentações" garante consistência (sem isso, uma falha entre os
        // dois passos deixaria o saldo do Estoque desalinhado do histórico).
        Task CreateComAtualizacaoEstoque(
            MovimentacaoEstoque movimentacao,
            Guid estoqueId,
            decimal novaQuantidadeEstoque
        );
    }
}
