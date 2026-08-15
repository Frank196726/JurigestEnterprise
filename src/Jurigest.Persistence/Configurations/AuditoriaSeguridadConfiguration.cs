using Jurigest.Domain.Seguridad.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class AuditoriaSeguridadConfiguration
    : IEntityTypeConfiguration<AuditoriaSeguridad>
{
    public void Configure(
        EntityTypeBuilder<AuditoriaSeguridad> builder)
    {
        builder.ToTable("AuditoriasSeguridad");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UsuarioActorId);

        builder.Property(x => x.Accion)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.UsuarioAfectadoId);

        builder.Property(x => x.Detalle)
            .HasMaxLength(1000);

        builder.Property(x => x.DireccionIp)
            .HasMaxLength(45);

        builder.Property(x => x.FechaUtc)
            .IsRequired();

        builder.HasIndex(x => x.FechaUtc);

        builder.HasIndex(x => x.UsuarioActorId);

        builder.HasIndex(x => x.UsuarioAfectadoId);
    }
}