using Jurigest.Domain.Judicial.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class CausaConfiguration
    : IEntityTypeConfiguration<Causa>
{
    public void Configure(
        EntityTypeBuilder<Causa> builder)
    {
        builder.ToTable("Causas");
        builder.Ignore(x => x.PuedeCorregirIngreso);
        builder.Ignore(x => x.PrimeraDiligencia);

        builder.HasKey(
            x => x.Id);

        builder.Property(
                x => x.Rit)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(
                x => x.TipoCausaId)
            .IsRequired(false);

        builder.Property(
                x => x.NumeroRol)
            .HasMaxLength(30)
            .IsRequired(false);

        builder.Property(
                x => x.Tribunal)
            .HasConversion(
                valor => Jurigest.Domain.Judicial.IdentificacionCausa.NormalizarTribunal(valor),
                valor => Jurigest.Domain.Judicial.IdentificacionCausa.NormalizarTribunal(valor))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(
                x => x.Descripcion)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(
                x => x.FechaCreacion)
            .IsRequired();

        // Fecha original en que la causa fue encargada.
        builder.Property(
                x => x.FechaEncargoCausa)
            .IsRequired();

        // Permanece NULL hasta que exista una
        // gestión efectivamente realizada.
        builder.Property(
                x => x.FechaGestionCausa)
            .IsRequired(false);

        builder.Property(
                x => x.Estado)
            .HasConversion<int>();

        builder.HasIndex(
            x => x.TipoCausaId);

        builder.HasIndex(
            x => new
            {
                x.TipoCausaId,
                x.NumeroRol
            });

        // Este índice ayudará posteriormente al dashboard
        // y a consultas de causas pendientes de gestión.
        builder.HasIndex(
            x => x.FechaGestionCausa);
    }
}