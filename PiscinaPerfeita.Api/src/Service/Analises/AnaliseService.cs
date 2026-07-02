using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Repository.Analises;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Repository.Piscinas;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Helpers.Authenticated;

namespace PiscinaPerfeita.Api.Service.Analises
{
    public class AnaliseService : IAnaliseService
    {
        private readonly IAnaliseRepository _analiseRepository;
        private readonly IAuthenticatedUser _user;
        private readonly IUsuarioRepository _userRepository;
        private readonly IPiscinaRepository _piscinaRepository;

        public AnaliseService(IAnaliseRepository analisesRepository, IAuthenticatedUser user, IUsuarioRepository userRepository, IPiscinaRepository piscinaRepository)
        {
            _analiseRepository = analisesRepository ?? throw new ArgumentNullException(nameof(analisesRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _piscinaRepository = piscinaRepository ?? throw new ArgumentNullException(nameof(piscinaRepository));
        }


        // Implementação dos métodos do serviço
        // Metodo Show: Retorna uma lista de todos os estoques, incluindo as informações relacionadas de piscina e produto.
        public async Task<List<AnaliseResponseDto>> Show()
        {
            return await _analiseRepository.Show();
        }


        // Metodo GetById: Retorna um estoque específico com base no ID, incluindo as informações relacionadas de piscina e produto.
        public async Task<AnaliseResponseDto> GetById(Guid id)
        {
            var analiseDb = await _analiseRepository.GetById(id);

            if (analiseDb == null)
            {
                throw new KeyNotFoundException($"Não possivel localizar uma analise com o id {id} informado");
            }

            return analiseDb;
        }


        // Metodo Create: Cria um novo Analise com base nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<AnaliseResponseDto> Create(AnaliseRequestDto dto)
        {

            var piscinaDb = await _piscinaRepository.GetById(dto.PiscinaId);
            if (piscinaDb == null)
                throw new KeyNotFoundException("Problemas ao registrar, piscina não localizado");

            var userDb = await _userRepository.GetById(_user.GetUserId());
            if (userDb == null)
                throw new KeyNotFoundException("Problemas ao registrar, usuario ID analise não localizado");




            var analise = new Analise
            {
                PiscinaId = dto.PiscinaId,
                UsuarioId = _user.GetUserId(),
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
                DataAnalise = analise.DataAnalise,
                Ph = analise.Ph,
                CloroLivre = analise.CloroLivre,
                Alcalinidade = analise.Alcalinidade,
                Temperatura = analise.Temperatura,
                Observacoes = analise.Observacoes,
                Piscina = new NomeIdDto(analise.PiscinaId, piscinaDb.Nome),
                Usuario = new NomeIdDto ( analise.UsuarioId, userDb.Nome )
            };
        }


        // Metodo Update: Atualiza um estoque existente com base no ID e nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<AnaliseResponseDto> Update(Guid id, AnaliseRequestDto dto)
        {
            var analisesDb = await _analiseRepository.GetById(id);
            if (analisesDb == null)
            {
                throw new KeyNotFoundException($"Analise com id {id} não encontrado.");
            }

            var analisesUpdated = new Analise
            {
                Id = id,
                PiscinaId = dto.PiscinaId,
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
                DataAnalise = analisesUpdated.DataAnalise,
                Ph = analisesUpdated.Ph,
                CloroLivre = analisesUpdated.CloroLivre,
                Alcalinidade = analisesUpdated.Alcalinidade,
                Temperatura = analisesUpdated.Temperatura,
                Observacoes = analisesUpdated.Observacoes,
                Piscina = new NomeIdDto(analisesUpdated.PiscinaId, null),
                Usuario = new NomeIdDto(analisesUpdated.UsuarioId, null)
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
