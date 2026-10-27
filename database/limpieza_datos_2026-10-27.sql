/* =============================================================================
   Jurigest Enterprise - Limpieza de datos (PROPUESTA, NO EJECUTADA)
   -----------------------------------------------------------------------------
   Base     : JurigestEnterprise  (localhost\SQLEXPRESS, ver src/Jurigest.API/appsettings.json)
   Fecha    : 2026-10-27
   Requiere : migracion 20261026150707_VincularDiligenciaConReceptor aplicada.

   COMO USARLO
   1. Respaldar la base antes de confirmar (ver PARTE 0).
   2. Ejecutar el script tal cual: PARTE 1 solo hace SELECT y PARTE 2 corre dentro
      de una transaccion que termina en ROLLBACK. Los conteos muestran lo que cambiaria.
   3. Para aplicar, cambiar @Confirmar = 1 y volver a ejecutar.

   QUE HACE LA PARTE 2
   2.1 SesionesUsuario: borra las sesiones cuyo refresh token vencio hace mas de
       @DiasRetencion dias. Un token vencido ya no sirve para renovar ni para detectar
       reutilizacion (RenovarSesionHandler lo rechaza igual), y la traza queda en
       AuditoriasSeguridad. Las sesiones vigentes, revocadas o no, no se tocan.
   2.2 TokensRecuperacionPassword: borra los tokens usados, revocados o vencidos hace
       mas de @DiasRetencion dias.
   2.3 ROL con año de 4 digitos (opcional, @NormalizarAnioRol = 1): "C-11747-2026"
       pasa a "C-11747-26" en Causas.Rit, Causas.NumeroRol y Recibos.Rol, como el
       resto de las causas. Se omite una causa si el ROL corto ya existe en su tribunal
       (indice UX_Causas_Rit_Tribunal).

   QUE NO HACE (revisar en PARTE 1 y decidir aparte)
   - Usuarios de prueba: solo se listan (1.4). Borrar un usuario borra en cascada sus
     sesiones, tokens y receptores asociados, y deja huerfanas sus filas de auditoria.
   - VehiculosEncargo con textos vacios (''): las columnas son NOT NULL y '' es el
     valor que usa la aplicacion para "sin dato".
   - Diligencias.Estampe con espacios dobles: es texto legal con formato, se deja igual.
   - RespaldoAgrupacionBusqueda / RespaldoClasificacionBusquedaPositiva: las usan los
     Down() de sus migraciones; no se borran.
   ============================================================================= */

USE JurigestEnterprise;
GO
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* =============================================================================
   PARTE 0 - RESPALDO (ejecutar manualmente antes de @Confirmar = 1)
   =============================================================================
BACKUP DATABASE JurigestEnterprise
TO DISK = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\JurigestEnterprise_antes_limpieza_20261027.bak'
WITH COPY_ONLY, CHECKSUM, INIT;
RESTORE VERIFYONLY
FROM DISK = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\JurigestEnterprise_antes_limpieza_20261027.bak'
WITH CHECKSUM;
*/

/* =============================================================================
   PARTE 1 - DIAGNOSTICO (solo lectura)
   ============================================================================= */
DECLARE @Ahora datetime2 = SYSUTCDATETIME();
DECLARE @DiasRetencion int = 30;
DECLARE @Limite datetime2 = DATEADD(DAY, -@DiasRetencion, @Ahora);

PRINT N'1.1 Sesiones';
SELECT COUNT(*) AS Total,
       SUM(CASE WHEN ExpiraUtc < @Ahora THEN 1 ELSE 0 END) AS Vencidas,
       SUM(CASE WHEN ExpiraUtc < @Limite THEN 1 ELSE 0 END) AS ABorrar,
       SUM(CASE WHEN RevocadaUtc IS NULL AND ExpiraUtc >= @Ahora THEN 1 ELSE 0 END) AS Vigentes
FROM dbo.SesionesUsuario;

PRINT N'1.2 Tokens de recuperacion';
SELECT COUNT(*) AS Total,
       SUM(CASE WHEN COALESCE(UsadoUtc, RevocadoUtc, ExpiraUtc) < @Limite THEN 1 ELSE 0 END) AS ABorrar
FROM dbo.TokensRecuperacionPassword;

PRINT N'1.3 ROL con año de 4 digitos';
SELECT c.Rit, c.NumeroRol, c.Tribunal,
       LEFT(c.Rit, LEN(c.Rit) - 4) + RIGHT(c.Rit, 2) AS RitPropuesto,
       CASE WHEN EXISTS (SELECT 1 FROM dbo.Causas x
                         WHERE x.Tribunal = c.Tribunal
                           AND x.Rit = LEFT(c.Rit, LEN(c.Rit) - 4) + RIGHT(c.Rit, 2))
            THEN N'CHOCA: se omite' ELSE N'' END AS Aviso
FROM dbo.Causas c
WHERE c.Rit LIKE N'%-[0-9]%-20[0-9][0-9]';

PRINT N'1.4 Usuarios que parecen de prueba (no se modifican)';
SELECT Email, Nombre, Rol, Activo, CONVERT(date, FechaCreacion) AS Creado
FROM dbo.Usuarios
WHERE Nombre LIKE N'%prueba%' OR Email LIKE N'%prueba%' OR Email LIKE N'auditoria.%';

