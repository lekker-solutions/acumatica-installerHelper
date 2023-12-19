# Acu Installer Helper Tests
# Run these from the repository root using Invoke-Pester -Path .\AcuInstallerHelper.Tests.ps1
$installPath = Get-Location
Describe "Add-AcuVersion" {
    BeforeAll{
        Import-Module (Join-Path $PSScriptRoot AcuInstallerHelper) -Verbose -Force
        Set-AcuDir (Get-Location)
        Set-AcuSiteDir "Sites"
        Set-AcuERPVersionDir "Versions"
    }
    Context "When adding 23R1 site version" {
        It "Installs the specified Acu site version" {
            # Arrange
            $version = "23.106.0050"

            # Act
            Add-AcuVersion -v $version

            # Assert
            Test-Path (Join-Path $installPath "Versions" "23.106.0050" "Data") | Should -Be $true
        }
    }
}

Describe "Add-AcuSite" {
    BeforeAll{
        Import-Module (Join-Path $PSScriptRoot AcuInstallerHelper) -Verbose -Force
        Set-AcuDir (Get-Location)
        Set-AcuSiteDir "Sites"
        Set-AcuERPVersionDir "Versions"
    }
    Context "When adding 23R1 site version" {
        It "Installs the specified Acu site version" {
            # Arrange
            $version = "23.106.0050"

            # Act
            Add-AcuSite -v $version -n "23R1"

            # Assert
            Test-Path (Join-Path $installPath "Sites" "web.config") | Should -Be $true
        }
    }
}