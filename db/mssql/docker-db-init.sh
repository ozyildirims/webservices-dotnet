#!/bin/bash
set -e

# Wait for SQL Server to be ready
sleep 30s

# Run the setup script with SSL disabled using connection string
/opt/mssql-tools18/bin/sqlcmd -b -S "localhost" -U sa -P SomeStrongPwd123 -d master -Q "SET NOCOUNT ON; IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'limitkurs') BEGIN CREATE DATABASE limitkurs; END" -C

# Make the script executable
chmod +x /db-init.sh
