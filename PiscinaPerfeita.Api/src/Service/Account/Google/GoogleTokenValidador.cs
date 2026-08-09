// Auth/Google/GoogleTokenValidator.cs
using Google.Apis.Auth;

namespace PiscinaPerfeita.Api.Service.Account.Google
{
    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        private readonly string _googleClientId;
        private readonly ILogger<GoogleTokenValidator> _logger;

        public GoogleTokenValidator(
            IConfiguration configuration,
            ILogger<GoogleTokenValidator> logger
        )
        {
            _googleClientId =
                configuration["Google:ClientId"]
                ?? throw new InvalidOperationException("Google:ClientId não configurado.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GoogleTokenPayload?> ValidarAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
                return null;

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    idToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _googleClientId },
                    }
                );

                return new GoogleTokenPayload(payload.Email, payload.Name, payload.EmailVerified);
            }
            catch (InvalidJwtException ex)
            {
                // Assinatura inválida, expirado, audience errada — sinal legítimo
                // de "token inválido", não falha de infraestrutura.
                _logger.LogWarning(ex, "Token do Google rejeitado na validação.");
                return null;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // Falha ao buscar as chaves públicas do Google (rede/timeout).
                // Não é "token inválido" — deixamos subir e virar 500, em vez de
                // Unauthorized, que esconderia o problema real.
                _logger.LogError(ex, "Falha de infraestrutura ao validar token do Google.");
                throw;
            }
        }
    }
}
