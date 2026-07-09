using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class UsuarioLocalResponseDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid? LocalId { get; set; } = null;

        // Nome do Local — conveniência para o front (ex.: seletor "Trocar Local")
        // sem precisar de uma segunda chamada à API.
        public string? LocalNome { get; set; }
        public Perfil Perfil { get; set; } = Perfil.Visualizador;
        public DateTimeOffset CreatedAt { get; set; }
        public bool Ativo { get; set; }
    }
}
