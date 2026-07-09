using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Repository.UsuariosLocal;

namespace PiscinaPerfeita.Api.Service.Account
{
    public class AccountService : IAccountService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUsuarioLocalRepository _usuarioLocalRepository;
        private readonly IConfiguration _configuration;

        public AccountService(
            IUsuarioRepository usuarioRepository,
            IUsuarioLocalRepository usuarioLocalRepository,
            IConfiguration configuration
        )
        {
            _usuarioRepository =
                usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _usuarioLocalRepository =
                usuarioLocalRepository
                ?? throw new ArgumentNullException(nameof(usuarioLocalRepository));
            _configuration =
                configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<AccountResponseDto> Login(AccountRequestDto request)
        {
            // Valida os dados enviados para login e busca usuário
            var usuario = await ValidacaoDadosLogin(request);

            // Verifica se a senha fornecida corresponde à armazenada
            VerifyPasswordCheck(request.Password, usuario.SenhaHash);

            // Validação da lógica de usuário Local (Retorna o Guid resolvido)
            Guid? localIdAtivo = await ValidacaoUsuarioLocal(usuario.Id, usuario);
            string stringLocalId = localIdAtivo?.ToString() ?? string.Empty;

            // Geração do Token JWT
            var stringToken = NewToken(usuario, stringLocalId);

            // Retorna o token JWT e informações do usuário
            return new AccountResponseDto
            {
                AccessToken = stringToken,
                TokenType = "Bearer",
                expiresIn = 28800, // 8 horas em segundos
                User = new UserResponseDto
                {
                    Nome = usuario.Nome ?? string.Empty,
                    Email = usuario.Email ?? string.Empty,
                    LocalId = localIdAtivo ?? Guid.Empty,
                    Role = usuario.Role,
                },
            };
        }

        // NOVO ENDPOINT: Alternar Condomínio/Local Ativo
        public async Task<AccountResponseDto> SwitchLocal(Guid userId, Guid newLocalId)
        {
            // 1. Busca o usuário para garantir que existe e obter as informações de Claims
            var usuario = await _usuarioRepository.GetById(userId);
            if (usuario == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            // 2. Valida se o usuário realmente tem vínculo ativo com o local que está tentando acessar
            var vinculos = await _usuarioLocalRepository.Vinculo(userId, newLocalId);

            if (vinculos == null)
                throw new UnauthorizedAccessException(
                    "Você não tem permissão para acessar este condomínio/local."
                );

            // 3. Atualiza o UltimoLocalId no registro do usuário para persistir a escolha
            usuario.UltimoLocalId = newLocalId;
            var usuarioUpdated = new Usuario
            {
                Id = usuario.Id,
                LocalId = usuario.LocalId,
                UltimoLocalId = usuario.UltimoLocalId,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Cpf = usuario.Cpf,
                Role = usuario.Role,
            };
            await _usuarioRepository.Update(userId, usuarioUpdated);

            // 4. Gera o novo Token com o local alterado
            var stringToken = NewToken(usuarioUpdated, newLocalId.ToString());

            return new AccountResponseDto
            {
                AccessToken = stringToken,
                TokenType = "Bearer",
                expiresIn = 28800,
                User = new UserResponseDto
                {
                    Nome = usuario.Nome ?? string.Empty,
                    Email = usuario.Email ?? string.Empty,
                    LocalId = newLocalId,
                    Role = usuario.Role,
                },
            };
        }

        // Validação dados login
        private async Task<Usuario> ValidacaoDadosLogin(AccountRequestDto request)
        {
            if (
                string.IsNullOrWhiteSpace(request.Email)
                || string.IsNullOrWhiteSpace(request.Password)
            )
                throw new ArgumentException("Por favor, preencha todos os campos.");

            var usuario = await _usuarioRepository.GetByEmail(request.Email);

            if (usuario == null || string.IsNullOrWhiteSpace(usuario.SenhaHash))
                throw new ArgumentException("E-mail ou Senha incorretos.");

            return usuario;
        }

        // Validação de senha
        private bool VerifyPasswordCheck(string password, string? senhaHash)
        {
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, senhaHash);

            if (!isPasswordValid)
                throw new ArgumentException("E-mail ou Senha incorretos.");

            return true;
        }

        // Validação e lógica do Usuario Local (Ajustado para retornar Guid?)
        private async Task<Guid?> ValidacaoUsuarioLocal(Guid userId, Usuario usuario)
        {
            var locaisDoUsuario = await _usuarioLocalRepository.GetByUserId(userId);

            if (locaisDoUsuario == null)
                throw new KeyNotFoundException("Este usuário não está vinculado a nenhum local!");

            Guid? localIdAtivo = null;
            var totalRegistros = await _usuarioLocalRepository.QtdUserByLocal(userId);

            if (totalRegistros > 1)
            {
                localIdAtivo = usuario.UltimoLocalId ?? usuario.LocalId ?? locaisDoUsuario.LocalId;
            }
            else
            {
                localIdAtivo = locaisDoUsuario.LocalId;
            }

            return localIdAtivo;
        }

        // Simplificado: removido o async desnecessário já que a criação do token é puramente síncrona em memória
        private string NewToken(Usuario usuario, string stringLocalId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key =
                _configuration["Jwt:Key"]
                ?? throw new ArgumentNullException("Chave JWT não configurada.");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                        new Claim(ClaimTypes.Email, usuario.Email ?? string.Empty),
                        new Claim(ClaimTypes.Role, usuario.Role.ToString()),
                        new Claim(ClaimTypes.Name, usuario.Nome ?? string.Empty),
                        new Claim("local_id", stringLocalId),
                    }
                ),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    SecurityAlgorithms.HmacSha256
                ),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
