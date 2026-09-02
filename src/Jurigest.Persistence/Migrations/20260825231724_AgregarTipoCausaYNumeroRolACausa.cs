using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTipoCausaYNumeroRolACausa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NumeroRol",
                table: "Causas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TipoCausaId",
                table: "Causas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Causas_TipoCausaId",
                table: "Causas",
                column: "TipoCausaId");

            migrationBuilder.CreateIndex(
                name: "IX_Causas_TipoCausaId_NumeroRol",
                table: "Causas",
                columns: new[] { "TipoCausaId", "NumeroRol" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Causas_TipoCausaId",
                table: "Causas");

            migrationBuilder.DropIndex(
                name: "IX_Causas_TipoCausaId_NumeroRol",
                table: "Causas");

            migrationBuilder.DropColumn(
                name: "NumeroRol",
                table: "Causas");

            migrationBuilder.DropColumn(
                name: "TipoCausaId",
                table: "Causas");
        }
    }
}
