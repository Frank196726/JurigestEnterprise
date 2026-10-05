using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Domain.Seguridad.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class RolCatalogoConfiguration : IEntityTypeConfiguration<RolCatalogo>
{
    public void Configure(EntityTypeBuilder<RolCatalogo> builder)
    {
        builder.ToTable("RolesCatalogo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
        builder.Property(x => x.NombreNormalizado).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Perfil).HasConversion<int>();
        builder.HasIndex(x => x.NombreNormalizado).IsUnique();
        builder.HasData(
            new RolCatalogo(Guid.Parse("71000000-0000-0000-0000-000000000001"), "Receptor", RolUsuario.Procurador),
            new RolCatalogo(Guid.Parse("71000000-0000-0000-0000-000000000002"), "Operador", RolUsuario.Procurador),
            new RolCatalogo(Guid.Parse("71000000-0000-0000-0000-000000000003"), "Digitador", RolUsuario.Procurador));
    }
}
