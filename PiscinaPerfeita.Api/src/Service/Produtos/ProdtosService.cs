using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Produtos;

namespace PiscinaPerfeita.Api.Service.Produtos
{
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IAuthenticatedUser _user;

        public ProdutoService(IProdutoRepository produtoRepository, IAuthenticatedUser user)
        {
            _produtoRepository =
                produtoRepository ?? throw new ArgumentNullException(nameof(produtoRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        // Implementação dos métodos do serviço
        // Metodo Show: Retorna uma lista de todos os estoques, incluindo as informações relacionadas de piscina e produto.
        public async Task<List<ProdutoResponseDto>> Show()
        {
            return await _produtoRepository.Show();

        }

        // Metodo GetById: Retorna um estoque específico com base no ID, incluindo as informações relacionadas de piscina e produto.
        public async Task<ProdutoResponseDto> GetById(Guid id)
        {
            var produtos = await _produtoRepository.GetById(id);

            if (produtos == null)
                throw new KeyNotFoundException($"Produto com id {id} não encontrado.");

            return produtos;
        }

        // Metodo Create: Cria um novo estoque com base nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<ProdutoResponseDto> Create(ProdutoRequestDto dto)
        {
            var produtos = new Produto { Nome = dto.Nome, UnidadeMedida = dto.UnidadeMedida };

            await _produtoRepository.Create(produtos);

            return new ProdutoResponseDto
            {
                Id = produtos.Id,
                Nome = produtos.Nome,
                UnidadeMedida = produtos.UnidadeMedida,
                Fabricante = dto.Fabricante ?? null,
                Marca = dto.Marca ?? null,
                Observacoes = dto.Observacoes ?? null,
                Categoria = dto.Categoria ?? null
            };
        }

        // Metodo Update: Atualiza um estoque existente com base no ID e nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<ProdutoResponseDto> Update(Guid id, ProdutoRequestDto dto)
        {
            var produtosDb = await _produtoRepository.GetById(id);
            if (produtosDb == null)
            {
                throw new KeyNotFoundException($"Produto com id {id} não encontrado.");
            }
            var u = new Produto { Nome = dto.Nome, UnidadeMedida = dto.UnidadeMedida };

            await _produtoRepository.Update(id, u);

            return new ProdutoResponseDto
            {
                Id = id,
                Nome = u.Nome,
                UnidadeMedida = u.UnidadeMedida,
                Fabricante = dto.Fabricante,
                Marca = dto.Marca,
                Observacoes = dto.Observacoes,
                Categoria = dto.Categoria
            };
        }

        // Metodo Delete: Exclui um estoque existente com base no ID.
        public async Task Delete(Guid id)
        {
            var produtosDb = await _produtoRepository.GetById(id);
            if (produtosDb == null)
            {
                throw new KeyNotFoundException($"Estoque com id {id} não encontrado.");
            }

            await _produtoRepository.Delete(id);
        }
    }
}
