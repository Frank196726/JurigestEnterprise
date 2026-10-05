using Jurigest.Domain.Judicial.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class DemandadoConfiguration : IEntityTypeConfiguration<Demandado>
{
    public void Configure(EntityTypeBuilder<Demandado> builder)
    {
        builder.ToTable("Demandados");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Rut).HasMaxLength(12);
        builder.Property(x => x.RepresentanteLegal).HasMaxLength(200);
        builder.Property(x => x.RutRepresentanteLegal).HasMaxLength(12);
        builder.Property(x => x.TipoPersona).HasConversion<int>().IsRequired();
        builder.HasIndex(x => new { x.CausaId, x.EsPrincipal })
            .IsUnique().HasFilter("[EsPrincipal] = 1");
        builder.HasMany(x => x.Avales).WithOne().HasForeignKey(x => x.DemandadoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Avales).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
