@echo off
REM Setup script for SpaBooker development environment (Windows)
REM Run this after cloning the repository

echo.
echo 🚀 Setting up SpaBooker development environment...
echo.

REM Check for required tools
echo 📋 Checking prerequisites...

where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ❌ .NET SDK not found. Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download
    exit /b 1
)
echo ✅ .NET SDK found

where git >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Git not found. Please install Git from https://git-scm.com/
    exit /b 1
)
echo ✅ Git found

REM Configure Git hooks
echo.
echo 🔧 Configuring Git hooks...
git config core.hooksPath .githooks
echo ✅ Git hooks configured

REM Restore dependencies
echo.
echo 📦 Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Failed to restore dependencies
    exit /b 1
)
echo ✅ Dependencies restored

REM Build solution
echo.
echo 🔨 Building solution...
dotnet build --no-restore
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Build failed
    exit /b 1
)
echo ✅ Solution built successfully

REM Run tests to verify setup
echo.
echo 🧪 Running tests to verify setup...
dotnet test --no-build --verbosity quiet
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Tests failed
    exit /b 1
)
echo ✅ All tests passed

REM Optional: Install coverage tools
echo.
set /p INSTALL_COVERAGE="📊 Would you like to install code coverage tools? (y/n) "
if /i "%INSTALL_COVERAGE%"=="y" (
    echo Installing coverage tools...
    dotnet tool install --global dotnet-coverage 2>nul || echo Coverage tool already installed
    dotnet tool install --global dotnet-reportgenerator-globaltool 2>nul || echo Report generator already installed
    echo ✅ Coverage tools installed
)

REM Create .env file if it doesn't exist
if not exist .env (
    echo.
    echo 📝 Creating .env file...
    (
        echo # Database Configuration
        echo ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=spabooker;Username=postgres;Password=your_password
        echo.
        echo # Environment
        echo ASPNETCORE_ENVIRONMENT=Development
        echo.
        echo # Add other environment variables as needed
    ) > .env
    echo ✅ .env file created (please update with your settings^)
)

echo.
echo ✅ Setup complete!
echo.
echo 📚 Next steps:
echo   1. Read TESTING_GUIDELINES.md to understand testing requirements
echo   2. Update .env with your database connection string
echo   3. Run 'dotnet ef database update' to create the database
echo   4. Start coding! Remember: all changes must include tests
echo.
echo 🔍 Useful commands:
echo   • Run all tests:        dotnet test
echo   • Run unit tests:       dotnet test --filter "FullyQualifiedName~Tests.Unit"
echo   • Run with coverage:    dotnet test --collect:"XPlat Code Coverage"
echo   • Start app:            cd src\SpaBooker.Web ^&^& dotnet run
echo.
echo Happy coding! 🎉

pause

