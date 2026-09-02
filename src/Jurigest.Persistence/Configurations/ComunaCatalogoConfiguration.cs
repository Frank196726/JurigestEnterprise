using Jurigest.Domain.Judicial.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class ComunaCatalogoConfiguration
    : IEntityTypeConfiguration<ComunaCatalogo>
{
    public void Configure(
        EntityTypeBuilder<ComunaCatalogo> builder)
    {
        builder.ToTable("Comunas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Activo)
            .IsRequired();

        builder.Property(x => x.FechaCreacion)
            .IsRequired();

        builder.HasIndex(x => x.Nombre)
            .IsUnique();
    }
}