using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Authorization
{
    // Gera a policy "Perfil:X,Y" em tempo real
    public class PerfilPolicyProvider : IAuthorizationPolicyProvider
    {
        private const string Prefixo = "Perfil:";
        public DefaultAuthorizationPolicyProvider FallbackProvider { get; }

        public PerfilPolicyProvider(IOptions<AuthorizationOptions> options) =>
            FallbackProvider = new DefaultAuthorizationPolicyProvider(options);

        private static readonly Dictionary<string, Perfil[]> Mapa = new()
        {
            //POLICY de uso global na maioria dos CRUDS
            [Policies.Listar] = [Perfil.Administrador, Perfil.Operador, Perfil.Visualizador],
            [Policies.Cadastrar] = [Perfil.Administrador, Perfil.Operador],
            [Policies.Editar] = [Perfil.Administrador, Perfil.Operador],
            [Policies.Deletar] = [Perfil.Administrador],

            //POLICY DEPOSITO
            [Policies.GerenciarDeposito] = [Perfil.Administrador],
            [Policies.GerenciarDepositoUpdate] = [Perfil.Administrador, Perfil.Operador],

            //POLICY LOCAL
            [Policies.GerenciarLocal] = [Perfil.Administrador],

            //POLICY USUARIO
            [Policies.GerenciarUsuario] = [Perfil.Administrador],

            //POLICY USUARIOLOCAL
            [Policies.GerenciarUsuarioLocal] = [Perfil.Administrador],
        };

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (Mapa.TryGetValue(policyName, out var perfis))
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PerfilRequirement(perfis))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return FallbackProvider.GetPolicyAsync(policyName);
        }

        public Task<AuthorizationPolicy?> GetDefaultPolicyAsync() =>
            FallbackProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
            FallbackProvider.GetFallbackPolicyAsync();
    }
}
