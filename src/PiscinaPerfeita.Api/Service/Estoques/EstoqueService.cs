using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Dtos.Request;

namespace PiscinaPerfeita.Api.Service.Estoques
{
    public class EstoqueService : IEstoqueService
    {
        private readonly IEstoqueService _estoqueRepository;

        public EstoqueService(IEstoqueService estoquesRepository)
        {
            _estoqueRepository = estoquesRepository ?? throw new ArgumentNullException(nameof(estoquesRepository));
        }


        // Implementação dos métodos do serviço
        // Metodo Show: Retorna uma lista de todos os estoques, incluindo as informações relacionadas de piscina e produto.
        public async Task<List<EstoqueResponseDto>> Show()
        {
            var estoques = await _estoqueRepository.Show();
            return estoques.Select(u => new EstoqueResponseDto
            {
                PiscinaId = u.PiscinaId,
                ProdutoId = u.ProdutoId,
                QuantidadeAtual = u.QuantidadeAtual,
            }).ToList();
        }


        // Metodo GetById: Retorna um estoque específico com base no ID, incluindo as informações relacionadas de piscina e produto.
        public async Task<EstoqueResponseDto> GetById(Guid id)
        {
            var estoques = await _estoqueRepository.GetById(id);
            return new EstoqueResponseDto
            {
                PiscinaId = estoques.PiscinaId,
                ProdutoId = estoques.ProdutoId,
                QuantidadeAtual = estoques.QuantidadeAtual,
            };
        }


        // Metodo Create: Cria um novo estoque com base nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<EstoqueResponseDto> Create(EstoqueRequestDto dto)
        {
            var estoqueId = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
            var estoque = new EstoqueRequestDto
            {
                Id = estoqueId,
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                QuantidadeAtual = dto.QuantidadeAtual,
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


        // Metodo Update: Atualiza um estoque existente com base no ID e nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<EstoqueResponseDto> Update(Guid id, EstoqueRequestDto dto)
        {
            var estoqueDb = await _estoqueRepository.GetById(id);
            if (estoqueDb == null)
            {
                throw new KeyNotFoundException($"Estoque com id {id} não encontrado.");
            }

            var estoqueUpdated = new EstoqueRequestDto
            {
                Id = id,
                PiscinaId = dto.PiscinaId,
                ProdutoId = dto.ProdutoId,
                QuantidadeAtual = dto.QuantidadeAtual,
            };

            await _estoqueRepository.Update(id, estoqueUpdated);

            return new EstoqueResponseDto
            {
                Id = estoqueDb.Id,
                PiscinaId = estoqueDb.PiscinaId,
                ProdutoId = estoqueDb.ProdutoId,
                QuantidadeAtual = estoqueDb.QuantidadeAtual
            };
        }


        // Metodo Delete: Exclui um estoque existente com base no ID.
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
