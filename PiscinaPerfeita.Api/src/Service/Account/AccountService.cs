using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Locais;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Repository.UsuariosLocal;

namespace PiscinaPerfeita.Api.Service.Account
{
    public class AccountService : IAccountService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUsuarioLocalRepository _usuarioLocalRepository;
        private readonly ILocalRepository _localRepository;
        private readonly IConfiguration _configuration;

        public AccountService(
            IUsuarioRepository usuarioRepository,
            IUsuarioLocalRepository usuarioLocalRepository,
            ILocalRepository localRepository,
            IConfiguration configuration
        )
        {
            _usuarioRepository =
                usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _usuarioLocalRepository =
                usuarioLocalRepository
                ?? throw new ArgumentNullException(nameof(usuarioLocalRepository));
            _localRepository =
                localRepository ?? throw new ArgumentNullException(nameof(localRepository));
            _configuration =
                configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<AccountResponseDto> Login(AccountRequestDto request)
        {
            // Valida os dados enviados para login e busca usuário
            var usuario = await ValidacaoDadosLogin(request);

            // Verifica se a senha fornecida corresponde à armazenada
            VerifyPasswordCheck(request.Password, usuario.SenhaHash);

            // Validação da lógica de usuário Local (retorna o Local ativo e o
            // Perfil correspondente a esse Local — ou ao vínculo pendente).
            var (localIdAtivo, perfilAtivo) = await ValidacaoUsuarioLocal(usuario.Id, usuario);
            string stringLocalId = localIdAtivo?.ToString() ?? string.Empty;

            // Geração do Token JWT
            var stringToken = NewToken(usuario, stringLocalId, perfilAtivo);

            // Retorna o token JWT e informações do usuário
            return new AccountResponseDto
            {
                AccessToken = stringToken,
                TokenType = "Bearer",
                expiresIn = 28800, // 8 horas em segundos
                User = new UserResponseDto
                {
                    UserId = usuario.Id,
                    Nome = usuario.Nome ?? string.Empty,
                    Email = usuario.Email ?? string.Empty,
                    LocalId = localIdAtivo ?? Guid.Empty,
                    Role = usuario.Role,
                    Perfil = perfilAtivo,
                },
            };
        }

        // NOVO ENDPOINT: Alternar Condomínio/Local Ativo
        public async Task<AccountResponseDto> SwitchLocal(Guid userId, Guid? newLocalId)
        {
            // 1. Busca o usuário para garantir que existe e obter as informações de Claims.
            var usuario = await _usuarioRepository.GetPasswordById(userId);
            if (usuario == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            // "Ver todos" (newLocalId nulo) só existe para SuperAdmin — é ele
            // quem tem a opção de sair de um Local específico e voltar a
            // enxergar tudo (ver PiscinaPerfeitaContext, filtro global).
            if (newLocalId == null)
            {
                if (usuario.Role != Role.SuperAdmin)
                    throw new ArgumentException("Informe o Local para o qual deseja trocar.");

                await _usuarioRepository.UpdateUltimoLocal(userId, Guid.Empty);
                var tokenVerTodos = NewToken(usuario, Guid.Empty.ToString(), Perfil.Administrador);

                return new AccountResponseDto
                {
                    AccessToken = tokenVerTodos,
                    TokenType = "Bearer",
                    expiresIn = 28800,
                    User = new UserResponseDto
                    {
                        UserId = usuario.Id,
                        Nome = usuario.Nome ?? string.Empty,
                        Email = usuario.Email ?? string.Empty,
                        LocalId = Guid.Empty,
                        Role = usuario.Role,
                        Perfil = null,
                    },
                };
            }

            // 2. Valida o acesso ao Local. SuperAdmin não depende de um vínculo em
            // UsuariosLocal — só confirmamos que o Local existe de verdade. Um
            // usuário comum precisa mesmo do vínculo ativo.
            Perfil perfilAtivo;
            if (usuario.Role == Role.SuperAdmin)
            {
                var local = await _localRepository.GetById(newLocalId.Value);
                if (local == null)
                    throw new KeyNotFoundException("Local não encontrado.");

                perfilAtivo = Perfil.Administrador;
            }
            else
            {
                var vinculo = await _usuarioLocalRepository.Vinculo(userId, newLocalId.Value);
                if (vinculo == null)
                    throw new UnauthorizedAccessException(
                        "Você não tem permissão para acessar este condomínio/local."
                    );

                perfilAtivo = vinculo.Perfil;
            }

            // 3. Persiste a escolha (só o UltimoLocalId).
            await _usuarioRepository.UpdateUltimoLocal(userId, newLocalId.Value);

            // 4. Gera o novo Token com o local e o perfil alterados.
            var stringToken = NewToken(usuario, newLocalId.Value.ToString(), perfilAtivo);

            return new AccountResponseDto
            {
                AccessToken = stringToken,
                TokenType = "Bearer",
                expiresIn = 28800,
                User = new UserResponseDto
                {
                    UserId = usuario.Id,
                    Nome = usuario.Nome ?? string.Empty,
                    Email = usuario.Email ?? string.Empty,
                    LocalId = newLocalId.Value,
                    Role = usuario.Role,
                    Perfil = perfilAtivo,
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

        // Validação e lógica do Usuario Local
        private async Task<(Guid? LocalId, Perfil Perfil)> ValidacaoUsuarioLocal(
            Guid userId,
            Usuario usuario
        )
        {
            var vinculos = await _usuarioLocalRepository.GetAllByUserId(userId);

            if (vinculos == null || vinculos.Count == 0)
                throw new KeyNotFoundException("Este usuário não está vinculado a nenhum local!");

            Guid? localIdAtivo;
            if (vinculos.Count > 1)
                localIdAtivo = usuario.UltimoLocalId ?? usuario.LocalId ?? vinculos[0].LocalId;
            else
                localIdAtivo = vinculos[0].LocalId;

            var perfilAtivo =
                vinculos.FirstOrDefault(v => v.LocalId == localIdAtivo)?.Perfil
                ?? vinculos[0].Perfil;

            return (localIdAtivo, perfilAtivo);
        }

        // Simplificado: criação síncrona do token em memória
        private string NewToken(Usuario usuario, string stringLocalId, Perfil perfil)
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
                        new Claim("perfil", perfil.ToString()),
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
