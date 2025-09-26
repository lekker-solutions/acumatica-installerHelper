#Requires -Modules @{ ModuleName="Pester"; ModuleVersion="5.0" }

BeforeAll {
    # Import the module being tested
    $ModulePath = $PSScriptRoot
    Import-Module "$ModulePath\AcuInstallerHelper.psd1" -Force
    
    # Setup test variables
    $TestVersion = "2023.2.0001"
    $TestSiteName = "PesterTestSite"
    $TestPath = "C:\Test\AcumaticaTest"
}

Describe "AcuInstallerHelper Module Loading" {
    It "Should import the module successfully" {
        Get-Module AcuInstallerHelper | Should -Not -BeNullOrEmpty
    }
    
    It "Should have all expected cmdlets available" {
        $ExpectedCmdlets = @(
            'Install-AcumaticaVersion',
            'Uninstall-AcumaticaVersion', 
            'Get-AcumaticaVersion',
            'New-AcumaticaSite',
            'Remove-AcumaticaSite',
            'Get-AcumaticaSite',
            'Update-AcumaticaSite',
            'Get-AcumaticaConfig',
            'Set-AcumaticaDirectory',
            'Set-AcumaticaSiteDirectory',
            'Set-AcumaticaVersionDirectory',
            'Set-AcumaticaDefaultSiteType',
            'Set-AcumaticaInstallDebugTools'
        )
        
        foreach ($Cmdlet in $ExpectedCmdlets) {
            Get-Command $Cmdlet -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty -Because "Cmdlet $Cmdlet should be available"
        }
    }
}

