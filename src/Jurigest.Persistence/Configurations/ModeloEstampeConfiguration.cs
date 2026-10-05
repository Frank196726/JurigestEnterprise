using Jurigest.Domain.Judicial.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class ModeloEstampeConfiguration : IEntityTypeConfiguration<ModeloEstampe>
{
    public void Configure(EntityTypeBuilder<ModeloEstampe> builder)
    {
        builder.ToTable("ModelosEstampe");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TipoDiligencia).HasConversion<int>().IsRequired();
        builder.Property(x => x.Resultado).HasConversion<int?>().IsRequired(false);
        builder.Property(x => x.DiligenciaRealizadaId).IsRequired(false);
        builder.Property(x => x.Contenido).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.Activo).IsRequired();
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.FechaActualizacion).IsRequired();
        builder.HasIndex(x => x.Nombre).IsUnique();
        builder.HasIndex(x => new { x.TipoDiligencia, x.Resultado, x.Activo });
        builder.HasIndex(x => new { x.DiligenciaRealizadaId, x.Resultado, x.Activo });
    }
}
