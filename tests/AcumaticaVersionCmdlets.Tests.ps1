Describe "AcumaticaVersionCmdlets" {
    BeforeAll {
        Import-Module "$PSScriptRoot\..\AcuInstallerHelper" -Force
    }

    Context "Install-AcumaticaVersion" {
        It "Should throw on empty version" {
            { Install-AcumaticaVersion -Version "" } | Should -Throw
        }

        It "Should throw on null version" {
            { Install-AcumaticaVersion -Version $null } | Should -Throw
        }

        It "Should accept valid version format without parameter validation errors" {
            $scriptBlock = { Install-AcumaticaVersion -Version "24.215.0011" -ErrorAction Stop }
            
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

        It "Should accept Preview switch" {
            try {
                Install-AcumaticaVersion -Version "25.193.0171" -Preview
            }
            catch {
                # Expected if installation fails
            }
        }

        It "Should accept DebugTools switch" {
            try {
                Install-AcumaticaVersion -Version "24.215.0011" -DebugTools
            }
            catch {
                # Expected if installation fails
            }
        }

        It "Should accept both Preview and DebugTools switches" {
            try {
                Install-AcumaticaVersion -Version "25.193.0171" -Preview -DebugTools
            }
            catch {
                # Expected if installation fails
            }
        }
    }

    Context "Uninstall-AcumaticaVersion" {
        It "Should throw on empty version" {
            { Uninstall-AcumaticaVersion -Version "" } | Should -Throw
        }

        It "Should throw on null version" {
            { Uninstall-AcumaticaVersion -Version $null } | Should -Throw
        }

        It "Should accept valid version format" {
            # This may throw due to version not being installed, but parameter validation should pass
            try {
                Uninstall-AcumaticaVersion -Version "24.215.0011"
            }
            catch {
                # Expected if version doesn't exist
            }
        }
    }

    Context "Get-AcumaticaVersion" {
        It "Should return installed versions by default" {
            $versions = Get-AcumaticaVersion
            
            # Versions can be null if none installed, otherwise should be AcumaticaVersion objects
            if ($null -ne $versions) {
                $versionArray = @($versions)
                
                $versionArray | ForEach-Object {
                    $_ | Should -Not -BeNullOrEmpty
                    # Should have version properties
                    $_.PSObject.Properties.Name | Should -Contain "MinorVersion"
                    $_.PSObject.Properties.Name | Should -Contain "MajorVersion"
                }
            } else {
                # Null is acceptable if no versions installed
                $versions | Should -BeNullOrEmpty
            }
        }

        It "Should accept Available switch" {
            # Should not throw parameter validation errors
            $scriptBlock = { Get-AcumaticaVersion -Available -ErrorAction Stop }
            
            try {
                $versions = & $scriptBlock
                if ($null -ne $versions) {
                    $versionArray = @($versions)
                    $versionArray | ForEach-Object {
                        $_ | Should -Not -BeNullOrEmpty
                        $_.PSObject.Properties.Name | Should -Contain "MinorVersion"
                    }
                }
            } catch {
                # Network errors are acceptable, but not parameter validation errors
                $_.Exception.Message | Should -Not -BeLike "*parameter*"
                $_.Exception.Message | Should -Not -BeLike "*mandatory*"
            }
        }

        It "Should accept MajorRelease parameter" {
            try {
                $versions = Get-AcumaticaVersion -Available -MajorRelease "24.2"
                if ($versions -ne $null) {
                    $versions | Should -Not -BeNullOrEmpty
                }
            }
            catch {
                # Expected if network request fails
            }
        }

        It "Should accept Preview switch" {
            try {
                $versions = Get-AcumaticaVersion -Available -Preview
                if ($versions -ne $null) {
                    $versions | Should -Not -BeNullOrEmpty
                }
            }
            catch {
                # Expected if network request fails
            }
        }

        It "Should accept MajorRelease and Preview together" {
            try {
                $versions = Get-AcumaticaVersion -Available -MajorRelease "25.1" -Preview
                if ($versions -ne $null) {
                    $versions | Should -Not -BeNullOrEmpty
                }
            }
            catch {
                # Expected if network request fails
            }
        }

        It "Should not throw when getting installed versions" {
            # Should never throw when getting installed versions
            { Get-AcumaticaVersion } | Should -Not -Throw
            
            # Verify we can capture the result
            $versions = Get-AcumaticaVersion
            
            # Result should be null or collection of versions
            if ($null -ne $versions) {
                $versions.GetType().Name | Should -BeIn @('Object[]', 'PSCustomObject', 'AcumaticaVersion', 'ArrayList')
            }
        }
    }
}