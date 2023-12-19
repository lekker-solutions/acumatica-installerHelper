function Get-ModuleBase{
    return Join-Path (Get-Module -Name 'AcuInstallerHelper').ModuleBase "AcuInstallerHelper_config.json"
}

# Get functions
function Get-AcuDir {
    $config = Get-Content -Path (Get-ModuleBase) | ConvertFrom-Json
    return $config.AcumaticaDir
}

function Get-AcuSiteDir {
    $config = Get-Content -Path (Get-ModuleBase) | ConvertFrom-Json
    return $config.AcumaticaSiteDir
}

function Get-AcuVersionDir {
    $config = Get-Content -Path (Get-ModuleBase)  | ConvertFrom-Json
    return $config.AcumaticaVersionDir
}

# Set functions
function Set-AcuDir {
    param(
        [Parameter(Mandatory=$true)]
        [string] $NewPath
    )
    $config = Get-Content -Path (Get-ModuleBase)  | ConvertFrom-Json
    $config.AcumaticaDir = $NewPath
    $config | ConvertTo-Json | Set-Content -Path (Get-ModuleBase) 
}

function Set-AcuSiteDir {
    param(
        [Parameter(Mandatory=$true)]
        [string] $NewPath
    )
    $config = Get-Content -Path (Get-ModuleBase)  | ConvertFrom-Json
    $config.AcumaticaSiteDir = $NewPath
    $config | ConvertTo-Json | Set-Content -Path (Get-ModuleBase) 
}

function Set-AcuVersionDir {
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
    $AcuDir = Get-AcuDir;
    $acuVersonDir = Get-AcuVersionDir
    $versionPath = (Join-Path $AcuDir (Join-Path $acuVersonDir $version))
    $exists = Test-Path $versionPath;
    return $exists
}

function Read-DefaultSiteInstallPath{
    $acuDir = Get-AcuDir
    $siteDir = Get-AcuSiteDir
    $fullPath = Join-Path $acuDir $siteDir
    return $fullPath
}

