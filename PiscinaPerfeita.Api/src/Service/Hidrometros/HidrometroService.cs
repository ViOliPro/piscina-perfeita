using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Analises;
using PiscinaPerfeita.Api.Repository.Hidrometros;
using PiscinaPerfeita.Api.Repository.Usuarios;

namespace PiscinaPerfeita.Api.Service.Hidrometros
{
    public class HidrometroService : IHidrometroService
    {
        private readonly IHidrometroRepository _hidrometroRepository;

        public HidrometroService(IHidrometroRepository hidrometrosRepository)
        {
            _hidrometroRepository =
                hidrometrosRepository
                ?? throw new ArgumentNullException(nameof(hidrometrosRepository));
        }

        // Implementação dos métodos do serviço
        // Metodo Show: Retorna uma lista de todos os estoques, incluindo as informações relacionadas de piscina e produto.
        public async Task<List<HidrometroResponseDto>> Show()
        {
            return await _hidrometroRepository.Show();
        }

        // Metodo GetById: Retorna um estoque específico com base no ID, incluindo as informações relacionadas de piscina e produto.
        public async Task<HidrometroResponseDto> GetById(Guid id)
        {
            var hidrometroDb = await _hidrometroRepository.GetById(id);

            return hidrometroDb == null
                ? throw new KeyNotFoundException(
                    $"Não possivel localizar um hidrometro com o id {id} informado"
                )
                : hidrometroDb;
        }

        // Metodo Create: Cria um novo Hidrometro com base nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<HidrometroResponseDto> Create(HidrometroRequestDto dto)
        {
            var hidrometro = new Hidrometro
            {
                Consumo =
                    dto.Consumo == 0
                        ? throw new ArgumentException("O consumo deve ser maior que zero.")
                        : dto.Consumo,
                CriadoEm = dto.CriadoEm ?? DateTimeOffset.UtcNow,
            };

            await _hidrometroRepository.Create(hidrometro);

            return new HidrometroResponseDto
            {
                Id = hidrometro.Id,
                Consumo = hidrometro.Consumo ?? 0,
                CriadoEm = hidrometro.CriadoEm,
            };
        }

        // Metodo Update: Atualiza um estoque existente com base no ID e nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<HidrometroResponseDto> Update(Guid id, HidrometroRequestDto dto)
        {
            var hidrometrosDb = await _hidrometroRepository.GetById(id);
            if (hidrometrosDb == null)
            {
                throw new KeyNotFoundException($"Hidrometro com id {id} não encontrado.");
            }

            var hidrometrosUpdated = new Hidrometro
            {
                Id = id,
                Consumo =
                    dto.Consumo == 0
                        ? throw new ArgumentException("O consumo deve ser maior que zero.")
                        : dto.Consumo,
                CriadoEm = dto.CriadoEm ?? DateTimeOffset.UtcNow,
            };

            await _hidrometroRepository.Update(id, hidrometrosUpdated);

            return new HidrometroResponseDto
            {
                Id = hidrometrosUpdated.Id,
                Consumo = hidrometrosUpdated.Consumo ?? 0,
                CriadoEm = hidrometrosUpdated.CriadoEm,
            };
        }

        // Metodo Delete: Exclui um estoque existente com base no ID.
        public async Task Delete(Guid id)
        {
            var hidrometrosDb = await _hidrometroRepository.GetById(id);
            if (hidrometrosDb == null)
            {
                throw new KeyNotFoundException($"Hidrometro com id {id} não encontrado.");
            }

            await _hidrometroRepository.Delete(id);
        }
    }
}
