using PiscinaPerfeita.Api.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    public class UsuarioRequestDto
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "O campo Email deve ser um endereço de e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [MinLength(6, ErrorMessage = "O campo Senha deve ter pelo menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [DisplayName("Password")]
        public string SenhaHash { get; set; } = string.Empty;
        public Role Role { get; set; }
    }

}