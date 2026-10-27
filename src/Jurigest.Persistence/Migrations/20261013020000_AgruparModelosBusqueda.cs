using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Jurigest.Persistence.Migrations;

[DbContext(typeof(JurigestDbContext))]
[Migration("20261013020000_AgruparModelosBusqueda")]
public class AgruparModelosBusqueda : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
SELECT Id, TipoDiligencia, DiligenciaRealizadaId INTO RespaldoAgrupacionBusqueda
FROM ModelosEstampe WHERE Nombre IN
(N'Busqueda Frustrada',N'Busqueda Frustrada Persona Adulta',N'Busqueda Negativa',
N'Busqueda Negativa Conserje',N'Busqueda Negativa No Existe Calle',N'Busqueda Positiva',N'Busqueda Positiva Dom. Laboral');
IF NOT EXISTS (SELECT 1 FROM DiligenciasRealizadas WHERE Nombre=N'Búsqueda' AND CodigoTipoDiligencia=112)
INSERT INTO DiligenciasRealizadas (Id,Nombre,CodigoTipoDiligencia,Activo,FechaCreacion,Arancel)
VALUES (NEWID(),N'Búsqueda',112,1,SYSUTCDATETIME(),NULL);
UPDATE m SET TipoDiligencia=112, DiligenciaRealizadaId=d.Id
FROM ModelosEstampe m JOIN RespaldoAgrupacionBusqueda b ON b.Id=m.Id
CROSS JOIN DiligenciasRealizadas d WHERE d.Nombre=N'Búsqueda' AND d.CodigoTipoDiligencia=112;
""");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
UPDATE m SET TipoDiligencia=b.TipoDiligencia, DiligenciaRealizadaId=b.DiligenciaRealizadaId
FROM ModelosEstampe m JOIN RespaldoAgrupacionBusqueda b ON b.Id=m.Id;
DROP TABLE RespaldoAgrupacionBusqueda;
""");
    }
}
