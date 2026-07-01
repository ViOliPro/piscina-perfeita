using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Estoques;
using PiscinaPerfeita.Api.Repository.Piscinas;
using PiscinaPerfeita.Api.Repository.Produtos;
using PiscinaPerfeita.Api.Helpers.Authenticated;

namespace PiscinaPerfeita.Api.Service.Estoques
{
    public class EstoqueService : IEstoqueService
    {
        private readonly IEstoqueRepository _estoqueRepository;
        private readonly IPiscinaRepository _piscinaRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IAuthenticatedUser _user;

        public EstoqueService(IEstoqueRepository estoqueRepository, IPiscinaRepository piscinaRepository, IProdutoRepository produtoRepository, IAuthenticatedUser user)
        {
            _estoqueRepository = estoqueRepository ?? throw new ArgumentNullException(nameof(estoqueRepository));
            _piscinaRepository = piscinaRepository ?? throw new ArgumentNullException(nameof(piscinaRepository));
            _produtoRepository = produtoRepository ?? throw new ArgumentNullException(nameof(produtoRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        // Implementação dos métodos do serviço
        // Cada método chama o repositório correspondente e transforma os dados em DTOs de resposta
        public async Task<List<EstoqueResponseDto>> Show()
        {
            return await _estoqueRepository.Show();

        }


        // O método GetById busca um estoque específico pelo ID e retorna um DTO de resposta
        public async Task<EstoqueResponseDto> GetById(Guid id)
        {
            var estoque = await _estoqueRepository.GetById(id);

            if (estoque == null)
                throw new KeyNotFoundException($"Não foi encontrado um estoque com o id {id}");

            return estoque;
        }


        // O método Create recebe um DTO de requisição, cria um novo estoque e retorna um DTO de resposta
        public async Task<EstoqueResponseDto> Create(EstoqueRequestDto dto)
        {
            var piscinaDb = await _piscinaRepository.GetById(dto.PiscinaId);
            if (piscinaDb == null)
                throw new KeyNotFoundException($"Não foi encontrada uma piscina com o id {dto.PiscinaId}");

            var produtoDb = await _produtoRepository.GetById(dto.ProdutoId);
            if (produtoDb == null)
                throw new KeyNotFoundException($"Não foi encontrado um produto com o id {dto.ProdutoId}");


            var estoque = new Estoque
            {
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                UsuarioId = _user.GetUserId(),
                QuantidadeAtual = dto.QuantidadeAtual
            };

            
            await _estoqueRepository.Create(estoque);

            return new EstoqueResponseDto
            {
                Id = estoque.Id,
                QuantidadeAtual = estoque.QuantidadeAtual,
                Piscina = new PiscinaEstoque { Id = estoque.PiscinaId, Nome = piscinaDb.Nome},
                Produto = new ProdutoEstoque { Id = estoque.ProdutoId, Nome = produtoDb.Nome, UnidadeMedida = produtoDb.UnidadeMedida}
            };
        }


        // O método Update recebe um ID e um DTO de requisição, atualiza o estoque correspondente e retorna um DTO de resposta
        public async Task<EstoqueResponseDto> Update(Guid id, EstoqueRequestDto dto)
        {
            var estoqueDb = await _estoqueRepository.GetById(id);
            if (estoqueDb == null)
            {
                throw new KeyNotFoundException($"Estoque com id {id} não encontrado.");
            }
            var piscinaDb = await _piscinaRepository.GetById(dto.PiscinaId);
            if (piscinaDb == null)
                throw new KeyNotFoundException($"Não foi encontrada uma piscina com o id {dto.PiscinaId}");

            var produtoDb = await _produtoRepository.GetById(dto.ProdutoId);
            if (produtoDb == null)
                throw new KeyNotFoundException($"Não foi encontrado um produto com o id {dto.ProdutoId}");



            var estoqueUpdated = new Estoque
            {
                Id = id,
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                QuantidadeAtual = dto.QuantidadeAtual
            };

            await _estoqueRepository.Update(id, estoqueUpdated);

            return new EstoqueResponseDto
            {
                Id = estoqueDb.Id,
                QuantidadeAtual = estoqueDb.QuantidadeAtual,
                Piscina = new PiscinaEstoque { Id = estoqueUpdated.PiscinaId, Nome = piscinaDb.Nome },
                Produto = new ProdutoEstoque { Id = estoqueUpdated.ProdutoId, Nome = produtoDb.Nome, UnidadeMedida = produtoDb.UnidadeMedida }
            };
        }


        // O método Delete recebe um ID, verifica se o estoque existe e o exclui
        public async Task Delete(Guid id)
        {
            var estoqueDb = await _estoqueRepository.GetById(id);
            if (estoqueDb == null)
            {
                throw new KeyNotFoundException($"Estoque com id {id} não encontrado.");
            }

            await _estoqueRepository.Delete(id);
        }
    }
}
