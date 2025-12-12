@echo off
SETLOCAL

ECHO ========================================
ECHO   Running Service Groups Data Migration
ECHO ========================================

cd C:\Mac\Home\Desktop\Repos\SpaBooker

ECHO Reading connection string from appsettings...
SET PGPASSWORD=password123
SET PGHOST=localhost
SET PGPORT=5433
SET PGDATABASE=spabooker
SET PGUSER=postgres

ECHO Step 1: Running ServiceGroups data migration...
"C:\Program Files\PostgreSQL\17\bin\psql" -U %PGUSER% -h %PGHOST% -p %PGPORT% -d %PGDATABASE% -f "src\SpaBooker.Infrastructure\Migrations\Scripts\MigrateToServiceGroups.sql"
IF %ERRORLEVEL% NEQ 0 (
    ECHO ERROR: ServiceGroups migration failed!
    ECHO.
    ECHO Trying to check if ServiceGroups already exist...
    "C:\Program Files\PostgreSQL\17\bin\psql" -U %PGUSER% -h %PGHOST% -p %PGPORT% -d %PGDATABASE% -c "SELECT COUNT(*) FROM \"ServiceGroups\";"
    GOTO :END
)

ECHO Step 2: Running ServiceGroupRooms data migration...
"C:\Program Files\PostgreSQL\17\bin\psql" -U %PGUSER% -h %PGHOST% -p %PGPORT% -d %PGDATABASE% -c "INSERT INTO \"ServiceGroupRooms\" (\"ServiceGroupId\", \"RoomId\", \"AssignedAt\") SELECT DISTINCT sg.\"Id\" as \"ServiceGroupId\", rsc.\"RoomId\", MIN(rsc.\"CreatedAt\") as \"AssignedAt\" FROM \"RoomServiceCapabilities\" rsc INNER JOIN \"SpaServices\" s ON rsc.\"ServiceId\" = s.\"Id\" INNER JOIN \"ServiceGroups\" sg ON (CASE WHEN s.\"Name\" ~* '\s*\((\d+)\s*min\)$' THEN REGEXP_REPLACE(s.\"Name\", '\s*\((\d+)\s*min\)$', '', 'i') ELSE s.\"Name\" END) = sg.\"Name\" AND s.\"LocationId\" = sg.\"LocationId\" GROUP BY sg.\"Id\", rsc.\"RoomId\" ON CONFLICT DO NOTHING;"
IF %ERRORLEVEL% NEQ 0 (
    ECHO ERROR: ServiceGroupRooms migration failed!
    GOTO :END
)

ECHO.
ECHO ========================================
ECHO   Migration completed successfully!
ECHO   Please refresh your browser.
ECHO ========================================

:END
ECHO.
PAUSE
ENDLOCAL











