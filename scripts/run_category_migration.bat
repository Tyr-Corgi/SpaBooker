@echo off
SETLOCAL

SET ProjectDir=C:\Mac\Home\Desktop\Repos\SpaBooker
SET InfrastructureProject=%ProjectDir%\src\SpaBooker.Infrastructure\SpaBooker.Infrastructure.csproj
SET WebProject=%ProjectDir%\src\SpaBooker.Web\SpaBooker.Web.csproj
SET MigrationScript=%ProjectDir%\src\SpaBooker.Infrastructure\Migrations\Scripts\MigrateRoomsToServiceGroups.sql

ECHO ========================================
ECHO   Service Category and Rooms Migration
ECHO ========================================
ECHO.

cd "%ProjectDir%"

ECHO Step 1: Generating EF Core migration...
ECHO ========================================
dotnet ef migrations add AddServiceCategoryAndRooms ^
    --project "%InfrastructureProject%" ^
    --startup-project "%WebProject%" ^
    --context ApplicationDbContext

IF %ERRORLEVEL% NEQ 0 (
    ECHO ERROR: Migration generation failed!
    GOTO :END
)
ECHO Migration generated successfully.
ECHO.

ECHO Step 2: Applying EF Core schema migration...
ECHO ========================================
dotnet ef database update ^
    --project "%InfrastructureProject%" ^
    --startup-project "%WebProject%" ^
    --context ApplicationDbContext

IF %ERRORLEVEL% NEQ 0 (
    ECHO ERROR: EF Core migration failed!
    GOTO :END
)
ECHO EF Core schema migration applied successfully.
ECHO.

ECHO Step 3: Running data migration script...
ECHO ========================================
SET PGPASSWORD=password123
"C:\Program Files\PostgreSQL\17\bin\psql" -U postgres -h localhost -p 5433 -d spabooker -f "%MigrationScript%"

IF %ERRORLEVEL% NEQ 0 (
    ECHO ERROR: Data migration failed!
    GOTO :END
)
ECHO Data migration script executed successfully.
ECHO.

ECHO ========================================
ECHO   All migrations completed successfully!
ECHO ========================================

:END
ECHO.
PAUSE
ENDLOCAL

