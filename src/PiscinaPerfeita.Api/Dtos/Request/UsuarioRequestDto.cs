using PiscinaPerfeita.Api.Dtos.Response;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    public class UsuarioRequestDto
    {
        public Guid Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string SenhaHash { get; set; } = string.Empty;

        public DateTimeOffset? CreatedAt { get; set; }

        public virtual ICollection<PiscinaResponseDto> Piscinas { get; set; } = new List<PiscinaResponseDto>();
    }
}
