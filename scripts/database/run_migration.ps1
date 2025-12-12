$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Applying Service Groups Migration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Apply EF Core migration
Write-Host "[1/2] Applying EF Core schema migration..." -ForegroundColor Yellow
cd src\SpaBooker.Web
dotnet ef database update --project ..\SpaBooker.Infrastructure --context ApplicationDbContext --verbose 2>&1 | Out-String | Write-Host

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: EF Core migration failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "[2/2] Running data migration script..." -ForegroundColor Yellow

# Apply data migration script
$env:PGPASSWORD = "postgres"
$scriptPath = "..\SpaBooker.Infrastructure\Migrations\Scripts\MigrateToServiceGroups.sql"
$result = psql -U postgres -d spabooker -f $scriptPath 2>&1 | Out-String
Write-Host $result

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Data migration failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Migration completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

