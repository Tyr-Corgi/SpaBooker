# =============================================
# Run Moss Spa Service Importer
# =============================================
# This script compiles and runs the service importer
# =============================================

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Moss Spa Service Importer" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check if cleaned CSV exists
$csvPath = "output\moss_services_cleaned.csv"
if (-not (Test-Path $csvPath)) {
    Write-Host "ERROR: Cleaned CSV file not found at: $csvPath" -ForegroundColor Red
    Write-Host "Please run the scraper first or ensure the file exists." -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Found cleaned CSV file" -ForegroundColor Green
Write-Host ""

# Get database connection string
Write-Host "Database Configuration:" -ForegroundColor Yellow
Write-Host ""
Write-Host "Please enter your PostgreSQL connection string." -ForegroundColor White
Write-Host "Example: Host=localhost;Database=spabooker;Username=postgres;Password=yourpassword" -ForegroundColor Gray
Write-Host ""
$connectionString = Read-Host "Connection String"

if ([string]::IsNullOrWhiteSpace($connectionString)) {
    Write-Host "ERROR: Connection string is required!" -ForegroundColor Red
    exit 1
}

Write-Host ""
$locationId = Read-Host "Location ID (usually 1)"

if (-not ($locationId -match '^\d+$') -or [int]$locationId -le 0) {
    Write-Host "ERROR: Invalid location ID! Must be a positive number." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Configuration Summary:" -ForegroundColor Yellow
Write-Host "  - CSV File: $csvPath" -ForegroundColor White
Write-Host "  - Target Location ID: $locationId" -ForegroundColor White
Write-Host ""
Write-Host "This will:" -ForegroundColor Yellow
Write-Host "  1. Delete all existing filler/demo services" -ForegroundColor White
Write-Host "  2. Import 37 real Moss Spa services" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Continue? (yes/no)"
if ($confirm -ne "yes" -and $confirm -ne "y") {
    Write-Host "Import cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Building importer..." -ForegroundColor Cyan
dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Starting Import Process" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Run with connection string and location ID as environment variables
$env:DB_CONNECTION_STRING = $connectionString
$env:LOCATION_ID = $locationId

# Use ProgramImport.cs as entry point
dotnet run --no-build -c Release --project MossScraper.csproj

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

