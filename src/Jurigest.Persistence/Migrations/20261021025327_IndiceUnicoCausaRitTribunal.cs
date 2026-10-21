using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IndiceUnicoCausaRitTribunal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Falla con un mensaje claro en vez del error genérico de CREATE UNIQUE INDEX.
            migrationBuilder.Sql("""
IF EXISTS (SELECT 1 FROM Causas GROUP BY Rit, Tribunal HAVING COUNT(*) > 1)
    THROW 50001, N'Hay causas duplicadas por (Rit, Tribunal). Ejecute database/limpieza_calidad_datos_2026-10-20.sql antes de esta migración.', 1;
""");

            migrationBuilder.CreateIndex(
                name: "UX_Causas_Rit_Tribunal",
                table: "Causas",
                columns: new[] { "Rit", "Tribunal" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Causas_Rit_Tribunal",
                table: "Causas");
        }
    }
}
