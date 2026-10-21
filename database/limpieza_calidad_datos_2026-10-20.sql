/* =============================================================================
   Jurigest Enterprise - Limpieza de calidad de datos (PROPUESTA, NO EJECUTADA)
   -----------------------------------------------------------------------------
   Base     : JurigestEnterprise  (localhost\SQLEXPRESS, ver src/Jurigest.API/appsettings.json)
   Fecha    : 2026-10-20
   Motor    : SQL Server 2022 Express (MSSQL16)

   COMO USARLO
   1. Ejecutar PARTE 0 (preparacion) y PARTE 1 (diagnostico). Solo crea tablas
      temporales (#...) en tempdb y hace SELECT: no modifica ninguna tabla de la base.
   2. Revisar los resultados. Si hace falta, ajustar #MapaTribunalManual (0.2)
      y volver a ejecutar PARTE 0 y PARTE 1.
   3. Ejecutar PARTE 2 en la MISMA sesion/ventana (usa las tablas #). Todo va dentro
      de una transaccion que termina en ROLLBACK, salvo que se cambie @Confirmar = 1.
      Los SELECT de verificacion del final muestran el estado "despues" antes del ROLLBACK.
   4. Recien con los datos limpios (COMMIT) aplicar la migracion del indice unico
      src/Jurigest.Persistence/Migrations/20261021025327_IndiceUnicoCausaRitTribunal.cs.

   RESUMEN DE LO QUE HACE CADA PASO DE LA PARTE 2
   2.1 Tribunales: reemplaza Causas.Tribunal por su forma canonica (#TribunalValor.Canonico):
       espacios colapsados, "01 º" -> "1°", variantes de mayusculas unificadas y erratas
       corregidas segun #MapaTribunalManual. En el catalogo Tribunales deja una fila por
       forma canonica (renombra la que se conserva y borra las demas; Tribunales no tiene
       FK entrantes). "10"/"10°" -> "10° Juzgado Civil de Santiago". Tambien corrige
       Recibos.Tribunal y crea en el catalogo los "1° ... 30° Juzgado Civil de Santiago"
       que falten.
   2.2 Causas duplicadas (mismo ROL normalizado + mismo tribunal canonico): por grupo
       conserva la causa con mas diligencias+documentos+recibos (desempate: diligencias,
       documentos, recibos, resoluciones, la mas antigua). Mueve a la conservada
       Diligencias, Documentos, Recibos y Resoluciones (VehiculosEncargo y Recibos siguen
       a su diligencia). Demandados: si la conservada no tiene, mueve los de la primera
       duplicada que tenga; el resto se borra en cascada junto con la duplicada (indice
       unico de demandado principal por causa). Luego borra las causas duplicadas.
       Los campos propios de la causa (descripcion, estado, fechas, materia) quedan los
       de la conservada: comparar en 1.1 antes de confirmar.
   2.2b ROL: normaliza Causas.Rit y Recibos.Rol como NormalizarRol ("1188-26" -> "C-1188-26").
   2.3 Tipos de diligencia con variantes de tilde/mayusculas ("Notificacion"/"Notificación"):
       conserva el sembrado por el sistema o, si no hay, el que lleva tilde. Las
       referencias son por codigo (CodigoSistema), asi que reasigna Diligencias.Tipo,
       ModelosEstampe.TipoDiligencia, DiligenciasRealizadas/Encargadas.CodigoTipoDiligencia
       al codigo del conservado y borra los duplicados.
   2.4 Diligencias realizadas con variantes ("Busqueda"/"Búsqueda") dentro del mismo tipo
       (ya con el codigo unificado): conserva la que lleva tilde (luego la activa y la mas
       usada), copia el arancel si la conservada no tiene, reasigna
       Diligencias.DiligenciaRealizadaId, Recibos.DiligenciaRealizadaId y
       ModelosEstampe.DiligenciaRealizadaId, borra las duplicadas y unifica el texto
       Diligencias.DiligenciaRealizada con el nombre del catalogo.
   2.5 Verificacion: vuelve a buscar duplicados (deben salir 0 filas) y hace
       COMMIT solo si @Confirmar = 1; si no, ROLLBACK.
   El punto 4 del encargo (diligencias Completadas sin resultado ni fecha de gestion)
   solo se lista en 1.7; la PARTE 2 no las modifica.

   Colaciones: las tablas # fijan colaciones explicitas para no depender de tempdb:
     Latin1_General_BIN2     -> valor exacto (distingue mayusculas y tildes)
     Modern_Spanish_CI_AS    -> igual que MismoTribunal (ignora mayusculas, respeta tildes)
     Latin1_General_CI_AI    -> agrupar variantes con y sin tilde
   ============================================================================= */

USE JurigestEnterprise;
GO
SET NOCOUNT ON;
-- Requeridos por los indices filtrados (Demandados, TiposDiligencia); sqlcmd los trae en OFF.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* =============================================================================
   PARTE 0 - PREPARACION (solo tablas temporales, sin cambios en la base)
   ============================================================================= */

/* 0.1 Valores de tribunal en uso y su forma normalizada.
       Replica IdentificacionCausa.NormalizarTribunal:
         - Trim + colapsar espacios (\s+ -> ' ')
         - ^(\d+)\s*[°º]?(?=\s|$)  ->  numero sin ceros a la izquierda + '°'          */
DROP TABLE IF EXISTS #TribunalValor;
CREATE TABLE #TribunalValor (
    Original    nvarchar(200) COLLATE Latin1_General_BIN2 NOT NULL PRIMARY KEY,
    Paso        nvarchar(200) COLLATE Latin1_General_BIN2 NULL,
    Normalizado nvarchar(200) COLLATE Latin1_General_BIN2 NULL,
    Base        nvarchar(200) COLLATE Latin1_General_BIN2 NULL,   -- tras correccion manual
    Canonico    nvarchar(200) COLLATE Latin1_General_BIN2 NULL,   -- forma final propuesta
    UsosCausas  int NOT NULL DEFAULT 0,
    UsosRecibos int NOT NULL DEFAULT 0,
    EnCatalogo  bit NOT NULL DEFAULT 0);

INSERT INTO #TribunalValor (Original)
SELECT Tribunal COLLATE Latin1_General_BIN2 FROM dbo.Causas
UNION
SELECT Tribunal COLLATE Latin1_General_BIN2 FROM dbo.Recibos WHERE Tribunal IS NOT NULL
UNION
SELECT Nombre COLLATE Latin1_General_BIN2 FROM dbo.Tribunales;

UPDATE t SET
    UsosCausas  = (SELECT COUNT(*) FROM dbo.Causas c
                   WHERE c.Tribunal COLLATE Latin1_General_BIN2 = t.Original),
    UsosRecibos = (SELECT COUNT(*) FROM dbo.Recibos r
                   WHERE r.Tribunal COLLATE Latin1_General_BIN2 = t.Original),
    EnCatalogo  = CASE WHEN EXISTS (SELECT 1 FROM dbo.Tribunales x
                   WHERE x.Nombre COLLATE Latin1_General_BIN2 = t.Original) THEN 1 ELSE 0 END
