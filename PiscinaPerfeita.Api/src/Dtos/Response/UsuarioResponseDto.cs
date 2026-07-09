using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class UsuarioResponseDto
    {
        public Guid Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Cpf { get; set; } = string.Empty;

        public Role Role { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<NomeIdDto> Piscinas { get; set; } = new List<NomeIdDto>();

        public Guid? LocalId { get; set; }

        public Guid? UltimoLocalId { get; set; }

        public Perfil Perfil { get; set; }
    }

    public class LoginUsuarioResponseDto
    {
        public string Nome { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;

        public Role Role { get; set; }
    }
}
