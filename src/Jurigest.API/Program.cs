using Jurigest.Application.Contabilidad;
using Jurigest.Application.Judicial;
using Jurigest.Persistence.Contabilidad;
using Jurigest.Persistence.Database;
using Jurigest.Persistence.Judicial;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<JurigestDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=jurigest.db"));

builder.Services.AddScoped<ICuentaRepository, EfCuentaRepository>();
builder.Services.AddScoped<IAsientoRepository, EfAsientoRepository>();
builder.Services.AddScoped<ContabilidadApplicationService>();

builder.Services.AddSingleton<ICausaRepository, InMemoryCausaRepository>();
builder.Services.AddSingleton<IEncargoRepository, InMemoryEncargoRepository>();
builder.Services.AddScoped<JudicialApplicationService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
