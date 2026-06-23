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
        // Metodo Show: Retorna uma lista de todos os estoques, incluindo as informações relacionadas de piscina e produto.
        public async Task<List<PiscinaResponseDto>> Show()
        {
            var piscinas = await _piscinaRepository.Show();
            return piscinas.Select(u => new PiscinaResponseDto
            {
                Id = u.Id,
                Nome = u.Nome,
                VolumeLitros = u.VolumeLitros,
                ProfundidadeMedia = u.ProfundidadeMedia
            }).ToList();
        }


        // Metodo GetById: Retorna um estoque específico com base no ID, incluindo as informações relacionadas de piscina e produto.
        public async Task<PiscinaResponseDto> GetById(Guid id)
        {
            var u = await _piscinaRepository.GetById(id);
            return new PiscinaResponseDto
            {
                Id = u.Id,
                Nome = u.Nome,
                VolumeLitros = u.VolumeLitros,
                ProfundidadeMedia = u.ProfundidadeMedia
            };
        }


        // Metodo Create: Cria um novo estoque com base nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
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


        // Metodo Update: Atualiza um estoque existente com base no ID e nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
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


        // Metodo Delete: Exclui um estoque existente com base no ID.
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