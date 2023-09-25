# Acu Installer Helper Tests
# Run these from the repository root using Invoke-Pester -Path .\AcuInstallerHelper.Tests.ps1

Describe "Add-AcuSiteVersion" {
    BeforeAll{
        Import-Module (Join-Path $PSScriptRoot AcuInstallerHelper) -Verbose -Force
        Set-AcumaticaDir (Get-Location)
        Set-AcumaticaSiteDir "Sites"
        Set-AcumaticaERPVersionDir "Versions"
    }
    Context "When adding 23R1 site version" {
        It "Installs the specified Acumatica site version" {
            # Arrange
            $version = "23.106.0050"

            # Act
            Add-AcuSiteVersion -v $version

            # Assert
            # For example, if Add-AcuSiteVersion creates a directory, you could check its existence:
            $pathToCheck = "Path\to\where\it\should\install"
            Test-Path $pathToCheck | Should -Be $true
        }
    }
}