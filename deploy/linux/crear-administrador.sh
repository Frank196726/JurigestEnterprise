#!/usr/bin/env bash
# Crea el primer administrador. Se instala como: sudo jurigest-crear-admin
#
# La API solo permite esta operación en modo Development, así que se levanta
# una copia temporal escuchando únicamente en 127.0.0.1:5099 (no accesible
# desde Internet), se llama a /api/seguridad/bootstrap y se detiene.
# Solo funciona mientras la base no tenga usuarios.
set -euo pipefail

fallar() { printf '\033[1;31m[error] %s\033[0m\n' "$1" >&2; exit 1; }
leer_env() { grep "^$2=" "$1" | head -n1 | cut -d= -f2-; }

[[ $EUID -eq 0 ]] || fallar "Ejecute con sudo."
[[ -x /opt/jurigest/actual/api/Jurigest.API ]] || fallar "Primero publique una versión con publicar.ps1."

read -rp "Nombre del administrador: " NOMBRE
read -rp "Correo: " CORREO
read -rsp "Contraseña (mín. 8, con mayúscula, número y símbolo): " CLAVE; echo
read -rsp "Repita la contraseña: " CLAVE2; echo
[[ "$CLAVE" == "$CLAVE2" ]] || fallar "Las contraseñas no coinciden."

DOMINIO_API="$(leer_env /etc/jurigest/api.env AllowedHosts)"
ENV_TEMPORAL=/run/jurigest-bootstrap.env
UNIDAD=jurigest-bootstrap

limpiar() {
    systemctl stop "$UNIDAD" &>/dev/null || true
    systemctl reset-failed "$UNIDAD" &>/dev/null || true
    rm -f "$ENV_TEMPORAL"
}
trap limpiar EXIT

( umask 077
  sed -e 's|^ASPNETCORE_ENVIRONMENT=.*|ASPNETCORE_ENVIRONMENT=Development|' \
      -e 's|^ASPNETCORE_URLS=.*|ASPNETCORE_URLS=http://127.0.0.1:5099|' \
      /etc/jurigest/api.env > "$ENV_TEMPORAL" )

systemd-run --quiet --unit="$UNIDAD" \
    --property=User=jurigest \
    --property=EnvironmentFile="$ENV_TEMPORAL" \
    --property=WorkingDirectory=/opt/jurigest/actual/api \
    /opt/jurigest/actual/api/Jurigest.API

for _ in $(seq 1 30); do
    curl -fsS -o /dev/null -H "Host: $DOMINIO_API" http://127.0.0.1:5099/health/live && break
    sleep 2
done

CUERPO="$(NOMBRE="$NOMBRE" CORREO="$CORREO" CLAVE="$CLAVE" python3 -c \
    'import json, os; print(json.dumps({"nombre": os.environ["NOMBRE"], "email": os.environ["CORREO"], "password": os.environ["CLAVE"]}))')"

RESPUESTA="$(curl -sS -w '\n%{http_code}' -H "Host: $DOMINIO_API" -H 'Content-Type: application/json' \
    -d "$CUERPO" http://127.0.0.1:5099/api/seguridad/bootstrap)"
CODIGO="$(tail -n1 <<< "$RESPUESTA")"

case "$CODIGO" in
    201) echo "Administrador creado. Ya puede iniciar sesión con $CORREO." ;;
    409) fallar "Ya existe un administrador; cree los demás usuarios desde la pantalla Usuarios." ;;
    *)   echo "$RESPUESTA" | head -n -1; fallar "La API respondió $CODIGO." ;;
esac
