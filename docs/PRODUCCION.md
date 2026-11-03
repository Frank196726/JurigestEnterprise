# Operación en producción

## Arquitectura recomendada

Publique `Jurigest.Web` y `Jurigest.API` como procesos separados detrás de IIS, Nginx o un balanceador que termine TLS. SQL Server debe aceptar conexiones cifradas y no debe exponerse a Internet.

## Secretos y configuración

Use variables de entorno o el almacén de secretos de la plataforma. Tome `deploy/production.env.example` únicamente como catálogo de claves. No copie claves JWT, contraseñas SMTP ni cadenas de conexión al repositorio.

Antes de iniciar, reemplace `AllowedHosts` y `ReverseProxy:KnownProxy` por el dominio y la dirección interna reales. La clave JWT debe tener al menos 64 caracteres aleatorios. La URL pública y la comunicación Web→API deben usar HTTPS.

## IP del usuario en la API

Los navegadores solo hablan con la Web; la API recibe las peticiones desde la Web. Para que el límite de intentos de inicio de sesión (5 por minuto) y de recuperación de contraseña (3 cada 15 minutos) se cuente por usuario, y para que la auditoría guarde la IP del usuario, la Web envía la IP del usuario en la cabecera `X-Jurigest-Cliente-Ip`.

La API solo acepta esa cabecera cuando la conexión viene de una dirección listada en `ReverseProxy:WebConfiable` (variable `ReverseProxy__WebConfiable`, separada por comas). En cualquier otro caso la descarta y usa la IP de la conexión, así que nadie puede cambiar su IP llamando directo a la API.

- `ReverseProxy:WebConfiable` es la dirección con que la Web llega a la API **después** de aplicar `X-Forwarded-For`. Con Web, Nginx y API en el mismo servidor es `127.0.0.1,::1` (valor de `appsettings.Production.json`).
- Si la Web corre en otro equipo, use su IP interna. No ponga direcciones de usuarios ni rangos públicos.
- Si se deja vacío, la API ignora la cabecera y vuelve a ver a todos los usuarios con la IP de la Web.
- El Nginx de la API debe usar `X-Forwarded-For $proxy_add_x_forwarded_for` y `ReverseProxy:KnownProxy` debe ser la IP de Nginx; así un cliente externo nunca aparece como `127.0.0.1`.

## Publicación

```powershell
dotnet publish src/Jurigest.API/Jurigest.API.csproj -c Release -o build/api
dotnet publish src/Jurigest.Web/Jurigest.Web.csproj -c Release -o build/web
dotnet ef database update --project src/Jurigest.Persistence --startup-project src/Jurigest.API --configuration Release
```

Ejecute las migraciones una sola vez antes de habilitar la nueva versión. Use una identidad de servicio sin permisos administrativos del sistema operativo.

## Verificación

- `GET /health/live`: confirma que el proceso API responde.
- `GET /health/ready`: confirma conectividad con SQL Server.
- Inicie sesión y revise `/sistema`.
- Verifique creación de una causa, diligencia y descarga de un respaldo.

Los logs se emiten como JSON a la salida estándar. La plataforma debe recolectarlos, conservarlos y generar alertas por respuestas 500, indisponibilidad y fallos reiterados de autenticación.

## Respaldo y recuperación

El respaldo de `/sistema` es un respaldo lógico judicial para inspección y contingencia. Además, configure respaldos nativos cifrados de SQL Server y del directorio de documentos. Pruebe la recuperación en un ambiente aislado al menos trimestralmente.

Procedimiento de recuperación:

1. Deshabilitar temporalmente el acceso de usuarios.
2. Conservar una copia del estado actual.
3. Restaurar SQL Server y el directorio documental con marcas de tiempo compatibles.
4. Ejecutar las migraciones pendientes.
5. Validar `/health/ready`, conteos y archivos.
6. Habilitar el acceso y documentar el incidente.

Nunca restaure directamente sobre producción sin validar antes el respaldo en un entorno aislado.
