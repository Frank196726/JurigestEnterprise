using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCatalogoRolesUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombreRolPersonalizado",
                table: "Usuarios",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RolCatalogoId",
                table: "Usuarios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RolesCatalogo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NombreNormalizado = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Perfil = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesCatalogo", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "RolesCatalogo",
                columns: new[] { "Id", "Nombre", "NombreNormalizado", "Perfil" },
                values: new object[,]
                {
                    { new Guid("71000000-0000-0000-0000-000000000001"), "Receptor", "RECEPTOR", 3 },
                    { new Guid("71000000-0000-0000-0000-000000000002"), "Operador", "OPERADOR", 3 },
                    { new Guid("71000000-0000-0000-0000-000000000003"), "Digitador", "DIGITADOR", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolCatalogoId",
                table: "Usuarios",
                column: "RolCatalogoId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesCatalogo_NombreNormalizado",
                table: "RolesCatalogo",
                column: "NombreNormalizado",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_RolesCatalogo_RolCatalogoId",
                table: "Usuarios",
                column: "RolCatalogoId",
                principalTable: "RolesCatalogo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_RolesCatalogo_RolCatalogoId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "RolesCatalogo");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_RolCatalogoId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "NombreRolPersonalizado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RolCatalogoId",
                table: "Usuarios");
        }
    }
}
