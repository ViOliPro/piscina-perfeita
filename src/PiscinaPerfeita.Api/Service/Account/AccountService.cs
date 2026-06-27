using Microsoft.IdentityModel.Tokens;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Repository.Usuarios;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PiscinaPerfeita.Api.Service.Account
{
    public class AccountService : IAccountService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;

        // CORREÇÃO: Adicionado IConfiguration configuration como parâmetro do construtor
        public AccountService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<AccountResponseDto> Login(AccountRequestDto request)
        {
            // Validacao dos campos email e senha
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Por favor, preencha todos os campos.");

            // Busca um usuario com base no email e senha fornecidos
            var usuario = await _usuarioRepository.GetByEmail(request.Email);

            if (usuario == null)
                throw new ArgumentException("Usuário não encontrado.");

            // Verifica se a senha fornecida corresponde à senha armazenada no banco de dados
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, usuario.SenhaHash);

            if (!isPasswordValid)
                throw new ArgumentException("Senha incorreta.");

            // Geração do Token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = _configuration["Jwt:Key"] ?? throw new ArgumentNullException("Chave JWT nao configurada.");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Role?.ToString() ?? string.Empty),
                    new Claim(ClaimTypes.Name, usuario.Nome)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = tokenHandler.WriteToken(token);

            // CORREÇÃO: Instanciando e retornando o tipo correto (AccountResponseDto)
            return new AccountResponseDto
            {
                AccessToken = stringToken,
                TokenType = "Bearer",
                expiresIn = 28800, // 8 horas em segundos
                User = new UserResponseDto
                {
                    UserId = usuario.Id.ToString(), // Adapte se o seu ID do banco for string/int/Guid
                    Name = usuario.Nome,
                    Role = usuario.Role ?? 0 // Garanta o mapeamento correto do tipo numérico da Role
                }
            };
        }
    }
}
