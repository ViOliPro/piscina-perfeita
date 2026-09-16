using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Piscinas;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Service.Audit;

namespace PiscinaPerfeita.Api.Service.Piscinas
{
    public class PiscinaService : IPiscinaService
    {
        private readonly IPiscinaRepository _piscinaRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAuthenticatedUser _user;
        private readonly IAuditService _audit;

        public PiscinaService(
            IPiscinaRepository piscinaRepository,
            IUsuarioRepository usuarioRepository,
            IAuthenticatedUser user,
            IAuditService audit
        )
        {
            _piscinaRepository =
                piscinaRepository ?? throw new ArgumentNullException(nameof(piscinaRepository));
            _usuarioRepository =
                usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        }

        public async Task<List<PiscinaResponseDto>> Show()
        {
            return await _piscinaRepository.Show();
        }

        public async Task<PiscinaResponseDto> GetById(Guid id)
        {
            var piscina = await _piscinaRepository.GetById(id);

            if (piscina == null)
            {
                throw new KeyNotFoundException($"Piscina com ID {id} não encontrada.");
            }

            return piscina;
        }

        public async Task<PiscinaDashboardResponseDto> GetDashboard(
            Guid piscinaId,
            DateTimeOffset? inicio,
            DateTimeOffset? fim,
            int limitAnalises = 10,
            int limitMovimentacoes = 10
        )
        {
            var agora = DateTimeOffset.UtcNow;
            var inicioEfetivo =
                inicio ?? new DateTimeOffset(agora.Year, agora.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var fimEfetivo = fim ?? agora;

            if (limitAnalises <= 0)
                limitAnalises = 10;
            if (limitAnalises > 50)
                limitAnalises = 50;
            if (limitMovimentacoes <= 0)
                limitMovimentacoes = 10;
            if (limitMovimentacoes > 50)
                limitMovimentacoes = 50;

            var dashboard = await _piscinaRepository.GetDashboard(
                piscinaId,
                inicioEfetivo,
                fimEfetivo,
                limitAnalises,
                limitMovimentacoes
            );

            if (dashboard == null)
            {
                throw new KeyNotFoundException($"Piscina com ID {piscinaId} não encontrada.");
            }

            return dashboard;
        }

        public async Task<PiscinaResponseDto> Create(PiscinaRequestDto dto)
        {
            var usuario = await _usuarioRepository.GetById(dto.UsuarioId);

            if (usuario == null)
            {
                throw new KeyNotFoundException(
                    $"Não foi possível criar a piscina usuario{dto.UsuarioId} não existe."
                );
            }

            var piscina = new Piscina
            {
                Nome = dto.Nome,
                VolumeLitros = dto.VolumeLitros,
                ProfundidadeMedia = dto.ProfundidadeMedia,
                UsuarioId = dto.UsuarioId,
                LocalId = dto.LocalId,
            };

            await _piscinaRepository.Create(piscina);

            await _audit.WriteAsync(
                new AuditEntry
                {
                    Action = AuditActions.PiscinaCreate,
                    EntityType = "Piscina",
                    EntityId = piscina.Id,
                    Summary = $"Piscina criada: {piscina.Nome}",
                    Payload = new
                    {
                        nome = piscina.Nome,
                        volumeLitros = piscina.VolumeLitros,
                        profundidadeMedia = piscina.ProfundidadeMedia,
                        usuarioId = piscina.UsuarioId,
                    },
                }
            );

            return new PiscinaResponseDto
            {
                Id = piscina.Id,
                Nome = piscina.Nome,
                VolumeLitros = piscina.VolumeLitros,
                ProfundidadeMedia = piscina.ProfundidadeMedia,
                CreatedAt = piscina.CreatedAt,
                UsuarioPiscina = new NomeIdDto(piscina.UsuarioId, usuario.Nome),
            };
        }

        public async Task<PiscinaResponseDto> Update(Guid id, PiscinaRequestDto dto)
        {
            var piscinaDb = await _piscinaRepository.GetById(id);
            if (piscinaDb == null)
            {
                throw new KeyNotFoundException($"Piscina com id {id} não encontrado.");
            }

            var usuario = await _usuarioRepository.GetById(dto.UsuarioId);

            if (usuario == null)
            {
                throw new KeyNotFoundException(
                    $"Não foi possível criar a piscina usuario{dto.UsuarioId} não existe."
                );
            }

            var piscinaUpdated = new Piscina
            {
                Id = id,
                Nome = dto.Nome,
                VolumeLitros = dto.VolumeLitros,
                ProfundidadeMedia = dto.ProfundidadeMedia,
                UsuarioId = dto.UsuarioId,
            };

            await _piscinaRepository.Update(id, piscinaUpdated);

            await _audit.WriteAsync(
                new AuditEntry
                {
                    Action = AuditActions.PiscinaUpdate,
                    EntityType = "Piscina",
                    EntityId = id,
                    Summary = $"Piscina atualizada: {dto.Nome}",
                    Payload = new
                    {
                        nome = dto.Nome,
                        volumeLitros = dto.VolumeLitros,
                        profundidadeMedia = dto.ProfundidadeMedia,
                        usuarioId = dto.UsuarioId,
                    },
                }
            );

            return new PiscinaResponseDto
            {
                Id = id,
                Nome = dto.Nome,
                VolumeLitros = dto.VolumeLitros,
                ProfundidadeMedia = dto.ProfundidadeMedia,
                CreatedAt = piscinaDb.CreatedAt,
                UsuarioPiscina = new NomeIdDto(dto.UsuarioId, usuario.Nome),
            };
        }

        public async Task Delete(Guid id)
        {
            var piscinaDb = await _piscinaRepository.GetById(id);
            if (piscinaDb == null)
            {
                throw new KeyNotFoundException($"Estoque com id {id} não encontrado.");
            }

            await _piscinaRepository.Delete(id);

            await _audit.WriteAsync(
                new AuditEntry
                {
                    Action = AuditActions.PiscinaDelete,
                    EntityType = "Piscina",
                    EntityId = id,
                    Summary = $"Piscina {id} excluída",
                }
            );
        }
    }
}
