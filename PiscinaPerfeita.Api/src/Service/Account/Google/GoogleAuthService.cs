using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Service.Usuarios;

namespace PiscinaPerfeita.Api.Service.Account.Google
{
    // Auth/Google/GoogleAuthService.cs
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IGoogleTokenValidator _googleTokenValidator;
        private readonly IUsuarioService _usuarioService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;

        public GoogleAuthService(
            IGoogleTokenValidator googleTokenValidator,
            IUsuarioService usuarioService,
            ITokenService tokenService,
            IConfiguration config
        )
        {
            _googleTokenValidator = googleTokenValidator;
            _usuarioService = usuarioService;
            _tokenService = tokenService;
            _config = config;
        }

        public async Task<AuthResult> AutenticarAsync(string idToken)
        {
            var payload = await _googleTokenValidator.ValidarAsync(idToken);
            if (payload is null)
                return AuthResult.Falha(AuthErro.TokenInvalido, "Token do Google inválido.");
            if (!payload.EmailVerificado)
                return AuthResult.Falha(AuthErro.TokenInvalido, "E-mail do Google não verificado.");

            var usuario = await _usuarioService.ObterOuVincularPorEmailGoogleAsync(payload.Email);
            if (usuario is not null)
            {
                var resultado = await _tokenService.GerarTokenAsync(usuario);
                var userDto = new UserResponseDto
                {
                    UserId = usuario.Id,
                    Nome = usuario.Nome ?? string.Empty,
                    Email = usuario.Email ?? string.Empty,
                    LocalId = resultado.LocalId,
                    Role = usuario.Role,
                    Perfil = resultado.Perfil,
                };
                return AuthResult.Ok(resultado.AccessToken, resultado.RefreshToken, userDto);
            }

            if (await _usuarioService.ExisteConviteAtivoAsync(payload.Email))
                return AuthResult.Falha(
                    AuthErro.ConvitePendente,
                    "Você tem um convite pendente. Complete seu cadastro para continuar."
                );

            var emailContato = _config["Contato:EmailSuporte"];
            return AuthResult.Falha(
                AuthErro.AcessoNaoLiberado,
                $"Seu acesso ainda não foi liberado. Entre em contato pelo e-mail {emailContato} para solicitar acesso."
            );
        }

        public async Task<AuthResult> CompletarCadastroAsync(string idToken, string? cpf, bool aceiteTermos)
        {
            var payload = await _googleTokenValidator.ValidarAsync(idToken);
            if (payload is null)
                return AuthResult.Falha(AuthErro.TokenInvalido, "Token do Google inválido.");
            if (!payload.EmailVerificado)
                return AuthResult.Falha(AuthErro.TokenInvalido, "E-mail do Google não verificado.");

            if (!aceiteTermos)
                return AuthResult.Falha(
                    AuthErro.AceiteTermosPendente,
                    "É necessário aceitar os Termos de Uso e a Política de Privacidade para concluir o cadastro."
                );

            var usuario = await _usuarioService.CompletarConviteGoogleAsync(
                payload.Email,
                payload.Nome,
                cpf,
                aceiteTermos
            );
            if (usuario is null)
            {
                var emailContato = _config["Contato:EmailSuporte"];
                return AuthResult.Falha(
                    AuthErro.AcessoNaoLiberado,
                    $"Não encontramos um convite ativo para este e-mail. Entre em contato pelo e-mail {emailContato} para solicitar acesso."
                );
            }

            var resultado = await _tokenService.GerarTokenAsync(usuario);
            var userDto = new UserResponseDto
            {
                UserId = usuario.Id,
                Nome = usuario.Nome ?? string.Empty,
                Email = usuario.Email ?? string.Empty,
                LocalId = resultado.LocalId,
                Role = usuario.Role,
                Perfil = resultado.Perfil,
            };
            return AuthResult.Ok(resultado.AccessToken, resultado.RefreshToken, userDto);
        }
    }
}
