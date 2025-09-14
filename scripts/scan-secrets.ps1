param(
  [switch]$Staged,
  [switch]$All
)

# Ensure gitleaks is installed
$gitleaks = Get-Command gitleaks -ErrorAction SilentlyContinue
if (-not $gitleaks) {
  Write-Error "gitleaks not found. Install with: winget install gitleaksproject.gitleaks or choco install gitleaks"
  exit 1
}

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent
$configPath = Join-Path $repoRoot ".gitleaks.toml"

$baseArgs = @("detect", "--redact")
if (Test-Path $configPath) { $baseArgs += @("--config", $configPath) }

if ($Staged) {
  $baseArgs += @("--staged")
} elseif ($All) {
  $baseArgs += @("--source", $repoRoot)
}

Write-Host "Running: gitleaks $($baseArgs -join ' ')"
& gitleaks @baseArgs
$exitCode = $LASTEXITCODE
if ($exitCode -ne 0) {
  Write-Error "Gitleaks reported findings. Review above output."
}
exit $exitCode
