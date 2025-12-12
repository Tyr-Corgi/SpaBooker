@echo off
cd C:\Mac\Home\Desktop\Repos\SpaBooker\src\SpaBooker.Web
dotnet ef database update --project ..\SpaBooker.Infrastructure --context ApplicationDbContext --verbose
pause










