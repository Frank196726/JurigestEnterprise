using Jurigest.Domain.Judicial.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public class CausaConfiguration : IEntityTypeConfiguration<Causa>
{
    public void Configure(EntityTypeBuilder<Causa> builder)
    {
        builder.ToTable("Causas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Rit)
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(x => x.Tribunal)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Descripcion)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.FechaCreacion)
               .IsRequired();

        builder.Property(x => x.Estado)
               .HasConversion<int>();
    }
}