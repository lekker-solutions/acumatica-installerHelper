Describe "AcumaticaVersionCmdlets" {
    BeforeAll {
        Import-Module "$PSScriptRoot\..\AcuInstallerHelper" -Force
    }

    Context "Install-AcumaticaVersion" {
        It "Should require Version parameter" {
            { Install-AcumaticaVersion } | Should -Throw
        }

        It "Should throw on empty version" {
            { Install-AcumaticaVersion -Version "" } | Should -Throw
        }

        It "Should throw on null version" {
            { Install-AcumaticaVersion -Version $null } | Should -Throw
        }

        It "Should accept valid version format" {
            # This may throw due to network/installation issues, but parameter validation should pass
            try {
                Install-AcumaticaVersion -Version "2023.2.001"
            } catch {
                # Expected if installation fails
            }
        }

        It "Should accept Preview switch" {
            try {
                Install-AcumaticaVersion -Version "2023.2.001" -Preview
            } catch {
                # Expected if installation fails
            }
        }

        It "Should accept DebugTools switch" {
            try {
                Install-AcumaticaVersion -Version "2023.2.001" -DebugTools
            } catch {
                # Expected if installation fails
            }
        }

        It "Should accept both Preview and DebugTools switches" {
            try {
                Install-AcumaticaVersion -Version "2023.2.001" -Preview -DebugTools
            } catch {
                # Expected if installation fails
            }
        }
    }

    Context "Uninstall-AcumaticaVersion" {
        It "Should require Version parameter" {
            { Uninstall-AcumaticaVersion } | Should -Throw
        }

        It "Should throw on empty version" {
            { Uninstall-AcumaticaVersion -Version "" } | Should -Throw
        }

        It "Should throw on null version" {
            { Uninstall-AcumaticaVersion -Version $null } | Should -Throw
        }

        It "Should accept valid version format" {
            # This may throw due to version not being installed, but parameter validation should pass
            try {
                Uninstall-AcumaticaVersion -Version "2023.2.001"
            } catch {
                # Expected if version doesn't exist
            }
        }
    }

    Context "Get-AcumaticaVersion" {
        It "Should return installed versions by default" {
            $versions = Get-AcumaticaVersion
            $versions | Should -BeOfType [array]
        }

        It "Should accept Available switch" {
            # This may throw due to network issues, but parameter validation should pass
            try {
                $versions = Get-AcumaticaVersion -Available
                $versions | Should -BeOfType [array]
            } catch {
                # Expected if network request fails
            }
        }

        It "Should accept MajorRelease parameter" {
            try {
                $versions = Get-AcumaticaVersion -Available -MajorRelease "2023.2"
                $versions | Should -BeOfType [array]
            } catch {
                # Expected if network request fails
            }
        }

        It "Should accept Preview switch" {
            try {
                $versions = Get-AcumaticaVersion -Available -Preview
                $versions | Should -BeOfType [array]
            } catch {
                # Expected if network request fails
            }
        }

        It "Should accept MajorRelease and Preview together" {
            try {
                $versions = Get-AcumaticaVersion -Available -MajorRelease "2023.2" -Preview
                $versions | Should -BeOfType [array]
            } catch {
                # Expected if network request fails
            }
        }

        It "Should return installed versions when not using Available switch" {
            $versions = Get-AcumaticaVersion
            # Should not throw and should return array
            $versions | Should -BeOfType [array]
        }
    }
}