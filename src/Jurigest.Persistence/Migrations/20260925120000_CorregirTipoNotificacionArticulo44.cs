using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations;

[DbContext(typeof(JurigestDbContext))]
[Migration("20260925120000_CorregirTipoNotificacionArticulo44")]
public partial class CorregirTipoNotificacionArticulo44 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE DiligenciasEncargadas
            SET CodigoTipoDiligencia = 1
            WHERE CodigoTipoDiligencia IS NULL
              AND (Nombre LIKE N'%Notificación%44%'
                   OR Nombre LIKE N'%Notificacion%44%');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE DiligenciasEncargadas
            SET CodigoTipoDiligencia = NULL
            WHERE CodigoTipoDiligencia = 1
              AND (Nombre LIKE N'%Notificación%44%'
                   OR Nombre LIKE N'%Notificacion%44%');
            """);
    }
}
