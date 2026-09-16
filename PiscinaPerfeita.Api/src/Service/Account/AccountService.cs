using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Service.Audit;

namespace PiscinaPerfeita.Api.Service.Account
{
    public class AccountService : IAccountService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;
        private readonly IAuditService _audit;

        public AccountService(
            IUsuarioRepository usuarioRepository,
            ITokenService tokenService,
            IAuditService audit
        )
        {
            _usuarioRepository =
                usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        }

        public async Task<LoginResult> Login(AccountRequestDto request)
        {
            try
            {
                var usuario = await ValidacaoDadosLogin(request);
                VerifyPasswordCheck(request.Password, usuario.SenhaHash);

                var resultado = await _tokenService.GerarTokenAsync(usuario);
                await _audit.WriteAsync(
                    new AuditEntry
                    {
                        Action = AuditActions.AuthLoginSuccess,
                        Success = true,
                        Summary = "Login ok",
                        UsuarioId = usuario.Id,
                        UsuarioEmail = usuario.Email,
                        Payload = new { method = "password" },
                    }
                );
                return new LoginResult(MontarResponse(usuario, resultado), resultado.RefreshToken);
            }
            catch (Exception)
            {
                await _audit.WriteAsync(
                    new AuditEntry
                    {
                        Action = AuditActions.AuthLoginFailure,
                        Success = false,
                        Summary = "Credenciais inválidas",
                        UsuarioEmail = request.Email,
                        Payload = new { method = "password" },
                    }
                );
                throw;
            }
        }

        public async Task<LoginResult> SwitchLocal(Guid userId, Guid? newLocalId)
        {
            var usuario = await _usuarioRepository.GetPasswordById(userId);
            if (usuario == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            var resultado = await _tokenService.GerarTokenParaLocalAsync(usuario, newLocalId);
            await _audit.WriteAsync(
                new AuditEntry
                {
                    Action = AuditActions.AuthSwitchLocal,
                    Success = true,
                    Summary = $"Troca de Local para {newLocalId}",
                    UsuarioId = userId,
                    UsuarioEmail = usuario.Email,
                    LocalId = newLocalId,
                    Payload = new { newLocalId },
                }
            );
            return new LoginResult(MontarResponse(usuario, resultado), resultado.RefreshToken);
        }

        public async Task<LoginResult> Refresh(string rawRefreshToken)
        {
            var usuario = await _tokenService.ValidarERotacionarRefreshTokenAsync(rawRefreshToken);
            if (usuario is null)
                throw new UnauthorizedAccessException("Sessão expirada. Faça login novamente.");

            var resultado = await _tokenService.GerarTokenAsync(usuario);
            return new LoginResult(MontarResponse(usuario, resultado), resultado.RefreshToken);
        }

        public Task RevogarRefreshToken(string rawRefreshToken) =>
            _tokenService.RevogarRefreshTokenAsync(rawRefreshToken);

        private static AccountResponseDto MontarResponse(
            Usuario usuario,
            AuthTokenResult resultado
        ) =>
            new()
            {
                AccessToken = resultado.AccessToken,
                TokenType = "Bearer",
                expiresIn = 3600,
                User = new UserResponseDto
                {
                    UserId = usuario.Id,
                    Nome = usuario.Nome ?? string.Empty,
                    Email = usuario.Email ?? string.Empty,
                    LocalId = resultado.LocalId,
                    Role = usuario.Role,
                    Perfil = resultado.Perfil,
                    PrecisaAceitarTermos =
                        usuario.TermosAceitosVersao != LegalConstants.VersaoTermosAtual,
                },
            };

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

        private bool VerifyPasswordCheck(string password, string? senhaHash)
        {
            if (!BCrypt.Net.BCrypt.Verify(password, senhaHash))
                throw new ArgumentException("E-mail ou Senha incorretos.");
            return true;
        }
    }
}
