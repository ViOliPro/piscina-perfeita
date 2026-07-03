using System.Globalization;
using System.Reflection;
using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PiscinaPerfeita.Api.Data;
using PiscinaPerfeita.Api.Extension;

// 1. Inicializa o builder e carrega as variáveis de ambiente IMEDIATAMENTE
var builder = WebApplication.CreateBuilder(args);

// Carrega as variáveis de ambiente do arquivo .env
if (builder.Environment.IsDevelopment())
{
    Env.Load("../.env");
}
builder.Configuration.AddEnvironmentVariables();

// 2. Configuração de Localização
var defaultCulture = new CultureInfo("pt-BR");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture },
};

// 3. JWT Authentication (Agora sim, depois do Env.Load())
var jwtKey = builder.Configuration["jwt__Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException(
        "A chave JWT não está configurada (jwt__Key no arquivo .env)."
    );
}

var key = Encoding.ASCII.GetBytes(jwtKey);
builder
    .Services.AddAuthentication(options =>
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
            ValidIssuer = builder.Configuration["jwt__Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["jwt__Audience"],
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllers();

if (Assembly.GetEntryAssembly()?.GetName().Name != "ef")
{
    builder.Services.AddOpenApi();
}

// 1. Recupera a string de conexão já formatada do .env
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "A string de conexão 'ConnectionStrings__DefaultConnection' não foi configurada no ambiente."
    );
}

// 2. Configura o DbContext com a string limpa
builder.Services.AddDbContext<PiscinaPerfeitaContext>(options =>
    options.UseNpgsql(connectionString).UseLowerCaseNamingConvention()
);

// Injeção de dependências
builder.Services.ResolveDependencies();
builder.Services.AddHttpContextAccessor();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AppCors",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    );
});

try
{
    var app = builder.Build();
    // --- BLOCO DA SEEDER ---
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<PiscinaPerfeitaContext>();

            await DbInitializer.SeedAsync(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Erro ao executar o Seed do banco.");
        }
    }
    // --- FIM DO BLOCO ---

    if (app.Environment.IsDevelopment() && Assembly.GetEntryAssembly()?.GetName().Name != "ef")
    {
        app.MapOpenApi();
    }

    app.UseRequestLocalization(localizationOptions);
    app.UseHttpsRedirection();
    app.UseCors("AppCors");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (HostAbortedException)
{
    throw;
}
catch (Exception ex)
{
    Console.Error.WriteLine("\n\n==================================================");
    Console.Error.WriteLine($"ERRO REAL NA INICIALIZAÇÃO DA API: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.Error.WriteLine($"DETALHE: {ex.InnerException.Message}");
    }
    Console.Error.WriteLine("==================================================\n\n");
    throw;
}
