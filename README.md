# Jurigest Enterprise

Plataforma para la gestión de causas, diligencias, documentos y resoluciones de oficinas de receptores judiciales de Chile.

## Arquitectura

La solución utiliza .NET 10 y una separación inspirada en DDD y Clean Architecture:

- `Jurigest.Domain`: entidades, invariantes, catálogos y reglas de negocio.
- `Jurigest.Application`: casos de uso con MediatR y abstracciones de infraestructura.
- `Jurigest.Persistence`: Entity Framework Core, SQL Server, repositorios, archivos y correo.
- `Jurigest.API`: API REST, autenticación JWT, autorización y rate limiting.
- `Jurigest.Web`: interfaz Blazor Server que consume la API.
- `Jurigest.Mobile`: cliente .NET MAUI en etapa inicial.

## Requisitos

- .NET SDK 10.
- SQL Server Express disponible como `localhost\\SQLEXPRESS`, o una cadena equivalente.
- Herramienta `dotnet-ef` 10 para administrar migraciones.

## Configuración local

La API requiere estas claves mediante secretos de usuario o variables de entorno:

```text
Jwt__Issuer
Jwt__Audience
Jwt__Key
Jwt__ExpirationMinutes
```

La cadena predeterminada está en `src/Jurigest.API/appsettings.json`. No deben incorporarse contraseñas, claves JWT ni credenciales SMTP al repositorio.

## Base de datos

```powershell
dotnet ef database update --project src/Jurigest.Persistence --startup-project src/Jurigest.API
```

Para comprobar que el modelo y las migraciones coinciden:

```powershell
dotnet ef migrations has-pending-model-changes --project src/Jurigest.Persistence --startup-project src/Jurigest.API
```

## Ejecución

En terminales separadas:

```powershell
dotnet run --project src/Jurigest.API
dotnet run --project src/Jurigest.Web
```

Direcciones predeterminadas:

- Web: `http://localhost:5088`
- API: `http://localhost:5289`

## Validación

```powershell
dotnet build JurigestEnterprise.slnx
dotnet test test/Jurigest.Domain.Tests
dotnet test test/Jurigest.Application.Tests
dotnet test test/Jurigest.Integration.Tests
```

La compilación completa incluye MAUI y requiere las cargas de trabajo y SDK de las plataformas móviles. Para validar solamente el servidor y la Web, como en CI:

```powershell
dotnet build src/Jurigest.API -c Release
dotnet build src/Jurigest.Web -c Release
```

El formato C# usa cuatro espacios y se comprueba en CI. Para verificar o corregir un proyecto:

```powershell
dotnet format whitespace src/Jurigest.Application --no-restore --verify-no-changes
dotnet format whitespace src/Jurigest.Application --no-restore
```

Para auditar también las dependencias transitivas:

```powershell
dotnet list src/Jurigest.API package --vulnerable --include-transitive
```

El flujo judicial crítico cubierto por pruebas es:

```text
Causa → Diligencia → Registro de resultado → Actualización de última gestión
```

## Seguridad

La API aplica JWT, roles, revocación de sesiones, bloqueo temporal, auditoría y límites de solicitudes. Los permisos principales son Administrador, Abogado, Procurador y Consulta.

## Producción

La preparación, variables obligatorias, despliegue, monitoreo y recuperación están documentados en [`docs/PRODUCCION.md`](docs/PRODUCCION.md). Los secretos deben suministrarse mediante variables de entorno o un almacén seguro.
