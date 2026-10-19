#!/usr/bin/env bash
# Backs up the database and the uploaded files (photos) of the Studio Manager.
#
#   ./scripts/backup.sh                -> ~/studio-backups/studio-2026-10-19-0300.sql
#                                         ~/studio-backups/studio-files-2026-10-19-0300.tgz
#   ./scripts/backup.sh before-update  -> same, with a label: studio-2026-10-19-0300-before-update.sql
#
# Backups older than KEEP_DAYS days (default 14) are deleted. Folder: BACKUP_DIR (default ~/studio-backups).
# Nightly at 3:00 (crontab -e):
#   0 3 * * * ~/studio-manager/scripts/backup.sh >> ~/studio-backups/backup.log 2>&1
set -euo pipefail
source "$(dirname "$0")/common.sh"

backup_dir="${BACKUP_DIR:-$HOME/studio-backups}"
keep_days="${KEEP_DAYS:-14}"
name="$(date +%F-%H%M)${1:+-$1}"
database_file="$backup_dir/studio-$name.sql"
files_file="$backup_dir/studio-files-$name.tgz"

mkdir -p "$backup_dir"

# If anything fails, do not leave a half written backup behind that looks complete
trap 'rm -f "$database_file" "$files_file"; echo "Backup FAILED - nothing was saved." >&2' ERR

echo "Backing up the database ..."
docker compose exec -T db pg_dump -U studio -d studio > "$database_file"

echo "Backing up the uploaded files ..."
docker compose exec -T api tar czf - -C /data/files . > "$files_file"

# A dump without tables would be useless - treat it as an error
grep -q 'CREATE TABLE' "$database_file" || false

trap - ERR

# Old backups go away, so the disk does not fill up
find "$backup_dir" -maxdepth 1 -name 'studio-*' -mtime +"$keep_days" -delete

echo "Backup done:"
ls -lh "$database_file" "$files_file"
