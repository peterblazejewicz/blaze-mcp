@echo off
setlocal
set DOTNET_NOLOGO=1
set DOTNET_CLI_TELEMETRY_OPTOUT=1

set TESTPROJ=%cd%\tests\Blaze.MCP.Tests\Blaze.MCP.Tests.csproj
if exist "%TESTPROJ%" (
  dotnet test "%TESTPROJ%" -c Debug --no-build --logger "trx;LogFileName=TestResults.trx" --results-directory .\.testresults
) else (
  echo No tests found yet (tests\Blaze.MCP.Tests). Skipping.
)
