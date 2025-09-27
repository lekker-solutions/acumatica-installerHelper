Describe "AcumaticaConfigCmdlets" {
    BeforeAll {
        Import-Module "$PSScriptRoot\..\AcuInstallerHelper" -Force
    }

    Context "Get-AcumaticaConfig" {
        It "Should return configuration object" {
            $config = Get-AcumaticaConfig
            $config | Should -Not -BeNullOrEmpty
            $config.AcumaticaDirectory | Should -Not -BeNullOrEmpty
            $config.SiteDirectory | Should -Not -BeNullOrEmpty
            $config.VersionDirectory | Should -Not -BeNullOrEmpty
            $config.DefaultSiteType | Should -Not -BeNullOrEmpty
            $config.InstallDebugTools | Should -Not -BeNullOrEmpty
        }

        It "Should have expected properties" {
            $config = Get-AcumaticaConfig
            $config.PSObject.Properties.Name | Should -Contain "AcumaticaDirectory"
            $config.PSObject.Properties.Name | Should -Contain "SiteDirectory"
            $config.PSObject.Properties.Name | Should -Contain "VersionDirectory"
            $config.PSObject.Properties.Name | Should -Contain "DefaultSiteType"
            $config.PSObject.Properties.Name | Should -Contain "InstallDebugTools"
        }
    }

    Context "Set-AcumaticaDirectory" {
        It "Should accept valid directory path" {
            $testPath = "C:\TestAcumatica"
            { Set-AcumaticaDirectory -Path $testPath } | Should -Not -Throw
        }

        It "Should throw on empty path" {
            { Set-AcumaticaDirectory -Path "" } | Should -Throw
        }

        It "Should throw on null path" {
            { Set-AcumaticaDirectory -Path $null } | Should -Throw
        }
    }

    Context "Set-AcumaticaSiteDirectory" {
        It "Should accept valid directory path" {
            $testPath = "C:\TestSites"
            { Set-AcumaticaSiteDirectory -Path $testPath } | Should -Not -Throw
        }

        It "Should throw on empty path" {
            { Set-AcumaticaSiteDirectory -Path "" } | Should -Throw
        }

        It "Should throw on null path" {
            { Set-AcumaticaSiteDirectory -Path $null } | Should -Throw
        }
    }

    Context "Set-AcumaticaVersionDirectory" {
        It "Should accept valid directory path" {
            $testPath = "C:\TestVersions"
            { Set-AcumaticaVersionDirectory -Path $testPath } | Should -Not -Throw
        }

        It "Should throw on empty path" {
            { Set-AcumaticaVersionDirectory -Path "" } | Should -Throw
        }

        It "Should throw on null path" {
            { Set-AcumaticaVersionDirectory -Path $null } | Should -Throw
        }
    }

    Context "Set-AcumaticaDefaultSiteType" {
        It "Should accept Production site type" {
            { Set-AcumaticaDefaultSiteType -SiteType Production } | Should -Not -Throw
        }

        It "Should accept Development site type" {
            { Set-AcumaticaDefaultSiteType -SiteType Development } | Should -Not -Throw
        }

        It "Should throw on invalid site type" {
            { Set-AcumaticaDefaultSiteType -SiteType "InvalidType" } | Should -Throw
        }
    }

    Context "Set-AcumaticaInstallDebugTools" {
        It "Should accept true value" {
            { Set-AcumaticaInstallDebugTools -InstallDebugTools $true } | Should -Not -Throw
        }

        It "Should accept false value" {
            { Set-AcumaticaInstallDebugTools -InstallDebugTools $false } | Should -Not -Throw
        }
    }
}