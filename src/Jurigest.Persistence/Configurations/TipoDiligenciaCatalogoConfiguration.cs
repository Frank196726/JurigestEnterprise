using Jurigest.Domain.Judicial.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jurigest.Persistence.Configurations;

public sealed class TipoDiligenciaCatalogoConfiguration
    : IEntityTypeConfiguration<TipoDiligenciaCatalogo>
{
    public void Configure(
        EntityTypeBuilder<TipoDiligenciaCatalogo> builder)
    {
        builder.ToTable("TiposDiligencia");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.CodigoSistema);

        builder.Property(x => x.Activo)
            .IsRequired();

        builder.Property(x => x.FechaCreacion)
            .IsRequired();

        builder.HasIndex(x => x.Nombre)
            .IsUnique();

        builder.HasIndex(x => x.CodigoSistema)
            .IsUnique()
            .HasFilter("[CodigoSistema] IS NOT NULL");

        builder.HasData(
            Crear(
                "10000000-0000-0000-0000-000000000001",
                "Notificación",
                1),

            Crear(
                "10000000-0000-0000-0000-000000000002",
                "Requerimiento de pago",
                2),

            Crear(
                "10000000-0000-0000-0000-000000000003",
                "Embargo",
                3),

            Crear(
                "10000000-0000-0000-0000-000000000004",
                "Lanzamiento",
                4),

            Crear(
                "10000000-0000-0000-0000-000000000005",
                "Retiro de exhorto",
                5),

            Crear(
                "10000000-0000-0000-0000-000000000006",
                "Retiro de expediente",
                6),

            Crear(
                "10000000-0000-0000-0000-000000000007",
                "Incautación",
                7),

            Crear(
                "10000000-0000-0000-0000-000000000008",
                "Protesto",
                8),

            Crear(
                "10000000-0000-0000-0000-000000000009",
                "Citación",
                9),

            Crear(
                "10000000-0000-0000-0000-000000000099",
                "Otro",
                99));
    }

    private static object Crear(
        string id,
        string nombre,
        int codigoSistema)
    {
        return new
        {
            Id = Guid.Parse(id),
            Nombre = nombre,
            CodigoSistema = (int?)codigoSistema,
            Activo = true,
            FechaCreacion = new DateTime(
                2026,
                8,
                24,
                0,
                0,
                0,
                DateTimeKind.Utc)
        };
    }
}