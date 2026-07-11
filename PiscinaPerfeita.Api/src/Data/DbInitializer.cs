using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            PiscinaPerfeitaContext context,
            IConfiguration configuration
        )
        {
            await context.Database.MigrateAsync();

            // Verifica se já existem usuários no banco de dados
            if (await context.Usuarios.AnyAsync())
                return;

            var admin = new Usuario
            {
                Nome = configuration["ADMIN_NAME"] ?? "Admin",
                Email = configuration["ADMIN_EMAIL"],
                Cpf = configuration["ADMIN_CPF"],
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(configuration["ADMIN_PASSWORD"]),
                Role = Role.SuperAdmin,
            };

            context.Usuarios.Add(admin);
            await context.SaveChangesAsync();

            // Um SuperAdmin tem acesso completo ao CRUD da aplicação (incluindo
            // o cadastro de Locais) e por isso não precisa estar vinculado a um
            // Local específico. Ainda assim é necessário existir o vínculo
            // UsuarioLocal (com LocalId nulo) — sem ele o login falha, pois
            // AccountService.ValidacaoUsuarioLocal exige ao menos um registro.
            context.UsuariosLocal.Add(
                new UsuarioLocal
                {
                    UsuarioId = admin.Id,
                    LocalId = null,
                    Perfil = Perfil.Administrador,
                    Ativo = true,
                }
            );

            await context.SaveChangesAsync();
        }
    }
}
