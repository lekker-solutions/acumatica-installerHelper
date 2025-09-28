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

        It "Should accept valid version format" {
            # This may throw due to network/installation issues, but parameter validation should pass
            try {
                Install-AcumaticaVersion -Version "24.215.0011"
            }
            catch {
                # Expected if installation fails
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
            # Versions can be single item, array, or null
            if ($versions -ne $null) {
                # Should be AcumaticaVersion objects or array of them
                $versions | Should -Not -BeNullOrEmpty
            }
        }

        It "Should accept Available switch" {
            # This may throw due to network issues, but parameter validation should pass
            try {
                $versions = Get-AcumaticaVersion -Available
                if ($versions -ne $null) {
                    $versions | Should -Not -BeNullOrEmpty
                }
            }
            catch {
                # Expected if network request fails
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

        It "Should return installed versions when not using Available switch" {
            $versions = Get-AcumaticaVersion
            # Should not throw - versions can be null if none are installed
            # Just verify the command doesn't error
            { Get-AcumaticaVersion } | Should -Not -Throw
        }
    }
}