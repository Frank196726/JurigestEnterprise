using Jurigest.Domain.Judicial.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class DocumentoConfiguration
    : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> builder)
    {
        builder.ToTable("Documentos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Tipo)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.RutaArchivo)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.TamanoBytes)
            .IsRequired();

        builder.Property(x => x.FechaRegistro)
            .IsRequired();

        builder.HasOne<Causa>()
            .WithMany()
            .HasForeignKey(x => x.CausaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CausaId);
    }
}