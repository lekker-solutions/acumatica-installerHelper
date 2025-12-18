#Requires -Version 7.0

<#
.SYNOPSIS
    Entry point script to run all AcuInstallerHelper Pester tests in a specific order
.DESCRIPTION
    This script checks for Pester installation, installs it if needed, imports the module, 
    and runs all tests in the specified order: Config, Version, Site, then Patch cmdlets.
.PARAMETER OutputFormat
    Specify the output format for test results (Normal, Detailed, Minimal, CI)
.EXAMPLE
    .\RunTests.ps1
    Runs all tests with normal output
.EXAMPLE
    .\RunTests.ps1 -OutputFormat CI
    Runs all tests with CI-friendly output
#>

param(
    [ValidateSet('Normal', 'Detailed', 'Minimal', 'CI')]
    [string]$OutputFormat = 'Normal'
)

Write-Host "=== AcuInstallerHelper Test Runner ===" -ForegroundColor Cyan

# Verify PowerShell 7+ is being used
if ($PSVersionTable.PSVersion.Major -lt 7) {
    Write-Error "This module requires PowerShell 7.0 or higher. Current version: $($PSVersionTable.PSVersion). Please install PowerShell 7 from https://github.com/PowerShell/PowerShell"
    exit 1
}
Write-Host "PowerShell $($PSVersionTable.PSVersion) detected" -ForegroundColor Green

# Check if Pester is available
Write-Host "Checking Pester installation..." -ForegroundColor Yellow
$pesterModule = Get-Module -Name Pester -ListAvailable | Sort-Object Version -Descending | Select-Object -First 1

if (-not $pesterModule) {
    Write-Host "Pester not found. Installing Pester..." -ForegroundColor Yellow
    try {
        Install-Module -Name Pester -Force -SkipPublisherCheck -Scope CurrentUser
        Write-Host "Pester installed successfully!" -ForegroundColor Green
    }
    catch {
        Write-Error "Failed to install Pester: $($_.Exception.Message)"
        exit 1
    }
}
else {
    Write-Host "Pester $($pesterModule.Version) found" -ForegroundColor Green
}

# Import Pester
Write-Host "Importing Pester module..." -ForegroundColor Yellow
try {
    Import-Module Pester -Force
    Write-Host "Pester imported successfully!" -ForegroundColor Green
}
catch {
    Write-Error "Failed to import Pester: $($_.Exception.Message)"
    exit 1
}

# Check if tests directory exists
if (-not (Test-Path ".\tests")) {
    Write-Error "Tests directory not found. Please ensure tests are located in .\tests\"
    exit 1
}

Write-Host "Building Module" -ForegroundColor Yellow
Write-Host
dotnet build -c Release
Remove-Module AcuInstallerHelper -ErrorAction SilentlyContinue
Import-Module .\AcuInstallerHelper -Verbose

Write-Host
Write-Host
Write-Host "Module Built, running Tests" -ForegroundColor Green
Write-Host

# Define test execution order
$testOrder = @(
    "*Config*.Tests.ps1",
    "*Version*.Tests.ps1", 
    "*Site*.Tests.ps1",
    "*Patch*.Tests.ps1"
)

# Find test files in the specified order
$orderedTestFiles = @()
foreach ($pattern in $testOrder) {
    $matchingFiles = Get-ChildItem -Path ".\tests" -Filter $pattern -Recurse
    if ($matchingFiles) {
        $orderedTestFiles += $matchingFiles
        Write-Host "Found $($matchingFiles.Count) file(s) matching '$pattern':" -ForegroundColor Yellow
        foreach ($file in $matchingFiles) {
            Write-Host "  - $($file.Name)" -ForegroundColor Gray
        }
    }
    else {
        Write-Host "No files found matching pattern '$pattern'" -ForegroundColor DarkYellow
    }
}

# Check for any remaining test files not matched by the patterns
$allTestFiles = Get-ChildItem -Path ".\tests" -Filter "*.Tests.ps1" -Recurse
$unmatchedFiles = $allTestFiles | Where-Object { $_.FullName -notin $orderedTestFiles.FullName }
if ($unmatchedFiles) {
    Write-Host "Additional test files found (will run after ordered tests):" -ForegroundColor Yellow
    foreach ($file in $unmatchedFiles) {
        Write-Host "  - $($file.Name)" -ForegroundColor Gray
    }
    $orderedTestFiles += $unmatchedFiles
}

if ($orderedTestFiles.Count -eq 0) {
    Write-Error "No test files found in .\tests\ directory"
    exit 1
}

Write-Host "`nTest execution order:" -ForegroundColor Cyan
for ($i = 0; $i -lt $orderedTestFiles.Count; $i++) {
    Write-Host "$($i + 1). $($orderedTestFiles[$i].Name)" -ForegroundColor White
}

# Run tests in order
Write-Host "`nRunning tests in specified order..." -ForegroundColor Yellow
$totalResults = @{
    TotalCount   = 0
    PassedCount  = 0
    FailedCount  = 0
    SkippedCount = 0
}

try {
    foreach ($testFile in $orderedTestFiles) {
        Write-Host "`n--- Running $($testFile.Name) ---" -ForegroundColor Magenta
        
        # Create Pester configuration for v5+
        $pesterConfig = New-PesterConfiguration
        $pesterConfig.Run.Path = $testFile.FullName
        $pesterConfig.Output.Verbosity = $OutputFormat
        
        # Set CI-specific settings
        if ($OutputFormat -eq 'CI') {
            $pesterConfig.Output.CIFormat = 'Auto'
        }
        
        $result = Invoke-Pester -Configuration $pesterConfig
        
        # Accumulate results
        $totalResults.TotalCount += $result.TotalCount
        $totalResults.PassedCount += $result.PassedCount
        $totalResults.FailedCount += $result.FailedCount
        $totalResults.SkippedCount += $result.SkippedCount
        
        Write-Host "Results for $($testFile.Name): $($result.PassedCount)/$($result.TotalCount) passed" -ForegroundColor $(if ($result.FailedCount -gt 0) { 'Red' } else { 'Green' })
    }
    
    # Display final summary
    Write-Host "`n=== Final Test Results Summary ===" -ForegroundColor Cyan
    Write-Host "Total Tests: $($totalResults.TotalCount)" -ForegroundColor White
    Write-Host "Passed: $($totalResults.PassedCount)" -ForegroundColor Green
    Write-Host "Failed: $($totalResults.FailedCount)" -ForegroundColor $(if ($totalResults.FailedCount -gt 0) { 'Red' } else { 'Green' })
    Write-Host "Skipped: $($totalResults.SkippedCount)" -ForegroundColor Yellow
    
    if ($totalResults.FailedCount -gt 0) {
        Write-Host "`nSome tests failed. Check output above for details." -ForegroundColor Red
        exit 1
    }
    else {
        Write-Host "`nAll tests passed!" -ForegroundColor Green
        exit 0
    }
}
catch {
    Write-Error "Failed to run tests: $($_.Exception.Message)"
    exit 1
}