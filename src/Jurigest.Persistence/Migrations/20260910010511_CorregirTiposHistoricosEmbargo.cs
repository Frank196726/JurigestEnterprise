using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CorregirTiposHistoricosEmbargo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE Diligencias
                SET Tipo = 3
                WHERE Descripcion = N'Embargo'
                  AND Tipo IN (99, 103);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Corrección de datos históricos no reversible:
            // después de normalizar 99 y 103 a 3 no es posible
            // reconstruir de forma segura el valor original de cada fila.
        }
    }
}