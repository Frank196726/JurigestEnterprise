# Despliegue en un servidor virtual (VPS) Linux

Guía para instalar Jurigest Enterprise en un VPS con **Ubuntu 24.04** y usarlo en marcha blanca o producción. Los archivos que se mencionan están en [`deploy/linux`](../deploy/linux).

## Qué queda instalado

```text
Internet ──HTTPS──► Nginx ──► Jurigest.Web  (127.0.0.1:5001) ──┐
                       └────► Jurigest.API  (127.0.0.1:5000) ◄─┘
                                     │
                                     ▼
                         SQL Server 2025 Express (solo 127.0.0.1)
```

| Componente | Detalle |
|---|---|
| Sitio web | `https://jurigest.<su-dominio>` |
| API | `https://api.<su-dominio>` (la Web la usa internamente) |
| Base de datos | SQL Server 2025 Express, gratuito y con licencia para producción. No se expone a Internet. |
| HTTPS | Certificados gratuitos de Let's Encrypt, con renovación automática |
| Seguridad | Firewall (solo SSH, HTTP y HTTPS), fail2ban y actualizaciones de seguridad automáticas |
| Respaldos | Base de datos y documentos todos los días a las 02:30; se guardan 14 días |
| Zona horaria | America/Santiago |

La API y la Web se publican con su propio runtime de .NET, así que no hace falta instalar .NET en el servidor.

## Antes de empezar

**Servidor.** Un VPS x64 con Ubuntu 24.04, al menos 2 vCPU y 8 GB de RAM (con 4 GB funciona, pero queda justo). Por ejemplo, el plan Cloud-2 de V2Networks en Santiago. Pida acceso SSH como `root` o como un usuario con `sudo`.

> No sirven servidores con procesador ARM (por ejemplo, Oracle Ampere): SQL Server no funciona en ARM.

**Dominio y DNS.** Cree dos registros `A` que apunten a la IP pública del servidor:

| Nombre | Tipo | Valor |
|---|---|---|
| `jurigest.su-dominio.cl` | A | IP del servidor |
| `api.su-dominio.cl` | A | IP del servidor |

**En su equipo Windows:**

- .NET SDK 10.
- La herramienta de migraciones: `dotnet tool install --global dotnet-ef --version 10.*`
- El cliente SSH de Windows (viene instalado en Windows 10 y 11; se comprueba con `ssh -V`).

## Paso 1 · Acceso por SSH con llave

En PowerShell, cree una llave si no tiene una y cópiela al servidor:

```powershell
ssh-keygen -t ed25519
type $env:USERPROFILE\.ssh\id_ed25519.pub | ssh root@IP_DEL_SERVIDOR "mkdir -p ~/.ssh && cat >> ~/.ssh/authorized_keys"
```

Si el proveedor le entregó solo `root`, cree un usuario administrador (reemplace `admin` por el nombre que prefiera):

```bash
adduser admin
usermod -aG sudo admin
rsync -a ~/.ssh /home/admin/ && chown -R admin:admin /home/admin/.ssh
```

Desde ahora conéctese con `ssh admin@IP_DEL_SERVIDOR`.

## Paso 2 · Preparar el servidor (una sola vez)

Desde la raíz del repositorio, en su equipo, copie los archivos de despliegue:

```powershell
scp -r deploy/linux admin@IP_DEL_SERVIDOR:~/jurigest-deploy
```

En el servidor:

```bash
cd ~/jurigest-deploy
chmod +x *.sh
sudo ./instalar-servidor.sh jurigest.su-dominio.cl api.su-dominio.cl su-correo@su-dominio.cl
```

Tarda entre 5 y 10 minutos. El script:

1. Actualiza Ubuntu y activa el firewall, fail2ban y las actualizaciones automáticas.
2. Instala SQL Server 2025 Express, crea la base `JurigestEnterprise` y un usuario `jurigest_app` con clave aleatoria.
3. Genera `/etc/jurigest/api.env` y `/etc/jurigest/web.env` con claves nuevas (JWT de 96 caracteres, clave de SQL).
4. Configura Nginx y pide los certificados HTTPS. Si el DNS todavía no apunta al servidor, se salta este paso y muestra el comando para hacerlo después.
5. Registra los servicios `jurigest-api` y `jurigest-web` y el respaldo diario.

Se puede volver a ejecutar sin perder claves ni datos.

## Paso 3 · Publicar la aplicación (desde Windows)

En la raíz del repositorio, con la rama que quiera publicar:

```powershell
.\deploy\linux\publicar.ps1 -Servidor admin@IP_DEL_SERVIDOR
```

El script compila la API y la Web para Linux, genera el ejecutable de migraciones, sube un paquete (unos 170 MB) y en el servidor ejecuta `jurigest-activar`, que:

1. aplica las migraciones pendientes;
2. cambia a la nueva versión y reinicia los servicios;
3. comprueba que la API y la Web respondan. Si no responden, vuelve automáticamente a la versión anterior.

Pedirá la contraseña de `sudo` del servidor.

## Paso 4 · Crear el primer administrador

En el servidor:

```bash
sudo jurigest-crear-admin
```

Pide nombre, correo y contraseña (mínimo 8 caracteres, con mayúscula, número y símbolo). Solo funciona mientras la base no tenga usuarios.

## Paso 5 · Crear los usuarios de la marcha blanca

