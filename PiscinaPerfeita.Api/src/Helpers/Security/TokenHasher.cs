using System.Security.Cryptography;
using System.Text;

namespace PiscinaPerfeita.Api.Helpers.Security;

// Hash usado por todo token de uso único guardado no banco (reset de senha,
// convite, refresh token) — nunca o valor puro, só o hash. Compartilhado
// entre UsuarioService e TokenService pra não duplicar a lógica de crypto.
public static class TokenHasher
{
    public static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
