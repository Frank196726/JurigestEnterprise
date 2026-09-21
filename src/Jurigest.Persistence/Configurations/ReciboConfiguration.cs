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
