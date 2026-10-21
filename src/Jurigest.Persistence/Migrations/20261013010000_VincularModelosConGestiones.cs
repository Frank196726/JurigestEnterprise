using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Jurigest.Persistence.Migrations;
[DbContext(typeof(JurigestDbContext))]
[Migration("20261013010000_VincularModelosConGestiones")]
public class VincularModelosConGestiones : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
INSERT INTO DiligenciasRealizadas (Id, Nombre, CodigoTipoDiligencia, Activo, FechaCreacion, Arancel)
SELECT NEWID(), m.Nombre, m.TipoDiligencia, 1, SYSUTCDATETIME(), NULL
FROM ModelosEstampe m WHERE m.Activo=1 AND m.DiligenciaRealizadaId IS NULL
AND NOT EXISTS (SELECT 1 FROM DiligenciasRealizadas d WHERE d.Nombre=m.Nombre AND d.CodigoTipoDiligencia=m.TipoDiligencia);
UPDATE d SET Activo=1 FROM DiligenciasRealizadas d JOIN ModelosEstampe m
ON d.Nombre=m.Nombre AND d.CodigoTipoDiligencia=m.TipoDiligencia
WHERE m.Activo=1 AND m.DiligenciaRealizadaId IS NULL;
UPDATE m SET DiligenciaRealizadaId=d.Id FROM ModelosEstampe m JOIN DiligenciasRealizadas d
ON d.Nombre=m.Nombre AND d.CodigoTipoDiligencia=m.TipoDiligencia
WHERE m.Activo=1 AND m.DiligenciaRealizadaId IS NULL;
""");
    }
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
