# Validación funcional local — 14 de octubre de 2026

## Entorno y alcance

Pruebas desde el navegador contra Web y API en Release, con SQL Server Express real.
Se creó la base aislada `JurigestEnterprise_Flujos_20261014` y se aplicaron las 39 migraciones desde cero.
La base habitual `JurigestEnterprise` no se modificó. Se usaron exclusivamente datos sintéticos.

## Flujos comprobados

| Flujo | Resultado observado |
| --- | --- |
| Inicio de sesión | Acceso al panel con administrador de prueba; credenciales incorrectas rechazadas. |
| Catálogos | Alta de tribunal, abogado, comuna, diligencia encargada, diligencia realizada y receptor desde la interfaz. |
| Ingreso judicial | Creación de `C-990014-26` con demandado, materia y primera diligencia; recuperación mediante búsqueda por ROL. |
| Resultado y estampe | Borrador generado, resultado positivo guardado y diligencia completada. |
| Última gestión | SQL confirmó que `Causas.FechaGestionCausa` coincide con `Diligencias.FechaGestion`. |
| Recibo | Emisión automática por $25.000; pago registrado conservando número 1 y monto; visualización en recibos pagados. |
| Documentos | PDF de 614 bytes cargado y listado; descarga con SHA-256 idéntico al original. |
| Validación documental | Archivo DOC con contenido incompatible rechazado; el formulario permite corregir y volver a intentar. |
| Salud y respaldo | API y SQL operativos; respaldo JSON descargado y leído con una causa y una diligencia. |
| Navegación adaptable | Menú abierto a 600 píxeles, con acceso a Documentos; navegación de escritorio revisada. |
| Cierre de sesión | Regreso a login; acceso posterior a Documentos redirige a login; API sin token devuelve 401. |

## Fallos encontrados y corregidos

1. El pago de recibos enviaba un UPDATE de la columna IDENTITY `Numero`, rechazado por SQL Server con error 8102. Se excluye el correlativo de las modificaciones. Dos pruebas de regresión comprueban el pago con entidades conectadas y desconectadas y la conservación del número y monto.
2. La carga documental perdía el elemento `InputFile` al reevaluar asincrónicamente el `AuthorizeView` durante el renderizado. Se extrajo el formulario a `CargaDocumento`, manteniendo la autorización en la página y la validación de cada petición en la API. La carga y descarga se repitieron correctamente desde el navegador.
3. El menú ocultaba las secciones judiciales y administrativas por debajo de 800 píxeles. Se añadió un control nativo para desplegarlas; también se corrigieron los selectores de estilos de los enlaces y se habilitó desplazamiento vertical en la barra lateral de escritorio.

## Verificaciones automáticas

- Dominio: 46 pruebas aprobadas.
- Aplicación: 10 pruebas aprobadas.
- Integración: 63 pruebas aprobadas, incluidas las dos nuevas regresiones.
- Solución completa Release, incluido MAUI: compilación sin errores ni advertencias.
- Formato C# y `git diff --check`: sin incidencias.

## Límites de esta validación

No se probaron envío SMTP, impresión física, clientes MAUI en dispositivos, restauración de respaldos, carga de archivos de gran tamaño ni todos los roles en SQL Server. Las pruebas automatizadas de permisos e integración continúan usando EF InMemory; la prueba manual de pago con SQL Server complementa esa cobertura.

La hora de registro documental se muestra en UTC, mientras otras pantallas presentan hora local. Conviene unificar el criterio de presentación de fechas en una revisión específica.

Los servicios temporales y la base aislada se retiran al finalizar. Los archivos de prueba y logs quedan en `.runlogs/`, excluido de Git; las capturas de evidencia se guardan fuera del repositorio.
