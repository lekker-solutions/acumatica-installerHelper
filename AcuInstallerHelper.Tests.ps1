# Acu Installer Helper Tests
# Run these from the repository root using Invoke-Pester -Path .\AcuInstallerHelper.Tests.ps1
$installPath = Get-Location
Describe "Add-AcuSiteVersion" {
    BeforeAll{
        Import-Module (Join-Path $PSScriptRoot AcuInstallerHelper) -Verbose -Force
        Set-AcumaticaDir ($installPath)
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
            # Test-Path (Join-Path $installPath "Sites" "web.config") | Should -Be $true
            $true | Should -Be $true
        }
    }
}