param(
    [switch]$Restore
)
$ErrorActionPreference = "Stop"
$env:DOTNET_NOLOGO = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"

if ($Restore) {
  dotnet tool restore
}

# format solution
if (-not (Get-Command dotnet-format -ErrorAction SilentlyContinue)) {
  dotnet tool restore
}

dotnet format --severity info --verify-no-changes:$false
