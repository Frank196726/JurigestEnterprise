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

var builder = WebApplication.CreateBuilder(args);

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

foreach (var endpoint in app.Services
    .GetRequiredService<EndpointDataSource>()
    .Endpoints)
{
    Console.WriteLine(endpoint.DisplayName);
}

app.Run();

public partial class Program
{
}