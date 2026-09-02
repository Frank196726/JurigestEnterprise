using Jurigest.Domain.Judicial.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class AbogadoCatalogoConfiguration
    : IEntityTypeConfiguration<AbogadoCatalogo>
{
    public void Configure(
        EntityTypeBuilder<AbogadoCatalogo> builder)
    {
        builder.ToTable("Abogados");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.UsuarioId)
            .IsRequired(false);

        builder.Property(x => x.Activo)
            .IsRequired();

        builder.Property(x => x.FechaCreacion)
            .IsRequired();

        builder.HasIndex(x => x.Nombre)
            .IsUnique();

        builder.HasIndex(x => x.UsuarioId);
    }
}