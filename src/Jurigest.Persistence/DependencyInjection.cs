using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Persistence.Context;
using Jurigest.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Jurigest.Application.Abstractions.Storage;
using Jurigest.Persistence.Storage;
using Jurigest.Application.Abstractions.Security;
using Jurigest.Persistence.Security;

namespace Jurigest.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<JurigestDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICausaRepository, CausaRepository>();

        services.AddScoped<IDiligenciaRepository, DiligenciaRepository>();

        services.AddScoped<IDocumentoRepository, DocumentoRepository>();

        services.AddScoped<IResolucionRepository, ResolucionRepository>();

        services.AddSingleton<IArchivoStorage, ArchivoStorage>();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        services.AddScoped<
            IAuditoriaSeguridadRepository,
            AuditoriaSeguridadRepository>();

        services.AddScoped<
            ISesionUsuarioRepository,
            SesionUsuarioRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddSingleton<
            IRefreshTokenService,
            RefreshTokenService>();

        return services;
    }
}