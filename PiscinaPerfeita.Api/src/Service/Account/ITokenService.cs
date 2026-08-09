using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Service.Account
{
    public record AuthTokenResult(string AccessToken, Guid LocalId, Perfil Perfil);

    public interface ITokenService
    {
        // Resolve o Local ativo automaticamente — login padrão e login via
        // Google usam este. SuperAdmin cai em "ver todos" (Guid.Empty);
        // usuário comum usa UltimoLocalId ou o primeiro vínculo ativo.
        Task<AuthTokenResult> GerarTokenAsync(Usuario usuario);

        // Troca explícita de Local (SwitchLocal). newLocalId nulo só é aceito
        // pra SuperAdmin (volta a "ver todos"); usuário comum precisa de
        // vínculo ativo com o Local pedido.
        Task<AuthTokenResult> GerarTokenParaLocalAsync(Usuario usuario, Guid? newLocalId);
    }
}
