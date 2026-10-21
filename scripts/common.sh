#!/usr/bin/env bash
# Shared helpers for the scripts in this folder. Not run directly - the other scripts load it with "source".

# Git Bash on Windows would turn container paths like /data/files into Windows paths - switch that off
# (no effect on Linux)
export MSYS_NO_PATHCONV=1

# Every script works in the repository folder (where docker-compose.yml is), wherever it is started from
cd "$(dirname "${BASH_SOURCE[0]}")/.."

# A setting from the environment or from .env, e.g. env_value OFFSITE_REMOTE (empty if not set).
# Trailing spaces and Windows line ends are removed.
env_value() {
  local name="$1"
  local value="${!name:-}"
  if [ -z "$value" ] && [ -f .env ]; then
    value="$(grep -E "^$name=" .env | tail -n 1 | cut -d= -f2- | sed 's/[[:space:]]*$//')"
  fi
  echo "$value"
}

# Port of the app on this machine: WEB_PORT from the environment or from .env ("127.0.0.1:8081" -> 8081), default 8080
web_port() {
  local value="${WEB_PORT:-$(grep -E '^WEB_PORT=' .env 2>/dev/null | cut -d= -f2 | tr -d '[:space:]')}"
  value="${value:-8080}"
  echo "${value##*:}"
}

# Version that is checked out, e.g. "v1.0.0" or "v1.0.0-3-g1a2b3c4" (3 commits after v1.0.0)
version() {
  git describe --tags --always
}

# Waits until the app answers (max. 2 minutes). The API only starts listening after the database migrations
# ran, so an answer means: containers up, database up to date.
# /api/auth/me without login answers 401 - that is the expected "I am alive" answer.
wait_for_app() {
  local url="http://127.0.0.1:$(web_port)/api/auth/me"
  for _ in $(seq 1 60); do
    if [ "$(curl -s -o /dev/null -w '%{http_code}' "$url")" = "401" ]; then
      return 0
    fi
    sleep 2
  done
  return 1
}
