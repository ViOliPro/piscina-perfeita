using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PiscinaPerfeita.Api.Data;
using PiscinaPerfeita.Api.Extension;
using System.Globalization;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var defaultCulture = new CultureInfo("pt-BR");

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("A chave JWT não está configurada (Jwt:Key).");

var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };
});

//Authorization
builder.Services.AddAuthorization();

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

    app.UseRequestLocalization(localizationOptions);
    app.UseHttpsRedirection();
    app.UseAuthentication();
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