FROM #TribunalValor t;

-- Espacios: tab, LF, CR y NBSP cuentan como \s en .NET; luego Trim y colapso.
UPDATE #TribunalValor SET Paso = TRIM(REPLACE(REPLACE(REPLACE(REPLACE(
    Original, NCHAR(9), N' '), NCHAR(10), N' '), NCHAR(13), N' '), NCHAR(160), N' '));
UPDATE #TribunalValor SET Paso = REPLACE(REPLACE(REPLACE(
    Paso, N' ', NCHAR(1) + NCHAR(2)), NCHAR(2) + NCHAR(1), N''), NCHAR(1) + NCHAR(2), N' ');

-- Numero inicial. NCHAR(176) = '°' (grado), NCHAR(186) = 'º' (ordinal).
UPDATE t SET Normalizado = CASE
        WHEN a.D = N'' THEN t.Paso
        WHEN b.R = N'' THEN b.N
        WHEN LEFT(b.R, 1) = N' ' AND LEFT(c.R1, 1) IN (NCHAR(176), NCHAR(186))
             AND (LEN(c.R1) = 1 OR SUBSTRING(c.R1, 2, 1) = N' ')
            THEN b.N + SUBSTRING(c.R1, 2, 400)
        WHEN LEFT(b.R, 1) = N' ' THEN b.N + b.R
        WHEN LEFT(b.R, 1) IN (NCHAR(176), NCHAR(186))
             AND (LEN(b.R) = 1 OR SUBSTRING(b.R, 2, 1) = N' ')
            THEN b.N + SUBSTRING(b.R, 2, 400)
        ELSE t.Paso   -- p. ej. "1°Juzgado": el regex de .NET tampoco lo toca
    END
FROM #TribunalValor t
CROSS APPLY (SELECT LEFT(t.Paso, PATINDEX(N'%[^0-9]%', t.Paso + N'x') - 1) AS D) a
CROSS APPLY (SELECT SUBSTRING(t.Paso, LEN(a.D) + 1, 400) AS R,
                    ISNULL(NULLIF(SUBSTRING(a.D, PATINDEX(N'%[^0]%', a.D + N'.'), 400), N''), N'0')
                        + NCHAR(176) AS N) b
CROSS APPLY (SELECT CASE WHEN LEFT(b.R, 1) = N' ' THEN SUBSTRING(b.R, 2, 400) ELSE b.R END AS R1) c;

/* 0.2 Correcciones manuales (erratas y casos que no se resuelven solo con mayusculas).
       Desde se compara sin distinguir mayusculas contra el valor ya normalizado.
       >>> REVISAR Y AJUSTAR AQUI <<<                                                  */
DROP TABLE IF EXISTS #MapaTribunalManual;
CREATE TABLE #MapaTribunalManual (
    Desde  nvarchar(200) COLLATE Modern_Spanish_CI_AS NOT NULL PRIMARY KEY,
    Hacia  nvarchar(200) COLLATE Latin1_General_BIN2 NOT NULL,
    Motivo nvarchar(200) NULL);

INSERT INTO #MapaTribunalManual (Desde, Hacia, Motivo) VALUES
    (N'1° Juzgado Civil de Santiago',  N'1° Juzgado Civil de Santiago',  N'Mayusculas ("1° juzgado civil de Santiago")'),
    (N'25° Juzgado Civil de Antiago',  N'25° Juzgado Civil de Santiago', N'Errata: "Antiago"');
-- "10" / "10°" sin nombre: confirmado por el usuario (2026-10-20).
INSERT INTO #MapaTribunalManual (Desde, Hacia, Motivo) VALUES
    (N'10°', N'10° Juzgado Civil de Santiago', N'Tribunal sin nombre (confirmado por el usuario)');

/* 0.2b Juzgados civiles de Santiago 1° a 30°: forma canonica (mayusculas) y lista de
        tribunales que deben existir en el catalogo (se crean en 2.1 los que falten). */
DROP TABLE IF EXISTS #JuzgadoSantiago;
SELECT TOP (30)
       CONVERT(nvarchar(200),
               CONCAT(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)), NCHAR(176), N' Juzgado Civil de Santiago'))
           COLLATE Latin1_General_BIN2 AS Nombre
INTO #JuzgadoSantiago
FROM sys.all_objects;

INSERT INTO #MapaTribunalManual (Desde, Hacia, Motivo)
SELECT j.Nombre, j.Nombre, N'Forma canonica 1°-30° Juzgado Civil de Santiago'
FROM #JuzgadoSantiago j
WHERE NOT EXISTS (SELECT 1 FROM #MapaTribunalManual m WHERE m.Desde = j.Nombre COLLATE Modern_Spanish_CI_AS);

UPDATE t SET Base = COALESCE(m.Hacia, t.Normalizado)
FROM #TribunalValor t
LEFT JOIN #MapaTribunalManual m ON m.Desde = t.Normalizado COLLATE Modern_Spanish_CI_AS;

/* 0.3 Forma canonica por grupo de variantes que solo difieren en mayusculas.
       Prioridad: destino de una correccion manual > ya existe tal cual en el catalogo >
       mas usado en Causas > orden binario (gana la variante con mas mayusculas).      */
WITH Cand AS (
    SELECT b.Base,
           CASE WHEN EXISTS (SELECT 1 FROM #MapaTribunalManual m WHERE m.Hacia = b.Base)
                THEN 0 ELSE 1 END AS PrioManual,
           CASE WHEN EXISTS (SELECT 1 FROM dbo.Tribunales x
                             WHERE x.Nombre COLLATE Latin1_General_BIN2 = b.Base)
                THEN 0 ELSE 1 END AS PrioCatalogo,
           b.Usos
    FROM (SELECT Base, SUM(UsosCausas) AS Usos FROM #TribunalValor GROUP BY Base) b),
Rnk AS (
    SELECT Base,
           ROW_NUMBER() OVER (PARTITION BY Base COLLATE Modern_Spanish_CI_AS
                              ORDER BY PrioManual, PrioCatalogo, Usos DESC, Base) AS rn
    FROM Cand)
UPDATE t SET Canonico = r.Base
FROM #TribunalValor t
JOIN Rnk r ON r.rn = 1
          AND r.Base COLLATE Modern_Spanish_CI_AS = t.Base COLLATE Modern_Spanish_CI_AS;

/* 0.4 Causas: ROL normalizado (replica IdentificacionCausa.NormalizarRol), tribunal
       canonico, cantidad de hijos y causa a conservar por grupo duplicado.            */
DROP TABLE IF EXISTS #CausaClave;
CREATE TABLE #CausaClave (
    Id               uniqueidentifier NOT NULL PRIMARY KEY,
    Rit              nvarchar(30)  COLLATE Latin1_General_BIN2 NOT NULL,
    RitNormalizado   nvarchar(60)  COLLATE Latin1_General_BIN2 NOT NULL,
    Tribunal         nvarchar(200) COLLATE Latin1_General_BIN2 NOT NULL,
    TribunalCanonico nvarchar(200) COLLATE Latin1_General_BIN2 NOT NULL,
    FechaCreacion    datetime2 NOT NULL,
    Diligencias      int NOT NULL,
    Documentos       int NOT NULL,
    Recibos          int NOT NULL,
    Resoluciones     int NOT NULL,
    Demandados       int NOT NULL,
    Grupo            int NULL,              -- NULL = sin duplicado
    Orden            int NULL,              -- 1 = se conserva
    ConservarId      uniqueidentifier NULL);

INSERT INTO #CausaClave (Id, Rit, RitNormalizado, Tribunal, TribunalCanonico, FechaCreacion,
                         Diligencias, Documentos, Recibos, Resoluciones, Demandados)
SELECT c.Id, c.Rit, UPPER(TRIM(c.Rit)), c.Tribunal,
       ISNULL(t.Canonico, c.Tribunal COLLATE Latin1_General_BIN2), c.FechaCreacion,
       (SELECT COUNT(*) FROM dbo.Diligencias  x WHERE x.CausaId = c.Id),
       (SELECT COUNT(*) FROM dbo.Documentos   x WHERE x.CausaId = c.Id),
       (SELECT COUNT(*) FROM dbo.Recibos      x WHERE x.CausaId = c.Id),
       (SELECT COUNT(*) FROM dbo.Resoluciones x WHERE x.CausaId = c.Id),
       (SELECT COUNT(*) FROM dbo.Demandados   x WHERE x.CausaId = c.Id)
FROM dbo.Causas c
LEFT JOIN #TribunalValor t ON t.Original = c.Tribunal COLLATE Latin1_General_BIN2;

-- \s*-\s* -> '-'
UPDATE #CausaClave SET RitNormalizado = REPLACE(REPLACE(REPLACE(REPLACE(RitNormalizado,
    NCHAR(9), N' '), NCHAR(160), N' '), NCHAR(10), N' '), NCHAR(13), N' ');
