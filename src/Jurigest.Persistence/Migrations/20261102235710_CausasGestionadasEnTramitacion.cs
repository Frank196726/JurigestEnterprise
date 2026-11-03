using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CausasGestionadasEnTramitacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Antes ninguna causa salía de "Ingresada" (1). Las que ya tienen
            // una gestión registrada pasan a "En tramitación" (2).
            migrationBuilder.Sql(
                "UPDATE Causas SET Estado = 2 " +
                "WHERE Estado = 1 AND FechaGestionCausa IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No se revierte: no es posible distinguir las causas corregidas
            // por esta migración de las que se cambiaron después.
        }
    }
}
