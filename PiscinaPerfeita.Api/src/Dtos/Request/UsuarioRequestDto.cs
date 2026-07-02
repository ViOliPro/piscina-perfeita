using PiscinaPerfeita.Api.Dtos.Response;
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

        [MinLength(8, ErrorMessage = "O campo Senha deve ter pelo menos 8 caracteres.")]
        [DataType(DataType.Password)]
        [DisplayName("Password")]
        public string? SenhaHash { get; set; } = string.Empty;
        public Role Role { get; set; }
    }

    public class UsuarioRequestUpdateDto
    {
        public string? Nome { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "O campo Email deve ser um endereço de e-mail válido.")]
        public string? Email { get; set; } = string.Empty;

        [MaxLength(30, ErrorMessage = "O campo Senha nao pode ultrapassar 30 caracteres.")]
        [DataType(DataType.Password)]
        [DisplayName("Password")]
        public string? SenhaHash { get; set; } = string.Empty;
        public Role? Role { get; set; }
    }

}