# Acu Installer Helper Tests
# Run these from the repository root using Invoke-Pester -Path .\AcuInstallerHelper.Tests.ps1
Describe "Add-AcuVersion" {
    BeforeAll{
        Import-Module (Join-Path $PSScriptRoot AcuInstallerHelper) -Verbose -Force
        Set-AcumaticaDir (Get-Location)
        Set-AcumaticaSiteDir "Sites"
        Set-AcumaticaVersionDir "Versions"
    }
    Context "When adding 23R1 site version" {
        It "Installs the specified Acumatica site version" {
            # Arrange
            $version = "23.106.0050"

            # Act
            Add-AcuVersion -v $version

            # Assert
            Test-Path (Join-Path Get-Location "Versions" "23.106.0050" "Data") | Should -Be $true
        }
    }
}

Describe "Add-AcuSite" {
    BeforeAll{
        Import-Module (Join-Path $PSScriptRoot AcuInstallerHelper) -Verbose -Force
        Set-AcumaticaDir (Get-Location)
        Set-AcumaticaSiteDir "Sites"
        Set-AcumaticaVersionDir "Versions"
    }
    Context "When adding 23R1 site version" {
        It "Installs the specified Acumatica site version" {
            # Arrange
            $version = "23.106.0050"

            # Act
            Add-AcuSite -v $version -n "23R1"

            # Assert
            Test-Path (Join-Path Get-Location "Sites" "web.config") | Should -Be $true
        }
    }
}