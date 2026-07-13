using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.AplicacoesProduto
{
    public interface IAplicacaoProdutoService
    {
        Task<List<AplicacaoProdutoResponseDto>> Show();
        Task<AplicacaoProdutoResponseDto> GetById(Guid id);
        Task<AplicacaoProdutoResponseDto> Create(AplicacaoProdutoRequestDto dto);
    }
}
