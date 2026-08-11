using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PiscinaPerfeita.Api.Authorization;
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

// 3. JWT Authentication
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
        // Em produção, o token só deve trafegar sobre HTTPS. Em Development
        // (rodando local sem certificado) mantemos false pra não travar os
        // testes locais.
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // Sem tolerância de tempo (pra não aceitar tokens expirados)
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var securityStampClaim = context.Principal?.FindFirst("security_stamp")?.Value;

                if (
                    string.IsNullOrWhiteSpace(userIdClaim)
                    || string.IsNullOrWhiteSpace(securityStampClaim)
                )
                {
                    context.Fail("Token inválido: ID do usuário ou security_stamp ausente.");
                    return;
                }

                // Recuperamos o DbContext (já que a injeção está certa)
                var dbContext =
                    context.HttpContext.RequestServices.GetRequiredService<PiscinaPerfeitaContext>();

                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    context.Fail("Token inválido: ID do usuário mal formatado.");
                    return;
                }

                // CÓDIGO CORRIGIDO: Em vez de FindAsync (que traz o usuário inteiro e joga no ChangeTracker),
                // fazemos um Select() apenas do campo que importa. Muito mais leve para o banco.
                var currentStamp = await dbContext
                    .Usuarios.AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => u.SecurityStamp)
                    .FirstOrDefaultAsync();

                if (currentStamp == null)
                {
                    context.Fail("Token inválido: Usuário não encontrado.");
                    return;
                }

                if (currentStamp != securityStampClaim)
                {
                    // Isso invalida o token antigo na hora!
                    context.Fail("Token invalidado devido a alterações na conta.");
                    return;
                }
            },
        };
    });

// Authorization
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PerfilPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PerfilHandler>();

builder.Services.AddAuthorization(options =>
{
    // Define que por padrão TODAS as rotas precisam estar autenticadas
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

    // Define políticas específicas para reuso
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("User", policy => policy.RequireRole("Usuario"));
    options.AddPolicy("UserOrSuper", policy => policy.RequireRole("SuperAdmin", "Usuario"));
});

// Add services to the container
builder.Services.AddControllers();

if (Assembly.GetEntryAssembly()?.GetName().Name != "ef")
{
    // Bearer no OpenAPI/Swagger, só pra facilitar testar os endpoints
    // autenticados localmente (sem isso não tem como colar o token no
    // "Authorize" da UI).
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer(
            (document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??=
                    new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Cole aqui o token retornado por POST /api/account/login",
                };
                return Task.CompletedTask;
            }
        );

        // Marca com o cadeado (no Swagger UI) toda operação que não seja
        // [AllowAnonymous] — só efeito cosmético/de teste local, não altera
        // a autorização real (quem faz valer isso é o [Authorize] de cada
        // controller/action).
        options.AddOperationTransformer(
            (operation, context, cancellationToken) =>
            {
                var allowsAnonymous = context.Description.ActionDescriptor.EndpointMetadata.Any(m =>
                    m is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute
                );

                if (!allowsAnonymous)
                {
                    operation.Security =
                    [
                        new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = [],
                        },
                    ];
                }

                return Task.CompletedTask;
            }
        );
    });
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
    options
        .UseNpgsql(
            connectionString,
            npgsql =>
            {
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null
                );
            }
        )
        .UseLowerCaseNamingConvention()
);

// Injeção de dependências
builder.Services.ResolveDependencies();
builder.Services.AddHttpContextAccessor();

// CORS
// Em Development, sem "Cors:AllowedOrigins" configurado, mantém o
// comportamento de liberar qualquer origem (facilita testar com o Vite dev
// server em qualquer porta). Fora de Development, a variável passa a ser
// obrigatória — preferimos falhar no startup a subir em produção liberando
// qualquer origem silenciosamente.
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"];

if (string.IsNullOrWhiteSpace(allowedOrigins) && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "Cors:AllowedOrigins não configurado. Fora do ambiente de Development, "
            + "é obrigatório definir os domínios permitidos (ver docker-compose.yml/.env.example)."
    );
}

// AllowCredentials() é obrigatório pro cookie httpOnly do refresh token
// atravessar domínios diferentes, e o navegador rejeita AllowCredentials()
// combinado com AllowAnyOrigin() — por isso a origem explícita passa a ser
// necessária mesmo em Development. Sem Cors:AllowedOrigins configurado, cai
// num default local (o Vite com HTTPS que já configuramos).
var origins = string.IsNullOrWhiteSpace(allowedOrigins)
    ? new[] { "https://localhost:5173" }
    : allowedOrigins.Split(
        ',',
        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
    );

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AppCors",
        policy => policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    );
});

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

// Rate limiting — hoje o login não tinha nenhum limite de tentativas.
// Em Development o limite é bem mais alto pra não travar os testes manuais.

var loginPermitLimit = builder.Environment.IsDevelopment() ? 1000 : 5;
var authSensitivePermitLimit = builder.Environment.IsDevelopment() ? 1000 : 10;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Partitionado por IP — cada IP tem sua própria janela, então um
    // atacante só se limita a si mesmo, não trava o login de todo mundo.
    options.AddPolicy(
        "login",
        context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermitLimit = loginPermitLimit,
                    QueueLimit = 0,
                }
            )
    );

    // Endpoints sensíveis e anônimos que não passam por "login": protege
    // esqueci-senha (email bombing / gasto de cota do Resend),
    // redefinir-senha e completar-convite (brute-force de token) contra
    // abuso não autenticado. Também por IP.
    options.AddPolicy(
        "auth-sensitive",
        context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermitLimit = authSensitivePermitLimit,
                    QueueLimit = 0,
                }
            )
    );
});

// Bearer no OpenAPI/Swagger já configurado acima, junto da declaração
// original de AddOpenApi (evita registrar o serviço duas vezes).

try
{
    var app = builder.Build();

    if (app.Environment.IsDevelopment() && Assembly.GetEntryAssembly()?.GetName().Name != "ef")
    {
        app.MapOpenApi();
    }

    app.UseRequestLocalization(localizationOptions);
    app.UseHttpsRedirection();

    // Handler global só para exceções que escaparem dos try/catch de cada
    // controller (ex: erro de banco inesperado). Os catches específicos que já
    // existem em cada endpoint continuam devolvendo suas mensagens normalmente;
    // isso aqui é a rede de segurança pra não vazar stack trace/detalhe interno
    // em algo que ninguém previu.
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var feature =
                context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            if (feature?.Error is not null)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(
                    feature.Error,
                    "Erro não tratado na requisição {Path}",
                    context.Request.Path
                );
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(
                new { message = "Ocorreu um erro interno inesperado." }
            );
        });
    });

    // Headers de segurança básicos (sem precisar de pacote extra).
    app.Use(
        async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin-allow-popups";
            await next();
        }
    );

    app.UseCors("AppCors");
    app.UseRateLimiter();
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

    var e = ex;
    while (e != null)
    {
        Console.Error.WriteLine(e.GetType().FullName);
        Console.Error.WriteLine(e.Message);
        Console.Error.WriteLine(e.StackTrace);
        Console.Error.WriteLine("--------------------------------");
        e = e.InnerException;
    }

    Console.Error.WriteLine("==================================================\n\n");
    throw;
}
