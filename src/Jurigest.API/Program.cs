using Jurigest.Application.Judicial.Causas.Commands.CrearCausa;
using Jurigest.Persistence;
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Habilita los controladores
app.MapControllers();

foreach (var endpoint in app.Services
    .GetRequiredService<EndpointDataSource>()
    .Endpoints)
{
    Console.WriteLine(endpoint.DisplayName);
}
app.Run();