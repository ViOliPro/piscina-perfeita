using PiscinaPerfeita.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class UsuarioResponseDto
    {
        public Guid Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public Role Role { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<NomeIdDto> Piscinas { get; set; } = new List<NomeIdDto>();
    }


    public class LoginUsuarioResponseDto
    {
        public string Nome { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;

        public Role Role { get; set; }
    }
}
