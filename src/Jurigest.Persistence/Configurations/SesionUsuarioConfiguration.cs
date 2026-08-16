using Jurigest.Domain.Seguridad.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class SesionUsuarioConfiguration
    : IEntityTypeConfiguration<SesionUsuario>
{
    public void Configure(
        EntityTypeBuilder<SesionUsuario> builder)
    {
        builder.ToTable("SesionesUsuario");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UsuarioId)
            .IsRequired();

        builder.Property(x => x.RefreshTokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.VersionSeguridad)
            .IsRequired();

        builder.Property(x => x.FechaCreacionUtc)
            .IsRequired();

        builder.Property(x => x.ExpiraUtc)
            .IsRequired();

        builder.Property(x => x.RevocadaUtc);

        builder.Property(x => x.ReemplazadaPorId);

        builder.Property(x => x.DireccionIp)
            .HasMaxLength(45);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(512);

        builder.HasIndex(x => x.RefreshTokenHash)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.UsuarioId,
            x.RevocadaUtc
        });

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}