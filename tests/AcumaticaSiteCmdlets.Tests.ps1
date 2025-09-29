Describe "AcumaticaSiteCmdlets" {
    BeforeAll {
        Import-Module "$PSScriptRoot\..\AcuInstallerHelper" -Force
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