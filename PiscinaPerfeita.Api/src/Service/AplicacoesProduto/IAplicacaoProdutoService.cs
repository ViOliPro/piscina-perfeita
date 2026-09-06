using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.AplicacoesProduto
{
    public interface IAplicacaoProdutoService
    {
        Task<List<AplicacaoProdutoResponseDto>> Show(
            DateTimeOffset? dataInicio = null,
            DateTimeOffset? dataFim = null,
            Guid? piscinaId = null,
            int? limit = null
        );

        Task<AplicacaoProdutoResponseDto> GetById(Guid id);

        Task<AplicacaoProdutoResponseDto> Create(AplicacaoProdutoRequestDto dto);

        Task<UsoProdutosResponseDto> ObterUsoProdutos(
            Guid piscinaId,
            DateTimeOffset? inicio,
            DateTimeOffset? fim
        );
    }
}
