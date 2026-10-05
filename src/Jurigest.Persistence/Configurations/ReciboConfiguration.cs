using Jurigest.Domain.Judicial.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class ReciboConfiguration : IEntityTypeConfiguration<Recibo>
{
    public void Configure(EntityTypeBuilder<Recibo> builder)
    {
        builder.ToTable("Recibos");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Numero).UseIdentityColumn();
        builder.HasIndex(x => x.Numero).IsUnique();
        builder.Property(x => x.Abogado).HasMaxLength(200);
        builder.Property(x => x.Receptor).HasMaxLength(200);
        builder.Property(x => x.Rol).HasMaxLength(100);
        builder.Property(x => x.Tribunal).HasMaxLength(200);
        builder.Property(x => x.Caratulado).HasMaxLength(500);
        builder.Property(x => x.DiligenciaEncargada).HasMaxLength(500);
        builder.Property(x => x.NumeroOperacion).HasMaxLength(100);
        builder.Property(x => x.Observacion).HasMaxLength(1000);
        builder.Property(x => x.DetalleAdicionales).HasMaxLength(1000);
        builder.Property(x => x.Cuantia).HasPrecision(18, 0);
        builder.Property(x => x.ValorGestion).HasPrecision(18, 0);
        builder.Property(x => x.TotalAdicionales).HasPrecision(18, 0);


        builder.Property(x => x.CausaId)
            .IsRequired();

        builder.Property(x => x.DiligenciaId)
            .IsRequired();

        builder.Property(x => x.DiligenciaRealizadaId)
            .IsRequired();

        builder.Property(x => x.DiligenciaRealizada)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Monto)
            .HasPrecision(18, 0)
            .IsRequired();

        builder.Property(x => x.Estado)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.FechaEmision)
            .IsRequired();

        builder.Property(x => x.FechaPago)
            .IsRequired(false);

        builder.HasIndex(x => x.CausaId);

        builder.HasIndex(x => x.DiligenciaRealizadaId);

        builder.HasIndex(x => x.DiligenciaId)
            .IsUnique();

        builder.HasOne<Causa>()
            .WithMany()
            .HasForeignKey(x => x.CausaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Diligencia>()
            .WithOne()
            .HasForeignKey<Recibo>(x => x.DiligenciaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
