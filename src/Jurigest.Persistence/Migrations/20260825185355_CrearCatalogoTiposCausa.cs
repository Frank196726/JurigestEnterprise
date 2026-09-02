using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearCatalogoTiposCausa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposCausa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposCausa", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TiposCausa",
                columns: new[] { "Id", "Activo", "Codigo", "FechaCreacion", "Nombre" },
                values: new object[] { new Guid("20000000-0000-0000-0000-000000000001"), true, "C", new DateTime(2026, 8, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Civil" });

            migrationBuilder.CreateIndex(
                name: "IX_TiposCausa_Codigo",
                table: "TiposCausa",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposCausa_Nombre",
                table: "TiposCausa",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TiposCausa");
        }
    }
}
