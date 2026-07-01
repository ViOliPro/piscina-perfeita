using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Repository.Produtos;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Service.Produtos
{
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _produtoRepository;

        public ProdutoService(IProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository ?? throw new ArgumentNullException(nameof(produtoRepository));
        }


        // Implementação dos métodos do serviço
        // Metodo Show: Retorna uma lista de todos os estoques, incluindo as informações relacionadas de piscina e produto.
        public async Task<List<ProdutoResponseDto>> Show()
        {
            var produtos = await _produtoRepository.Show();
            return [.. produtos.Select(u => new ProdutoResponseDto
            {
                Id = u.Id,
                Nome = u.Nome,
                UnidadeMedida = u.UnidadeMedida
            })];
        }


        // Metodo GetById: Retorna um estoque específico com base no ID, incluindo as informações relacionadas de piscina e produto.
        public async Task<ProdutoResponseDto> GetById(Guid id)
        {
            var produtos = await _produtoRepository.GetById(id);

            if (produtos == null)
                throw new KeyNotFoundException($"Produto com id {id} não encontrado.");

            return new ProdutoResponseDto
            {
                Id = produtos.Id,
                Nome = produtos.Nome,
                UnidadeMedida = produtos.UnidadeMedida
            };
        }


        // Metodo Create: Cria um novo estoque com base nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<ProdutoResponseDto> Create(ProdutoRequestDto dto)
        {
            var produtos = new Produto
            {
                Nome = dto.Nome,
                UnidadeMedida = dto.UnidadeMedida
            };

            await _produtoRepository.Create(produtos);


            return new ProdutoResponseDto
            {
                Id = produtos.Id,
                Nome = produtos.Nome,
                UnidadeMedida = produtos.UnidadeMedida
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
            var u = new Produto
            {
                Nome = dto.Nome,
                UnidadeMedida = dto.UnidadeMedida
            };

            await _produtoRepository.Update(id, u);

            return new ProdutoResponseDto
            {
                Id = id,
                Nome = u.Nome,
                UnidadeMedida = u.UnidadeMedida
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
