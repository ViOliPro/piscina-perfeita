using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(PiscinaPerfeitaContext context)
        {
            await context.Database.MigrateAsync();

            if (await context.Usuarios.AnyAsync())
                return;

            context.Usuarios.Add(
                new Usuario
                {
                    Nome = Environment.GetEnvironmentVariable("ADMIN_NAME") ?? "Admin",
                    Email = Environment.GetEnvironmentVariable("ADMIN_EMAIL"),
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword(
                        Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                    ),
                    Role = Role.Admin,
                }
            );

            await context.SaveChangesAsync();
        }
    }
}
