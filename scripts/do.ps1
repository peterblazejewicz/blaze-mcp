param(
  [ValidateSet('Restore','Format','Build','Test','All')]
  [string]$Target = 'All'
)
$ErrorActionPreference = 'Stop'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'

function Do-Restore {
  dotnet restore
  dotnet tool restore
}

function Do-Format {
  if (-not (Get-Command dotnet-format -ErrorAction SilentlyContinue)) { dotnet tool restore }
  dotnet format --severity info --verify-no-changes:$false
}

function Do-Build {
  dotnet build .\Blaze.MCP.sln -c Debug
}

function Do-Test {
  dotnet test .\Blaze.MCP.sln -c Debug --no-build
}

switch ($Target) {
  'Restore' { Do-Restore }
  'Format'  { Do-Format }
  'Build'   { Do-Build }
  'Test'    { Do-Test }
  'All'     { Do-Restore; Do-Format; Do-Build; Do-Test }
}
