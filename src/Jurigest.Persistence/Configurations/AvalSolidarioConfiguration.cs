using Jurigest.Domain.Judicial.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class AvalSolidarioConfiguration : IEntityTypeConfiguration<AvalSolidario>
{
    public void Configure(EntityTypeBuilder<AvalSolidario> builder)
    {
        builder.ToTable("AvalesSolidarios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Rut).HasMaxLength(12);
        builder.Property(x => x.RepresentanteLegal).HasMaxLength(200);
        builder.Property(x => x.RutRepresentanteLegal).HasMaxLength(12);
        builder.Property(x => x.TipoPersona).HasConversion<int>().IsRequired();
        builder.HasIndex(x => x.DemandadoId);
    }
}
