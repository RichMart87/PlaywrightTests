<#
.SYNOPSIS
    Builds and runs the Playwright MSTest suites with the same filters CI uses.

.EXAMPLE
    ./Scripts/run-tests.ps1                      # all suites, headless, traces kept for failures
    ./Scripts/run-tests.ps1 -Suite Smoke -Headed # watch the smoke tests run
    ./Scripts/run-tests.ps1 -Suite Regression -Trace on
    ./Scripts/run-tests.ps1 -Filter "Name~Logo"  # any custom dotnet test filter
#>
[CmdletBinding()]
param(
    [ValidateSet('All', 'Api', 'Smoke', 'Regression')]
    [string]$Suite = 'All',

    # Custom dotnet test --filter expression; overrides -Suite.
    [string]$Filter,

    [switch]$Headed,

    [ValidateSet('off', 'on', 'retain-on-failure')]
    [string]$Trace = 'retain-on-failure',

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    # Skip restore/build/browser install when iterating quickly.
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot 'PlaywrightTests.csproj'

# Suites map to [TestCategory] values (Infrastructure/TestCategories.cs), same as the CI matrix filters
if (-not $Filter -and $Suite -ne 'All') {
    $Filter = "TestCategory=$Suite"
}

$slug = if ($Suite -eq 'All' -or $PSBoundParameters.ContainsKey('Filter')) { 'local' } else { $Suite.ToLowerInvariant() }
$resultsDir = Join-Path $repoRoot "TestResults/$slug"

if (-not $NoBuild) {
    dotnet build $project --configuration $Configuration
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    if ($Suite -ne 'Api' -or $Filter) {
        $playwrightScript = Join-Path $repoRoot "bin/$Configuration/net9.0/playwright.ps1"
        & $playwrightScript install chromium
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
}

# Read by Infrastructure/RunSettings.cs
$env:HEADED = if ($Headed) { '1' } else { '0' }
$env:PW_TRACE = $Trace
$env:TEST_ARTIFACTS_DIR = Join-Path $resultsDir 'artifacts'

$testArgs = @(
    'test', $project,
    '--configuration', $Configuration,
    '--no-build',
    '--logger', "trx;LogFileName=$slug.trx",
    '--logger', 'console;verbosity=normal',
    '--results-directory', $resultsDir
)
if ($Filter) { $testArgs += @('--filter', $Filter) }

Write-Host "Running tests (suite: $Suite, headed: $Headed, trace: $Trace)" -ForegroundColor Cyan
dotnet @testArgs
$exitCode = $LASTEXITCODE

Write-Host "Results:   $resultsDir"
if (Test-Path $env:TEST_ARTIFACTS_DIR) {
    Write-Host "Artifacts: $env:TEST_ARTIFACTS_DIR (screenshots/traces)" -ForegroundColor Yellow
}

exit $exitCode
