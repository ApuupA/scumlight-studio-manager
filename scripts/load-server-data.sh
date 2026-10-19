#!/usr/bin/env bash
# For the PC: loads a backup of the server into the TEST database "studio_test" and the folder data/test-files.
# Use it to try a new version with database changes on real data before updating the server.
# Your local database "studio" is NOT touched.
#
#   1. Copy the newest backup from the server (in Git Bash on the PC):
#        scp apu@studio.scumlight-cloud.de:'~/studio-backups/studio-*2026-10-19-0300*' data/
#   2. Load it:
#        bash scripts/load-server-data.sh data/studio-2026-10-19-0300.sql data/studio-files-2026-10-19-0300.tgz
#   3. Start the API against the test data (see the output) and test.
set -euo pipefail

database_file=$(realpath "${1:?Usage: bash scripts/load-server-data.sh <database .sql> [files .tgz]}")
files_file=${2:+$(realpath "$2")}
source "$(dirname "$0")/common.sh"

[ -f "$database_file" ] || { echo "Not found: $database_file" >&2; exit 1; }
[ -z "$files_file" ] || [ -f "$files_file" ] || { echo "Not found: $files_file" >&2; exit 1; }

# Only the test database - never "studio"
test_database="studio_test"
test_files="data/test-files"

echo "Loading the backup into the test database '$test_database' ..."
docker compose up -d db > /dev/null
docker compose exec -T db psql -U studio -d postgres -v ON_ERROR_STOP=1 -q \
  -c "DROP DATABASE IF EXISTS $test_database WITH (FORCE)" \
  -c "CREATE DATABASE $test_database OWNER studio"
docker compose exec -T db psql -U studio -d "$test_database" -v ON_ERROR_STOP=1 -q < "$database_file" > /dev/null

if [ -n "$files_file" ]; then
  echo "Unpacking the files into $test_files ..."
  rm -rf "$test_files"
  mkdir -p "$test_files"
  tar xzf "$files_file" -C "$test_files"
fi

echo
echo "Done. Start the API against the test data (it applies new database changes to the copy):"
echo "  dotnet run --project src/StudioManager.API --launch-profile http -- \\"
echo "    \"--ConnectionStrings:StudioDatabase=Host=localhost;Port=5433;Database=$test_database;Username=studio;Password=studio\" \\"
echo "    --FileStorage:RootPath=../../$test_files"
echo "and the frontend with: npm run dev --prefix web  -> http://localhost:5173 (log in with your server login)"
