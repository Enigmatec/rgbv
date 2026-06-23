#!/bin/bash
set -e

# Configuration from environment variables
SA_PASSWORD="${MSSQL_SA_PASSWORD}"
DB_NAME="${DB_NAME}"
BACPAC_PATH="${BACPAC_PATH}"
MARKER_FILE="/var/opt/mssql/data/.import_completed"

echo "===================================================="
echo "Database Initializer Starting..."
echo "Target DB: ${DB_NAME}"
echo "Using Bacpac: ${BACPAC_PATH}"
echo "===================================================="

# Wait for SQL Server port to be open
echo "Waiting for SQL Server (db:1433) to accept TCP connections..."
until bash -c 'exec 3<>/dev/tcp/db/1433' 2>/dev/null; do
  echo "SQL Server is not ready yet. Retrying in 3 seconds..."
  sleep 3
done
echo "SQL Server port 1433 is open!"

# Wait a little longer to ensure SQL Server engine is actually ready
echo "Waiting 5 additional seconds to ensure service is initialized..."
sleep 5

# Check if import marker file exists
if [ -f "$MARKER_FILE" ]; then
  echo "Import marker file found at $MARKER_FILE."
  echo "Database is already restored. Skipping import."
  exit 0
fi

echo "No import marker file found. Starting restoration..."

# Install sqlpackage tool
echo "Installing microsoft.sqlpackage tool..."
dotnet tool install -g microsoft.sqlpackage || echo "sqlpackage already installed"
export PATH="$PATH:/root/.dotnet/tools"

# Run the import
echo "Importing bacpac file into SQL Server..."
sqlpackage /Action:Import \
  /SourceFile:"${BACPAC_PATH}" \
  /TargetServerName:"db" \
  /TargetDatabaseName:"${DB_NAME}" \
  /TargetUser:"sa" \
  /TargetPassword:"${SA_PASSWORD}" \
  /TargetTrustServerCertificate:True

echo "Import completed successfully!"

# Create marker file to skip next run
echo "Creating marker file at $MARKER_FILE..."
touch "$MARKER_FILE"

echo "===================================================="
echo "Database Initializer Completed Successfully!"
echo "===================================================="
