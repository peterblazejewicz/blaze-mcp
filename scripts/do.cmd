@echo off
setlocal
set DOTNET_NOLOGO=1
set DOTNET_CLI_TELEMETRY_OPTOUT=1

set TARGET=%1
if "%TARGET%"=="" set TARGET=All

if /I "%TARGET%"=="Restore" (
  dotnet restore
  dotnet tool restore
  goto :eof
)
if /I "%TARGET%"=="Format" (
  where dotnet-format >nul 2>&1 || dotnet tool restore
  dotnet format --severity info --verify-no-changes:false
  goto :eof
)
if /I "%TARGET%"=="Build" (
  dotnet build .\Blaze.MCP.sln -c Debug
  goto :eof
)
if /I "%TARGET%"=="Test" (
  dotnet test .\Blaze.MCP.sln -c Debug --no-build
  goto :eof
)

rem Default: All
dotnet restore
dotnet tool restore
where dotnet-format >nul 2>&1 || dotnet tool restore
dotnet format --severity info --verify-no-changes:false
dotnet build .\Blaze.MCP.sln -c Debug
dotnet test .\Blaze.MCP.sln -c Debug --no-build
