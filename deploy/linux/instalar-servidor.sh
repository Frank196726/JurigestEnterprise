#!/usr/bin/env bash
# Prepara un servidor Ubuntu 24.04 limpio para Jurigest Enterprise.
#
# Uso (como root, desde la carpeta deploy/linux copiada al servidor):
#   sudo ./instalar-servidor.sh jurigest.midominio.cl api.midominio.cl correo@midominio.cl
#
# Instala: SQL Server 2025 Express, Nginx, Certbot (HTTPS), firewall, fail2ban,
# actualizaciones de seguridad automáticas, servicios systemd y respaldo diario.
# Se puede volver a ejecutar: no regenera claves ni borra datos existentes.
set -euo pipefail

DOMINIO_WEB="${1:-}"
DOMINIO_API="${2:-}"
CORREO="${3:-}"
DIR_SCRIPTS="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

paso() { printf '\n\033[1;34m==> %s\033[0m\n' "$1"; }
aviso() { printf '\033[1;33m[aviso] %s\033[0m\n' "$1"; }
fallar() { printf '\033[1;31m[error] %s\033[0m\n' "$1" >&2; exit 1; }

# Claves alfanuméricas: no rompen la cadena de conexión ni los reemplazos con sed.
generar_clave() { tr -dc 'A-Za-z0-9' < /dev/urandom | head -c "$1"; }

[[ $EUID -eq 0 ]] || fallar "Ejecute el script con sudo."
[[ -n "$DOMINIO_WEB" && -n "$DOMINIO_API" && -n "$CORREO" ]] ||
    fallar "Uso: sudo $0 <dominio-web> <dominio-api> <correo-para-certificados>"
grep -q 'VERSION_ID="24.04"' /etc/os-release || fallar "Este script está preparado para Ubuntu 24.04."
for archivo in jurigest-api.service jurigest-web.service nginx-jurigest.conf api.env.plantilla \
    web.env.plantilla activar-version.sh crear-administrador.sh respaldo.sh; do
    [[ -f "$DIR_SCRIPTS/$archivo" ]] || fallar "Falta $archivo junto al script."
done

export DEBIAN_FRONTEND=noninteractive

paso "Actualizando el sistema e instalando paquetes base"
apt-get update -q
apt-get upgrade -yq
apt-get install -yq curl gnupg ca-certificates ufw fail2ban unattended-upgrades \
    nginx certbot python3-certbot-nginx dnsutils libicu74 tar gzip python3

paso "Zona horaria America/Santiago"
timedatectl set-timezone America/Santiago

paso "Firewall: solo SSH, HTTP y HTTPS"
ufw allow OpenSSH >/dev/null
ufw allow 'Nginx Full' >/dev/null
ufw --force enable >/dev/null
systemctl enable --now fail2ban >/dev/null
dpkg-reconfigure -f noninteractive unattended-upgrades

paso "Usuario de servicio y directorios"
if ! id jurigest &>/dev/null; then
    useradd --system --home-dir /var/lib/jurigest --shell /usr/sbin/nologin jurigest
fi
install -d -o root -g root -m 755 /opt/jurigest /opt/jurigest/releases
install -d -o jurigest -g jurigest -m 750 /var/lib/jurigest /var/lib/jurigest/documentos
install -d -o root -g jurigest -m 750 /etc/jurigest
install -d -o root -g root -m 700 /var/backups/jurigest /var/backups/jurigest/documentos

paso "SQL Server 2025 Express"
ARCHIVO_SA=/root/.jurigest-sa
if [[ ! -f "$ARCHIVO_SA" ]]; then
    ( umask 077; printf '%s' "$(generar_clave 28)Sa9" > "$ARCHIVO_SA" )
fi
CLAVE_SA="$(cat "$ARCHIVO_SA")"

if [[ ! -x /opt/mssql/bin/sqlservr ]]; then
    curl -fsSL https://packages.microsoft.com/keys/microsoft.asc |
        gpg --batch --yes --dearmor -o /usr/share/keyrings/microsoft-prod.gpg
    curl -fsSL https://packages.microsoft.com/config/ubuntu/24.04/mssql-server-2025.list \
        -o /etc/apt/sources.list.d/mssql-server-2025.list
    curl -fsSL https://packages.microsoft.com/config/ubuntu/24.04/prod.list \
        -o /etc/apt/sources.list.d/mssql-release.list
    apt-get update -q
    apt-get install -yq mssql-server
    MSSQL_PID=Express MSSQL_SA_PASSWORD="$CLAVE_SA" /opt/mssql/bin/mssql-conf -n setup accept-eula
fi
ACCEPT_EULA=Y apt-get install -yq mssql-tools18 unixodbc-dev
# Solo escucha en la máquina: nunca se expone a Internet.
/opt/mssql/bin/mssql-conf set network.ipaddress 127.0.0.1 >/dev/null
/opt/mssql/bin/mssql-conf set memory.memorylimitmb 3072 >/dev/null
systemctl restart mssql-server
systemctl enable mssql-server >/dev/null

SQLCMD=/opt/mssql-tools18/bin/sqlcmd
for _ in $(seq 1 30); do
    "$SQLCMD" -S localhost -U sa -P "$CLAVE_SA" -C -Q "SELECT 1" &>/dev/null && break
    sleep 2
done

paso "Base de datos y usuario de la aplicación"
ARCHIVO_API=/etc/jurigest/api.env
if [[ -f "$ARCHIVO_API" ]]; then
    aviso "$ARCHIVO_API ya existe: se conservan sus claves."
    CLAVE_SQL="$(grep '^ConnectionStrings__DefaultConnection=' "$ARCHIVO_API" | sed -E 's/.*Password=([^;]*).*/\1/')"