WHILE EXISTS (SELECT 1 FROM #CausaClave WHERE RitNormalizado LIKE N'% -%' OR RitNormalizado LIKE N'%- %')
    UPDATE #CausaClave SET RitNormalizado = REPLACE(REPLACE(RitNormalizado, N' -', N'-'), N'- ', N'-')
    WHERE RitNormalizado LIKE N'% -%' OR RitNormalizado LIKE N'%- %';
-- ^\d+-\d+$ -> 'C-' + valor
UPDATE #CausaClave SET RitNormalizado = N'C-' + RitNormalizado
WHERE RitNormalizado LIKE N'[0-9]%-[0-9]%'
  AND RitNormalizado NOT LIKE N'%[^0-9-]%'
  AND LEN(RitNormalizado) - LEN(REPLACE(RitNormalizado, N'-', N'')) = 1;

WITH r AS (
    SELECT Id,
           DENSE_RANK() OVER (ORDER BY RitNormalizado, TribunalCanonico COLLATE Modern_Spanish_CI_AS) AS G,
           ROW_NUMBER() OVER (PARTITION BY RitNormalizado, TribunalCanonico COLLATE Modern_Spanish_CI_AS
                              ORDER BY Diligencias + Documentos + Recibos DESC,
                                       Diligencias DESC, Documentos DESC, Recibos DESC,
                                       Resoluciones DESC, FechaCreacion, Id) AS O,
           COUNT(*) OVER (PARTITION BY RitNormalizado, TribunalCanonico COLLATE Modern_Spanish_CI_AS) AS N
    FROM #CausaClave)
UPDATE k SET Grupo = CASE WHEN r.N > 1 THEN r.G END,
             Orden = CASE WHEN r.N > 1 THEN r.O END
FROM #CausaClave k JOIN r ON r.Id = k.Id;

UPDATE k SET ConservarId = c.Id
FROM #CausaClave k
JOIN #CausaClave c ON c.Grupo = k.Grupo AND c.Orden = 1;

/* 0.5 Demandados de las causas a eliminar: MOVER o ELIMINAR (cascada). */
DROP TABLE IF EXISTS #DemandadoPlan;
CREATE TABLE #DemandadoPlan (
    DemandadoId    uniqueidentifier NOT NULL PRIMARY KEY,
    CausaOrigenId  uniqueidentifier NOT NULL,
    CausaDestinoId uniqueidentifier NOT NULL,
    Accion         nvarchar(10) NOT NULL);

WITH Fuente AS (
    SELECT Id, ROW_NUMBER() OVER (PARTITION BY Grupo ORDER BY Orden) AS rn
    FROM #CausaClave
    WHERE Grupo IS NOT NULL AND Demandados > 0)
INSERT INTO #DemandadoPlan (DemandadoId, CausaOrigenId, CausaDestinoId, Accion)
SELECT d.Id, k.Id, k.ConservarId,
       CASE WHEN f.Id IS NOT NULL THEN N'MOVER' ELSE N'ELIMINAR' END
FROM dbo.Demandados d
JOIN #CausaClave k ON k.Id = d.CausaId AND k.Orden > 1
LEFT JOIN Fuente f ON f.Id = k.Id AND f.rn = 1;

/* 0.6 Tipos de diligencia: grupos por nombre sin tilde ni mayusculas. */
DROP TABLE IF EXISTS #TipoMapa;
CREATE TABLE #TipoMapa (
    Id            uniqueidentifier NOT NULL PRIMARY KEY,
    Nombre        nvarchar(150) COLLATE Latin1_General_BIN2 NOT NULL,
    Grupo         nvarchar(150) COLLATE Latin1_General_CI_AI NOT NULL,
    CodigoSistema int NULL,
    Activo        bit NOT NULL,
    FechaCreacion datetime2 NOT NULL,
    Sembrado      bit NOT NULL,
    ConTilde      bit NOT NULL,
    Referencias   int NOT NULL,
    Orden         int NULL,
    ConservarId   uniqueidentifier NULL,
    CodigoDestino int NULL);

INSERT INTO #TipoMapa (Id, Nombre, Grupo, CodigoSistema, Activo, FechaCreacion, Sembrado, ConTilde, Referencias)
SELECT t.Id, t.Nombre, TRIM(t.Nombre), t.CodigoSistema, t.Activo, t.FechaCreacion,
       CASE WHEN CONVERT(char(36), t.Id) LIKE '10000000-0000-0000-0000-%' THEN 1 ELSE 0 END,
       CASE WHEN t.Nombre COLLATE Latin1_General_BIN2 LIKE N'%[áéíóúüñÁÉÍÓÚÜÑ]%' THEN 1 ELSE 0 END,
       CASE WHEN t.CodigoSistema IS NULL THEN 0 ELSE
             (SELECT COUNT(*) FROM dbo.Diligencias           x WHERE x.Tipo = t.CodigoSistema)
           + (SELECT COUNT(*) FROM dbo.ModelosEstampe        x WHERE x.TipoDiligencia = t.CodigoSistema)
           + (SELECT COUNT(*) FROM dbo.DiligenciasRealizadas x WHERE x.CodigoTipoDiligencia = t.CodigoSistema)
           + (SELECT COUNT(*) FROM dbo.DiligenciasEncargadas x WHERE x.CodigoTipoDiligencia = t.CodigoSistema)
       END
