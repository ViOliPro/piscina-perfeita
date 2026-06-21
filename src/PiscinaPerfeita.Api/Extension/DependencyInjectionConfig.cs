using PiscinaPerfeita.Api.Service.Usuarios;
using PiscinaPerfeita.Api.Repository.Usuarios;
using PiscinaPerfeita.Api.Service.Estoques;
using PiscinaPerfeita.Api.Repository.Estoques;
using PiscinaPerfeita.Api.Service.Analises;
using PiscinaPerfeita.Api.Repository.Analises;
using PiscinaPerfeita.Api.Service.MovimentacoesEstoque;
using PiscinaPerfeita.Api.Repository.MovimentacoesEstoque;


namespace PiscinaPerfeita.Api.Extension
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection ResolveDependencies(this IServiceCollection services)
        {
            // 1. Registre aqui todos os seus Services
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IEstoqueService, EstoqueService>();
            services.AddScoped<IProdutosService, ProdtosService>();
            services.AddScoped<IMovimentacaoService, MovimentacaoService>();
            

            // 2. Registre aqui todos os seus Repositories
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IEstoqueRepository, EstoqueRepository>();
            services.AddScoped<IPiscinaRepository, PiscinaRepository>();
            services.AddScoped<IMovimentacaoRepository, MovimentacaoRepository>();
            

            // Qualquer outra injeção (Validadores, Helpers, etc) entra aqui embaixo

            return services;
        }
    }
}
