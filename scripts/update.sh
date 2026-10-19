#!/usr/bin/env bash
# Updates the Studio Manager on the server to the newest version of the main branch:
# show what is new -> backup -> get the new version -> rebuild and start -> check that the app answers.
#
#   ./scripts/update.sh        asks before updating
#   ./scripts/update.sh -y     no question
set -euo pipefail
source "$(dirname "$0")/common.sh"

echo "== 1/5 Looking for a new version"
git fetch --quiet --tags origin main
previous=$(git rev-parse HEAD)

if [ "$previous" = "$(git rev-parse origin/main)" ]; then
  echo "Already up to date ($(version))."
  exit 0
fi

echo "Installed: $(version)"
echo "New since then:"
git log --oneline "$previous..origin/main"
echo

if [ "${1:-}" != "-y" ]; then
  read -r -p "Update now? [y/N] " answer
  [ "$answer" = "y" ] || { echo "Nothing changed."; exit 0; }
fi

echo "== 2/5 Backup"
bash scripts/backup.sh before-update

echo "== 3/5 Getting the new version"
# --ff-only: only moves forward to origin/main, never merges - the server has no own changes
git merge --ff-only origin/main

echo "== 4/5 Building and starting (database changes are applied when the API starts)"
docker compose up -d --build

echo "== 5/5 Checking that the app answers"
if ! wait_for_app; then
  echo
  echo "!! The app does not answer. Last lines of the API log:"
  docker compose logs --tail 30 api
  echo
  echo "!! To go back to the version before the update:"
  echo "     git reset --hard $previous"
  echo "     docker compose up -d --build"
  echo "   If the database was changed by the update, also restore the backup from step 2:"
  echo "     ./scripts/restore.sh <the two *-before-update files in ${BACKUP_DIR:-$HOME/studio-backups}>"
  exit 1
fi

# Old images of previous builds are not needed anymore - frees disk space
docker image prune -f > /dev/null

echo
echo "Updated to $(version) - the app is running."
