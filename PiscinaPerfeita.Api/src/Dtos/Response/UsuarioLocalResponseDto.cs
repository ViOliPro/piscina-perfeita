using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class UsuarioLocalResponseDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid? LocalId { get; set; } = null;
        public Perfil Perfil { get; set; } = Perfil.Visualizador;
        public DateTimeOffset CreatedAt { get; set; }
        public bool Ativo { get; set; }
    }
}
