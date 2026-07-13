using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.MovimentacoesEstoque
{
    public interface IMovimentacaoService
    {
        Task<List<MovimentacaoEstoqueResponseDto>> Show();
        Task<MovimentacaoEstoqueResponseDto?> GetById(Guid id);
        Task<MovimentacaoEstoqueResponseDto> Create(MovimentacaoEstoqueRequestDto dto);
        Task<MovimentacaoEstoqueResponseDto> Update(Guid id, MovimentacaoEstoqueRequestDto dto);
        Task Delete(Guid id);

        // Feature de contagem física / Ajuste de Inventário: recebe a
        // contagem de vários produtos de um Depósito de uma vez e gera as
        // MovimentacoesEstoque de ajuste necessárias.
        Task<List<ContagemInventarioResultadoDto>> RegistrarContagemInventario(
            ContagemInventarioRequestDto dto
        );
    }
}
