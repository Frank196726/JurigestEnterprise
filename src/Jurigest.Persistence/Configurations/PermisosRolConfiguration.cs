using Jurigest.Domain.Seguridad.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class PermisosRolConfiguration : IEntityTypeConfiguration<PermisosRol>
{
    public void Configure(EntityTypeBuilder<PermisosRol> builder)
    {
        builder.ToTable("PermisosRoles");
        builder.HasKey(x => x.Clave);
        builder.Property(x => x.Clave).HasMaxLength(80);
        builder.Property(x => x.SeleccionJson).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