FROM dbo.TiposDiligencia t;

WITH r AS (
    SELECT Id,
           ROW_NUMBER() OVER (PARTITION BY Grupo
                              ORDER BY Sembrado DESC, ConTilde DESC,
                                       CASE WHEN CodigoSistema IS NULL THEN 1 ELSE 0 END,
                                       Activo DESC, Referencias DESC, FechaCreacion, Id) AS O,
           COUNT(*) OVER (PARTITION BY Grupo) AS N
    FROM #TipoMapa)
UPDATE k SET Orden = CASE WHEN r.N > 1 THEN r.O END
FROM #TipoMapa k JOIN r ON r.Id = k.Id;

UPDATE k SET ConservarId = c.Id,
             CodigoDestino = COALESCE(c.CodigoSistema,
                 (SELECT MIN(x.CodigoSistema) FROM #TipoMapa x WHERE x.Grupo = k.Grupo))
FROM #TipoMapa k
JOIN #TipoMapa c ON c.Grupo = k.Grupo AND c.Orden = 1;

DROP TABLE IF EXISTS #MapaCodigo;
SELECT DISTINCT CodigoSistema AS Desde, CodigoDestino AS Hacia
INTO #MapaCodigo
FROM #TipoMapa
WHERE Orden IS NOT NULL AND CodigoSistema IS NOT NULL AND CodigoSistema <> CodigoDestino;

/* 0.7 Diligencias realizadas: grupos por (codigo de tipo ya unificado, nombre sin tilde). */
DROP TABLE IF EXISTS #RealizadaMapa;
CREATE TABLE #RealizadaMapa (
    Id            uniqueidentifier NOT NULL PRIMARY KEY,
    Nombre        nvarchar(200) COLLATE Latin1_General_BIN2 NOT NULL,
    Grupo         nvarchar(200) COLLATE Latin1_General_CI_AI NOT NULL,
    CodigoTipo    int NOT NULL,
    CodigoDestino int NOT NULL,
    Activo        bit NOT NULL,
    Arancel       decimal(18, 0) NULL,
    FechaCreacion datetime2 NOT NULL,
    ConTilde      bit NOT NULL,
    Referencias   int NOT NULL,
    Orden         int NULL,
    ConservarId   uniqueidentifier NULL);

INSERT INTO #RealizadaMapa (Id, Nombre, Grupo, CodigoTipo, CodigoDestino, Activo, Arancel,
                            FechaCreacion, ConTilde, Referencias)
SELECT r.Id, r.Nombre, TRIM(r.Nombre), r.CodigoTipoDiligencia,
       ISNULL(m.Hacia, r.CodigoTipoDiligencia), r.Activo, r.Arancel, r.FechaCreacion,
       CASE WHEN r.Nombre COLLATE Latin1_General_BIN2 LIKE N'%[áéíóúüñÁÉÍÓÚÜÑ]%' THEN 1 ELSE 0 END,
         (SELECT COUNT(*) FROM dbo.Diligencias    x WHERE x.DiligenciaRealizadaId = r.Id)
       + (SELECT COUNT(*) FROM dbo.Recibos        x WHERE x.DiligenciaRealizadaId = r.Id)
       + (SELECT COUNT(*) FROM dbo.ModelosEstampe x WHERE x.DiligenciaRealizadaId = r.Id)
FROM dbo.DiligenciasRealizadas r
LEFT JOIN #MapaCodigo m ON m.Desde = r.CodigoTipoDiligencia;

WITH r AS (
    SELECT Id,
           ROW_NUMBER() OVER (PARTITION BY CodigoDestino, Grupo
                              ORDER BY ConTilde DESC, Activo DESC, Referencias DESC,
                                       FechaCreacion, Id) AS O,
           COUNT(*) OVER (PARTITION BY CodigoDestino, Grupo) AS N
    FROM #RealizadaMapa)
UPDATE k SET Orden = CASE WHEN r.N > 1 THEN r.O END
FROM #RealizadaMapa k JOIN r ON r.Id = k.Id;

UPDATE k SET ConservarId = c.Id
FROM #RealizadaMapa k
JOIN #RealizadaMapa c ON c.CodigoDestino = k.CodigoDestino AND c.Grupo = k.Grupo AND c.Orden = 1;
GO

/* =============================================================================
   PARTE 1 - DIAGNOSTICO (solo SELECT)
   ============================================================================= */

/* 1.0 Colaciones: el indice unico propuesto depende de que Causas.Tribunal sea CI
       (case-insensitive) y AS (sensible a tildes), igual que MismoTribunal.          */
SELECT DATABASEPROPERTYEX(DB_NAME(), 'Collation') AS ColacionBase,
       OBJECT_NAME(c.object_id) AS Tabla, c.name AS Columna, c.collation_name AS ColacionColumna
FROM sys.columns c
WHERE (c.object_id = OBJECT_ID(N'dbo.Causas')     AND c.name IN (N'Rit', N'Tribunal'))
   OR (c.object_id = OBJECT_ID(N'dbo.Tribunales') AND c.name = N'Nombre');

/* 1.1 Causas duplicadas: que se conserva y que se elimina.
       Esperado: C-TEST-002-2026 (x2), C-TEST-ESTADOS-2026 (x3), C-11573-26 (x2).     */
SELECT k.Grupo, k.Orden,
       CASE WHEN k.Orden = 1 THEN N'CONSERVAR' ELSE N'ELIMINAR -> ' + CONVERT(nvarchar(36), k.ConservarId) END AS Accion,
       k.Id, k.Rit, k.RitNormalizado, k.Tribunal, k.TribunalCanonico,
       k.Diligencias, k.Documentos, k.Recibos, k.Resoluciones, k.Demandados,
       c.Estado, c.Descripcion, c.FechaCreacion, c.FechaEncargoCausa, c.FechaGestionCausa,
       c.MateriaId, c.Materia, c.TipoCausaId, c.NumeroRol
FROM #CausaClave k
JOIN dbo.Causas c ON c.Id = k.Id
WHERE k.Grupo IS NOT NULL
   OR k.RitNormalizado IN (N'C-TEST-002-2026', N'C-TEST-ESTADOS-2026', N'C-11573-26')
ORDER BY k.Grupo, k.Orden;

/* 1.1b Hijos que se moverian (detalle por fila). */
SELECT N'Diligencia' AS Tipo, d.Id, d.CausaId AS DesdeCausa, k.ConservarId AS HaciaCausa,
       CONVERT(nvarchar(200), d.Descripcion) AS Detalle
FROM dbo.Diligencias d JOIN #CausaClave k ON k.Id = d.CausaId AND k.Orden > 1
UNION ALL
SELECT N'Documento', x.Id, x.CausaId, k.ConservarId, x.Nombre
FROM dbo.Documentos x JOIN #CausaClave k ON k.Id = x.CausaId AND k.Orden > 1
UNION ALL
SELECT N'Recibo', x.Id, x.CausaId, k.ConservarId, CONCAT(N'N° ', x.Numero)
FROM dbo.Recibos x JOIN #CausaClave k ON k.Id = x.CausaId AND k.Orden > 1
UNION ALL
SELECT N'Resolucion', x.Id, x.CausaId, k.ConservarId, LEFT(x.Descripcion, 200)
FROM dbo.Resoluciones x JOIN #CausaClave k ON k.Id = x.CausaId AND k.Orden > 1
ORDER BY DesdeCausa, Tipo;

/* 1.2 Demandados de causas duplicadas (MOVER a la conservada / ELIMINAR en cascada,
       junto con sus avales).                                                         */
SELECT p.Accion, p.CausaOrigenId, p.CausaDestinoId, d.Id AS DemandadoId, d.Nombre, d.Rut,
       d.EsPrincipal,
       (SELECT COUNT(*) FROM dbo.AvalesSolidarios a WHERE a.DemandadoId = d.Id) AS Avales,
       (SELECT COUNT(*) FROM dbo.Demandados x WHERE x.CausaId = p.CausaDestinoId) AS DemandadosEnConservada
FROM #DemandadoPlan p
JOIN dbo.Demandados d ON d.Id = p.DemandadoId
ORDER BY p.CausaOrigenId, d.EsPrincipal DESC;

/* 1.3 Tribunales: valores que cambian y grupos de variantes. */
SELECT t.Canonico AS FormaCanonica, t.Original, t.Normalizado,
       CASE WHEN t.Original <> t.Canonico THEN N'CAMBIA' ELSE N'' END AS Cambio,
       m.Motivo AS CorreccionManual,
       t.UsosCausas, t.UsosRecibos, t.EnCatalogo
FROM #TribunalValor t
LEFT JOIN #MapaTribunalManual m ON m.Desde = t.Normalizado COLLATE Modern_Spanish_CI_AS
WHERE t.Original <> t.Canonico
   OR EXISTS (SELECT 1 FROM #TribunalValor o
              WHERE o.Canonico = t.Canonico AND o.Original <> t.Original)
ORDER BY t.Canonico, t.Original;

/* 1.3b Tribunales sin nombre (solo numero, p. ej. "10°") y causas que los usan. */
SELECT k.Id AS CausaId, k.Rit, k.Tribunal, k.TribunalCanonico, c.Descripcion, c.FechaEncargoCausa
FROM #CausaClave k JOIN dbo.Causas c ON c.Id = k.Id
WHERE k.TribunalCanonico NOT LIKE N'%[A-Za-z]%'
ORDER BY k.TribunalCanonico, k.Rit;

/* 1.3c Formas canonicas en uso que no existen en el catalogo Tribunales ni estan entre
       los 1°-30° que se crearan (posibles erratas aun no cubiertas).                 */
SELECT t.Canonico, SUM(t.UsosCausas) AS UsosCausas, SUM(t.UsosRecibos) AS UsosRecibos
FROM #TribunalValor t
WHERE NOT EXISTS (SELECT 1 FROM #TribunalValor x WHERE x.Canonico = t.Canonico AND x.EnCatalogo = 1)
  AND NOT EXISTS (SELECT 1 FROM #JuzgadoSantiago j WHERE j.Nombre = t.Canonico)
GROUP BY t.Canonico
ORDER BY t.Canonico;

/* 1.3d Juzgados 1°-30° que se crearian en el catalogo (2.1). */
SELECT j.Nombre AS TribunalACrear
FROM #JuzgadoSantiago j
WHERE NOT EXISTS (SELECT 1 FROM #TribunalValor t WHERE t.Canonico = j.Nombre AND t.EnCatalogo = 1)
ORDER BY LEN(j.Nombre), j.Nombre;

/* 1.4 Tipos de diligencia con variantes de tilde/mayusculas. */
SELECT k.Grupo, k.Orden,
       CASE WHEN k.Orden = 1 THEN N'CONSERVAR' ELSE N'ELIMINAR' END AS Accion,
       k.Id, k.Nombre, k.CodigoSistema, k.CodigoDestino, k.Sembrado, k.ConTilde, k.Activo, k.Referencias
FROM #TipoMapa k
WHERE k.Orden IS NOT NULL
ORDER BY k.Grupo, k.Orden;

SELECT m.Desde AS CodigoActual, m.Hacia AS CodigoNuevo,
       (SELECT COUNT(*) FROM dbo.Diligencias           x WHERE x.Tipo = m.Desde)                 AS Diligencias,
       (SELECT COUNT(*) FROM dbo.ModelosEstampe        x WHERE x.TipoDiligencia = m.Desde)       AS Modelos,
       (SELECT COUNT(*) FROM dbo.DiligenciasRealizadas x WHERE x.CodigoTipoDiligencia = m.Desde) AS Realizadas,
       (SELECT COUNT(*) FROM dbo.DiligenciasEncargadas x WHERE x.CodigoTipoDiligencia = m.Desde) AS Encargadas
FROM #MapaCodigo m;

/* 1.5 Diligencias realizadas con variantes de tilde/mayusculas. */
SELECT k.CodigoDestino, k.Grupo, k.Orden,
       CASE WHEN k.Orden = 1 THEN N'CONSERVAR' ELSE N'ELIMINAR -> ' + CONVERT(nvarchar(36), k.ConservarId) END AS Accion,
       k.Id, k.Nombre, k.CodigoTipo, k.Activo, k.Arancel, k.ConTilde,
       (SELECT COUNT(*) FROM dbo.Diligencias    x WHERE x.DiligenciaRealizadaId = k.Id) AS Diligencias,
       (SELECT COUNT(*) FROM dbo.Recibos        x WHERE x.DiligenciaRealizadaId = k.Id) AS Recibos,
       (SELECT COUNT(*) FROM dbo.ModelosEstampe x WHERE x.DiligenciaRealizadaId = k.Id) AS Modelos
FROM #RealizadaMapa k
WHERE k.Orden IS NOT NULL
ORDER BY k.CodigoDestino, k.Grupo, k.Orden;

/* 1.5b Texto Diligencias.DiligenciaRealizada distinto al nombre del catalogo solo por
        tildes/mayusculas (se unificaria en 2.4).                                     */
SELECT d.DiligenciaRealizada AS TextoActual, r.Nombre AS TextoCatalogo, COUNT(*) AS Diligencias
FROM dbo.Diligencias d
JOIN #RealizadaMapa k ON k.Id = d.DiligenciaRealizadaId
JOIN #RealizadaMapa r ON r.Id = ISNULL(k.ConservarId, k.Id)
WHERE d.DiligenciaRealizada COLLATE Latin1_General_CI_AI = r.Nombre COLLATE Latin1_General_CI_AI
  AND d.DiligenciaRealizada COLLATE Latin1_General_BIN2 <> r.Nombre
GROUP BY d.DiligenciaRealizada, r.Nombre;

/* 1.5c Solo informativo: diligencias encargadas con variantes (la PARTE 2 no las toca). */
SELECT e.Nombre COLLATE Latin1_General_CI_AI AS Grupo, e.Id, e.Nombre, e.CodigoTipoDiligencia, e.Activo
FROM dbo.DiligenciasEncargadas e
WHERE EXISTS (SELECT 1 FROM dbo.DiligenciasEncargadas o
              WHERE o.Id <> e.Id
                AND TRIM(o.Nombre) COLLATE Latin1_General_CI_AI = TRIM(e.Nombre) COLLATE Latin1_General_CI_AI)
ORDER BY 1, e.Nombre;

/* 1.6 ROL guardado sin normalizar: se corrige en 2.2b (Causas.Rit y Recibos.Rol). */
SELECT N'Causa' AS Origen, Id, Rit AS Actual, RitNormalizado AS Nuevo, Tribunal
FROM #CausaClave
WHERE Rit <> RitNormalizado AND (Orden IS NULL OR Orden = 1)
UNION ALL
SELECT N'Recibo N° ' + CONVERT(nvarchar(20), r.Numero), r.Id, r.Rol COLLATE Latin1_General_BIN2,
       k.RitNormalizado, r.Tribunal COLLATE Latin1_General_BIN2
FROM dbo.Recibos r
JOIN #CausaClave k ON k.Id = r.CausaId
WHERE r.Rol COLLATE Latin1_General_BIN2 <> k.RitNormalizado
  AND (REPLACE(UPPER(TRIM(r.Rol)), N' ', N'') COLLATE Latin1_General_BIN2 = k.RitNormalizado
       OR N'C-' + REPLACE(UPPER(TRIM(r.Rol)), N' ', N'') COLLATE Latin1_General_BIN2 = k.RitNormalizado)
ORDER BY Nuevo, Origen;

/* 1.7 Diligencias Completadas (Estado = 3) sin resultado (Resultado = 0) y/o sin fecha de
       gestion. SOLO LISTADO; la PARTE 2 no las modifica.                             */
SELECT d.Id, c.Rit, c.Tribunal, d.Tipo, d.DiligenciaRealizada, d.Estado, d.Resultado,
       d.ResultadoDetalle, d.FechaGestion, d.FechaRealizada, d.FechaCreacion,
       CASE WHEN d.Resultado = 0 AND d.FechaGestion IS NULL THEN N'SIN RESULTADO NI FECHA'
            WHEN d.Resultado = 0 THEN N'SIN RESULTADO'
            ELSE N'SIN FECHA DE GESTION' END AS Problema
FROM dbo.Diligencias d
JOIN dbo.Causas c ON c.Id = d.CausaId
WHERE d.Estado = 3
  AND (d.Resultado = 0 OR d.FechaGestion IS NULL)
ORDER BY CASE WHEN d.Resultado = 0 AND d.FechaGestion IS NULL THEN 0 ELSE 1 END, c.Rit, d.FechaCreacion;

/* 1.8 Simulacion del indice unico tal como esta hoy (con la colacion de la columna):
       grupos que harian fallar CREATE UNIQUE INDEX (Rit, Tribunal).                  */
SELECT Rit, Tribunal, COUNT(*) AS Filas
FROM dbo.Causas
GROUP BY Rit, Tribunal
HAVING COUNT(*) > 1;
GO

/* =============================================================================
   PARTE 2 - CAMBIOS (transaccion con ROLLBACK por defecto)
   Ejecutar en la MISMA sesion que la PARTE 0.
   ============================================================================= */
DECLARE @Confirmar bit = 0;   -- <<< 0 = ROLLBACK (por defecto) | 1 = COMMIT
DECLARE @n int;

SET XACT_ABORT ON;

IF OBJECT_ID(N'tempdb..#CausaClave') IS NULL OR OBJECT_ID(N'tempdb..#TribunalValor') IS NULL
   OR OBJECT_ID(N'tempdb..#TipoMapa') IS NULL OR OBJECT_ID(N'tempdb..#RealizadaMapa') IS NULL
   OR OBJECT_ID(N'tempdb..#DemandadoPlan') IS NULL OR OBJECT_ID(N'tempdb..#MapaCodigo') IS NULL
   OR OBJECT_ID(N'tempdb..#JuzgadoSantiago') IS NULL
BEGIN
    RAISERROR(N'Ejecute primero la PARTE 0 en esta misma sesion.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

/* ---- 2.1 Tribunales ------------------------------------------------------- */
UPDATE c SET Tribunal = t.Canonico
FROM dbo.Causas c
JOIN #TribunalValor t ON t.Original = c.Tribunal COLLATE Latin1_General_BIN2
WHERE c.Tribunal COLLATE Latin1_General_BIN2 <> t.Canonico;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.1 Causas con tribunal canonizado: ', @n);

-- Tambien la foto del tribunal en los recibos emitidos (pedido por el usuario).
UPDATE r SET Tribunal = t.Canonico
FROM dbo.Recibos r
JOIN #TribunalValor t ON t.Original = r.Tribunal COLLATE Latin1_General_BIN2
WHERE r.Tribunal COLLATE Latin1_General_BIN2 <> t.Canonico;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.1 Recibos con tribunal canonizado: ', @n);

DROP TABLE IF EXISTS #TribunalCatalogoPlan;
WITH cat AS (
    SELECT x.Id, t.Canonico,
           ROW_NUMBER() OVER (PARTITION BY t.Canonico
                              ORDER BY CASE WHEN x.Nombre COLLATE Latin1_General_BIN2 = t.Canonico THEN 0 ELSE 1 END,
                                       x.Activo DESC, x.FechaCreacion, x.Id) AS rn,
           MAX(CAST(x.Activo AS int)) OVER (PARTITION BY t.Canonico) AS AlgunoActivo
    FROM dbo.Tribunales x
    JOIN #TribunalValor t ON t.Original = x.Nombre COLLATE Latin1_General_BIN2)
SELECT * INTO #TribunalCatalogoPlan FROM cat;

DELETE x FROM dbo.Tribunales x
JOIN #TribunalCatalogoPlan p ON p.Id = x.Id AND p.rn > 1;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.1 Tribunales duplicados eliminados del catalogo: ', @n);

UPDATE x SET Nombre = p.Canonico,
             Activo = CASE WHEN p.AlgunoActivo = 1 THEN 1 ELSE x.Activo END
FROM dbo.Tribunales x
JOIN #TribunalCatalogoPlan p ON p.Id = x.Id AND p.rn = 1
WHERE x.Nombre COLLATE Latin1_General_BIN2 <> p.Canonico
   OR (p.AlgunoActivo = 1 AND x.Activo = 0);
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.1 Tribunales del catalogo renombrados/reactivados: ', @n);

INSERT INTO dbo.Tribunales (Id, Nombre, Activo, FechaCreacion)
SELECT NEWID(), j.Nombre, 1, SYSUTCDATETIME()
FROM #JuzgadoSantiago j
WHERE NOT EXISTS (SELECT 1 FROM dbo.Tribunales x
                  WHERE x.Nombre = j.Nombre COLLATE Modern_Spanish_CI_AS);
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.1 Juzgados 1°-30° creados en el catalogo: ', @n);

/* ---- 2.2 Causas duplicadas ------------------------------------------------ */
UPDATE x SET CausaId = k.ConservarId
FROM dbo.Diligencias x JOIN #CausaClave k ON k.Id = x.CausaId AND k.Orden > 1;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.2 Diligencias reasignadas: ', @n);

UPDATE x SET CausaId = k.ConservarId
FROM dbo.Documentos x JOIN #CausaClave k ON k.Id = x.CausaId AND k.Orden > 1;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.2 Documentos reasignados: ', @n);

UPDATE x SET CausaId = k.ConservarId
FROM dbo.Recibos x JOIN #CausaClave k ON k.Id = x.CausaId AND k.Orden > 1;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.2 Recibos reasignados: ', @n);

UPDATE x SET CausaId = k.ConservarId
FROM dbo.Resoluciones x JOIN #CausaClave k ON k.Id = x.CausaId AND k.Orden > 1;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.2 Resoluciones reasignadas: ', @n);

UPDATE d SET CausaId = p.CausaDestinoId
FROM dbo.Demandados d JOIN #DemandadoPlan p ON p.DemandadoId = d.Id AND p.Accion = N'MOVER';
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.2 Demandados movidos: ', @n);

IF EXISTS (SELECT 1 FROM dbo.Diligencias  x JOIN #CausaClave k ON k.Id = x.CausaId AND k.Orden > 1)
OR EXISTS (SELECT 1 FROM dbo.Documentos   x JOIN #CausaClave k ON k.Id = x.CausaId AND k.Orden > 1)
OR EXISTS (SELECT 1 FROM dbo.Recibos      x JOIN #CausaClave k ON k.Id = x.CausaId AND k.Orden > 1)
OR EXISTS (SELECT 1 FROM dbo.Resoluciones x JOIN #CausaClave k ON k.Id = x.CausaId AND k.Orden > 1)
    THROW 50010, N'Quedan hijos en causas duplicadas; no se elimina nada.', 1;

-- Borra las duplicadas; Demandados (ELIMINAR) y sus avales caen por cascada.
DELETE c FROM dbo.Causas c JOIN #CausaClave k ON k.Id = c.Id AND k.Orden > 1;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.2 Causas duplicadas eliminadas: ', @n);

/* ---- 2.2b ROL normalizado (IdentificacionCausa.NormalizarRol), p. ej. 1188-26 -> C-1188-26 */
UPDATE r SET Rol = k.RitNormalizado
FROM dbo.Recibos r
JOIN #CausaClave k ON k.Id = r.CausaId
WHERE r.Rol COLLATE Latin1_General_BIN2 <> k.RitNormalizado
  AND (REPLACE(UPPER(TRIM(r.Rol)), N' ', N'') COLLATE Latin1_General_BIN2 = k.RitNormalizado
       OR N'C-' + REPLACE(UPPER(TRIM(r.Rol)), N' ', N'') COLLATE Latin1_General_BIN2 = k.RitNormalizado);
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.2b Recibos con ROL normalizado: ', @n);

UPDATE c SET Rit = k.RitNormalizado
FROM dbo.Causas c
JOIN #CausaClave k ON k.Id = c.Id
WHERE c.Rit COLLATE Latin1_General_BIN2 <> k.RitNormalizado;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.2b Causas con ROL normalizado: ', @n);

/* ---- 2.4 (antes que 2.3) Diligencias realizadas -------------------------------
   Se fusionan antes de cambiar codigos para no chocar con el indice unico
   (CodigoTipoDiligencia, Nombre).                                               */
UPDATE x SET DiligenciaRealizadaId = k.ConservarId
FROM dbo.Diligencias x JOIN #RealizadaMapa k ON k.Id = x.DiligenciaRealizadaId AND k.Orden > 1;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.4 Diligencias con diligencia realizada reasignada: ', @n);

UPDATE x SET DiligenciaRealizadaId = k.ConservarId
FROM dbo.Recibos x JOIN #RealizadaMapa k ON k.Id = x.DiligenciaRealizadaId AND k.Orden > 1;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.4 Recibos con diligencia realizada reasignada: ', @n);
-- Recibos.DiligenciaRealizada (texto impreso en el recibo) se deja como se emitio.

UPDATE x SET DiligenciaRealizadaId = k.ConservarId
FROM dbo.ModelosEstampe x JOIN #RealizadaMapa k ON k.Id = x.DiligenciaRealizadaId AND k.Orden > 1;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.4 Modelos de estampe reasignados: ', @n);

UPDATE r SET Arancel = ISNULL(r.Arancel, g.Arancel),
             Activo  = CASE WHEN g.AlgunoActivo = 1 THEN 1 ELSE r.Activo END
FROM dbo.DiligenciasRealizadas r
JOIN (SELECT ConservarId, MAX(Arancel) AS Arancel, MAX(CAST(Activo AS int)) AS AlgunoActivo
      FROM #RealizadaMapa WHERE Orden IS NOT NULL GROUP BY ConservarId) g ON g.ConservarId = r.Id
WHERE (r.Arancel IS NULL AND g.Arancel IS NOT NULL) OR (g.AlgunoActivo = 1 AND r.Activo = 0);

DELETE r FROM dbo.DiligenciasRealizadas r JOIN #RealizadaMapa k ON k.Id = r.Id AND k.Orden > 1;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.4 Diligencias realizadas duplicadas eliminadas: ', @n);

/* ---- 2.3 Tipos de diligencia ---------------------------------------------- */
UPDATE x SET CodigoTipoDiligencia = m.Hacia
FROM dbo.DiligenciasRealizadas x JOIN #MapaCodigo m ON m.Desde = x.CodigoTipoDiligencia;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.3 Diligencias realizadas con codigo de tipo unificado: ', @n);

UPDATE x SET CodigoTipoDiligencia = m.Hacia
FROM dbo.DiligenciasEncargadas x JOIN #MapaCodigo m ON m.Desde = x.CodigoTipoDiligencia;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.3 Diligencias encargadas con codigo de tipo unificado: ', @n);

UPDATE x SET Tipo = m.Hacia
FROM dbo.Diligencias x JOIN #MapaCodigo m ON m.Desde = x.Tipo;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.3 Diligencias con tipo unificado: ', @n);

UPDATE x SET TipoDiligencia = m.Hacia
FROM dbo.ModelosEstampe x JOIN #MapaCodigo m ON m.Desde = x.TipoDiligencia;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.3 Modelos de estampe con tipo unificado: ', @n);

DELETE t FROM dbo.TiposDiligencia t JOIN #TipoMapa k ON k.Id = t.Id AND k.Orden > 1;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.3 Tipos de diligencia duplicados eliminados: ', @n);

UPDATE t SET CodigoSistema = k.CodigoDestino,
             Activo = CASE WHEN g.AlgunoActivo = 1 THEN 1 ELSE t.Activo END
FROM dbo.TiposDiligencia t
JOIN #TipoMapa k ON k.Id = t.Id AND k.Orden = 1
JOIN (SELECT ConservarId, MAX(CAST(Activo AS int)) AS AlgunoActivo
      FROM #TipoMapa WHERE Orden IS NOT NULL GROUP BY ConservarId) g ON g.ConservarId = t.Id
WHERE (t.CodigoSistema IS NULL AND k.CodigoDestino IS NOT NULL) OR (g.AlgunoActivo = 1 AND t.Activo = 0);

/* ---- 2.4 (fin) Texto de la diligencia realizada en Diligencias ------------- */
UPDATE d SET DiligenciaRealizada = r.Nombre
FROM dbo.Diligencias d
JOIN dbo.DiligenciasRealizadas r ON r.Id = d.DiligenciaRealizadaId
WHERE d.DiligenciaRealizada COLLATE Latin1_General_CI_AI = r.Nombre COLLATE Latin1_General_CI_AI
  AND d.DiligenciaRealizada COLLATE Latin1_General_BIN2 <> r.Nombre COLLATE Latin1_General_BIN2;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.4 Textos de diligencia realizada unificados (con Id): ', @n);

UPDATE d SET DiligenciaRealizada = r.Nombre
FROM dbo.Diligencias d
JOIN dbo.DiligenciasRealizadas r
  ON r.CodigoTipoDiligencia = d.Tipo
 AND TRIM(d.DiligenciaRealizada) COLLATE Latin1_General_CI_AI = r.Nombre COLLATE Latin1_General_CI_AI
WHERE d.DiligenciaRealizadaId IS NULL
  AND d.DiligenciaRealizada COLLATE Latin1_General_BIN2 <> r.Nombre COLLATE Latin1_General_BIN2;
SET @n = @@ROWCOUNT; PRINT CONCAT(N'2.4 Textos de diligencia realizada unificados (sin Id): ', @n);

/* ---- 2.5 Verificacion (estado "despues", dentro de la transaccion) --------- */
-- Debe devolver 0 filas: es lo que comprobara CREATE UNIQUE INDEX (Rit, Tribunal).
SELECT N'Causas duplicadas (Rit, Tribunal)' AS Verificacion, Rit, Tribunal, COUNT(*) AS Filas
FROM dbo.Causas GROUP BY Rit, Tribunal HAVING COUNT(*) > 1;

-- Debe devolver 0 filas (variantes de mayusculas/tilde restantes).
SELECT N'Tribunales con variantes' AS Verificacion, Tribunal COLLATE Modern_Spanish_CI_AS AS Tribunal,
       COUNT(DISTINCT Tribunal COLLATE Latin1_General_BIN2) AS Variantes
FROM dbo.Causas GROUP BY Tribunal COLLATE Modern_Spanish_CI_AS
HAVING COUNT(DISTINCT Tribunal COLLATE Latin1_General_BIN2) > 1;

SELECT N'Tipos de diligencia con variantes' AS Verificacion, TRIM(Nombre) COLLATE Latin1_General_CI_AI AS Nombre, COUNT(*) AS Filas
FROM dbo.TiposDiligencia GROUP BY TRIM(Nombre) COLLATE Latin1_General_CI_AI HAVING COUNT(*) > 1;

SELECT N'Diligencias realizadas con variantes' AS Verificacion, CodigoTipoDiligencia,
       TRIM(Nombre) COLLATE Latin1_General_CI_AI AS Nombre, COUNT(*) AS Filas
FROM dbo.DiligenciasRealizadas
GROUP BY CodigoTipoDiligencia, TRIM(Nombre) COLLATE Latin1_General_CI_AI HAVING COUNT(*) > 1;

-- ROL sin normalizar restantes (debe ser 0 filas).
SELECT N'ROL sin normalizar' AS Verificacion, c.Id, c.Rit
FROM dbo.Causas c
WHERE c.Rit COLLATE Latin1_General_BIN2 LIKE N'[0-9]%-[0-9]%'
  AND c.Rit NOT LIKE N'%[^0-9-]%';

-- Juzgados 1°-30° ausentes del catalogo (debe ser 0 filas).
SELECT N'Juzgado faltante en catalogo' AS Verificacion, j.Nombre
FROM #JuzgadoSantiago j
WHERE NOT EXISTS (SELECT 1 FROM dbo.Tribunales x WHERE x.Nombre COLLATE Latin1_General_BIN2 = j.Nombre);

-- Tribunales usados por causas que no estan en el catalogo (revisar).
SELECT N'Tribunal de causa fuera del catalogo' AS Verificacion, c.Tribunal, COUNT(*) AS Causas
FROM dbo.Causas c
WHERE NOT EXISTS (SELECT 1 FROM dbo.Tribunales x WHERE x.Nombre COLLATE Latin1_General_BIN2 = c.Tribunal COLLATE Latin1_General_BIN2)
GROUP BY c.Tribunal;

SELECT N'Recibos (despues)' AS Verificacion, Numero, Rol, Tribunal FROM dbo.Recibos ORDER BY Numero;

-- Referencias huerfanas por codigo de tipo (deben ser 0).
SELECT N'Codigos de tipo sin catalogo' AS Verificacion, x.Origen, x.Codigo, COUNT(*) AS Filas
FROM (SELECT N'Diligencias' AS Origen, Tipo AS Codigo FROM dbo.Diligencias
      UNION ALL SELECT N'ModelosEstampe', TipoDiligencia FROM dbo.ModelosEstampe
      UNION ALL SELECT N'DiligenciasRealizadas', CodigoTipoDiligencia FROM dbo.DiligenciasRealizadas
      UNION ALL SELECT N'DiligenciasEncargadas', CodigoTipoDiligencia FROM dbo.DiligenciasEncargadas
                WHERE CodigoTipoDiligencia IS NOT NULL) x
WHERE x.Codigo IN (SELECT Desde FROM #MapaCodigo)
GROUP BY x.Origen, x.Codigo;

-- Referencias a diligencias realizadas eliminadas (deben ser 0).
SELECT N'DiligenciaRealizadaId huerfano' AS Verificacion, x.Origen, COUNT(*) AS Filas
FROM (SELECT N'Diligencias' AS Origen, DiligenciaRealizadaId AS Id FROM dbo.Diligencias
      UNION ALL SELECT N'Recibos', DiligenciaRealizadaId FROM dbo.Recibos
      UNION ALL SELECT N'ModelosEstampe', DiligenciaRealizadaId FROM dbo.ModelosEstampe) x
JOIN #RealizadaMapa k ON k.Id = x.Id AND k.Orden > 1
GROUP BY x.Origen;

IF @Confirmar = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT N'>>> COMMIT: cambios confirmados.';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT N'>>> ROLLBACK: no se guardo ningun cambio (cambie @Confirmar = 1 para confirmar).';
END;
GO
