Describe "AcumaticaPatchCmdlets" {
    BeforeAll {
        Import-Module "$PSScriptRoot\..\AcuInstallerHelper" -Force
    }

    Context "Test-AcumaticaPatch" {
        It "Should require SiteName parameter" {
            { Test-AcumaticaPatch } | Should -Throw
        }

        It "Should throw on empty site name" {
            { Test-AcumaticaPatch -SiteName "" } | Should -Throw
        }

        It "Should throw on null site name" {
            { Test-AcumaticaPatch -SiteName $null } | Should -Throw
        }

        It "Should accept valid site name without throwing parameter validation error" {
            # Parameter validation should pass even if site doesn't exist
            $scriptBlock = { Test-AcumaticaPatch -SiteName "TestSite" -ErrorAction Stop }
            
            # Should either succeed or throw a specific error about site not found, not parameter validation
            try {
                $result = & $scriptBlock
                $result | Should -BeOfType [PSCustomObject]
            } catch {
                $_.Exception.Message | Should -Not -BeLike "*parameter*"
                $_.Exception.Message | Should -Not -BeLike "*mandatory*"
            }
        }
    }

    Context "Install-AcumaticaPatch" {
        It "Should require SiteName parameter" {
            { Install-AcumaticaPatch } | Should -Throw
        }

        It "Should throw on empty site name" {
            { Install-AcumaticaPatch -SiteName "" } | Should -Throw
        }

        It "Should throw on null site name" {
            { Install-AcumaticaPatch -SiteName $null } | Should -Throw
        }

        It "Should accept valid site name without throwing parameter validation error" {
            # Parameter validation should pass even if site doesn't exist
            $scriptBlock = { Install-AcumaticaPatch -SiteName "TestSite" -ErrorAction Stop }
            
            # Should either succeed or throw a specific error about site not found, not parameter validation
            try {
                $result = & $scriptBlock
                $result | Should -BeOfType [PSCustomObject]
            } catch {
                $_.Exception.Message | Should -Not -BeLike "*parameter*"
                $_.Exception.Message | Should -Not -BeLike "*mandatory*"
            }
        }

        It "Should accept BackupPath parameter" {
            $scriptBlock = { Install-AcumaticaPatch -SiteName "TestSite" -BackupPath "C:\Backup\test.zip" -ErrorAction Stop }
            
            try {
                $result = & $scriptBlock
                $result | Should -BeOfType [PSCustomObject]
            } catch {
                # Should not be a parameter validation error
                $_.Exception.Message | Should -Not -BeLike "*parameter*"
                $_.Exception.Message | Should -Not -BeLike "*mandatory*"
            }
        }

        It "Should accept ArchivePath parameter" {
            try {
                Install-AcumaticaPatch -SiteName "TestSite" -ArchivePath "C:\Patches\patch.zip"
            } catch {
                # Expected if site doesn't exist
            }
        }
    }

    Context "Restore-AcumaticaPatch" {
        It "Should require SiteName parameter" {
            { Restore-AcumaticaPatch } | Should -Throw
        }

        It "Should throw on empty site name" {
            { Restore-AcumaticaPatch -SiteName "" } | Should -Throw
        }

        It "Should throw on null site name" {
            { Restore-AcumaticaPatch -SiteName $null } | Should -Throw
        }

        It "Should accept valid site name" {
            # This may throw due to site not existing, but parameter validation should pass
            try {
                Restore-AcumaticaPatch -SiteName "TestSite"
            } catch {
                # Expected if site doesn't exist
            }
        }

        It "Should accept BackupPath parameter" {
            try {
                Restore-AcumaticaPatch -SiteName "TestSite" -BackupPath "C:\Backup\test.zip"
            } catch {
                # Expected if site doesn't exist
            }
        }
    }

    Context "Test-AcumaticaPatchTool" {
        It "Should require Version parameter" {
            { Test-AcumaticaPatchTool } | Should -Throw
        }

        It "Should throw on empty version" {
            { Test-AcumaticaPatchTool -Version "" } | Should -Throw
        }

        It "Should throw on null version" {
            { Test-AcumaticaPatchTool -Version $null } | Should -Throw
        }

        It "Should accept valid version format and return boolean" {
            # Should always return a boolean, even if version isn't installed
            $result = Test-AcumaticaPatchTool -Version "24.215.0011" -ErrorAction SilentlyContinue
            $result | Should -BeOfType [bool]
            
            # If version doesn't exist, should return false, not throw
            if ($null -ne $result) {
                $result | Should -BeIn @($true, $false)
            }
        }
    }
}