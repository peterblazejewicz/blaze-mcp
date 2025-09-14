$ErrorActionPreference = "Stop"
$env:DOTNET_NOLOGO = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"

dotnet restore

# Build default sln-less project
$Configuration = $env:CONFIGURATION
if (-not $Configuration) { $Configuration = "Debug" }

dotnet build -c $Configuration
