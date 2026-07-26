using Microsoft.AspNetCore.Authorization;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Authorization
{
    public class PerfilRequirement : IAuthorizationRequirement
    {
        public Perfil[] PerfisPermitidos { get; }

        public PerfilRequirement(Perfil[] perfisPermitidos) => PerfisPermitidos = perfisPermitidos;
    }
}
