#!/usr/bin/env bash
# Respaldo diario de la base y los documentos. Se instala como /usr/local/sbin/jurigest-respaldo
# y cron lo ejecuta a las 02:30. Conserva 14 días.
set -euo pipefail

DIAS=14
FECHA="$(date +%Y%m%d-%H%M%S)"
DIR=/var/backups/jurigest
ARCHIVO_SQL="$DIR/sql/JurigestEnterprise_$FECHA.bak"

/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$(cat /root/.jurigest-sa)" -C -b -Q \
    "BACKUP DATABASE [JurigestEnterprise] TO DISK = N'$ARCHIVO_SQL' WITH CHECKSUM, INIT;"
gzip "$ARCHIVO_SQL"

tar -czf "$DIR/documentos/documentos_$FECHA.tar.gz" -C /var/lib/jurigest documentos

find "$DIR/sql" -name '*.bak.gz' -mtime +$DIAS -delete
find "$DIR/documentos" -name '*.tar.gz' -mtime +$DIAS -delete

echo "$(date '+%F %T') respaldo correcto: $FECHA"
