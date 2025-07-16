. (Join-Path $PSScriptRoot 'AcuInstallerHelper_Logging.ps1')
. (Join-Path $PSScriptRoot 'AcuInstallerHelper_Functions_Config.ps1')
. (Join-Path $PSScriptRoot 'AcuInstallerHelper_Functions.ps1')
. (Join-Path $PSScriptRoot 'AcuInstallerHelper_Versions.ps1')
. (Join-Path $PSScriptRoot 'AcuInstallerHelper_Nuget.ps1')
. (Join-Path $PSScriptRoot 'AcuInstallerHelper_web_config.ps1')

function Add-AcuSite {
    param (
        [Parameter(Mandatory = $true)]
        [Alias("v")]
        [string] $version,
        
        [Parameter(Mandatory = $true)]
        [Alias("n")]
        [string] $siteName,
        
        [Alias("p")]
        [string] $siteInstallPath,
        
        [Alias("nv")]
        [switch] $installNewVersion,
        
        [Alias("pt")]
        [switch] $portal,
        
        [Alias("d")]
        [switch] $devSite,
        
        [switch] $preview,
        
        [Alias("dt")]
        [switch] $debuggerTools
    )

    Write-AcuHeader -Title "Acumatica Site Installation" -Subtitle "Version $version • Site: $siteName"
    
    Write-AcuSection -Title "Validating Prerequisites"
    
    Test-VersionFormat -version $version
    Write-AcuSuccess "Version format validated: $version"
    
    # Check if version exists
    if (!(Read-AcuVersionPath $version)) {
        Write-AcuWarning "Version $version not found locally"
        
        # Determine if we should install
        $shouldInstall = $installNewVersion -or (PromptYesNo "You do not have version $version installed, do you want to install?")
        
        if (!$shouldInstall) {
            Write-AcuError "Site installation cancelled - version not available"
            return
        }
        
        Write-AcuSection -Title "Installing Required Version"
        
        # Use config default if debuggerTools not explicitly set
        $useDebugTools = if ($PSBoundParameters.ContainsKey('debuggerTools')) { 
            $debuggerTools 
        }
        else { 
            Get-InstallDebugTools 
        }
        
        Add-AcuVersion -version:$version -preview:$preview -debuggerTools:$useDebugTools
    }
    else {
        Write-AcuSuccess "Version $version found at $(Get-AcuVersionPath -versionNbr $version)"
    }

    Write-AcuSection -Title "Configuring Site Parameters"
    
    # Set default site path if not provided
    if ([string]::IsNullOrWhiteSpace($siteInstallPath)) {
        $siteInstallPath = Join-Path (Read-DefaultSiteInstallPath) $siteName
        Write-AcuInfo "Using default site path: $siteInstallPath"
    }
    else {
        Write-AcuInfo "Using specified site path: $siteInstallPath"
    }

    # Apply dev configuration based on explicit parameter or config default
    $isDev = if ($PSBoundParameters.ContainsKey('devSite')) {
        $devSite
    }
    else {
        (Get-SiteType) -eq "Dev"
    }
    
    Write-AcuTable -Data @{
        "Site Name"     = $siteName
        "Version"       = $version
        "Install Path"  = $siteInstallPath
        "Portal Site"   = $(if ($portal) { "Yes" } else { "No" })
        "Site Type"     = $(if ($isDev) { "Development" } else { "Production" })
        "Preview Build" = $(if ($preview) { "Yes" } else { "No" })
    } -Title "Site Configuration"

    Write-AcuSection -Title "Installing Site"
    
    # Install site
    $acuArgs = Build-AcuExeArgs -siteName $siteName -sitePath $siteInstallPath -portal $portal -newInstance
    Invoke-AcuExe -arguments $acuArgs -version $version
    
    if ($isDev) {
        Write-AcuStep "Applying development configuration"
        Update-WebConfigForDev -webConfigPath (Join-Path $siteInstallPath "web.config")
    }
    
    Write-AcuSummary -Operation "Site Installation" -Status "Completed Successfully" -Details @{
        "Site Name" = $siteName
        "Version"   = $version
        "Path"      = $siteInstallPath
        "Type"      = $(if ($isDev) { "Development" } else { "Production" })
        "Portal"    = $(if ($portal) { "Yes" } else { "No" })
    }
}

function Remove-AcuSite {
    param (
        [Parameter(Mandatory = $true)]
        [Alias("n")]
        [string] $siteName
    )
    
    Write-AcuHeader -Title "Acumatica Site Removal" -Subtitle "Site: $siteName"
    
    Write-AcuSection -Title "Removing Site Registration"
    
    try {
        $version = Get-AcuSiteVersion -siteName $siteName
        Write-AcuInfo "Found site using version: $version"
        
        $acuArgs = Build-AcuExeArgs -siteName $siteName -d
        Invoke-AcuExe -arguments $acuArgs -version $version
        
        Write-AcuSummary -Operation "Site Removal" -Status "Completed Successfully" -Details @{
            "Site Name" = $siteName
            "Version"   = $version
        }
    }
    catch {
        Write-AcuError "Failed to remove site: $_"
        throw
    }
}

function Update-AcuSite {
    param (
        [Parameter(Mandatory = $true)]
        [string] $siteName,
        
        [Parameter(Mandatory = $true)]
        [string] $newVersion
    )

    Write-AcuHeader -Title "Acumatica Site Update" -Subtitle "Site: $siteName → Version: $newVersion"
    
    Test-VersionFormat -version $newVersion
    
    # TODO: Implement site upgrade logic
    Write-AcuWarning "Update-AcuSite is not yet implemented"
    Write-AcuInfo "This feature will be available in a future version"
}