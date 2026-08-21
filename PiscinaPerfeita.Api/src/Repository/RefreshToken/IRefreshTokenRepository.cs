using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Repository
{
    public interface IRefreshTokenRepository
    {
        Task Create(RefreshToken token);
        Task<RefreshToken?> GetByHash(string tokenHash);
        Task Revoke(Guid id);
        Task RevokeAllByUsuario(Guid usuarioId); // logout de todos os dispositivos
    }
}
