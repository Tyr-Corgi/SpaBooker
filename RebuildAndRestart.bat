@echo off
echo Cleaning and rebuilding SpaBooker...
cd src\SpaBooker.Web
dotnet clean
dotnet build
echo.
echo Done! Now run: dotnet run

