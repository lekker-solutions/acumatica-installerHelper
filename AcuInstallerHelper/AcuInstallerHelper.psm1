. (Join-Path $PSScriptRoot 'AcuInstallerHelper_Functions_Config.ps1')
. (Join-Path $PSScriptRoot 'AcuInstallerHelper_Functions.ps1')
. (Join-Path $PSScriptRoot 'AcuInstallerHelper_Versions.ps1')

function Add-AcuSite{
    param (
        [string] [Parameter(Mandatory=$true)] [Alias("v")] $version,
        [string] [Parameter(Mandatory=$true)] [Alias("n")] $siteName,
        [string] [Alias("p")] $siteInstallPath,
        [switch] [Alias("nv")] $installNewVersion,
        [switch] [Alias("pt")] $portal,
        [bool] [Alias("dt")] $debuggerTools
    )

    Test-VersionFormat -version $version
    $versionExists = Read-AcuVersionPath $version
    if ($versionExists -eq $false){
        # We need to install a new version
        if (!$installNewVersion){
            # Ask for new version install
            $installResponse = PromptYesNo "You do not have version $($version) installed, do you want to install?"
        }
        else {
            $installResponse = $installNewVersion
        }
 
        if (!$installResponse){
            # Cancel entire run
            Write-Output "Site install cancelled"
            return;
        } 

        if (!$debuggerTools){
            # Prompt for debug tools because it was not set
            $debuggerTools = PromptYesNo "Do you want to install debugger tools?"
        }

        # Install the new version
        Add-AcuVersion -debuggerTools $debuggerTools -version $version
    }

    if ([string]::IsNullOrWhiteSpace($siteInstallPath)){
        $siteInstallPath = Join-Path (Read-DefaultSiteInstallPath) $siteName
    }

    $acuArgs = Build-AcuExeArgs -siteName $siteName -sitePath $siteInstallPath -portal $portal -newInstance
    Invoke-AcuExe -arguments $acuArgs -version $version
    Write-Output "Site Installed"
}

function Remove-AcuSite{
    param (
        [string] [Alias("n")] $siteName
    )
    
    Write-Output "Removing Acumatica Registry for Site $($siteName)"
    $version = Get-AcuSiteVersion -siteName $siteName
    $acuArgs = Build-AcuExeArgs -siteName $siteName -d

    Invoke-AcuExe -arguments $acuArgs -version $version
}

function Update-AcuSite{
    param (
        [string] $siteName,
        [string] $newVersion
    )

    Test-VersionFormat -version $newVersion
}