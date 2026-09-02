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

El flujo judicial crítico cubierto por pruebas es:

```text
Causa → Diligencia → Registro de resultado → Actualización de última gestión
```

## Seguridad

La API aplica JWT, roles, revocación de sesiones, bloqueo temporal, auditoría y límites de solicitudes. Los permisos principales son Administrador, Abogado, Procurador y Consulta.
