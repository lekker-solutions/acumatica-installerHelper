function Get-AcuVersionPath{
    param(
        [string] [Alias("v")] $versionNbr
    )
    return $Global:AcumaticaERPVersionDir + $versionNbr
}

function Read-AcuVersionPath{
    param(
        [string] $versionNbr
    )
    $versionDir = Join-Path (Get-AcuVersionPath -v $version) "\Data\ac.exe"
    return Test-Path $versionDir
}

function Get-AcuConfigurationExe{
    param(
        [string] $versionNbr
    )
    return $Global:AcumaticaERPVersionDir + $versionNbr + "\Data\ac.exe"
}

function PromptYesNo{
    param(
        [string] $message
    )

    $response = Read-Host "$($message) (Y/N)"

    while (!$chosen) {
        switch ($response) {
        "Y" {
            Write-Host "You chose Yes."
            $chosen = $true
        }
        "N" {
            Write-Host "You chose No."
            $chosen = $true
        }
        default {
            Write-Host "Invalid input. Please enter Y or N"
        }
        }
    }

    return $response -eq "Y"
}

# Run Ac.exe with args and catch an error
function Invoke-AcuExe {
    param (
        [string[]]$arguments,
        [string]$version
    )
        <# -- Get Ac.exe --#>
        $versionPath = Get-AcuVersionPath -v $version;
        $acProcess = "$($versionPath)\Data\ac.exe"
    
        if (!(Test-Path $acProcess)){
            throw "Acumatica Version $($version) not installed"
        }

        & $acProcess $arguments
        if ($LASTEXITCODE -ne 0){
            throw [System.Exception]("Acumatica Failed to create a site")
        }
}

function Read-SitePathFromRegistryEntry{
    param([string] $siteName)
    
    $key = "HKLM:\SOFTWARE\Acumatica ERP\$($siteName)"
    if (-not (Test-Path $key)){
        throw "No registry key for site $($siteName) exists"
    }
    return Get-ItemPropertyValue -Path $key -Name "Path"
}

function Get-AcuSiteVersion{
    param([string] $sitePath,
          [string] $siteName)

    if ([string]::IsNullOrWhiteSpace($sitePath)){
        if ([string]::IsNullOrWhiteSpace($siteName)){
            throw "Either Site Name or Site Path must be provided"
        }
        $sitePath = Read-SitePathFromRegistryEntry $siteName
    }

    $webConfig = Join-Path $sitePath "web.config"
    if (-not (Test-Path $webConfig)){
        throw "No web.config file found at $($webConfig) to determine site version"
    }

    # Load the XML file
    $xmlContent = [xml](Get-Content $webConfig)

    # Extract the value of the 'Version' key from the appSettings section
    $versionValue = $xmlContent.configuration.appSettings.add | Where-Object { $_.key -eq 'Version' } | Select-Object -ExpandProperty value
    return $versionValue
}

function Build-AcuExeArgs {
    param (
        [string] $siteName,
        [string] $newClient,
        [string] $sitePath,
        [bool][Alias("p")] $portal,
        [switch][Alias("u")] $upgrade,
        [switch][Alias("n")] $newInstance,
        [switch][Alias("d")] $delete,
        [switch][Alias("r")] $rename
    )
    
    $switchCount = 0;
    if ($upgrade){$switchCount++;}
    if ($newInstance){$switchCount++;}
    if ($delete){$switchCount++;}
    if ($rename){$switchCount++;}
    if (!($switchCount -eq 1)){
        throw [System.Exception]("Please provide a single choice of Upgrade, New, Rename, or Delete Instance");
    }
    if ($portal -and !$newInstance){
        throw [System.Exception]("A portal can only be deployed as a new instance, please choose -p and -n if you want to create a portal");
    }

    $virtDir = $portal ? $siteName + "Portal" : $siteName;

    if ($newInstance){
        $arguments =  ("-configmode:`"NewInstance`"", 
        "-dbsrvname:`"(local)`"",
        "-dbname:`"$($siteName)`"",
        "-output:Forced",
        "-iname:`"$($virtDir)`"",
        "-company:`"CompanyID=1;CompanyType=;LoginName=;`"",
        "-company:`"CompanyID=2;CompanyType=SalesDemo;ParentID=1;Visible=Yes;LoginName=Company;`"",
        "-ipath:`"$($sitePath)`"",
        "-swebsite:`"Default Web Site`"",
        "-svirtdir:`"$($virtDir)`"",
        "-spool:`"DefaultAppPool`"")

        if($portal){
            $arguments += , "-portal:`"Yes`"", "-dbnew:`"No`""
        }
    }
    if ($upgrade){
        $arguments =  (
            "-configmode:`"UpgradeSite`"", 
            "-output:Forced",
            "-iname:`"$($virtDir)`"")
    }
    if ($delete){
        $arguments =  (
            "-configmode:`"DeleteSite`"", 
            "-output:Forced",
            "-iname:`"$($virtDir)`"")
    }
    if ($rename){
        $arguments =  (
            "-configmode:`"RenameSite`"", 
            "-output:Forced",
            "-iname:`"$($newClient)`"",
            "-ioldname:`"$($virtDir)`"")
    }

    $arguments;
}

