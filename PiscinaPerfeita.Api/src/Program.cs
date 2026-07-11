using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PiscinaPerfeita.Api.Data;
using PiscinaPerfeita.Api.Extension;

// 1. Inicializa o builder e carrega as variáveis de ambiente IMEDIATAMENTE
var builder = WebApplication.CreateBuilder(args);

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
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new Exception("Jwt:Key não configurado no ambiente");

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
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
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
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "A string de conexão 'ConnectionStrings:DefaultConnection' não foi configurada no ambiente."
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
// Sem "Cors:AllowedOrigins" configurado, mantém o comportamento atual
// (libera qualquer origem) — adequado enquanto o projeto está em fase de
// testes com poucas pessoas. Quando o domínio de produção for definido,
// basta configurar essa variável (ver docker-compose.yml/.env.example)
// para restringir e não precisar tocar em código depois.
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AppCors",
        policy =>
        {
            if (string.IsNullOrWhiteSpace(allowedOrigins))
            {
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            }
            else
            {
                var origins = allowedOrigins.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                );

                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
            }
        }
    );
});

try
{
    var app = builder.Build();

    if (app.Environment.IsDevelopment() && Assembly.GetEntryAssembly()?.GetName().Name != "ef")
    {
        app.MapOpenApi();
    }

    app.UseRequestLocalization(localizationOptions);
    app.UseHttpsRedirection();
    app.UseCors("AppCors");
    app.UseAuthentication();
    app.UseAuthorization();

    // Endpoint simples e anônimo para health check (usado pelo HEALTHCHECK
    // do Dockerfile e por orquestradores como Docker Compose/Kubernetes).
    app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

    app.MapControllers();

    // Seeder
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<PiscinaPerfeitaContext>();

            // Isso aplica qualquer Migration pendente no banco de dados automaticamente
            context.Database.Migrate();

            // Buscando o serviço de configuração do container de Injeção de Dependência
            var configuration = services.GetRequiredService<IConfiguration>();

            await DbInitializer.SeedAsync(context, configuration);
            // Se você tiver um DbInitializer (seeder) para criar o Admin:
            // DbInitializer.Initialize(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Ocorreu um erro ao aplicar as migrations no banco.");
        }
    }
    //Fim do bloco
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
