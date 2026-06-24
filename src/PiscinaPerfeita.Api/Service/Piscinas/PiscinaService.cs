using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Repository.Piscinas;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Service.Piscinas
{
    public class PiscinaService : IPiscinaService
    {
        private readonly IPiscinaRepository _piscinaRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public PiscinaService(IPiscinaRepository piscinaRepository, IUsuarioRepository usuarioRepository)
        {
            _piscinaRepository = piscinaRepository ?? throw new ArgumentNullException(nameof(piscinaRepository));
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        }


        // Implementação dos métodos do serviço
        // Metodo Show: Retorna uma lista de todos as piscinas.
        public async Task<List<PiscinaResponseDto>> Show()
        {
            return await _piscinaRepository.Show();
        }


        // Metodo GetById: Retorna uma piscina específico com base no ID.
        public async Task<PiscinaResponseDto> GetById(Guid id)
        {
            var piscina = await _piscinaRepository.GetById(id);

            if(piscina == null)
            {
                throw new KeyNotFoundException($"Piscina com ID {id} não encontrada.");
            }

            return piscina;
        }


        // Metodo Create: Cria um novo piscina com base nos dados fornecidos.
        public async Task<PiscinaResponseDto> Create(PiscinaRequestDto dto)
        {
            var usuario = await _usuarioRepository.GetById(dto.UsuarioId);

            if(usuario == null)
            {
                throw new KeyNotFoundException($"Não foi possível criar a piscina usuario{dto.UsuarioId} não existe.");
            }

            var piscina = new Piscina
            {
                Nome = dto.Nome,
                VolumeLitros = dto.VolumeLitros,
                ProfundidadeMedia = dto.ProfundidadeMedia,
                UsuarioId = usuario.Id
            };

            await _piscinaRepository.Create(piscina);


            return new PiscinaResponseDto
            {
                Nome = piscina.Nome,
                VolumeLitros = piscina.VolumeLitros,
                ProfundidadeMedia = piscina.ProfundidadeMedia
            };

        }


        // Metodo Update: Atualiza uma piscina existente com base no ID e nos dados fornecidos.
        public async Task<PiscinaResponseDto> Update(Guid id, PiscinaRequestDto dto)
        {
            var piscinaDb = await _piscinaRepository.GetById(id);
            if (piscinaDb == null)
            {
                throw new KeyNotFoundException($"Estoque com id {id} não encontrado.");
            }

            var piscinaUpdated = new Piscina
            {
                Id = id,
                Nome = dto.Nome,
                VolumeLitros = dto.VolumeLitros,
                ProfundidadeMedia = dto.ProfundidadeMedia
            };

            await _piscinaRepository.Update(id, piscinaUpdated);


            return new PiscinaResponseDto
            {
                Id = piscinaUpdated.Id,
                Nome = piscinaUpdated.Nome,
                VolumeLitros = piscinaUpdated.VolumeLitros,
                ProfundidadeMedia = piscinaUpdated.ProfundidadeMedia
            };
        }


        // Metodo Delete: Exclui uma piscina existente com base no ID.
        public async Task Delete(Guid id)
        {
            var piscinaDb = await _piscinaRepository.GetById(id);
            if (piscinaDb == null)
            {
                throw new KeyNotFoundException($"Estoque com id {id} não encontrado.");
            }

            await _piscinaRepository.Delete(id);
        }
    }
}