using Jurigest.Domain.Judicial.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Jurigest.Persistence.Configurations;
public sealed class MateriaConfiguration : IEntityTypeConfiguration<MateriaCatalogo>
{
    public void Configure(EntityTypeBuilder<MateriaCatalogo> builder)
    {
        builder.ToTable("Materias"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Nombre).IsUnique();
        builder.HasData(new { Id = Guid.Parse("ab854cf1-3121-4bf3-912f-2157537ab901"), Nombre = "Ejecutivo" });
    }
}
