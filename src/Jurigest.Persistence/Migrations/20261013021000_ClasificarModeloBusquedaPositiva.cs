using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Jurigest.Persistence.Migrations;

[DbContext(typeof(JurigestDbContext))]
[Migration("20261013021000_ClasificarModeloBusquedaPositiva")]
public class ClasificarModeloBusquedaPositiva : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
SELECT Id,Resultado INTO RespaldoClasificacionBusquedaPositiva FROM ModelosEstampe
WHERE Nombre=N'Busqueda Positiva' AND TipoDiligencia=112 AND Resultado IS NULL;
UPDATE m SET Resultado=1 FROM ModelosEstampe m JOIN RespaldoClasificacionBusquedaPositiva b ON b.Id=m.Id;
""");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
UPDATE m SET Resultado=b.Resultado FROM ModelosEstampe m JOIN RespaldoClasificacionBusquedaPositiva b ON b.Id=m.Id;
DROP TABLE RespaldoClasificacionBusquedaPositiva;
""");
    }
}
