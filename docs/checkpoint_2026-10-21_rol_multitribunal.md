# Checkpoint 2026-10-21: limpieza de datos, índice único y ROL en varios tribunales

Punto de retorno para retomar las pruebas y la verificación funcional.

## Estado al crear el checkpoint

- **Rama:** `fix/frontend-menu-estetica`, HEAD `d16c07a`. Nada de este trabajo está confirmado en git; convive con otros cambios sin confirmar de la rama (unos 178 archivos en `git status`).
- **Base:** `JurigestEnterprise` en `localhost\SQLEXPRESS`. 31 causas, 30 tribunales, 44 diligencias, 9 recibos. Última migración: `20261021025327_IndiceUnicoCausaRitTribunal`.
- **Respaldo de este punto:** `C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\JurigestEnterprise_checkpoint_20261021.bak` (COPY_ONLY, verificado).
- **Respaldos anteriores**, en la misma carpeta:
  - `JurigestEnterprise_antes_limpieza_20261020.bak`: estado original, antes de la limpieza.
  - `JurigestEnterprise_despues_limpieza_20261020.bak`: limpieza aplicada, antes de borrar la causa C-WEB-001-2026.
- **Servicios:** API en `http://localhost:5289` y Web en `http://localhost:5088`, perfil `http` (`dotnet run --no-build --project src/Jurigest.<API|Web> --launch-profile http`).

## Datos de prueba vigentes

Hay 3 causas con el ROL **C-9001-26** en el 1°, 2° y 10° Juzgado Civil de Santiago. Su descripción empieza con "PRUEBA MULTITRIBUNAL" y cada una tiene una diligencia pendiente con dirección. Se dejaron para generar diligencias y probar.