1. Entre a `https://jurigest.su-dominio.cl` con el administrador.
2. Vaya a **Usuarios → Crear usuario** y registre a cada persona con su RUT, teléfono, dirección, oficina, correo, rol y una contraseña temporal.
3. Entregue a cada persona su contraseña temporal. Al entrar por primera vez, el sistema le pedirá cambiarla.
4. Si necesita ajustar qué puede hacer cada rol, use **Administrar permisos por rol**.

La aplicación no limita la cantidad de usuarios ni cuántos trabajan al mismo tiempo. Para 4 personas simultáneas, el servidor recomendado tiene capacidad de sobra.

## Paso 6 · Verificación de marcha blanca

- [ ] `https://jurigest.su-dominio.cl` abre con candado (HTTPS) y `http://` redirige a `https://`.
- [ ] Los 4 usuarios entran y cambian su contraseña temporal.
- [ ] Se crea una causa con su primera diligencia, se registra el resultado y aparece el recibo.
- [ ] Se carga y descarga un documento PDF.
- [ ] `/sistema` muestra la API y la base de datos operativas, y el respaldo judicial se descarga.
- [ ] Al día siguiente existe un respaldo en `/var/backups/jurigest/sql`.

## Operación

### Publicar una nueva versión

Repita el paso 3. Los usuarios conectados **pierden la sesión** al reiniciar (las sesiones se guardan en memoria), así que publique fuera del horario de trabajo.

### Ver registros y estado

```bash
sudo systemctl status jurigest-api jurigest-web
sudo journalctl -u jurigest-api -f          # registros de la API en vivo
sudo journalctl -u jurigest-web --since "1 hour ago"
```

### Reiniciar

```bash
sudo systemctl restart jurigest-api jurigest-web
```

### Volver a una versión anterior

El servidor conserva las 3 últimas versiones:

```bash
ls -1t /opt/jurigest/releases
sudo ln -sfn /opt/jurigest/releases/<version-anterior> /opt/jurigest/actual
sudo systemctl restart jurigest-api jurigest-web
```

Las migraciones de base de datos no se revierten. Antes de volver a una versión anterior a un cambio de base, restaure el respaldo correspondiente.

### Respaldos

- Automáticos todos los días a las 02:30: `/var/backups/jurigest/sql` (base) y `/var/backups/jurigest/documentos`.
- Manual: `sudo jurigest-respaldo`
- Historial: `/var/log/jurigest-respaldo.log`

Copie los respaldos fuera del servidor periódicamente, por ejemplo a su equipo:

```powershell
scp -r admin@IP_DEL_SERVIDOR:/var/backups/jurigest .\respaldos-jurigest
```

(Requiere que el usuario `admin` tenga permiso de lectura; si no, cópielos antes a su carpeta personal con `sudo cp`.)

### Restaurar un respaldo

Pruebe primero la restauración en un ambiente aparte. Para restaurar en el servidor:

```bash
sudo systemctl stop jurigest-web jurigest-api
gunzip -k /var/backups/jurigest/sql/JurigestEnterprise_<fecha>.bak.gz
sudo /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$(sudo cat /root/.jurigest-sa)" -C -Q \
  "RESTORE DATABASE [JurigestEnterprise] FROM DISK = N'/var/backups/jurigest/sql/JurigestEnterprise_<fecha>.bak' WITH REPLACE"
sudo tar -xzf /var/backups/jurigest/documentos/documentos_<fecha>.tar.gz -C /var/lib/jurigest
sudo chown -R jurigest:jurigest /var/lib/jurigest/documentos
sudo systemctl start jurigest-api jurigest-web
```

### Correo para recuperar contraseñas (opcional)

Para que funcione "¿Olvidó su contraseña?", complete los datos SMTP en `/etc/jurigest/api.env` (por ejemplo, una cuenta de Gmail con contraseña de aplicación) y reinicie:

```bash
sudo nano /etc/jurigest/api.env      # Email__Smtp__Username, Email__Smtp__Password, Email__Smtp__FromAddress
sudo systemctl restart jurigest-api
```

## Dónde está cada cosa

| Ruta | Contenido |
|---|---|
| `/opt/jurigest/actual` | Versión en uso (enlace a `/opt/jurigest/releases/<version>`) |
| `/etc/jurigest/api.env`, `web.env` | Configuración y claves. Solo `root` y el servicio pueden leerlas. |
| `/var/lib/jurigest/documentos` | Documentos cargados |
| `/var/backups/jurigest` | Respaldos |
| `/root/.jurigest-sa` | Clave del usuario `sa` de SQL Server |
| `/etc/nginx/sites-available/jurigest` | Configuración de Nginx |

## Limitaciones conocidas para la marcha blanca

- **Límite de intentos de inicio de sesión compartido.** La API limita a 5 intentos por minuto por dirección IP. Como todos los inicios de sesión pasan por la Web, la API los ve con la misma IP: si varias personas escriben mal su contraseña en el mismo minuto, las demás deben esperar hasta un minuto. Por el mismo motivo, la auditoría registra la IP del servidor y no la del usuario. Corregirlo requiere un cambio en el código.
- **Sesiones en memoria.** Reiniciar la Web cierra la sesión de todos.
- **Respaldos en el mismo servidor.** Si se pierde el servidor, se pierden también; cópielos fuera con regularidad.
