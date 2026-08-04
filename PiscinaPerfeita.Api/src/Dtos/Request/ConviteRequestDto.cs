using System.ComponentModel.DataAnnotations;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Request
{
    // Espelha UsuarioRequestDto, tirando Nome/Cpf/SenhaHash — quem preenche
    // isso é o convidado, não quem convida.
    public class ConviteRequestDto
    {
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O campo Email deve ser um endereço de e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        public Role Role { get; set; }

        public Perfil? Perfil { get; set; }

        public Guid? LocalId { get; set; }
    }

    public class CompletarConviteRequestDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        public string Senha { get; set; } = string.Empty;
    }
}
