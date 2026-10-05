using Jurigest.Domain.Judicial.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Jurigest.Persistence.Configurations;
public sealed class VehiculoOpcionConfiguration : IEntityTypeConfiguration<VehiculoOpcion>
{
    public void Configure(EntityTypeBuilder<VehiculoOpcion> b)
    {
        b.ToTable("VehiculoOpciones"); b.HasKey(x => x.Id);
        b.Property(x => x.Categoria).HasMaxLength(40); b.Property(x => x.Nombre).HasMaxLength(200);
        b.HasIndex(x => new { x.Categoria, x.Nombre }).IsUnique();
    }
}
