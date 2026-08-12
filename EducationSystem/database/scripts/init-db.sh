#!/bin/bash
set -e

MSSQL_HOST="${MSSQL_HOST:-mssql}"
MSSQL_PORT="${MSSQL_PORT:-1433}"
MSSQL_SA_PASSWORD="${MSSQL_SA_PASSWORD:-Your_password123}"
DB_NAME="${DB_NAME:-TayDoV2}"

SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
if [ ! -f "$SQLCMD" ]; then
    SQLCMD="/opt/mssql-tools/bin/sqlcmd"
fi

echo "=========================================================="
echo "Starting EducationSystem Database Wait & Initialization..."
echo "Target Host: $MSSQL_HOST:$MSSQL_PORT"
echo "Target Database: $DB_NAME"
echo "=========================================================="

# 1. Polling loop waiting for MS SQL Server connection
echo "Waiting for SQL Server ($MSSQL_HOST:$MSSQL_PORT) to accept connections..."
MAX_TRIES=60
COUNT=0
until $SQLCMD -S "$MSSQL_HOST,$MSSQL_PORT" -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" > /dev/null 2>&1; do
    COUNT=$((COUNT+1))
    if [ $COUNT -ge $MAX_TRIES ]; then
        echo "Error: SQL Server unavailable after $MAX_TRIES attempts."
        exit 1
    fi
    echo "SQL Server is starting up... (attempt $COUNT/$MAX_TRIES)"
    sleep 3
done

echo "SQL Server is online and accepting connections!"

# 2. Create Target Database TayDoV2 if it doesn't exist
echo "Ensuring database [$DB_NAME] exists..."
$SQLCMD -S "$MSSQL_HOST,$MSSQL_PORT" -U sa -P "$MSSQL_SA_PASSWORD" -C -b -Q "
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'$DB_NAME')
BEGIN
    CREATE DATABASE [$DB_NAME];
    PRINT 'Created database $DB_NAME';
END"

# 3. Create schemas for schema-per-service model
echo "Ensuring schemas (identity, academic, exam, communication, career) exist..."
$SQLCMD -S "$MSSQL_HOST,$MSSQL_PORT" -U sa -P "$MSSQL_SA_PASSWORD" -d "$DB_NAME" -C -b -Q "
IF SCHEMA_ID(N'identity') IS NULL EXEC(N'CREATE SCHEMA [identity];');
IF SCHEMA_ID(N'academic') IS NULL EXEC(N'CREATE SCHEMA [academic];');
IF SCHEMA_ID(N'exam') IS NULL EXEC(N'CREATE SCHEMA [exam];');
IF SCHEMA_ID(N'communication') IS NULL EXEC(N'CREATE SCHEMA [communication];');
IF SCHEMA_ID(N'career') IS NULL EXEC(N'CREATE SCHEMA [career];');
"

# 4. Create baseline entity tables across schemas
echo "Creating baseline schema tables for microservices..."
$SQLCMD -S "$MSSQL_HOST,$MSSQL_PORT" -U sa -P "$MSSQL_SA_PASSWORD" -d "$DB_NAME" -C -b -Q "
-- identity schema
IF OBJECT_ID(N'identity.Users', N'U') IS NULL
CREATE TABLE [identity].[Users] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200) NULL,
    PasswordHash NVARCHAR(500) NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'Student',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

IF OBJECT_ID(N'identity.AuditLogs', N'U') IS NULL
CREATE TABLE [identity].[AuditLogs] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NULL,
    Action NVARCHAR(200) NOT NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

IF OBJECT_ID(N'identity.PasswordResets', N'U') IS NULL
CREATE TABLE [identity].[PasswordResets] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(500) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL
);

IF OBJECT_ID(N'identity.Settings', N'U') IS NULL
CREATE TABLE [identity].[Settings] (
    KeyName NVARCHAR(100) PRIMARY KEY,
    Value NVARCHAR(MAX) NULL
);

IF OBJECT_ID(N'identity.UserDevices', N'U') IS NULL
CREATE TABLE [identity].[UserDevices] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    DeviceName NVARCHAR(200) NULL
);

-- academic schema
IF OBJECT_ID(N'academic.Students', N'U') IS NULL
CREATE TABLE [academic].[Students] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    StudentCode NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NULL,
    GPA FLOAT NULL DEFAULT 0.0,
    MajorId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

IF OBJECT_ID(N'academic.Subjects', N'U') IS NULL
CREATE TABLE [academic].[Subjects] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SubjectCode NVARCHAR(50) NOT NULL,
    SubjectName NVARCHAR(200) NOT NULL,
    Credits INT NOT NULL DEFAULT 3
);

IF OBJECT_ID(N'academic.StudentProjects', N'U') IS NULL
CREATE TABLE [academic].[StudentProjects] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    StudentId UNIQUEIDENTIFIER NOT NULL,
    ProjectName NVARCHAR(200) NOT NULL,
    Technologies NVARCHAR(500) NULL,
    Description NVARCHAR(MAX) NULL,
    SourceUrl NVARCHAR(500) NULL,
    TeamSize INT NULL,
    Role NVARCHAR(200) NULL,
    Contribution NVARCHAR(MAX) NULL,
    SubjectId UNIQUEIDENTIFIER NULL
);

IF OBJECT_ID(N'academic.StudentInternships', N'U') IS NULL
CREATE TABLE [academic].[StudentInternships] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    StudentId UNIQUEIDENTIFIER NOT NULL,
    CompanyName NVARCHAR(200) NOT NULL,
    Position NVARCHAR(200) NULL,
    Tasks NVARCHAR(MAX) NULL,
    Duration NVARCHAR(100) NULL
);

-- exam schema
IF OBJECT_ID(N'exam.Questions', N'U') IS NULL
CREATE TABLE [exam].[Questions] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Content NVARCHAR(MAX) NOT NULL,
    SubjectId UNIQUEIDENTIFIER NULL
);

-- communication schema
IF OBJECT_ID(N'communication.FormTemplates', N'U') IS NULL
CREATE TABLE [communication].[FormTemplates] (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    TemplateUrl NVARCHAR(500) NULL
);
"

# 5. Run SQL scripts in correct order
echo "Executing official SQL scripts..."

SCRIPTS=(
    "/scripts/career/001_create_career_schema.sql"
    "/scripts/career/002_create_career_tables.sql"
    "/scripts/career/003_seed_progression_levels.sql"
    "/scripts/career/004_create_career_indexes.sql"
    "/scripts/career/005_verify_career_schema.sql"
    "/scripts/career-import/001_create_import_tables.sql"
    "/scripts/career-import/002_create_import_constraints.sql"
    "/scripts/career-import/003_create_import_indexes.sql"
    "/scripts/career-import/004_add_subject_selection.sql"
    "/scripts/career-import/006_add_approved_outcome_document.sql"
    "/scripts/career-import/005_verify_import_schema.sql"
    "/scripts/vector-match/001_create_vector_match_reader_role.sql"
    "/scripts/vector-match/002_verify_vector_match_reader.sql"
)

for script in "${SCRIPTS[@]}"; do
    if [ -f "$script" ]; then
        echo "Running script: $script"
        $SQLCMD -S "$MSSQL_HOST,$MSSQL_PORT" -U sa -P "$MSSQL_SA_PASSWORD" -d "$DB_NAME" -C -b -f 65001 -i "$script"
    else
        echo "Warning: Script not found at $script"
    fi
done

echo "=========================================================="
echo "Database Initialization & Seeding Completed Successfully!"
echo "=========================================================="
