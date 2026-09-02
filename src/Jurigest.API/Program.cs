using Jurigest.Application.Judicial.Causas.Commands.CrearCausa;
using Jurigest.Persistence;
using Jurigest.API.Security;
using Jurigest.Application.Abstractions.Security;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using MediatR;
using Microsoft.AspNetCore.Routing;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddJsonConsole(options => options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ");
}

builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = 12 * 1024 * 1024);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    var proxy = builder.Configuration["ReverseProxy:KnownProxy"];
    if (IPAddress.TryParse(proxy, out var address)) options.KnownProxies.Add(address);
});

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

// MVC
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
// OpenAPI
builder.Services.AddOpenApi();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CrearCausaCommand).Assembly);
});

// Entity Framework / Persistence
builder.Services.AddPersistence(builder.Configuration);

// JWT
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

builder.Services.AddScoped<JwtTokenValidationEvents>();

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "Falta la configuracion Jwt:Issuer.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "Falta la configuracion Jwt:Audience.");

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Falta la configuracion Jwt:Key.");

if (!builder.Environment.IsDevelopment())
{
    if (jwtKey.Length < 64) throw new InvalidOperationException("Jwt:Key debe contener al menos 64 caracteres en producción.");
    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection"))) throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.");
    var recoveryUrl = builder.Configuration["Email:Smtp:RecoveryUrl"];
    if (!Uri.TryCreate(recoveryUrl, UriKind.Absolute, out var recoveryUri) || recoveryUri.Scheme != Uri.UriSchemeHttps) throw new InvalidOperationException("Email:Smtp:RecoveryUrl debe usar HTTPS en producción.");
}

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

        options.EventsType =
            typeof(JwtTokenValidationEvents);
    });
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy(
        "Login",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    httpContext.Connection.RemoteIpAddress
                        ?.ToString()
                    ?? "ip-desconocida",
                factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder =
                            QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    }));

    options.AddPolicy(
    "RecuperacionPassword",
    httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                httpContext.Connection.RemoteIpAddress
                    ?.ToString()
                ?? "ip-desconocida",
            factory: _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 3,
                    Window = TimeSpan.FromMinutes(15),
                    QueueLimit = 0,
                    QueueProcessingOrder =
                        QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                }));
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "DocumentosLectura",
        policy => policy.RequireRole(
            "Administrador",
            "Abogado",
            "Procurador",
            "Consulta"));

    options.AddPolicy(
        "DocumentosCarga",
        policy => policy.RequireRole(
            "Administrador",
            "Abogado",
            "Procurador"));

    options.AddPolicy(
        "DocumentosEliminacion",
        policy => policy.RequireRole(
            "Administrador"));

    options.AddPolicy(
        "ResolucionesLectura",
        policy => policy.RequireRole(
            "Administrador",
            "Abogado",
            "Procurador",
            "Consulta"));

    options.AddPolicy(
        "ResolucionesRegistro",
        policy => policy.RequireRole(
            "Administrador",
            "Abogado",
            "Procurador"));

    options.AddPolicy(
        "ResolucionesEliminacion",
        policy => policy.RequireRole(
            "Administrador"));

    options.AddPolicy(
        "CausasLectura",
        policy => policy.RequireRole(
            "Administrador",
            "Abogado",
            "Procurador",
            "Consulta"));

    options.AddPolicy(
        "CausasEscritura",
        policy => policy.RequireRole(
            "Administrador",
            "Abogado"));

    options.AddPolicy(
        "CausasEliminacion",
        policy => policy.RequireRole(
            "Administrador"));

    options.AddPolicy(
        "DiligenciasLectura",
        policy => policy.RequireRole(
            "Administrador",
            "Abogado",
            "Procurador",
            "Consulta"));

    options.AddPolicy(
        "DiligenciasGestion",
        policy => policy.RequireRole(
            "Administrador",
            "Abogado",
            "Procurador"));
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception exception)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Jurigest.Api.Error");
        logger.LogError(exception, "Error no controlado. TraceId={TraceId} Metodo={Metodo} Ruta={Ruta} Usuario={Usuario}", context.TraceIdentifier, context.Request.Method, context.Request.Path, context.User.Identity?.Name ?? "anonimo");
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { mensaje = "Ocurrió un error interno.", traceId = context.TraceIdentifier });
        }
    }
});

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next(context);
});

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health/live", () => Results.Ok(new { estado = "saludable", servicio = "Jurigest.API", horaUtc = DateTime.UtcNow }));
app.MapGet("/health/ready", async (JurigestDbContext db, CancellationToken ct) =>
{
    var inicio = DateTime.UtcNow;
    var disponible = await db.Database.CanConnectAsync(ct);
    return disponible
        ? Results.Ok(new { estado = "saludable", baseDatos = "disponible", duracionMs = (DateTime.UtcNow - inicio).TotalMilliseconds, horaUtc = DateTime.UtcNow })
        : Results.Json(new { estado = "degradado", baseDatos = "no disponible", horaUtc = DateTime.UtcNow }, statusCode: 503);
});

app.Run();

public partial class Program
{
}
