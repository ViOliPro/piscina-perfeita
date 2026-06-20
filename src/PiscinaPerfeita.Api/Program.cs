using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Data;
using PiscinaPerfeita.Api.Extension;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PiscinaPerfeitaContext>(options =>
    options.UseNpgsql(connectionString));

// Injecao de dependecias
builder.Services.ResolveDependencies();

try
{
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

    }
    // ... restante do pipeline
    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

  
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("========================================");
    Console.WriteLine($"ERRO NA DI: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"DETALHE: {ex.InnerException.Message}");
    }
    Console.WriteLine("========================================");
    throw;
}


