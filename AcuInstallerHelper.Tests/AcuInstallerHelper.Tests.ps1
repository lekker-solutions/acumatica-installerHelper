function Initialize-Test(){
    $path = Join-Path $PSScriptRoot "\AcuInstallerHelper\"
    Import-Module "$($path)" -Verbose
}

Describe "Add-AcuSiteVersion"{
    BeforeAll {
        Initialize-Test
    }

    Context "Add 23R1 Site"{
        It "Should install an acumatica site at "{
            Invoke-ApiPackageUpload -u $username -p $password -url $url -pn $packageName -pp $packagePath -r
        }
    }
}

Describe "Remove-AcuSiteVersion"{
    BeforeAll {
        Initialize-Test
    }

    Context "Add 23R1 Site"{
        It "Should install an acumatica site at "{
            Invoke-ApiPackageUpload -u $username -p $password -url $url -pn $packageName -pp $packagePath -r
        }
    }
}

Describe "Add-AcuSite"{
    BeforeAll {
        Initialize-Test
    }

    Context "Add 23R1 Site"{
        It "Should install an acumatica site at "{
            Invoke-ApiPackageUpload -u $username -p $password -url $url -pn $packageName -pp $packagePath -r
        }
    }
}

Describe "Remove-AcuSite"{
    BeforeAll {
        Initialize-Test
    }

    Context "Add 23R1 Site"{
        It "Should install an acumatica site at "{
            Invoke-ApiPackageUpload -u $username -p $password -url $url -pn $packageName -pp $packagePath -r
        }
    }
}

Describe "Update-AcuSite"{
    BeforeAll {

    }

    Context "Add 23R1 Site"{
        It "Should install an acumatica site at "{
            Invoke-ApiPackageUpload -u $username -p $password -url $url -pn $packageName -pp $packagePath -r
        }
    }
}