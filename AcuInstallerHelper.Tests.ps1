# Acu Installer Helper Tests
# Run these from the repository root using Invoke-Pester -Path .\AcuInstallerHelper.Tests.ps1


Describe "Add-AcuVersion" {
    BeforeAll{
        Import-Module (Join-Path $PSScriptRoot AcuInstallerHelper) -Verbose -Force
        $testTempDir = (Join-Path (Get-Location) "TestOutput")
        Set-AcuDir $testTempDir
        Set-AcuSiteDir "Sites"
        Set-AcuVersionDir "Versions"
    }
    Context "When adding 23R1 site version" {
        It "Installs the specified Acu site version" {
            # Arrange
            $version = "23.106.0050"

            # Act
            Add-AcuVersion -v $version

            # Assert
            Test-Path (Join-Path $testTempDir "Versions" "23.106.0050" "Data") | Should -BeTrue
        }
    }
}

Describe "Add-AcuSite" {
    BeforeAll{
        Import-Module (Join-Path $PSScriptRoot AcuInstallerHelper) -Verbose -Force
        $testTempDir = (Join-Path (Get-Location) "TestOutput")
        Set-AcuDir $testTempDir
        Set-AcuSiteDir "Sites"
        Set-AcuVersionDir "Versions"
    }
    Context "When adding a new 23R1 site" {
        It "Installs the specified Acu site" {
            # Arrange
            $version = "23.106.0050"

            # Act
            Add-AcuSite -v $version -n "23R1"

            # Assert
            Test-Path (Join-Path $testTempDir "Sites" "web.config") | Should -BeTrue
        }
    }
}