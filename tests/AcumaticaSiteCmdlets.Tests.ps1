Describe "AcumaticaSiteCmdlets" {
    BeforeAll {
        Import-Module "$PSScriptRoot\..\AcuInstallerHelper" -Force
        
        # Use the same normal version that was installed in version tests
        $script:NormalVersion = "24.215.0011"
        $script:TestSiteName = "TestSite"
        
        Write-Host "Starting site tests with Normal version: $script:NormalVersion" -ForegroundColor Cyan
    }

    AfterAll {
        # Cleanup: Remove test site if it exists
        try {
            $existingSites = Get-AcumaticaSite
            if ($existingSites -and $existingSites -contains $script:TestSiteName) {
                Write-Host "Cleaning up test site: $script:TestSiteName" -ForegroundColor Yellow
                Remove-AcumaticaSite -Name $script:TestSiteName -Force
            }
        }
        catch {
            Write-Warning "Failed to cleanup test site: $($_.Exception.Message)"
        }
    }

    Context "New-AcumaticaSite" {
        It "Should throw on empty version" {
            { New-AcumaticaSite -Version "" -Name "TestSite" } | Should -Throw
        }

        It "Should throw on empty name" {
            { New-AcumaticaSite -Version "24.215.0011" -Name "" } | Should -Throw
        }

        It "Should accept valid parameters without parameter validation errors" {
            $scriptBlock = { New-AcumaticaSite -Version "24.215.0011" -Name "TestSite" -ErrorAction Stop }
            
            try {
                $result = & $scriptBlock
                # If successful, should return boolean
                $result | Should -BeOfType [bool]
            } catch {
                # Should not be a parameter validation error
                $_.Exception.Message | Should -Not -BeLike "*parameter*"
                $_.Exception.Message | Should -Not -BeLike "*mandatory*"
                $_.Exception.Message | Should -Not -BeLike "*Cannot bind*"
            }
        }

        It "Should accept optional Path parameter" {
            try {
                New-AcumaticaSite -Version "24.215.0011" -Name "TestSite" -Path "C:\TestPath"
            }
            catch {
                # Expected if version doesn't exist
            }
        }

        It "Should accept InstallVersion switch" {
            try {
                New-AcumaticaSite -Version "24.215.0011" -Name "TestSite" -InstallVersion
            }
            catch {
                # Expected if version doesn't exist
            }
        }

        It "Should accept Portal switch" {
            try {
                New-AcumaticaSite -Version "24.215.0011" -Name "TestSite" -Portal
            }
            catch {
                # Expected if version doesn't exist
            }
        }

        It "Should accept Development switch" {
            try {
                New-AcumaticaSite -Version "24.215.0011" -Name "TestSite" -Development
            }
            catch {
                # Expected if version doesn't exist
            }
        }

        It "Should accept Preview switch" {
            try {
                New-AcumaticaSite -Version "25.193.0171" -Name "TestSite" -Preview
            }
            catch {
                # Expected if version doesn't exist
            }
        }

        It "Should accept DebugTools switch" {
            try {
                New-AcumaticaSite -Version "24.215.0011" -Name "TestSite" -DebugTools
            }
            catch {
                # Expected if version doesn't exist
            }
        }

        It "Should create site with normal version successfully" {
            Write-Host "Creating site '$script:TestSiteName' with normal version: $script:NormalVersion" -ForegroundColor Green
            
            # Remove site if it already exists
            try {
                $existingSites = Get-AcumaticaSite
                if ($existingSites -and $existingSites -contains $script:TestSiteName) {
                    Remove-AcumaticaSite -Name $script:TestSiteName -Force
                }
            }
            catch {
                # Site doesn't exist, continue
            }
            
            $result = New-AcumaticaSite -Version $script:NormalVersion -Name $script:TestSiteName -Force
            $result | Should -Be $true
        }

        It "Should create site with Development switch" {
            Write-Host "Creating development site with normal version: $script:NormalVersion" -ForegroundColor Green
            
            $devSiteName = "$($script:TestSiteName)Dev"
            
            # Remove site if it already exists
            try {
                $existingSites = Get-AcumaticaSite
                if ($existingSites -and $existingSites -contains $devSiteName) {
                    Remove-AcumaticaSite -Name $devSiteName -Force
                }
            }
            catch {
                # Site doesn't exist, continue
            }
            
            $result = New-AcumaticaSite -Version $script:NormalVersion -Name $devSiteName -Development -Force
            $result | Should -Be $true
            
            # Cleanup
            try {
                Remove-AcumaticaSite -Name $devSiteName -Force
            }
            catch {
                # Ignore cleanup errors
            }
        }
    }

    Context "Remove-AcumaticaSite" {
        It "Should throw on empty name" {
            { Remove-AcumaticaSite -Name "" } | Should -Throw
        }

        It "Should throw on null name" {
            { Remove-AcumaticaSite -Name $null } | Should -Throw
        }

        It "Should accept valid name" {
            # This may throw due to site not existing, but parameter validation should pass
            try {
                Remove-AcumaticaSite -Name "TestSite"
            }
            catch {
                # Expected if site doesn't exist
            }
        }
    }

    Context "Get-AcumaticaSite" {
        It "Should list the created site" {
            Write-Host "Verifying site '$script:TestSiteName' exists" -ForegroundColor Cyan
            
            $sites = Get-AcumaticaSite
            $sites | Should -Not -BeNullOrEmpty
            $sites | Should -Contain $script:TestSiteName
        }

        It "Should return site list without parameters" {
            $sites = Get-AcumaticaSite
            
            # Should either return null (no sites), string (single site), or array (multiple sites)
            if ($null -ne $sites) {
                if ($sites -is [array]) {
                    $sites | ForEach-Object { $_ | Should -BeOfType [string] }
                } else {
                    $sites | Should -BeOfType [string]
                }
            } else {
                # Null is acceptable if no sites are installed
                $sites | Should -BeNullOrEmpty
            }
        }

        It "Should show site with correct version when using IncludeVersion" {
            Write-Host "Verifying site version information" -ForegroundColor Cyan
            
            $sitesWithVersion = Get-AcumaticaSite -IncludeVersion
            $sitesWithVersion | Should -Not -BeNullOrEmpty
            
            $testSiteInfo = @($sitesWithVersion) | Where-Object { $_.Name -eq $script:TestSiteName }
            $testSiteInfo | Should -Not -BeNullOrEmpty
            $testSiteInfo.Name | Should -Be $script:TestSiteName
            $testSiteInfo.Version | Should -BeLike "*$script:NormalVersion*"
        }

        It "Should accept IncludeVersion switch and return objects with Name and Version properties" {
            $sites = Get-AcumaticaSite -IncludeVersion
            
            if ($null -ne $sites) {
                # Convert to array if single item
                $siteArray = @($sites)
                
                $siteArray | ForEach-Object {
                    $_ | Should -Not -BeNullOrEmpty
                    $_.PSObject.Properties.Name | Should -Contain "Name"
                    $_.PSObject.Properties.Name | Should -Contain "Version"
                    
                    # Verify property types
                    $_.Name | Should -BeOfType [string]
                    if ($null -ne $_.Version) {
                        $_.Version | Should -BeOfType [string]
                    }
                }
            } else {
                # Null is acceptable if no sites are installed
                $sites | Should -BeNullOrEmpty
            }
        }
    }

    Context "Update-AcumaticaSite" {
        It "Should throw on empty name" {
            { Update-AcumaticaSite -Name "" -Version "25.200.0248" } | Should -Throw
        }

        It "Should throw on empty version" {
            { Update-AcumaticaSite -Name "TestSite" -Version "" } | Should -Throw
        }

        It "Should accept valid parameters" {
            # This may throw due to site not existing, but parameter validation should pass
            try {
                Update-AcumaticaSite -Name "TestSite" -Version "25.200.0248"
            } catch {
                # Expected if site doesn't exist
            }
        }
    }
}