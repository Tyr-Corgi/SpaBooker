@echo off
SETLOCAL

ECHO ========================================
ECHO   Service Rooms Migration
ECHO ========================================

ECHO Step 1: Generating EF Core migration...
cd C:\Mac\Home\Desktop\Repos\SpaBooker
dotnet ef migrations add AddServiceGroupRooms --project src\SpaBooker.Infrastructure\SpaBooker.Infrastructure.csproj --startup-project src\SpaBooker.Web\SpaBooker.Web.csproj --context ApplicationDbContext
IF %ERRORLEVEL% NEQ 0 (
    ECHO ERROR: Migration generation failed!
    GOTO :END
)

ECHO Step 2: Applying EF Core migration...
cd C:\Mac\Home\Desktop\Repos\SpaBooker
dotnet ef database update --project src\SpaBooker.Infrastructure\SpaBooker.Infrastructure.csproj --startup-project src\SpaBooker.Web\SpaBooker.Web.csproj --context ApplicationDbContext
IF %ERRORLEVEL% NEQ 0 (
    ECHO ERROR: Migration application failed!
    GOTO :END
)

ECHO Step 3: Running data migration to populate ServiceGroupRooms...
SET PGPASSWORD=password123
psql -U postgres -h localhost -p 5433 -d spabooker -c "INSERT INTO \"ServiceGroupRooms\" (\"ServiceGroupId\", \"RoomId\", \"AssignedAt\") SELECT DISTINCT sg.\"Id\" as \"ServiceGroupId\", rsc.\"RoomId\", MIN(rsc.\"CreatedAt\") as \"AssignedAt\" FROM \"RoomServiceCapabilities\" rsc INNER JOIN \"SpaServices\" s ON rsc.\"ServiceId\" = s.\"Id\" INNER JOIN \"ServiceGroups\" sg ON (CASE WHEN s.\"Name\" ~* '\s*\((\d+)\s*min\)$' THEN REGEXP_REPLACE(s.\"Name\", '\s*\((\d+)\s*min\)$', '', 'i') ELSE s.\"Name\" END) = sg.\"Name\" AND s.\"LocationId\" = sg.\"LocationId\" GROUP BY sg.\"Id\", rsc.\"RoomId\" ON CONFLICT DO NOTHING;"
IF %ERRORLEVEL% NEQ 0 (
    ECHO ERROR: Data migration failed!
    GOTO :END
)

ECHO.
ECHO ========================================
ECHO   Migration completed successfully!
ECHO ========================================

:END
ECHO.
PAUSE
ENDLOCAL

