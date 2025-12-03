# =============================================
# Quick Import - Moss Spa Services
# =============================================

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Moss Spa Service Importer" -ForegroundColor Cyan
Write-Host "  Ready to import 37 services!" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Default values
$defaultHost = "localhost"
$defaultDatabase = "spabooker"
$defaultUsername = "postgres"
$defaultPort = "5432"
$defaultLocationId = "1"

Write-Host "Let's get your database connection details:" -ForegroundColor Yellow
Write-Host ""

# Get database details
Write-Host "Database Host (press Enter for '$defaultHost'): " -NoNewline -ForegroundColor White
$host_input = Read-Host
$dbHost = if ([string]::IsNullOrWhiteSpace($host_input)) { $defaultHost } else { $host_input }

Write-Host "Database Name (press Enter for '$defaultDatabase'): " -NoNewline -ForegroundColor White
$db_input = Read-Host
$dbName = if ([string]::IsNullOrWhiteSpace($db_input)) { $defaultDatabase } else { $db_input }

Write-Host "Username (press Enter for '$defaultUsername'): " -NoNewline -ForegroundColor White
$user_input = Read-Host
$dbUser = if ([string]::IsNullOrWhiteSpace($user_input)) { $defaultUsername } else { $user_input }

Write-Host "Password: " -NoNewline -ForegroundColor White
$dbPassword = Read-Host -AsSecureString
$dbPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($dbPassword))

Write-Host "Port (press Enter for '$defaultPort'): " -NoNewline -ForegroundColor White
$port_input = Read-Host
$dbPort = if ([string]::IsNullOrWhiteSpace($port_input)) { $defaultPort } else { $port_input }

Write-Host ""
Write-Host "Location ID (press Enter for '$defaultLocationId'): " -NoNewline -ForegroundColor White
$loc_input = Read-Host
$locationId = if ([string]::IsNullOrWhiteSpace($loc_input)) { $defaultLocationId } else { $loc_input }

# Build connection string
$connectionString = "Host=$dbHost;Port=$dbPort;Database=$dbName;Username=$dbUser;Password=$dbPasswordPlain"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Configuration Summary" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Database: $dbHost/$dbName" -ForegroundColor White
Write-Host "  Username: $dbUser" -ForegroundColor White
Write-Host "  Location ID: $locationId" -ForegroundColor White
Write-Host ""
Write-Host "This will:" -ForegroundColor Yellow
Write-Host "  1. Delete 8 filler/demo services" -ForegroundColor White
Write-Host "  2. Import 37 real Moss Spa services" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Continue with import? (yes/no)"

if ($confirm -ne "yes" -and $confirm -ne "y") {
    Write-Host ""
    Write-Host "Import cancelled." -ForegroundColor Yellow
    Write-Host ""
    exit 0
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Starting Import..." -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Run the import
try {
    dotnet run --project MossScraper.csproj import "$connectionString" $locationId
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "============================================" -ForegroundColor Green
        Write-Host "  Import Successful!" -ForegroundColor Green
        Write-Host "============================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "  1. Check your service management page" -ForegroundColor White
        Write-Host "  2. Verify 37 services are showing" -ForegroundColor White
        Write-Host "  3. Assign therapists to services" -ForegroundColor White
        Write-Host ""
    } else {
        Write-Host ""
        Write-Host "Import may have encountered issues. Check the output above." -ForegroundColor Yellow
        Write-Host ""
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