PRINT N'1.5 Integridad (todo deberia ser 0)';
SELECT N'Recibo con causa inexistente' AS Control, COUNT(*) AS Filas FROM dbo.Recibos r
 WHERE r.CausaId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Causas c WHERE c.Id = r.CausaId)
UNION ALL SELECT N'Recibo con diligencia inexistente', COUNT(*) FROM dbo.Recibos r
 WHERE r.DiligenciaId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Diligencias d WHERE d.Id = r.DiligenciaId)
UNION ALL SELECT N'Vehiculo con diligencia inexistente', COUNT(*) FROM dbo.VehiculosEncargo v
 WHERE NOT EXISTS (SELECT 1 FROM dbo.Diligencias d WHERE d.Id = v.DiligenciaId)
UNION ALL SELECT N'Diligencia con receptor inexistente', COUNT(*) FROM dbo.Diligencias d
 WHERE d.ReceptorJudicialId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.ReceptoresJudiciales x WHERE x.Id = d.ReceptorJudicialId)
UNION ALL SELECT N'Diligencia con receptor sin Id', COUNT(*) FROM dbo.Diligencias d
 WHERE d.ReceptorJudicialId IS NULL AND NULLIF(d.ReceptorJudicial, N'') IS NOT NULL
UNION ALL SELECT N'Causa con tribunal fuera del catalogo', COUNT(*) FROM dbo.Causas c
 WHERE NOT EXISTS (SELECT 1 FROM dbo.Tribunales t WHERE t.Nombre = c.Tribunal)
UNION ALL SELECT N'Texto con caracteres dañados (U+FFFD o mojibake)', COUNT(*) FROM dbo.Diligencias d
 WHERE CHARINDEX(NCHAR(65533), CONCAT(d.Descripcion, d.Estampe, d.ResultadoDetalle, d.Observaciones)) > 0
    OR CONCAT(d.Descripcion, d.Estampe, d.ResultadoDetalle, d.Observaciones) LIKE N'%Ã%' COLLATE Latin1_General_BIN2;
GO

/* =============================================================================
   PARTE 2 - LIMPIEZA (ROLLBACK salvo @Confirmar = 1)
   ============================================================================= */
SET XACT_ABORT ON;
DECLARE @Confirmar bit = 0;
DECLARE @NormalizarAnioRol bit = 1;
DECLARE @DiasRetencion int = 30;
DECLARE @Limite datetime2 = DATEADD(DAY, -@DiasRetencion, SYSUTCDATETIME());

BEGIN TRANSACTION;

-- 2.1 Sesiones vencidas
DELETE FROM dbo.SesionesUsuario WHERE ExpiraUtc < @Limite;
PRINT CONCAT(N'2.1 Sesiones borradas: ', @@ROWCOUNT);

-- 2.2 Tokens de recuperacion inutilizables
DELETE FROM dbo.TokensRecuperacionPassword
WHERE COALESCE(UsadoUtc, RevocadoUtc, ExpiraUtc) < @Limite;
PRINT CONCAT(N'2.2 Tokens borrados: ', @@ROWCOUNT);

-- 2.3 ROL con año de 4 digitos
IF @NormalizarAnioRol = 1
BEGIN
    DECLARE @Rol TABLE (CausaId uniqueidentifier PRIMARY KEY,
                        RitAnterior nvarchar(100), RitNuevo nvarchar(100));
    INSERT INTO @Rol
    SELECT c.Id, c.Rit, LEFT(c.Rit, LEN(c.Rit) - 4) + RIGHT(c.Rit, 2)
    FROM dbo.Causas c
    WHERE c.Rit LIKE N'%-[0-9]%-20[0-9][0-9]'
      AND NOT EXISTS (SELECT 1 FROM dbo.Causas x
                      WHERE x.Tribunal = c.Tribunal
                        AND x.Rit = LEFT(c.Rit, LEN(c.Rit) - 4) + RIGHT(c.Rit, 2));

    UPDATE c SET
        Rit = m.RitNuevo,
        NumeroRol = CASE WHEN c.NumeroRol LIKE N'%-20[0-9][0-9]'
                         THEN LEFT(c.NumeroRol, LEN(c.NumeroRol) - 4) + RIGHT(c.NumeroRol, 2)
                         ELSE c.NumeroRol END
    FROM dbo.Causas c JOIN @Rol m ON m.CausaId = c.Id;
    PRINT CONCAT(N'2.3 Causas con ROL corregido: ', @@ROWCOUNT);

    UPDATE r SET Rol = m.RitNuevo
    FROM dbo.Recibos r JOIN @Rol m ON m.CausaId = r.CausaId
    WHERE r.Rol = m.RitAnterior;
    PRINT CONCAT(N'2.3 Recibos con ROL corregido: ', @@ROWCOUNT);
END;

-- 2.4 Verificacion (todo deberia ser 0)
SELECT
    (SELECT COUNT(*) FROM dbo.SesionesUsuario WHERE ExpiraUtc < @Limite) AS SesionesVencidas,
    (SELECT COUNT(*) FROM dbo.Causas WHERE Rit LIKE N'%-[0-9]%-20[0-9][0-9]') AS RolAnio4,
    (SELECT COUNT(*) FROM dbo.Recibos WHERE Rol LIKE N'%-[0-9]%-20[0-9][0-9]') AS RecibosAnio4;

IF @Confirmar = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT N'COMMIT: cambios aplicados.';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT N'ROLLBACK: ejecucion de prueba, no se modifico nada.';
END;
GO
