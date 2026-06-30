using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Estoques;
using PiscinaPerfeita.Api.Helpers.Authenticated;

namespace PiscinaPerfeita.Api.Service.Estoques
{
    public class EstoqueService : IEstoqueService
    {
        private readonly IEstoqueRepository _estoqueRepository;
        private readonly IAuthenticatedUser _user;

        public EstoqueService(IEstoqueRepository estoqueRepository, IAuthenticatedUser user)
        {
            _estoqueRepository = estoqueRepository ?? throw new ArgumentNullException(nameof(estoqueRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        // Implementação dos métodos do serviço
        // Cada método chama o repositório correspondente e transforma os dados em DTOs de resposta
        public async Task<List<EstoqueResponseDto>> Show()
        {
            var estoques = await _estoqueRepository.Show();
            return estoques.Select(u => new EstoqueResponseDto
            {
                Id = u.Id,
                PiscinaId = u.PiscinaId,
                ProdutoId = u.ProdutoId,
                QuantidadeAtual = u.QuantidadeAtual,
            }).ToList();
        }


        // O método GetById busca um estoque específico pelo ID e retorna um DTO de resposta
        public async Task<EstoqueResponseDto> GetById(Guid id)
        {
            var estoque = await _estoqueRepository.GetById(id);
            return new EstoqueResponseDto
            {
                Id = estoque.Id,
                PiscinaId = estoque.PiscinaId,
                ProdutoId = estoque.ProdutoId,
                QuantidadeAtual = estoque.QuantidadeAtual,
            };
        }


        // O método Create recebe um DTO de requisição, cria um novo estoque e retorna um DTO de resposta
        public async Task<EstoqueResponseDto> Create(EstoqueRequestDto dto)
        {
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
                PiscinaId = estoque.PiscinaId,
                ProdutoId = estoque.ProdutoId,
                QuantidadeAtual = estoque.QuantidadeAtual
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

            estoqueDb.PiscinaId = dto.PiscinaId;
            estoqueDb.ProdutoId = dto.ProdutoId;
            estoqueDb.QuantidadeAtual = dto.QuantidadeAtual;

            await _estoqueRepository.Update(id, estoqueDb);

            return new EstoqueResponseDto
            {
                Id = estoqueDb.Id,
                PiscinaId = estoqueDb.PiscinaId,
                ProdutoId = estoqueDb.ProdutoId,
                QuantidadeAtual = estoqueDb.QuantidadeAtual
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
