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

function Get-AcumaticaERPVersionDir {
    $config = Get-Content -Path (Get-ModuleBase)  | ConvertFrom-Json
    return $config.AcumaticaERPVersionDir
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

function Set-AcumaticaERPVersionDir {
    param(
        [Parameter(Mandatory=$true)]
        [string] $NewPath
    )
    $config = Get-Content -Path (Get-ModuleBase)  | ConvertFrom-Json
    $config.AcumaticaERPVersionDir = $NewPath
    $config | ConvertTo-Json | Set-Content -Path (Get-ModuleBase) 
}
