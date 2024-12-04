. (Join-Path $PSScriptRoot 'AcuInstallerHelper_Functions_Config.ps1')
. (Join-Path $PSScriptRoot 'AcuInstallerHelper_Functions.ps1')
. (Join-Path $PSScriptRoot 'AcuInstallerHelper_Versions.ps1')
. (Join-Path $PSScriptRoot 'AcuInstallerHelper_Nuget.ps1')
. (Join-Path $PSScriptRoot 'AcuInstallerHelper_web_config.ps1')

function Add-AcuSite {
    param (
        [string] [Parameter(Mandatory = $true)] [Alias("v")] $version,
        [string] [Parameter(Mandatory = $true)] [Alias("n")] $siteName,
        [string] [Alias("p")] $siteInstallPath,
        [switch] [Alias("nv")] $installNewVersion,
        [switch] [Alias("pt")] $portal,
        [switch] [Alias("d")] $devSite,
        [switch] $preview,
        [switch] [Alias("dt")] $debuggerTools
    )

    Test-VersionFormat -version $version
    $versionExists = Read-AcuVersionPath $version
    if ($versionExists -eq $false) {
        # We need to install a new version
        if (!$installNewVersion) {
            # Ask for new version install
            $installResponse = PromptYesNo "You do not have version $($version) installed, do you want to install?"
        }
        else {
            $installResponse = $installNewVersion
        }
 
        if (!$installResponse) {
            # Cancel entire run
            Write-Output "Site install cancelled"
            return;
        } 

        # Install the new version
        Add-AcuVersion -debuggerTools:$debuggerTools -version:$version -preview:$preview
    }

    if ([string]::IsNullOrWhiteSpace($siteInstallPath)) {
        $siteInstallPath = Join-Path (Read-DefaultSiteInstallPath) $siteName
    }

    #$acuArgs = Build-AcuExeArgs -siteName $siteName -sitePath $siteInstallPath -portal $portal -newInstance
    #Invoke-AcuExe -arguments $acuArgs -version $version
    if($devSite) {
        Update-WebConfigForDev -webConfigPath (Join-Path $siteInstallPath "web.config")
    }
    Write-Output "Site Installed"
}

function Remove-AcuSite {
    param (
        [string] [Alias("n")] $siteName
    )
    
    Write-Output "Removing Acu Registry for Site $($siteName)"
    $version = Get-AcuSiteVersion -siteName $siteName
    $acuArgs = Build-AcuExeArgs -siteName $siteName -d

    Invoke-AcuExe -arguments $acuArgs -version $version
}

function Update-AcuSite {
    param (
        [string] $siteName,
        [string] $newVersion
    )

    Test-VersionFormat -version $newVersion
}