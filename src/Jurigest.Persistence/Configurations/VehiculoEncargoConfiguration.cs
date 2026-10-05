using Jurigest.Domain.Judicial.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Jurigest.Persistence.Configurations;
public sealed class VehiculoEncargoConfiguration : IEntityTypeConfiguration<VehiculoEncargo>
{
    public void Configure(EntityTypeBuilder<VehiculoEncargo> b)
    {
        b.ToTable("VehiculosEncargo"); b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(300); b.Property(x => x.Direccion).HasMaxLength(500);
        b.Property(x => x.Patente).HasMaxLength(20); b.Property(x => x.Marca).HasMaxLength(100);
        b.Property(x => x.Modelo).HasMaxLength(200); b.Property(x => x.Color).HasMaxLength(100);
        b.Property(x => x.Cilindrada).HasMaxLength(50); b.Property(x => x.Motor).HasMaxLength(100);
        b.Property(x => x.Chasis).HasMaxLength(100); b.Property(x => x.Observaciones).HasMaxLength(1000);
        b.Property(x => x.TipoVehiculo).HasMaxLength(200);
        b.Property(x => x.Serie).HasMaxLength(200);
        b.Property(x => x.TipoAdquisicion).HasMaxLength(200);
        b.Property(x => x.AlzamientoProhibicion).HasMaxLength(200);
        b.Property(x => x.LimitacionDominio).HasMaxLength(200);
        b.Property(x => x.LugarSolicitud).HasMaxLength(200);
        b.Property(x => x.NumeroSolicitud).HasMaxLength(200);
        b.Property(x => x.TipoDocumento).HasMaxLength(200);
        b.Property(x => x.RutTitular).HasMaxLength(200);
        b.Property(x => x.DerechosInscripcion).HasPrecision(18, 0);
        b.HasIndex(x => new { x.DiligenciaId, x.Patente }).IsUnique();
        b.HasOne<Diligencia>().WithMany().HasForeignKey(x => x.DiligenciaId).OnDelete(DeleteBehavior.Restrict);
    }
}
