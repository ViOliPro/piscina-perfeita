using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Data;
using PiscinaPerfeita.Api.Extension;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
if (Assembly.GetEntryAssembly()?.GetName().Name != "ef")
{
    builder.Services.AddOpenApi();
}

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PiscinaPerfeitaContext>(options =>
    options.UseNpgsql(connectionString)
            .UseLowerCaseNamingConvention());

// Injecao de dependecias
builder.Services.ResolveDependencies();

try
{
    var app = builder.Build();

    if (app.Environment.IsDevelopment() && Assembly.GetEntryAssembly()?.GetName().Name != "ef")
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (HostAbortedException)
{
    // Ignora silenciosamente. Esta exceção é gerada intencionalmente pelas ferramentas 
    // do Entity Framework (dotnet ef) para parar a API após mapear os metadados.
    throw;
}
catch (Exception ex)
{
    // Captura erros reais de verdade (banco fora do ar, DI quebrada, falta de configuração)
    Console.Error.WriteLine("\n\n==================================================");
    Console.Error.WriteLine($"ERRO REAL NA INICIALIZAÇÃO DA API: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.Error.WriteLine($"DETALHE: {ex.InnerException.Message}");
    }
    Console.Error.WriteLine("==================================================\n\n");
    throw;
}



