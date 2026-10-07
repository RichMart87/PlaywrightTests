<#
.SYNOPSIS
    Opens a Playwright trace in the trace viewer. Defaults to the most recent trace under TestResults.

.EXAMPLE
    ./Scripts/show-trace.ps1
    ./Scripts/show-trace.ps1 -Path TestResults/smoke/artifacts/SomeTest/SomeTest_20261003120000000.zip
#>
[CmdletBinding()]
param(
    [string]$Path,

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

if (-not $Path) {
    $latest = Get-ChildItem (Join-Path $repoRoot 'TestResults') -Recurse -Filter '*.zip' -ErrorAction SilentlyContinue |
        Where-Object { $_.DirectoryName -match '[\\/]artifacts[\\/]' } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $latest) {
        Write-Error "No traces found under TestResults. Run ./Scripts/run-tests.ps1 (traces are kept for failing tests, or all tests with -Trace on)."
    }
    $Path = $latest.FullName
}

$playwrightScript = Join-Path $repoRoot "bin/$Configuration/net9.0/playwright.ps1"
if (-not (Test-Path $playwrightScript)) {
    Write-Error "$playwrightScript not found. Build the project first (dotnet build -c $Configuration)."
}

Write-Host "Opening trace: $Path" -ForegroundColor Cyan
& $playwrightScript show-trace $Path
