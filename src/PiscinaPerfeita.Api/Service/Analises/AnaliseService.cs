using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Service.Analises
{
    public class AnaliseService : IAnaliseService
    {
        private readonly IAnaliseService _analiseRepository;

        public AnaliseService(IAnaliseService analisesRepository)
        {
            _analiseRepository = analisesRepository ?? throw new ArgumentNullException(nameof(analisesRepository));
        }


        // Implementação dos métodos do serviço
        // Metodo Show: Retorna uma lista de todos os estoques, incluindo as informações relacionadas de piscina e produto.
        public async Task<List<AnaliseResponseDto>> Show()
        {
            var analises = await _analiseRepository.Show();
            return analises.Select(u => new AnaliseResponseDto
            {
                Id = u.Id,
                PiscinaId = u.PiscinaId,
                DataAnalise = u.DataAnalise,
                Ph = u.Ph,
                CloroLivre = u.CloroLivre,
                Alcalinidade = u.Alcalinidade,
                Temperatura = u.Temperatura,
                Observacoes = u.Observacoes
            }).ToList();
        }


        // Metodo GetById: Retorna um estoque específico com base no ID, incluindo as informações relacionadas de piscina e produto.
        public async Task<AnaliseResponseDto> GetById(Guid id)
        {
            var analises = await _analiseRepository.GetById(id);
            return new AnaliseResponseDto
            {
                Id = analises.Id,
                PiscinaId = analises.PiscinaId,
                DataAnalise = analises.DataAnalise,
                Ph = analises.Ph,
                CloroLivre = analises.CloroLivre,
                Alcalinidade = analises.Alcalinidade,
                Temperatura = analises.Temperatura,
                Observacoes = analises.Observacoes
            };
        }


        // Metodo Create: Cria um novo estoque com base nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<AnaliseResponseDto> Create(AnaliseRequestDto dto)
        {
            var analiseId = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
            var analise = new AnaliseRequestDto
            {
                Id = analiseId,
                PiscinaId = dto.PiscinaId,
                DataAnalise = dto.DataAnalise,
                Ph = dto.Ph,
                CloroLivre = dto.CloroLivre,
                Alcalinidade = dto.Alcalinidade,
                Temperatura = dto.Temperatura,
                Observacoes = dto.Observacoes
            };

            await _analiseRepository.Create(analise);


            return new AnaliseResponseDto
            {
                Id = analise.Id,
                PiscinaId = analise.PiscinaId,
                DataAnalise = analise.DataAnalise,
                Ph = analise.Ph,
                CloroLivre = analise.CloroLivre,
                Alcalinidade = analise.Alcalinidade,
                Temperatura = analise.Temperatura,
                Observacoes = analise.Observacoes
            };
        }


        // Metodo Update: Atualiza um estoque existente com base no ID e nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<AnaliseResponseDto> Update(Guid id, AnaliseRequestDto dto)
        {
            var analisesDb = await _analiseRepository.GetById(id);
            if (analisesDb == null)
            {
                throw new KeyNotFoundException($"Estoque com id {id} não encontrado.");
            }

            var analisesUpdated = new AnaliseRequestDto
            {
                Id = id,
                PiscinaId = dto.PiscinaId,
                DataAnalise = dto.DataAnalise,
                Ph = dto.Ph,
                CloroLivre = dto.CloroLivre,
                Alcalinidade = dto.Alcalinidade,
                Temperatura = dto.Temperatura,
                Observacoes = dto.Observacoes
            };

            await _analiseRepository.Update(id, analisesUpdated);


            return new AnaliseResponseDto
            {
                Id = analisesUpdated.Id,
                PiscinaId = analisesUpdated.PiscinaId,
                DataAnalise = analisesUpdated.DataAnalise,
                Ph = analisesUpdated.Ph,
                CloroLivre = analisesUpdated.CloroLivre,
                Alcalinidade = analisesUpdated.Alcalinidade,
                Temperatura = analisesUpdated.Temperatura,
                Observacoes = analisesUpdated.Observacoes
            };
        }


        // Metodo Delete: Exclui um estoque existente com base no ID.
        public async Task Delete(Guid id)
        {
            var analisesDb = await _analiseRepository.GetById(id);
            if (analisesDb == null)
            {
                throw new KeyNotFoundException($"Estoque com id {id} não encontrado.");
            }

            await _analiseRepository.Delete(id);

        }
    }
}
