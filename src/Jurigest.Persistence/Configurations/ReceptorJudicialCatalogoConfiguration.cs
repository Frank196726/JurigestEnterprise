using Jurigest.Domain.Judicial.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class ReceptorJudicialCatalogoConfiguration
    : IEntityTypeConfiguration<ReceptorJudicialCatalogo>
{
    public void Configure(
        EntityTypeBuilder<ReceptorJudicialCatalogo> builder)
    {
        builder.ToTable("ReceptoresJudiciales");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Activo)
            .IsRequired();

        builder.Property(x => x.FechaCreacion)
            .IsRequired();

        builder.HasIndex(x => x.Nombre)
            .IsUnique();
    }
}