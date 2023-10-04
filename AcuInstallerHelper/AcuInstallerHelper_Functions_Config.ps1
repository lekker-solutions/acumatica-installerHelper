function Get-ModuleBase{
    return Join-Path (Get-Module -Name 'AcuInstallerHelper').ModuleBase "AcuInstallerHelper_config.json"
}

# Get functions
function Get-AcumaticaDir {
    $config = Get-Content -Path (Get-ModuleBase) | ConvertFrom-Json
    return $config.AcumaticaDir
}

function Get-AcumaticaSiteDir {
    $config = Get-Content -Path (Get-ModuleBase) | ConvertFrom-Json
    return $config.AcumaticaSiteDir
}

function Get-AcumaticaVersionDir {
    $config = Get-Content -Path (Get-ModuleBase)  | ConvertFrom-Json
    return $config.AcumaticaVersionDir
}

# Set functions
function Set-AcumaticaDir {
    param(
        [Parameter(Mandatory=$true)]
        [string] $NewPath
    )
    $config = Get-Content -Path (Get-ModuleBase)  | ConvertFrom-Json
    $config.AcumaticaDir = $NewPath
    $config | ConvertTo-Json | Set-Content -Path (Get-ModuleBase) 
}

function Set-AcumaticaSiteDir {
    param(
        [Parameter(Mandatory=$true)]
        [string] $NewPath
    )
    $config = Get-Content -Path (Get-ModuleBase)  | ConvertFrom-Json
    $config.AcumaticaSiteDir = $NewPath
    $config | ConvertTo-Json | Set-Content -Path (Get-ModuleBase) 
}

function Set-AcumaticaVersionDir {
    param(
        [Parameter(Mandatory=$true)]
        [string] $NewPath
    )
    $config = Get-Content -Path (Get-ModuleBase)  | ConvertFrom-Json
    $config.AcumaticaVersionDir = $NewPath
    $config | ConvertTo-Json | Set-Content -Path (Get-ModuleBase) 
}

function Read-AcuVersionPath {
    param (
        [string]$version
    )
    return Test-Path (Join-Path Get-AcumaticaDir Get-AcumaticaVersionDir $version)
}

function Read-DefaultSiteInstallPath{
    $acuDir = Get-AcumaticaDir
    $siteDir = Get-AcumaticaSiteDir
    $fullPath = Join-Path $acuDir $siteDir
    return $fullPath
}

