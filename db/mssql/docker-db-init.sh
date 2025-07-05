#!/bin/bash
set -e

# Wait for SQL Server to be ready
sleep 30s

# Run the setup script
/opt/mssql-tools18/bin/sqlcmd -b -S localhost -U sa -P SomeStrongPwd123 -Q "
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'limitkurs')
BEGIN
    CREATE DATABASE limitkurs;
END
GO
USE limitkurs;
GO"

# Make the script executable
chmod +x /db-init.sh
