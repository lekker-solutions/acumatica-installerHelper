function Get-ModuleBase{
    return Join-Path (Get-Module -Name 'AcuInstallerHelper').ModuleBase "AcuInstallerHelper_config.json"
}

# Get functions
function Get-AcuDir {
    $config = Get-Content -Path (Get-ModuleBase) | ConvertFrom-Json
    return $config.AcuDir
}

function Get-AcuSiteDir {
    $config = Get-Content -Path (Get-ModuleBase) | ConvertFrom-Json
    return $config.AcuSiteDir
}

function Get-AcuVersionDir {
    $config = Get-Content -Path (Get-ModuleBase)  | ConvertFrom-Json
    return $config.AcuVersionDir
}

# Set functions
function Set-AcuDir {
    param(
        [Parameter(Mandatory=$true)]
        [string] $NewPath
    )
    $config = Get-Content -Path (Get-ModuleBase)  | ConvertFrom-Json
    $config.AcuDir = $NewPath
    $config | ConvertTo-Json | Set-Content -Path (Get-ModuleBase) 
}

function Set-AcuSiteDir {
    param(
        [Parameter(Mandatory=$true)]
        [string] $NewPath
    )
    $config = Get-Content -Path (Get-ModuleBase)  | ConvertFrom-Json
    $config.AcuSiteDir = $NewPath
    $config | ConvertTo-Json | Set-Content -Path (Get-ModuleBase) 
}

function Set-AcuVersionDir {
    param(
        [Parameter(Mandatory=$true)]
        [string] $NewPath
    )
    $config = Get-Content -Path (Get-ModuleBase)  | ConvertFrom-Json
    $config.AcuVersionDir = $NewPath
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

