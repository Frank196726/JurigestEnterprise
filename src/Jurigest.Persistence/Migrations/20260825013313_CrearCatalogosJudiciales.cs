using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearCatalogosJudiciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comunas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comunas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReceptoresJudiciales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceptoresJudiciales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposDiligencia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CodigoSistema = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDiligencia", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TiposDiligencia",
                columns: new[] { "Id", "Activo", "CodigoSistema", "FechaCreacion", "Nombre" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), true, 1, new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Notificación" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), true, 2, new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Requerimiento de pago" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), true, 3, new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Embargo" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), true, 4, new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Lanzamiento" },
                    { new Guid("10000000-0000-0000-0000-000000000005"), true, 5, new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Retiro de exhorto" },
                    { new Guid("10000000-0000-0000-0000-000000000006"), true, 6, new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Retiro de expediente" },
                    { new Guid("10000000-0000-0000-0000-000000000007"), true, 7, new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Incautación" },
                    { new Guid("10000000-0000-0000-0000-000000000008"), true, 8, new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Protesto" },
                    { new Guid("10000000-0000-0000-0000-000000000009"), true, 9, new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Citación" },
                    { new Guid("10000000-0000-0000-0000-000000000099"), true, 99, new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Otro" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comunas_Nombre",
                table: "Comunas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceptoresJudiciales_Nombre",
                table: "ReceptoresJudiciales",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposDiligencia_CodigoSistema",
                table: "TiposDiligencia",
                column: "CodigoSistema",
                unique: true,
                filter: "[CodigoSistema] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TiposDiligencia_Nombre",
                table: "TiposDiligencia",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comunas");

            migrationBuilder.DropTable(
                name: "ReceptoresJudiciales");

            migrationBuilder.DropTable(
                name: "TiposDiligencia");
        }
    }
}
