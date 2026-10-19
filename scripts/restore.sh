#!/usr/bin/env bash
# Restores a backup made by backup.sh. REPLACES ALL CURRENT DATA of this installation with the backup.
#
#   ./scripts/restore.sh ~/studio-backups/studio-2026-10-19-0300.sql
#   ./scripts/restore.sh ~/studio-backups/studio-2026-10-19-0300.sql ~/studio-backups/studio-files-2026-10-19-0300.tgz
#
# Without the second file the uploaded files (photos) stay as they are.
set -euo pipefail

# Paths of the backup files as given (before common.sh changes into the repository folder)
database_file=$(realpath "${1:?Usage: ./scripts/restore.sh <database .sql> [files .tgz]}")
files_file=${2:+$(realpath "$2")}
source "$(dirname "$0")/common.sh"

[ -f "$database_file" ] || { echo "Not found: $database_file" >&2; exit 1; }
[ -z "$files_file" ] || [ -f "$files_file" ] || { echo "Not found: $files_file" >&2; exit 1; }

echo "This REPLACES all data of the Studio Manager in $(pwd) with:"
echo "  database: $database_file"
echo "  files:    ${files_file:-(not restored, they stay as they are)}"
read -r -p "Type RESTORE to continue: " answer
[ "$answer" = "RESTORE" ] || { echo "Nothing changed."; exit 0; }

# Nobody may write while the data is replaced
echo "Stopping the app ..."
docker compose stop api

echo "Restoring the database ..."
docker compose exec -T db psql -U studio -d postgres -v ON_ERROR_STOP=1 -q \
  -c 'DROP DATABASE IF EXISTS studio WITH (FORCE)' \
  -c 'CREATE DATABASE studio OWNER studio'
docker compose exec -T db psql -U studio -d studio -v ON_ERROR_STOP=1 -q < "$database_file" > /dev/null

if [ -n "$files_file" ]; then
  echo "Restoring the files ..."
  # A temporary container with the same files volume as the API: empty the folder, unpack the backup
  docker compose run --rm --no-deps -T --entrypoint sh api \
    -c 'find /data/files -mindepth 1 -delete && tar xzf - -C /data/files' < "$files_file"
fi

echo "Starting the app ..."
docker compose start api

if wait_for_app; then
  echo "Restore done - the app is running."
else
  echo "!! The app does not answer - see: docker compose logs --tail 30 api" >&2
  exit 1
fi
