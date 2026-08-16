#!/usr/bin/env bash
set -euo pipefail

for attempt in {1..30}; do
  if /opt/mssql-tools18/bin/sqlcmd -b -S db -d master -U sa -P "$DB_SA_PASSWORD" -C \
    -v DB_NAME="$DB_NAME" DB_APP_USER="$DB_APP_USER" DB_APP_PASSWORD="$DB_APP_PASSWORD" \
    -i /scripts/init-db.sql; then
    exit 0
  fi

  echo "SQL Server is still starting; retrying database initialization ($attempt/30)..."
  sleep 5
done

echo "Database initialization did not complete after 150 seconds." >&2
exit 1
