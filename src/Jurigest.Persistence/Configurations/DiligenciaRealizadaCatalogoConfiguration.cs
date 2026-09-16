using Jurigest.Domain.Judicial.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class DiligenciaRealizadaCatalogoConfiguration
    : IEntityTypeConfiguration<DiligenciaRealizadaCatalogo>
{
    public void Configure(
        EntityTypeBuilder<DiligenciaRealizadaCatalogo> builder)
    {
        builder.ToTable("DiligenciasRealizadas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CodigoTipoDiligencia)
            .IsRequired();

        builder.Property(x => x.Activo)
            .IsRequired();

        builder.Property(x => x.FechaCreacion)
            .IsRequired();

        builder.HasIndex(x => new
            {
                x.CodigoTipoDiligencia,
                x.Nombre
            })
            .IsUnique();

        builder.HasIndex(x =>
            x.CodigoTipoDiligencia);
    }
}