else
    CLAVE_SQL="$(generar_clave 28)Ap7"
fi

"$SQLCMD" -S localhost -U sa -P "$CLAVE_SA" -C -b -Q "
IF DB_ID('JurigestEnterprise') IS NULL
    CREATE DATABASE JurigestEnterprise COLLATE Modern_Spanish_CI_AS;
IF SUSER_ID('jurigest_app') IS NULL
    CREATE LOGIN jurigest_app WITH PASSWORD = '$CLAVE_SQL', CHECK_POLICY = ON, DEFAULT_DATABASE = JurigestEnterprise;
ELSE
    ALTER LOGIN jurigest_app WITH PASSWORD = '$CLAVE_SQL';
"
"$SQLCMD" -S localhost -U sa -P "$CLAVE_SA" -C -b -d JurigestEnterprise -Q "
IF USER_ID('jurigest_app') IS NULL CREATE USER jurigest_app FOR LOGIN jurigest_app;
ALTER ROLE db_owner ADD MEMBER jurigest_app;
"

paso "Variables de entorno de la API y la Web"
if [[ ! -f "$ARCHIVO_API" ]]; then
    sed -e "s|__DOMINIO_WEB__|$DOMINIO_WEB|g" -e "s|__DOMINIO_API__|$DOMINIO_API|g" \
        -e "s|__CLAVE_SQL__|$CLAVE_SQL|g" -e "s|__CLAVE_JWT__|$(generar_clave 96)|g" \
        "$DIR_SCRIPTS/api.env.plantilla" > "$ARCHIVO_API"
fi
if [[ ! -f /etc/jurigest/web.env ]]; then
    sed -e "s|__DOMINIO_WEB__|$DOMINIO_WEB|g" -e "s|__DOMINIO_API__|$DOMINIO_API|g" \
        "$DIR_SCRIPTS/web.env.plantilla" > /etc/jurigest/web.env
fi
chown root:jurigest /etc/jurigest/*.env
chmod 640 /etc/jurigest/*.env

# La Web llama a la API por HTTPS; así esa llamada no sale a Internet.
grep -qE "^127\.0\.0\.1\s+$DOMINIO_API\b" /etc/hosts || echo "127.0.0.1 $DOMINIO_API" >> /etc/hosts

paso "Nginx"
sed -e "s|__DOMINIO_WEB__|$DOMINIO_WEB|g" -e "s|__DOMINIO_API__|$DOMINIO_API|g" \
    "$DIR_SCRIPTS/nginx-jurigest.conf" > /etc/nginx/sites-available/jurigest
ln -sfn /etc/nginx/sites-available/jurigest /etc/nginx/sites-enabled/jurigest
rm -f /etc/nginx/sites-enabled/default
sed -i 's/^\s*#\?\s*server_tokens .*/\tserver_tokens off;/' /etc/nginx/nginx.conf
nginx -t
systemctl reload nginx

paso "Certificados HTTPS (Let's Encrypt)"
IP_PUBLICA="$(curl -fsS https://api.ipify.org || true)"
DNS_OK=true
for dominio in "$DOMINIO_WEB" "$DOMINIO_API"; do
    if ! dig +short A "$dominio" | grep -qx "$IP_PUBLICA"; then
        aviso "$dominio todavía no apunta a $IP_PUBLICA."
        DNS_OK=false
    fi
done
if $DNS_OK; then
    certbot --nginx --non-interactive --agree-tos --redirect -m "$CORREO" \
        -d "$DOMINIO_WEB" -d "$DOMINIO_API"
else
    aviso "Se omitió HTTPS. Cuando el DNS apunte a este servidor, ejecute:"
    echo "  sudo certbot --nginx --agree-tos --redirect -m $CORREO -d $DOMINIO_WEB -d $DOMINIO_API"
fi

paso "Servicios, comandos de operación y respaldo diario"
install -m 644 "$DIR_SCRIPTS/jurigest-api.service" /etc/systemd/system/jurigest-api.service
install -m 644 "$DIR_SCRIPTS/jurigest-web.service" /etc/systemd/system/jurigest-web.service
install -m 750 "$DIR_SCRIPTS/activar-version.sh" /usr/local/sbin/jurigest-activar
install -m 750 "$DIR_SCRIPTS/crear-administrador.sh" /usr/local/sbin/jurigest-crear-admin
install -m 750 "$DIR_SCRIPTS/respaldo.sh" /usr/local/sbin/jurigest-respaldo
install -d -o mssql -g root -m 750 /var/backups/jurigest/sql
systemctl daemon-reload
systemctl enable jurigest-api jurigest-web >/dev/null
echo "30 2 * * * root /usr/local/sbin/jurigest-respaldo >> /var/log/jurigest-respaldo.log 2>&1" \
    > /etc/cron.d/jurigest-respaldo

paso "Listo"
cat <<EOF
Servidor preparado.
  - Clave de 'sa' de SQL Server: $ARCHIVO_SA (solo root).
  - Configuración: /etc/jurigest/api.env y /etc/jurigest/web.env
  - Documentos: /var/lib/jurigest/documentos
  - Respaldos diarios (02:30): /var/backups/jurigest

Siguiente paso: desde su equipo Windows ejecute
  .\\deploy\\linux\\publicar.ps1 -Servidor usuario@$IP_PUBLICA
y luego, en el servidor:
  sudo jurigest-crear-admin
EOF
