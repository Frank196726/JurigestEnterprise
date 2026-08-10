using Jurigest.Domain.Judicial.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class DiligenciaConfiguration : IEntityTypeConfiguration<Diligencia>
{
    public void Configure(EntityTypeBuilder<Diligencia> builder)
    {
        builder.ToTable("Diligencias");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Descripcion)
            .IsRequired();

        builder.Property(x => x.Tipo)
            .HasConversion<int>();

        builder.Property(x => x.Estado)
            .HasConversion<int>();

        builder.Property(x => x.Latitud)
            .HasPrecision(9, 6);

        builder.Property(x => x.Longitud)
            .HasPrecision(10, 6);
    }
}