Para borrarlas al terminar (incluye recibos y vehículos, que no se eliminan en cascada):

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -C -d JurigestEnterprise -Q "SET XACT_ABORT ON; SET QUOTED_IDENTIFIER ON; BEGIN TRAN; DECLARE @ids TABLE (Id uniqueidentifier); INSERT @ids SELECT Id FROM Causas WHERE Rit = N'C-9001-26' AND Descripcion LIKE N'PRUEBA MULTITRIBUNAL%'; DELETE r FROM Recibos r WHERE r.CausaId IN (SELECT Id FROM @ids); DELETE v FROM VehiculosEncargo v JOIN Diligencias d ON d.Id = v.DiligenciaId WHERE d.CausaId IN (SELECT Id FROM @ids); DELETE FROM Causas WHERE Id IN (SELECT Id FROM @ids); SELECT @@ROWCOUNT AS CausasEliminadas; COMMIT;"
```

## Qué se hizo

1. **Limpieza de datos** (`database/limpieza_calidad_datos_2026-10-20.sql`, aplicada con COMMIT):
   - Se eliminaron 5 causas duplicadas.
   - Los tribunales quedaron en su forma canónica, incluidos los recibos, y el catálogo tiene exactamente del 1° al 30° Juzgado Civil de Santiago.
   - Los ROL quedaron normalizados (por ejemplo "1188-26" pasó a "C-1188-26").
   - Se fusionó "Notificacion Personal" con "Notificación personal".
   - Además se borró la causa de prueba C-WEB-001-2026.
2. **Índice único** `UX_Causas_Rit_Tribunal` sobre (Rit, Tribunal): migración `20261021025327_IndiceUnicoCausaRitTribunal`, aplicada.
3. **Mensaje de causa duplicada:** "Ya existe una causa con ese ROL en el mismo tribunal." al crear y al editar.
   - Editar ahora revisa duplicados antes de guardar.
   - Si el índice rechaza un guardado concurrente (SQL 2601/2627), el repositorio lo traduce al mismo mensaje.
   - La Web muestra solo el texto de `mensaje` de la API.
4. **Mismo ROL en otro tribunal:**
   - Botón en la búsqueda rápida del inicio y en "Buscar ROL".
   - Lleva a "Nueva gestión" con el ROL precargado, preselecciona un tribunal libre y avisa en qué tribunales ya existe.
5. **Búsqueda de un ROL presente en varios tribunales:**
   - Endpoint nuevo `GET /api/Causas/rit/{rit}/causas`.
   - El inicio muestra una tarjeta por tribunal y "Buscar ROL" un selector de tribunal.
   - Las pantallas que vuelven a la búsqueda pasan `&causa={id}` para abrir la misma causa.
   - El filtro de receptor ahora revisa todas las causas del ROL.
6. **Tribunales en orden correlativo** (1°, 2°… 30°; los sin número al final), aplicado en la API.

## Archivos tocados

Ninguno está confirmado en git. Los marcados con * ya tenían cambios del usuario antes de este trabajo, y aquí solo se les agregó código.

- **Dominio:** `src/Jurigest.Domain/Judicial/IdentificacionCausa.cs`.
- **Aplicación:**
  - `src/Jurigest.Application/Abstractions/Persistence/ICausaRepository.cs`
  - `.../Causas/Commands/CrearCausa/CrearCausaCommandHandler.cs`
  - `.../Causas/Commands/ActualizarCausa/ActualizarCausaCommandHandler.cs`*
  - `.../Causas/Queries/BuscarPorRit/`: `BuscarCausaPorRitHandler.cs`, y los nuevos `BuscarCausaPorRitMapper.cs` y `BuscarCausasPorRitQuery.cs`
- **Persistencia:**
  - `Configurations/CausaConfiguration.cs`
  - `Repositories/CausaRepository.cs`*
  - `Repositories/TribunalCatalogoRepository.cs`
  - `Migrations/20261021025327_IndiceUnicoCausaRitTribunal.cs` y su `.Designer.cs`
  - `Migrations/JurigestDbContextModelSnapshot.cs`
- **API:** `src/Jurigest.API/Controllers/CausasController.cs`.
- **Web** (`src/Jurigest.Web/Components/Pages/`):
  - `Home.razor`
  - `BuscarRol.razor`*
  - `NuevaGestion.razor`
  - `EditarCausa.razor`
  - `NuevaDiligencia.razor`
  - `RegistrarResultadoDiligencia.razor`*
- **Pruebas:**
  - Nuevas en `test/Jurigest.Integration.Tests/Judicial/`: `CausaDuplicadaTests.cs`, `BuscarRolVariosTribunalesTests.cs` y `OrdenTribunalesTests.cs`.
  - Modificada: `test/Jurigest.Application.Tests/RegistrarResultadoDiligenciaHandlerTests.cs` (el fake del repositorio).
- **Scripts:**
  - `database/limpieza_calidad_datos_2026-10-20.sql`
  - `database/propuesta_migracion_UX_Causas_Rit_Tribunal.cs`: ya no se usa, se puede borrar.

## Verificación hecha

- **Pruebas:** 46 de dominio, 10 de aplicación y 73 de integración, todas correctas (compilación Release).
- **En la Web:**
  - El botón "Mismo ROL en otro tribunal" y los avisos de la pantalla de creación.
  - Las 3 tarjetas de C-9001-26 y que "Historial" abre la causa correcta.
  - El selector de tribunal en "Buscar ROL".
  - El orden 1° a 30° en el selector.

## Pendiente o por verificar al retomar

- [ ] Generar diligencias en las causas C-9001-26 y revisar que cada tarjeta y cada historial muestren las suyas.
- [ ] En la Web, intentar crear y editar una causa duplicada y confirmar que se muestra el mensaje.
- [ ] Probar con un usuario Receptor que la búsqueda muestre solo las causas donde tiene diligencias.
- [ ] Tres diligencias Completadas sin resultado ni fecha de gestión, solo listadas y sin corregir: `ECE724CB…` (C-1000-2026), `AE497156…` (C-INTEGRAL-001-2026) y `2BC9CCCA…` (C-TEST-ESTADOS-2026).
- [ ] Causas de prueba con tribunal fuera del catálogo: "Tribunal de Prueba" (2) y "Tribunal de Prueba Integral" (1).
- [ ] Caso borde de `NormalizarTribunal`: "1°Juzgado" (sin espacio) no se normaliza.
- [ ] Borrar los datos C-9001-26 al terminar (comando de arriba).
- [ ] Decidir cómo confirmar este trabajo en git, separado de los demás cambios de la rama.

## Cómo retomar

1. Levantar la API y la Web (perfil `http`) y comprobar que el endpoint nuevo existe: sin sesión, `curl -s -o /dev/null -w "%{http_code}" http://localhost:5289/api/Causas/rit/C-9001-26/causas` debe devolver 401.
2. Para volver la base exactamente a este punto:

   ```bash
   sqlcmd -S "localhost\SQLEXPRESS" -E -C -d master -Q "ALTER DATABASE JurigestEnterprise SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE JurigestEnterprise FROM DISK = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\JurigestEnterprise_checkpoint_20261021.bak' WITH REPLACE; ALTER DATABASE JurigestEnterprise SET MULTI_USER;"
   ```

   Hay que detener la API antes de restaurar. Se pierde todo lo hecho después del checkpoint.
3. Ejecutar las pruebas en Release (si la API corre desde `bin\Debug`, esa carpeta queda bloqueada):

   ```bash
   dotnet test JurigestEnterprise.slnx --configuration Release
   ```
