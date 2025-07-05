#!/bin/bash
set -e

# Start SQL Server
/opt/mssql/bin/sqlservr & 

# Wait for SQL Server to be ready
sleep 30s

# Run the initialization script
/db-init.sh

# Wait for SQL Server to exit
wait $!
