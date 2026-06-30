using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Dtos.Response
{
    public class AccountResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public UserResponseDto User { get; set; } = new UserResponseDto();
        public int expiresIn { get; set; }
    }

    public class UserResponseDto
    {
        public Guid UserId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Role? Role { get; set; }
    }

}