using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jurigest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RolSistemaOperador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // "Operador" pasa de rol del catálogo a rol del sistema (RolUsuario.Operador = 5).
            // Sus usuarios toman el perfil Operador y deben volver a iniciar sesión, y los
            // permisos configurados para el rol se conservan bajo la clave del perfil.
            migrationBuilder.Sql(@"
UPDATE Usuarios
SET Rol = 5, RolCatalogoId = NULL, NombreRolPersonalizado = NULL, VersionSeguridad = VersionSeguridad + 1
WHERE RolCatalogoId = '71000000-0000-0000-0000-000000000002';

IF NOT EXISTS (SELECT 1 FROM PermisosRoles WHERE Clave = N'Operador')
    UPDATE PermisosRoles SET Clave = N'Operador' WHERE Clave = N'71000000-0000-0000-0000-000000000002';
DELETE FROM PermisosRoles WHERE Clave = N'71000000-0000-0000-0000-000000000002';");

            migrationBuilder.DeleteData(
                table: "RolesCatalogo",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000002"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RolesCatalogo",
                columns: new[] { "Id", "Nombre", "NombreNormalizado", "Perfil" },
                values: new object[] { new Guid("71000000-0000-0000-0000-000000000002"), "Operador", "OPERADOR", 3 });

            migrationBuilder.Sql(@"
UPDATE RolesCatalogo SET Perfil = 3 WHERE Perfil = 5;

UPDATE Usuarios
SET Rol = 3, RolCatalogoId = '71000000-0000-0000-0000-000000000002', NombreRolPersonalizado = N'Operador',
    VersionSeguridad = VersionSeguridad + 1
WHERE Rol = 5 AND RolCatalogoId IS NULL;

UPDATE Usuarios SET Rol = 3, VersionSeguridad = VersionSeguridad + 1 WHERE Rol = 5;

UPDATE PermisosRoles SET Clave = N'71000000-0000-0000-0000-000000000002' WHERE Clave = N'Operador';");
        }
    }
}
