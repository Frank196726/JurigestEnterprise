using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Domain.Seguridad.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class UsuarioReceptorConfiguration
    : IEntityTypeConfiguration<UsuarioReceptor>
{
    public void Configure(EntityTypeBuilder<UsuarioReceptor> builder)
    {
        builder.ToTable("UsuariosReceptores");

        builder.HasKey(x => new { x.UsuarioId, x.ReceptorJudicialId });

        builder.Property(x => x.FechaAsignacion)
            .IsRequired();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ReceptorJudicialCatalogo>()
            .WithMany()
            .HasForeignKey(x => x.ReceptorJudicialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ReceptorJudicialId);
    }
}
