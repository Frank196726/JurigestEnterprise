using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearCatalogosTribunalesAbogadosYDiligenciasEncargadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Abogados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abogados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiligenciasEncargadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CodigoTipoDiligencia = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiligenciasEncargadas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tribunales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tribunales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Abogados_Nombre",
                table: "Abogados",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Abogados_UsuarioId",
                table: "Abogados",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DiligenciasEncargadas_CodigoTipoDiligencia",
                table: "DiligenciasEncargadas",
                column: "CodigoTipoDiligencia");

            migrationBuilder.CreateIndex(
                name: "IX_DiligenciasEncargadas_Nombre",
                table: "DiligenciasEncargadas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tribunales_Nombre",
                table: "Tribunales",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Abogados");

            migrationBuilder.DropTable(
                name: "DiligenciasEncargadas");

            migrationBuilder.DropTable(
                name: "Tribunales");
        }
    }
}
