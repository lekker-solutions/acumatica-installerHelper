function Get-ModuleBase {
    return Join-Path (Get-Module -Name 'AcuInstallerHelper').ModuleBase "AcuInstallerHelper_config.json"
}

function Get-AcuConfig {
    $configPath = Get-ModuleBase
    Write-AcuDebug "Loading configuration from: $configPath"
    
    if (!(Test-Path $configPath)) {
        Write-AcuWarning "Configuration file not found, creating default configuration"
        # Create default config if it doesn't exist
        $defaultConfig = @{
            AcumaticaDir        = "C:\Acumatica"
            AcumaticaSiteDir    = "Sites"
            AcumaticaVersionDir = "Versions"
            InstallDebugTools   = $false
            SiteType            = "Production"
        }
        Save-AcuConfig -config $defaultConfig
        return $defaultConfig
    }
    
    try {
        $config = Get-Content -Path $configPath | ConvertFrom-Json
        Write-AcuDebug "Configuration loaded successfully"
        return $config
    }
    catch {
        Write-AcuError "Failed to load configuration file"
        throw "Failed to load configuration: $_"
    }
}

function Save-AcuConfig {
    param($config)
    
    $configPath = Get-ModuleBase
    Write-AcuDebug "Saving configuration to: $configPath"
    
    try {
        $config | ConvertTo-Json | Set-Content -Path $configPath
        Write-AcuDebug "Configuration saved successfully"
    }
    catch {
        Write-AcuError "Failed to save configuration"
        throw "Failed to save configuration: $_"
    }
}

# Get functions
function Get-AcuDir {
    $value = (Get-AcuConfig).AcumaticaDir
    Write-AcuDebug "Acumatica directory: $value"
    return $value
}

function Get-AcuSiteDir {
    $value = (Get-AcuConfig).AcumaticaSiteDir
    Write-AcuDebug "Site directory: $value"
    return $value
}

function Get-AcuVersionDir {
    $value = (Get-AcuConfig).AcumaticaVersionDir
    Write-AcuDebug "Version directory: $value"
    return $value
}

function Get-InstallDebugTools {
    $config = Get-AcuConfig
    $value = if ($null -eq $config.InstallDebugTools) { $false } else { $config.InstallDebugTools }
    Write-AcuDebug "Install debug tools setting: $value"
    return $value
}

function Get-SiteType {
    $config = Get-AcuConfig
    $value = if ($null -eq $config.SiteType) { "Production" } else { $config.SiteType }
    Write-AcuDebug "Site type setting: $value"
    return $value
}

# Set functions
function Set-AcuDir {
    param(
        [Parameter(Mandatory = $true)]
        [string] $NewPath
    )
    
    Write-AcuHeader -Title "Update Acumatica Directory" -Subtitle "New Path: $NewPath"
    
    Write-AcuSection -Title "Validating Path"
    
    if (!(Test-Path $NewPath)) {
        Write-AcuWarning "Path does not exist, creating directory"
        try {
            New-Item -ItemType Directory -Path $NewPath -Force | Out-Null
            Write-AcuSuccess "Directory created successfully"
        }
        catch {
            Write-AcuError "Failed to create directory: $_"
            throw
        }
    }
    else {
        Write-AcuSuccess "Path exists and is accessible"
    }
    
    Write-AcuSection -Title "Updating Configuration"
    
    $config = Get-AcuConfig
    $oldPath = $config.AcumaticaDir
    $config.AcumaticaDir = $NewPath
    Save-AcuConfig $config
    
    Write-AcuSummary -Operation "Directory Update" -Status "Completed Successfully" -Details @{
        "Setting"       = "Acumatica Directory"
        "Previous Path" = $oldPath
        "New Path"      = $NewPath
    }
}

function Set-AcuSiteDir {
    param(
        [Parameter(Mandatory = $true)]
        [string] $NewPath
    )
    
    Write-AcuHeader -Title "Update Site Directory" -Subtitle "New Path: $NewPath"
    
    Write-AcuSection -Title "Updating Configuration"
    
    $config = Get-AcuConfig
    $oldPath = $config.AcumaticaSiteDir
    $config.AcumaticaSiteDir = $NewPath
    Save-AcuConfig $config
    
    Write-AcuSummary -Operation "Directory Update" -Status "Completed Successfully" -Details @{
        "Setting"       = "Site Directory"
        "Previous Path" = $oldPath
        "New Path"      = $NewPath
    }
}

function Set-AcuVersionDir {
    param(
        [Parameter(Mandatory = $true)]
        [string] $NewPath
    )
    
    Write-AcuHeader -Title "Update Version Directory" -Subtitle "New Path: $NewPath"
    
    Write-AcuSection -Title "Updating Configuration"
    
    $config = Get-AcuConfig
    $oldPath = $config.AcumaticaVersionDir
    $config.AcumaticaVersionDir = $NewPath
    Save-AcuConfig $config
    
    Write-AcuSummary -Operation "Directory Update" -Status "Completed Successfully" -Details @{
        "Setting"       = "Version Directory"
        "Previous Path" = $oldPath
        "New Path"      = $NewPath
    }
}

function Set-InstallDebugTools {
    param(
        [Parameter(Mandatory = $true)]
        [bool] $Value
    )
    
    Write-AcuHeader -Title "Update Debug Tools Setting" -Subtitle "Value: $(if ($Value) { 'Enabled' } else { 'Disabled' })"
    
    Write-AcuSection -Title "Updating Configuration"
    
    $config = Get-AcuConfig
    $oldValue = Get-InstallDebugTools
    $config.InstallDebugTools = $Value
    Save-AcuConfig $config
    
    Write-AcuSummary -Operation "Setting Update" -Status "Completed Successfully" -Details @{
        "Setting"        = "Install Debug Tools"
        "Previous Value" = $(if ($oldValue) { 'Enabled' } else { 'Disabled' })
        "New Value"      = $(if ($Value) { 'Enabled' } else { 'Disabled' })
    }
}

function Set-SiteType {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet("Dev", "Production")]
        [string] $Value
    )
    
    Write-AcuHeader -Title "Update Site Type Setting" -Subtitle "Value: $Value"
    
    Write-AcuSection -Title "Updating Configuration"
    
    $config = Get-AcuConfig
    $oldValue = Get-SiteType
    $config.SiteType = $Value
    Save-AcuConfig $config
    
    Write-AcuSummary -Operation "Setting Update" -Status "Completed Successfully" -Details @{
        "Setting"        = "Default Site Type"
        "Previous Value" = $oldValue
        "New Value"      = $Value
    }
}

function Read-AcuVersionPath {
    param (
        [string]$version
    )
    
    Write-AcuDebug "Checking if version exists: $version"
    $versionPath = Get-AcuVersionPath -versionNbr $version
    $exists = Test-Path $versionPath
    Write-AcuDebug "Version exists: $exists"
    return $exists
}

function Read-DefaultSiteInstallPath {
    $path = Join-Path (Get-AcuDir) (Get-AcuSiteDir)
    Write-AcuDebug "Default site install path: $path"
    return $path
}