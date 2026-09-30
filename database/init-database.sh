#!/usr/bin/env bash
set -euo pipefail

sqlcmd=/opt/mssql-tools/bin/sqlcmd
attempt=0

until "$sqlcmd" -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -b -Q "SELECT 1" >/dev/null 2>&1; do
    attempt=$((attempt + 1))
    if [ "$attempt" -ge 60 ]; then
        echo "SQL Server did not become ready after 120 seconds." >&2
        exit 1
    fi
    sleep 2
done

for script in /database/[0-9][0-9]_*.sql; do
    echo "Applying $(basename "$script")"
    "$sqlcmd" -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -b -i "$script"
done