#!/usr/bin/env bash
# Backs up the database and the uploaded files (photos, receipts, finance archives) of the Studio Manager.
#
#   ./scripts/backup.sh                -> ~/studio-backups/studio-2026-10-19-0300.sql
#                                         ~/studio-backups/studio-files-2026-10-19-0300.tgz
#   ./scripts/backup.sh before-update  -> same, with a label: studio-2026-10-19-0300-before-update.sql
#
# Backups older than KEEP_DAYS days (default 14) are deleted. Folder: BACKUP_DIR (default ~/studio-backups).
#
# Off the server (optional): with OFFSITE_REMOTE in .env (an rclone remote, e.g. "studio-crypt:") both files are
# also copied there; backups older than OFFSITE_KEEP_DAYS (default 90) are deleted there.
# See docs/DEPLOYMENT.md, "Backups off the server".
#
# After every backup the app gets a status file (shown on the profile page, warning on the start page if too old).
#
# Nightly at 3:00 (crontab -e):
#   0 3 * * * ~/studio-manager/scripts/backup.sh >> ~/studio-backups/backup.log 2>&1
set -euo pipefail
source "$(dirname "$0")/common.sh"

backup_dir="${BACKUP_DIR:-$HOME/studio-backups}"
keep_days="${KEEP_DAYS:-14}"
offsite_remote="$(env_value OFFSITE_REMOTE)"
offsite_keep_days="$(env_value OFFSITE_KEEP_DAYS)"
offsite_keep_days="${offsite_keep_days:-90}"
name="$(date +%F-%H%M)${1:+-$1}"
database_file="$backup_dir/studio-$name.sql"
files_file="$backup_dir/studio-files-$name.tgz"

mkdir -p "$backup_dir"

echo "$(date '+%F %T') Backup $name"

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

# ---------- Copy off the server ----------
# The local backup is complete at this point - a problem here does not delete it, but the script ends with
# exit code 2 (visible in backup.log; update.sh only warns) and the app shows "off-site failed".
offsite_ok="null"
if [ -n "$offsite_remote" ]; then
  if ! command -v rclone > /dev/null; then
    echo "Off-site copy FAILED: rclone is not installed (sudo apt install rclone)." >&2
    offsite_ok="false"
  elif rclone copy "$database_file" "$offsite_remote" && rclone copy "$files_file" "$offsite_remote"; then
    echo "Copied off the server to $offsite_remote"
    offsite_ok="true"
    # Old backups there go away as well (only our files)
    rclone delete "$offsite_remote" --min-age "${offsite_keep_days}d" --include 'studio-*' || echo "Could not delete old backups off the server." >&2
  else
    echo "Off-site copy to $offsite_remote FAILED." >&2
    offsite_ok="false"
  fi
fi

# ---------- Tell the app ----------
# backup-status.json in the file storage of the app (read by FinanceArchiveService.GetStatus)
database_bytes="$(wc -c < "$database_file" | tr -d '[:space:]')"
offsite_json="null"
if [ -n "$offsite_remote" ]; then
  offsite_json="\"$offsite_remote\""
fi
status_json="{\"finishedAt\":\"$(date -Iseconds)\",\"databaseBytes\":$database_bytes,\"offsite\":$offsite_json,\"offsiteOk\":$offsite_ok}"
echo "$status_json" | docker compose exec -T api sh -c 'cat > /data/files/backup-status.json' \
  || echo "Could not write the backup status for the app (the backup itself is fine)." >&2

# Exit code 2 = the backup on the server is fine, only the copy off the server failed
if [ "$offsite_ok" = "false" ]; then
  exit 2
fi
