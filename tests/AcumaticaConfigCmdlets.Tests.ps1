Describe "AcumaticaConfigCmdlets" {
    BeforeAll {
        Import-Module "$PSScriptRoot\..\AcuInstallerHelper" -Force
    }

    Context "Get-AcumaticaConfig" {
        It "Should return configuration object with all required properties" {
            $config = Get-AcumaticaConfig
            $config | Should -Not -BeNullOrEmpty
            $config | Should -BeOfType [PSCustomObject]
            
            # Verify property values and types
            $config.AcumaticaDirectory | Should -Not -BeNullOrEmpty
            $config.AcumaticaDirectory | Should -BeOfType [string]
            
            $config.SiteDirectory | Should -Not -BeNullOrEmpty
            $config.SiteDirectory | Should -BeOfType [string]
            
            $config.VersionDirectory | Should -Not -BeNullOrEmpty
            $config.VersionDirectory | Should -BeOfType [string]
            
            $config.DefaultSiteType | Should -Not -BeNullOrEmpty
            $config.DefaultSiteType | Should -BeIn @('Production', 'Development', 'NotSet')
            
            $config.InstallDebugTools | Should -Not -Be $null
            $config.InstallDebugTools | Should -BeOfType [bool]
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
        It "Should accept valid directory path and persist the change" {
            $originalPath = (Get-AcumaticaConfig).AcumaticaDirectory
            $testPath = "C:\TestAcumatica"
            
            try {
                { Set-AcumaticaDirectory -Path $testPath } | Should -Not -Throw
            
                # Verify the change was applied
                $config = Get-AcumaticaConfig
                $config.AcumaticaDirectory | Should -Be $testPath
                $config.AcumaticaDirectory | Should -BeOfType [string]
            } finally {
                # Restore original path
                if ($originalPath) {
                    Set-AcumaticaDirectory -Path $originalPath
                }
            }
        }
    }

    Context "Set-AcumaticaSiteDirectory" {
        It "Should accept valid directory path and persist the change" {
            $originalPath = (Get-AcumaticaConfig).SiteDirectory
            $testPath = "TestSites2"
            
            try {
                { Set-AcumaticaSiteDirectory -Path $testPath } | Should -Not -Throw
            
                # Verify the change was applied
                $config = Get-AcumaticaConfig
                $config.SiteDirectory | Should -Be $testPath
                $config.SiteDirectory | Should -BeOfType [string]
            } finally {
                # Restore original path
                if ($originalPath) {
                    Set-AcumaticaSiteDirectory -Path $originalPath
                }
            }
        }
    }

    Context "Set-AcumaticaVersionDirectory" {
        It "Should accept valid directory path and persist the change" {
            $originalPath = (Get-AcumaticaConfig).VersionDirectory
            $testPath = "TestVersions2"
            
            try {
                { Set-AcumaticaVersionDirectory -Path $testPath } | Should -Not -Throw
            
                # Verify the change was applied
                $config = Get-AcumaticaConfig
                $config.VersionDirectory | Should -Be $testPath
                $config.VersionDirectory | Should -BeOfType [string]
            } finally {
                # Restore original path
                if ($originalPath) {
                    Set-AcumaticaVersionDirectory -Path $originalPath
                }
            }
        }
    }

    Context "Set-AcumaticaDefaultSiteType" {
        It "Should accept Production site type" {
            { Set-AcumaticaDefaultSiteType -SiteType Production } | Should -Not -Throw
        
            $config = Get-AcumaticaConfig
            $config.DefaultSiteType | Should -Be "Production"
        }

        It "Should accept Development site type" {
            { Set-AcumaticaDefaultSiteType -SiteType Development } | Should -Not -Throw
        
            $config = Get-AcumaticaConfig
            $config.DefaultSiteType | Should -Be "Development"
        }
    }

    Context "Set-AcumaticaInstallDebugTools" {
        It "Should accept true value" {
            { Set-AcumaticaInstallDebugTools -InstallDebugTools $true } | Should -Not -Throw
        
            $config = Get-AcumaticaConfig
            $config.InstallDebugTools | Should -Be $true
        }

        It "Should accept false value" {
            { Set-AcumaticaInstallDebugTools -InstallDebugTools $false } | Should -Not -Throw
        
            $config = Get-AcumaticaConfig
            $config.InstallDebugTools | Should -Be $false
        }
    }
}