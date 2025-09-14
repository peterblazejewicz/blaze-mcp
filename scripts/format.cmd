@echo off
setlocal enabledelayedexpansion
set DOTNET_NOLOGO=1
set DOTNET_CLI_TELEMETRY_OPTOUT=1

rem Usage: scripts\format.cmd [--restore]
for %%x in (%*) do (
  if "%%x"=="--restore" (
    dotnet tool restore
  )
)

where dotnet-format >nul 2>&1
if errorlevel 1 (
  dotnet tool restore
)

dotnet format --severity info --verify-no-changes:false
