@echo off
echo Starting Torch2 Web UI (Debug)...

REM Set environment variables
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://localhost:7076

REM Run the application in Debug mode
dotnet run --launch-profile "https" --configuration Debug

echo Application exited.
pause