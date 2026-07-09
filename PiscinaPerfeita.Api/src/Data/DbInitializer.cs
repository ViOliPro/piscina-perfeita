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

            context.Usuarios.Add(
                new Usuario
                {
                    Nome = configuration["ADMIN_NAME"] ?? "Admin",
                    Email = configuration["ADMIN_EMAIL"],
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword(configuration["ADMIN_PASSWORD"]),
                    Role = Role.SuperAdmin,
                }
            );

            await context.SaveChangesAsync();
        }
    }
}
