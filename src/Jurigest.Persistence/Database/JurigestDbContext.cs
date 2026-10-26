using Jurigest.Domain.Contabilidad.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Database;

public sealed class JurigestDbContext : DbContext
{
    public JurigestDbContext(DbContextOptions<JurigestDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cuenta> Cuentas => Set<Cuenta>();

    public DbSet<AsientoContable> Asientos => Set<AsientoContable>();

    public DbSet<MovimientoContable> Movimientos => Set<MovimientoContable>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cuenta>(entity =>
        {
            entity.ToTable("Cuentas");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Codigo).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Saldo).HasColumnType("decimal(18,2)");
            entity.HasMany(c => c.Movimientos)
                .WithOne()
                .HasForeignKey(m => m.CuentaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AsientoContable>(entity =>
        {
            entity.ToTable("Asientos");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Descripcion).IsRequired().HasMaxLength(500);
            entity.Property(a => a.Fecha).IsRequired();
            entity.HasMany(a => a.Movimientos)
                .WithOne()
                .HasForeignKey(m => m.AsientoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MovimientoContable>(entity =>
        {
            entity.ToTable("Movimientos");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.CuentaId).IsRequired();
            entity.Property(m => m.AsientoId);
            entity.Property(m => m.Monto).HasColumnType("decimal(18,2)");
            entity.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20);
            entity.Property(m => m.Descripcion).IsRequired().HasMaxLength(500);
            entity.Property(m => m.Fecha).IsRequired();
        });
    }
}
