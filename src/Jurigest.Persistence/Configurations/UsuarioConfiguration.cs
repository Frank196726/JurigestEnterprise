using Jurigest.Domain.Seguridad.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class UsuarioConfiguration
    : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.Rut).HasMaxLength(12);
        builder.Property(x => x.Telefono).HasMaxLength(30);
        builder.Property(x => x.Direccion).HasMaxLength(250);
        builder.Property(x => x.NumeroOficina).HasMaxLength(50);

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Rol)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.NombreRolPersonalizado).HasMaxLength(80);
        builder.HasOne<RolCatalogo>().WithMany().HasForeignKey(x => x.RolCatalogoId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.DebeCambiarPassword).IsRequired();

        builder.Property(x => x.Activo)
            .IsRequired();

        builder.Property(x => x.VersionSeguridad)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.IntentosFallidos)
               .HasDefaultValue(0)
               .IsRequired();

        builder.Property(x => x.BloqueadoHastaUtc);

        builder.Property(x => x.FechaCreacion)
               .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasIndex(x => x.Rut)
            .IsUnique()
            .HasFilter("[Rut] IS NOT NULL");
    }
}
