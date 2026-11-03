#!/usr/bin/env bash
# Instala una versión de Jurigest generada por publicar.ps1.
# Se instala en el servidor como: sudo jurigest-activar /tmp/jurigest-<version>.tar.gz
#
# Pasos: descomprime, aplica migraciones, cambia a la nueva versión, reinicia
# y comprueba salud. Si la comprobación falla, vuelve a la versión anterior
# (las migraciones ya aplicadas no se revierten).
set -euo pipefail

PAQUETE="${1:-}"
RAIZ=/opt/jurigest
ACTUAL="$RAIZ/actual"
CONSERVAR=3

paso() { printf '\n\033[1;34m==> %s\033[0m\n' "$1"; }
fallar() { printf '\033[1;31m[error] %s\033[0m\n' "$1" >&2; exit 1; }
leer_env() { grep "^$2=" "$1" | head -n1 | cut -d= -f2-; }

[[ $EUID -eq 0 ]] || fallar "Ejecute con sudo."
[[ -f "$PAQUETE" ]] || fallar "Uso: sudo jurigest-activar /ruta/jurigest-<version>.tar.gz"

VERSION="$(basename "$PAQUETE" .tar.gz)"
DESTINO="$RAIZ/releases/$VERSION"
DOMINIO_API="$(leer_env /etc/jurigest/api.env AllowedHosts)"
DOMINIO_WEB="$(leer_env /etc/jurigest/web.env AllowedHosts)"
ANTERIOR="$(readlink -f "$ACTUAL" 2>/dev/null || true)"

paso "Descomprimiendo $VERSION"
rm -rf "$DESTINO"
mkdir -p "$DESTINO"
tar -xzf "$PAQUETE" -C "$DESTINO"
[[ -f "$DESTINO/api/Jurigest.API" && -f "$DESTINO/web/Jurigest.Web" && -f "$DESTINO/efbundle" ]] ||
    fallar "El paquete no contiene api/, web/ y efbundle."
# Los archivos empaquetados en Windows llegan sin permiso de ejecución.
chmod +x "$DESTINO/api/Jurigest.API" "$DESTINO/web/Jurigest.Web" "$DESTINO/efbundle"
chown -R root:jurigest "$DESTINO"
chmod -R o-rwx "$DESTINO"

paso "Aplicando migraciones de base de datos"
CONEXION="$(leer_env /etc/jurigest/api.env ConnectionStrings__DefaultConnection)"
"$DESTINO/efbundle" --connection "$CONEXION"

comprobar() {
    local puerto="$1" ruta="$2" dominio="$3"
    for _ in $(seq 1 30); do
        if curl -fsS -o /dev/null -H "Host: $dominio" -H "X-Forwarded-Proto: https" \
            "http://127.0.0.1:$puerto$ruta"; then
            return 0
        fi
        sleep 2
    done
    return 1
}

activar() {
    ln -sfn "$1" "$ACTUAL.nuevo"
    mv -T "$ACTUAL.nuevo" "$ACTUAL"
    systemctl restart jurigest-api
    comprobar 5000 /health/ready "$DOMINIO_API" || return 1
    systemctl restart jurigest-web
    comprobar 5001 /login "$DOMINIO_WEB" || return 1
}

paso "Activando $VERSION"
if ! activar "$DESTINO"; then
    journalctl -u jurigest-api -u jurigest-web -n 40 --no-pager || true
    if [[ -n "$ANTERIOR" && -d "$ANTERIOR" ]]; then
        printf '\033[1;31mLa nueva versión no respondió. Volviendo a %s\033[0m\n' "$(basename "$ANTERIOR")"
        activar "$ANTERIOR" || true
    fi
    fallar "No se pudo activar $VERSION."
fi

paso "Limpiando versiones antiguas (se conservan $CONSERVAR)"
ls -1dt "$RAIZ"/releases/*/ | tail -n +$((CONSERVAR + 1)) | while read -r viejo; do
    [[ "$(readlink -f "$viejo")" == "$(readlink -f "$ACTUAL")" ]] || rm -rf "$viejo"
done
rm -f "$PAQUETE"

echo
echo "Versión $VERSION activa: https://$DOMINIO_WEB"
