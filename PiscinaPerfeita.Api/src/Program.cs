using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Add services to the container.
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
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
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
                var allowsAnonymous = context.Description.ActionDescriptor.EndpointMetadata.Any(
                    m => m is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute
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

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter(
        "login",
        limiterOptions =>
        {
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.PermitLimit = loginPermitLimit;
            limiterOptions.QueueLimit = 0;
        }
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
            var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            if (feature?.Error is not null)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(feature.Error, "Erro não tratado na requisição {Path}", context.Request.Path);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "Ocorreu um erro interno inesperado." });
        });
    });

    // Headers de segurança básicos (sem precisar de pacote extra).
    app.Use(
        async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
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
