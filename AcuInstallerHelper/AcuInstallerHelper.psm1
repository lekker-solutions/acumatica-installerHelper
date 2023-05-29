
# Set Default Values for Global Varibles if they do not already exist
$acumaticaDirGlobalVariableName = "AcumaticaDir";
$acumaticaSiteDirGlobalVariableName = "AcumaticaSiteDir";
$acumaticaERPVersionDirGlobalVariableName = "AcumaticaERPVersionDir";

$acumaticaDirDefaultValue = "C:\Acumatica\"
$acumaticaSiteDirDefaultValue = $acumaticaDirDefaultValue + "Sites\"
$acumaticaERPVersionDirDefaultValue = $acumaticaDirDefaultValue + "Version_ERP\"

if (-not (Get-Variable -Name $acumaticaDirGlobalVariableName -Scope Global -ErrorAction SilentlyContinue)) {
    Set-Variable -Name $acumaticaDirGlobalVariableName -Value $acumaticaDirDefaultValue -Scope Global
}
if (-not (Get-Variable -Name $acumaticaSiteDirGlobalVariableName -Scope Global -ErrorAction SilentlyContinue)) {
    Set-Variable -Name $acumaticaSiteDirGlobalVariableName -Value $acumaticaSiteDirDefaultValue -Scope Global
}
if (-not (Get-Variable -Name $acumaticaERPVersionDirGlobalVariableName -Scope Global -ErrorAction SilentlyContinue)) {
    Set-Variable -Name $acumaticaERPVersionDirGlobalVariableName -Value $acumaticaERPVersionDirDefaultValue -Scope Global
}

function Add-AcuSiteVersion{
    param (
        [string] [Parameter(Mandatory=$true)] [Alias("v")] $version,
        [switch] [Alias("dt")] $debuggerTools
    )

    $dir = Get-AcuVersionPath($version)
    $majRel = $version.Substring(0,4)

    <#-- Download Installer --#>
    $site = "http://acumatica-builds.s3.amazonaws.com/builds/"
    $downloadUrl = "$($site)$($majRel)/$($version)/AcumaticaERP/AcumaticaERPInstall.msi"
    $tempInstaller = Join-Path $env:TEMP "install.msi"

    <# --  Extract MSI into appropriate folder and rename -- #>
    Write-Output "Checking for existing Acumatica Install"
    if (Test-Path "$($dir)\Data"){
        Write-Output "EXISTING INSTALL FOR THIS VERSION AT $($dir)"
        return
    }
    else {
        Write-Output "No Existing Install at $($dir), Downloading Installer from builds.acumatica.com"
        Start-BitsTransfer $downloadUrl $tempInstaller
        $null = [System.IO.Directory]::CreateDirectory($dir)
        Write-Output "Directory Created for new Install: $dir"
    }

    # Install Acumatica and Remove temp installer
    Write-Output "Installing Acumatica ERP to $($dir)"
    try {
        if($debuggerTools){
            $argumentList = "/a `"$($tempInstaller)`" ADDLOCAL=DEBUGGERTOOLS /qn TARGETDIR=`"$dir`" /passive /l `"$($dir)\log.txt`""
        }
        else{
            $argumentList = "/a `"$($tempInstaller)`" /qn TARGETDIR=`"$dir`" /passive /l `"$($dir)\log.txt`""
        }
        Start-Process -FilePath "$env:systemroot\system32\msiexec.exe" -ArgumentList $argumentList -Wait
        $possibleDir = "$($dir)\Acumatica ERP"  # Sometimes it installs here
        if (Test-Path $possibleDir){
            robocopy $possibleDir $dir /E /COPYALL /NFL /NJH /NJS /NDL /NC /NS
            Remove-Item -Recurse -Force $possibleDir
        }
        Write-Output "---- INSTALL SUCCESS ----"
    }
    catch {
        Write-Output "---- INSTALL FAILURE ----"
        Write-Output $_.Exception.Message
    }
    finally {
        # remove the directory to where the tempinstaller was downloaded
        Write-Output "Cleaning up Acumatica Installer"
        Remove-Item $tempInstaller
    }
}

function Remove-AcuSiteVersion{
    param (
        [string] $version
    )
    $dir = Get-AcuVersionPath($version)
    Remove-Item -Recurse -Force $dir
}

function Add-AcuSite{
    param (
        [string] [Parameter(Mandatory=$true)] [Alias("v")] $version,
        [string] [Parameter(Mandatory=$true)] [Alias("n")] $siteName,
        [string] [Alias("p")] $siteInstallPath,
        [switch] [Alias("nv")] $installNewVersion,
        [switch] [Alias("pt")] $portal,
        [bool] [Alias("dt")] $debuggerTools
    )

    if (Read-AcuVersionPath -versionNbr $version -eq $false){
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

        if ($null -eq $debuggerTools){
            # Prompt for debug tools because it was not set
            $debuggerTools = PromptYesNo "Do you want to install debugger tools?"
        }

        # Install the new version
        Add-AcuSiteVersion -debuggerTools $debuggerTools -version $version
    }

    if ([string]::IsNullOrWhiteSpace($siteInstallPath)){
        $siteInstallPath = Join-Path $global:AcumaticaSiteDir "\$($siteName)"
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
}