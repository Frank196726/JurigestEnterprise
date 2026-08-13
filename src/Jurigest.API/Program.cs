using Jurigest.Application.Judicial.Causas.Commands.CrearCausa;
using Jurigest.Persistence;
using Jurigest.API.Security;
using Jurigest.Application.Abstractions.Security;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Habilita los controladores
app.MapControllers();

foreach (var endpoint in app.Services
    .GetRequiredService<EndpointDataSource>()
    .Endpoints)
{
    Console.WriteLine(endpoint.DisplayName);
}
app.Run();