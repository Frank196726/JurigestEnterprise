using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CorregirTipoEmbargoDiligenciasEncargadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE DiligenciasEncargadas
                SET CodigoTipoDiligencia = 3
                WHERE Nombre = N'Embargo'
                  AND CodigoTipoDiligencia IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE DiligenciasEncargadas
                SET CodigoTipoDiligencia = NULL
                WHERE Nombre = N'Embargo'
                  AND CodigoTipoDiligencia = 3;
                """);
        }
    }
}
