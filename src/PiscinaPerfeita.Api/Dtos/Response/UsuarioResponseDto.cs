using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class UsuarioResponseDto
    {
        public Guid Id { get; set; }

        public string Nome { get; set; } = string.Empty;


        public string?Email { get; set; } = string.Empty;

        public DateTimeOffset? CreatedAt { get; set; }

        public Guid PiscinaId { get; set; }
    }
}
