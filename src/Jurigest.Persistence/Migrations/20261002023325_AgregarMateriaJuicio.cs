using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMateriaJuicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Materia",
                table: "Causas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MateriaId",
                table: "Causas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Materias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Materias",
                columns: new[] { "Id", "Nombre" },
                values: new object[] { new Guid("ab854cf1-3121-4bf3-912f-2157537ab901"), "Ejecutivo" });

            migrationBuilder.CreateIndex(
                name: "IX_Causas_MateriaId",
                table: "Causas",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Materias_Nombre",
                table: "Materias",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Causas_Materias_MateriaId",
                table: "Causas",
                column: "MateriaId",
                principalTable: "Materias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Causas_Materias_MateriaId",
                table: "Causas");

            migrationBuilder.DropTable(
                name: "Materias");

            migrationBuilder.DropIndex(
                name: "IX_Causas_MateriaId",
                table: "Causas");

            migrationBuilder.DropColumn(
                name: "Materia",
                table: "Causas");

            migrationBuilder.DropColumn(
                name: "MateriaId",
                table: "Causas");
        }
    }
}
