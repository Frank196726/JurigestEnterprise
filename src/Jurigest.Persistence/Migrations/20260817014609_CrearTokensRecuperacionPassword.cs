using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearTokensRecuperacionPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TokensRecuperacionPassword",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiraUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsadoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevocadoUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokensRecuperacionPassword", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokensRecuperacionPassword_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokensRecuperacionPassword_TokenHash",
                table: "TokensRecuperacionPassword",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokensRecuperacionPassword_UsuarioId_ExpiraUtc",
                table: "TokensRecuperacionPassword",
                columns: new[] { "UsuarioId", "ExpiraUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokensRecuperacionPassword");
        }
    }
}
