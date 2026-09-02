using Jurigest.Domain.Judicial.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class TipoCausaCatalogoConfiguration
    : IEntityTypeConfiguration<TipoCausaCatalogo>
{
    public void Configure(
        EntityTypeBuilder<TipoCausaCatalogo> builder)
    {
        builder.ToTable("TiposCausa");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Codigo)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Activo)
            .IsRequired();

        builder.Property(x => x.FechaCreacion)
            .IsRequired();

        builder.HasIndex(x => x.Nombre)
            .IsUnique();

        builder.HasIndex(x => x.Codigo)
            .IsUnique();

        builder.HasData(
            new
            {
                Id = Guid.Parse(
                    "20000000-0000-0000-0000-000000000001"),

                Nombre = "Civil",

                Codigo = "C",

                Activo = true,

                FechaCreacion = new DateTime(
                    2026,
                    8,
                    25,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc)
            });
    }
}