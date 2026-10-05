using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations;

[DbContext(typeof(JurigestDbContext))]
[Migration("20260925123000_AsociarArticulo44AOtrasResoluciones")]
public partial class AsociarArticulo44AOtrasResoluciones : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DECLARE @codigoTipo int = (
                SELECT TOP (1) CodigoSistema
                FROM TiposDiligencia
                WHERE Nombre = N'Notificación Otras Resoluciones'
                  AND CodigoSistema IS NOT NULL
            );

            IF @codigoTipo IS NULL
            BEGIN
                SELECT @codigoTipo =
                    CASE
                        WHEN ISNULL(MAX(CodigoSistema), 99) < 100 THEN 100
                        ELSE MAX(CodigoSistema) + 1
                    END
                FROM TiposDiligencia;

                INSERT INTO TiposDiligencia
                    (Id, Nombre, CodigoSistema, Activo, FechaCreacion)
                VALUES
                    (NEWID(), N'Notificación Otras Resoluciones',
                     @codigoTipo, 1, SYSUTCDATETIME());
            END;

            UPDATE DiligenciasEncargadas
            SET CodigoTipoDiligencia = @codigoTipo
            WHERE Nombre LIKE N'%Notificación%44%'
               OR Nombre LIKE N'%Notificacion%44%';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DECLARE @codigoTipo int = (
                SELECT TOP (1) CodigoSistema
                FROM TiposDiligencia
                WHERE Nombre = N'Notificación Otras Resoluciones'
            );

            UPDATE DiligenciasEncargadas
            SET CodigoTipoDiligencia = 1
            WHERE CodigoTipoDiligencia = @codigoTipo
              AND (Nombre LIKE N'%Notificación%44%'
                   OR Nombre LIKE N'%Notificacion%44%');

            DELETE FROM TiposDiligencia
            WHERE Nombre = N'Notificación Otras Resoluciones'
              AND NOT EXISTS (
                  SELECT 1
                  FROM DiligenciasEncargadas
                  WHERE CodigoTipoDiligencia = @codigoTipo
              );
            """);
    }
}