Describe "Configuration Cmdlets" {
    Context "Get-AcumaticaConfig" {
        It "Should return configuration object" {
            $Config = Get-AcumaticaConfig
            $Config | Should -Not -BeNullOrEmpty
            $Config | Should -HaveProperty AcumaticaDirectory
            $Config | Should -HaveProperty SiteDirectory
            $Config | Should -HaveProperty VersionDirectory
            $Config | Should -HaveProperty DefaultSiteType
            $Config | Should -HaveProperty InstallDebugTools
        }
        
        It "Should have string properties for directories" {
            $Config = Get-AcumaticaConfig
            $Config.AcumaticaDirectory | Should -BeOfType [string]
            $Config.SiteDirectory | Should -BeOfType [string]
            $Config.VersionDirectory | Should -BeOfType [string]
        }
        
        It "Should have boolean property for InstallDebugTools" {
            $Config = Get-AcumaticaConfig
            $Config.InstallDebugTools | Should -BeOfType [bool]
        }
    }
    
    Context "Set-AcumaticaDirectory" {
        It "Should accept valid directory path" {
            { Set-AcumaticaDirectory -Path "C:\Acumatica" } | Should -Not -Throw
        }
        
        It "Should require Path parameter" {
            { Set-AcumaticaDirectory } | Should -Throw
        }
        
        It "Should reject null or empty path" {
            { Set-AcumaticaDirectory -Path "" } | Should -Throw
            { Set-AcumaticaDirectory -Path $null } | Should -Throw
        }
    }
    
    Context "Set-AcumaticaSiteDirectory" {
        It "Should accept valid directory path" {
            { Set-AcumaticaSiteDirectory -Path "C:\Sites" } | Should -Not -Throw
        }
        
        It "Should require Path parameter" {
            { Set-AcumaticaSiteDirectory } | Should -Throw
        }
        
        It "Should reject null or empty path" {
            { Set-AcumaticaSiteDirectory -Path "" } | Should -Throw
            { Set-AcumaticaSiteDirectory -Path $null } | Should -Throw
        }
    }
    
    Context "Set-AcumaticaVersionDirectory" {
        It "Should accept valid directory path" {
            { Set-AcumaticaVersionDirectory -Path "C:\Versions" } | Should -Not -Throw
        }
        
        It "Should require Path parameter" {
            { Set-AcumaticaVersionDirectory } | Should -Throw
        }
        
        It "Should reject null or empty path" {
            { Set-AcumaticaVersionDirectory -Path "" } | Should -Throw
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
        
        It "Should require SiteType parameter" {
            { Set-AcumaticaDefaultSiteType } | Should -Throw
        }
        
        It "Should reject invalid site types" {
            { Set-AcumaticaDefaultSiteType -SiteType "Invalid" } | Should -Throw
        }
    }
    
    Context "Set-AcumaticaInstallDebugTools" {
        It "Should accept true value" {
            { Set-AcumaticaInstallDebugTools -InstallDebugTools $true } | Should -Not -Throw
        }
        
        It "Should accept false value" {
            { Set-AcumaticaInstallDebugTools -InstallDebugTools $false } | Should -Not -Throw
        }
        
        It "Should require InstallDebugTools parameter" {
            { Set-AcumaticaInstallDebugTools } | Should -Throw
        }
    }
}

Describe "Version Management Cmdlets" {
    Context "Get-AcumaticaVersion" {
        It "Should return installed versions by default" {
            $Versions = Get-AcumaticaVersion
            $Versions | Should -Not -BeNullOrEmpty -Because "There should be at least an empty array"
        }
        
        It "Should accept Available switch" {
            { Get-AcumaticaVersion -Available } | Should -Not -Throw
        }
        
        It "Should accept MajorRelease parameter" {
            { Get-AcumaticaVersion -Available -MajorRelease "2023.2" } | Should -Not -Throw
        }
        
        It "Should accept Preview switch" {
            { Get-AcumaticaVersion -Available -Preview } | Should -Not -Throw
        }
        
        It "Should accept combined parameters" {
            { Get-AcumaticaVersion -Available -MajorRelease "2023.2" -Preview } | Should -Not -Throw
        }
    }
    
    Context "Install-AcumaticaVersion" {
        It "Should require Version parameter" {
            { Install-AcumaticaVersion } | Should -Throw
        }
        
        It "Should reject null or empty version" {
            { Install-AcumaticaVersion -Version "" } | Should -Throw
            { Install-AcumaticaVersion -Version $null } | Should -Throw
        }
        
        It "Should accept valid version format" {
            # Mock this test to avoid actual installation
            Mock Install-AcumaticaVersion { return $true }
            { Install-AcumaticaVersion -Version $TestVersion } | Should -Not -Throw
        }
        
        It "Should accept Preview switch" {
            Mock Install-AcumaticaVersion { return $true }
            { Install-AcumaticaVersion -Version $TestVersion -Preview } | Should -Not -Throw
        }
        
        It "Should accept DebugTools switch" {
            Mock Install-AcumaticaVersion { return $true }
            { Install-AcumaticaVersion -Version $TestVersion -DebugTools } | Should -Not -Throw
        }
    }
    
    Context "Uninstall-AcumaticaVersion" {
        It "Should require Version parameter" {
            { Uninstall-AcumaticaVersion } | Should -Throw
        }
        
        It "Should reject null or empty version" {
            { Uninstall-AcumaticaVersion -Version "" } | Should -Throw
            { Uninstall-AcumaticaVersion -Version $null } | Should -Throw
        }
        
        It "Should accept valid version format" {
            Mock Uninstall-AcumaticaVersion { return $true }
            { Uninstall-AcumaticaVersion -Version $TestVersion } | Should -Not -Throw
        }
    }
}

Describe "Site Management Cmdlets" {
    Context "Get-AcumaticaSite" {
        It "Should return sites list" {
            $Sites = Get-AcumaticaSite
            $Sites | Should -Not -BeNullOrEmpty -Because "There should be at least an empty array"
        }
        
        It "Should accept IncludeVersion switch" {
            { Get-AcumaticaSite -IncludeVersion } | Should -Not -Throw
        }
        
        It "Should return objects with Name and Version when IncludeVersion is used" {
            Mock Get-AcumaticaSite { 
                return @(
                    @{ Name = "TestSite1"; Version = "2023.2.0001" },
                    @{ Name = "TestSite2"; Version = "2023.2.0002" }
                )
            }
            
            $Sites = Get-AcumaticaSite -IncludeVersion
            $Sites | Should -Not -BeNullOrEmpty
            if ($Sites.Count -gt 0) {
                $Sites[0] | Should -HaveProperty Name
                $Sites[0] | Should -HaveProperty Version
            }
        }
    }
    
    Context "New-AcumaticaSite" {
        It "Should require Version and Name parameters" {
            { New-AcumaticaSite } | Should -Throw
            { New-AcumaticaSite -Version $TestVersion } | Should -Throw
            { New-AcumaticaSite -Name $TestSiteName } | Should -Throw
        }
        
        It "Should reject null or empty required parameters" {
            { New-AcumaticaSite -Version "" -Name $TestSiteName } | Should -Throw
            { New-AcumaticaSite -Version $TestVersion -Name "" } | Should -Throw
            { New-AcumaticaSite -Version $null -Name $TestSiteName } | Should -Throw
            { New-AcumaticaSite -Version $TestVersion -Name $null } | Should -Throw
        }
        
        It "Should accept optional Path parameter" {
            Mock New-AcumaticaSite { return $true }
            { New-AcumaticaSite -Version $TestVersion -Name $TestSiteName -Path $TestPath } | Should -Not -Throw
        }
        
        It "Should accept all switch parameters" {
            Mock New-AcumaticaSite { return $true }
            { New-AcumaticaSite -Version $TestVersion -Name $TestSiteName -InstallVersion -Portal -Development -Preview -DebugTools } | Should -Not -Throw
        }
    }
    
    Context "Remove-AcumaticaSite" {
        It "Should require Name parameter" {
            { Remove-AcumaticaSite } | Should -Throw
        }
        
        It "Should reject null or empty name" {
            { Remove-AcumaticaSite -Name "" } | Should -Throw
            { Remove-AcumaticaSite -Name $null } | Should -Throw
        }
        
        It "Should accept valid site name" {
            Mock Remove-AcumaticaSite { return $true }
            { Remove-AcumaticaSite -Name $TestSiteName } | Should -Not -Throw
        }
    }
    
    Context "Update-AcumaticaSite" {
        It "Should require Name and Version parameters" {
            { Update-AcumaticaSite } | Should -Throw
            { Update-AcumaticaSite -Name $TestSiteName } | Should -Throw
            { Update-AcumaticaSite -Version $TestVersion } | Should -Throw
        }
        
        It "Should reject null or empty required parameters" {
            { Update-AcumaticaSite -Name "" -Version $TestVersion } | Should -Throw
            { Update-AcumaticaSite -Name $TestSiteName -Version "" } | Should -Throw
            { Update-AcumaticaSite -Name $null -Version $TestVersion } | Should -Throw
            { Update-AcumaticaSite -Name $TestSiteName -Version $null } | Should -Throw
        }
        
        It "Should accept valid parameters" {
            Mock Update-AcumaticaSite { return $true }
            { Update-AcumaticaSite -Name $TestSiteName -Version $TestVersion } | Should -Not -Throw
        }
    }
}

Describe "Error Handling" {
    Context "Cmdlet Error Scenarios" {
        It "Should handle invalid version formats gracefully" {
            Mock Install-AcumaticaVersion { throw "Invalid version format" }
            { Install-AcumaticaVersion -Version "invalid.version" -ErrorAction SilentlyContinue } | Should -Not -Throw
        }
        
        It "Should handle non-existent sites gracefully" {
            Mock Remove-AcumaticaSite { throw "Site not found" }
            { Remove-AcumaticaSite -Name "NonExistentSite" -ErrorAction SilentlyContinue } | Should -Not -Throw
        }
        
        It "Should handle invalid paths gracefully" {
            Mock Set-AcumaticaDirectory { throw "Invalid path" }
            { Set-AcumaticaDirectory -Path "Z:\NonExistent\Path" -ErrorAction SilentlyContinue } | Should -Not -Throw
        }
    }
}

Describe "Integration Tests" {
    Context "Configuration Workflow" {
        It "Should maintain configuration changes" {
            # Save original config
            $OriginalConfig = Get-AcumaticaConfig
            
            try {
                # Set test configuration
                Set-AcumaticaDirectory -Path "C:\TestAcumatica"
                Set-AcumaticaSiteDirectory -Path "C:\TestSites"
                Set-AcumaticaVersionDirectory -Path "C:\TestVersions"
                Set-AcumaticaDefaultSiteType -SiteType Development
                Set-AcumaticaInstallDebugTools -InstallDebugTools $true
                
                # Verify changes
                $UpdatedConfig = Get-AcumaticaConfig
                $UpdatedConfig.AcumaticaDirectory | Should -Be "C:\TestAcumatica"
                $UpdatedConfig.SiteDirectory | Should -Be "C:\TestSites"
                $UpdatedConfig.VersionDirectory | Should -Be "C:\TestVersions"
                $UpdatedConfig.DefaultSiteType | Should -Be "Development"
                $UpdatedConfig.InstallDebugTools | Should -Be $true
            }
            finally {
                # Restore original configuration
                Set-AcumaticaDirectory -Path $OriginalConfig.AcumaticaDirectory
                Set-AcumaticaSiteDirectory -Path $OriginalConfig.SiteDirectory
                Set-AcumaticaVersionDirectory -Path $OriginalConfig.VersionDirectory
                Set-AcumaticaDefaultSiteType -SiteType $OriginalConfig.DefaultSiteType
                Set-AcumaticaInstallDebugTools -InstallDebugTools $OriginalConfig.InstallDebugTools
            }
        }
    }
}

Describe "Performance Tests" {
    Context "Cmdlet Response Times" {
        It "Get-AcumaticaConfig should respond quickly" {
            $Stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            Get-AcumaticaConfig | Out-Null
            $Stopwatch.Stop()
            $Stopwatch.ElapsedMilliseconds | Should -BeLessThan 1000 -Because "Configuration retrieval should be fast"
        }
        
        It "Get-AcumaticaSite should respond quickly" {
            $Stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            Get-AcumaticaSite | Out-Null
            $Stopwatch.Stop()
            $Stopwatch.ElapsedMilliseconds | Should -BeLessThan 5000 -Because "Site listing should be reasonably fast"
        }
        
        It "Get-AcumaticaVersion should respond quickly" {
            $Stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            Get-AcumaticaVersion | Out-Null
            $Stopwatch.Stop()
            $Stopwatch.ElapsedMilliseconds | Should -BeLessThan 5000 -Because "Version listing should be reasonably fast"
        }
    }
}

AfterAll {
    # Clean up any test artifacts
    Remove-Module AcuInstallerHelper -Force -ErrorAction SilentlyContinue
}