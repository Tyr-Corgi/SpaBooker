@echo off
echo ========================================
echo   Generating Service Groups Migration
echo ========================================
echo.

REM Get the directory where the batch file is located
set SCRIPT_DIR=%~dp0
cd /d "%SCRIPT_DIR%"

cd src\SpaBooker.Web

echo Generating migration...
dotnet ef migrations add AddServiceGroupsAndDurations --project ..\SpaBooker.Infrastructure --context ApplicationDbContext --verbose

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Migration generation failed!
    cd /d "%SCRIPT_DIR%"
    pause
    exit /b 1
)

echo.
echo Applying migration to database...
dotnet ef database update --project ..\SpaBooker.Infrastructure --context ApplicationDbContext --verbose

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Database update failed!
    cd /d "%SCRIPT_DIR%"
    pause
    exit /b 1
)

echo.
echo Running data migration script...
set PGPASSWORD=postgres
psql -U postgres -d spabooker -f ..\SpaBooker.Infrastructure\Migrations\Scripts\MigrateToServiceGroups.sql

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Data migration failed!
    cd /d "%SCRIPT_DIR%"
    pause
    exit /b 1
)

cd /d "%SCRIPT_DIR%"
echo.
echo ========================================
echo   Migration completed successfully!
echo ========================================
pause

