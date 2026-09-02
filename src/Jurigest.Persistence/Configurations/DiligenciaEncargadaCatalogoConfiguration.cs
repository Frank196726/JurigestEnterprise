using Jurigest.Domain.Judicial.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class DiligenciaEncargadaCatalogoConfiguration
    : IEntityTypeConfiguration<DiligenciaEncargadaCatalogo>
{
    public void Configure(
        EntityTypeBuilder<DiligenciaEncargadaCatalogo> builder)
    {
        builder.ToTable("DiligenciasEncargadas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CodigoTipoDiligencia)
            .IsRequired(false);

        builder.Property(x => x.Activo)
            .IsRequired();

        builder.Property(x => x.FechaCreacion)
            .IsRequired();

        builder.HasIndex(x => x.Nombre)
            .IsUnique();

        builder.HasIndex(x => x.CodigoTipoDiligencia);
    }
}