using Jurigest.Domain.Judicial.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class ResolucionConfiguration
    : IEntityTypeConfiguration<Resolucion>
{
    public void Configure(EntityTypeBuilder<Resolucion> builder)
    {
        builder.ToTable("Resoluciones");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Tipo)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Fecha)
            .IsRequired();

        builder.Property(x => x.Descripcion)
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasOne<Causa>()
            .WithMany()
            .HasForeignKey(x => x.CausaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CausaId);
    }
}