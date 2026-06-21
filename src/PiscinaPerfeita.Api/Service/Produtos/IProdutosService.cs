using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Produtos

{
    public interface IProdutosService
    {
        Task<List<ProdutoResponseDto>> Show();
        Task<ProdutoResponseDto> GetById(Guid id);
        Task<ProdutoResponseDto> Create(ProdutoRequestDto dto);
        Task<ProdutoResponseDto> Update(Guid id, ProdutoRequestDto dto);
        Task Delete(Guid id);

    }
}
