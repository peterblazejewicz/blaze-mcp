$ErrorActionPreference = "Stop"
$env:DOTNET_NOLOGO = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"

# Discover and run tests if test project exists
$testProj = Join-Path (Get-Location) "tests\Blaze.MCP.Tests\Blaze.MCP.Tests.csproj"
if (Test-Path $testProj) {
  dotnet test $testProj -c Debug --no-build --logger "trx;LogFileName=TestResults.trx" --results-directory .\.testresults
} else {
  Write-Host "No tests found yet (tests/Blaze.MCP.Tests). Skipping."
}
