using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Seguridad.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Context;

public class JurigestDbContext : DbContext
{
    public JurigestDbContext(DbContextOptions<JurigestDbContext> options)
        : base(options)
    {
    }

    public DbSet<Causa> Causas => Set<Causa>();

    public DbSet<Diligencia> Diligencias => Set<Diligencia>();

    public DbSet<Documento> Documentos => Set<Documento>();

    public DbSet<Resolucion> Resoluciones => Set<Resolucion>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<SesionUsuario> SesionesUsuario =>
    Set<SesionUsuario>();

    public DbSet<AuditoriaSeguridad> AuditoriasSeguridad =>
    Set<AuditoriaSeguridad>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JurigestDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}