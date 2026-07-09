using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    public class UsuarioLocalRequestDto
    {
        [Required(ErrorMessage = "O campo UsuarioId é obrigatório.")]
        public Guid UsuarioId { get; set; }
        public Guid? LocalId { get; set; } = null;
        public Perfil Perfil { get; set; } = Perfil.Visualizador;
    }
}
