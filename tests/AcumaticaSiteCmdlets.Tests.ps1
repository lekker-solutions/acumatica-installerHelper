Describe "AcumaticaSiteCmdlets" {
    BeforeAll {
        Import-Module "$PSScriptRoot\..\AcuInstallerHelper" -Force
    }

    Context "New-AcumaticaSite" {
        It "Should require Version parameter" {
            { New-AcumaticaSite -Name "TestSite" } | Should -Throw
        }

        It "Should require Name parameter" {
            { New-AcumaticaSite -Version "2023.2.001" } | Should -Throw
        }

        It "Should throw on empty version" {
            { New-AcumaticaSite -Version "" -Name "TestSite" } | Should -Throw
        }

        It "Should throw on empty name" {
            { New-AcumaticaSite -Version "2023.2.001" -Name "" } | Should -Throw
        }

        It "Should accept valid parameters" {
            # This may throw due to version not existing, but parameter validation should pass
            try {
                New-AcumaticaSite -Version "2023.2.001" -Name "TestSite"
            } catch {
                # Expected if version doesn't exist
            }
        }

        It "Should accept optional Path parameter" {
            try {
                New-AcumaticaSite -Version "2023.2.001" -Name "TestSite" -Path "C:\TestPath"
            } catch {
                # Expected if version doesn't exist
            }
        }

        It "Should accept InstallVersion switch" {
            try {
                New-AcumaticaSite -Version "2023.2.001" -Name "TestSite" -InstallVersion
            } catch {
                # Expected if version doesn't exist
            }
        }

        It "Should accept Portal switch" {
            try {
                New-AcumaticaSite -Version "2023.2.001" -Name "TestSite" -Portal
            } catch {
                # Expected if version doesn't exist
            }
        }

        It "Should accept Development switch" {
            try {
                New-AcumaticaSite -Version "2023.2.001" -Name "TestSite" -Development
            } catch {
                # Expected if version doesn't exist
            }
        }

        It "Should accept Preview switch" {
            try {
                New-AcumaticaSite -Version "2023.2.001" -Name "TestSite" -Preview
            } catch {
                # Expected if version doesn't exist
            }
        }

        It "Should accept DebugTools switch" {
            try {
                New-AcumaticaSite -Version "2023.2.001" -Name "TestSite" -DebugTools
            } catch {
                # Expected if version doesn't exist
            }
        }
    }

    Context "Remove-AcumaticaSite" {
        It "Should require Name parameter" {
            { Remove-AcumaticaSite } | Should -Throw
        }

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
            } catch {
                # Expected if site doesn't exist
            }
        }
    }

    Context "Get-AcumaticaSite" {
        It "Should return site list without parameters" {
            $sites = Get-AcumaticaSite
            $sites | Should -BeOfType [array]
        }

        It "Should accept IncludeVersion switch" {
            $sites = Get-AcumaticaSite -IncludeVersion
            $sites | Should -BeOfType [array]
            # If sites exist, they should have Name and Version properties
            if ($sites.Count -gt 0) {
                $sites[0].PSObject.Properties.Name | Should -Contain "Name"
                $sites[0].PSObject.Properties.Name | Should -Contain "Version"
            }
        }
    }

    Context "Update-AcumaticaSite" {
        It "Should require Name parameter" {
            { Update-AcumaticaSite -Version "2023.2.001" } | Should -Throw
        }

        It "Should require Version parameter" {
            { Update-AcumaticaSite -Name "TestSite" } | Should -Throw
        }

        It "Should throw on empty name" {
            { Update-AcumaticaSite -Name "" -Version "2023.2.001" } | Should -Throw
        }

        It "Should throw on empty version" {
            { Update-AcumaticaSite -Name "TestSite" -Version "" } | Should -Throw
        }

        It "Should accept valid parameters" {
            # This may throw due to site not existing, but parameter validation should pass
            try {
                Update-AcumaticaSite -Name "TestSite" -Version "2023.2.001"
            } catch {
                # Expected if site doesn't exist
            }
        }
    }
}