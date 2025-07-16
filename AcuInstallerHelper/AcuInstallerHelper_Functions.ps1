function Get-AcuVersionPath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $versionNbr
    )
    return Join-Path (Get-AcuDir) (Get-AcuVersionDir) $versionNbr
}

function Get-AcuConfigurationExe {
    param(
        [Parameter(Mandatory = $true)]
        [string] $versionNbr
    )
    return Join-Path (Get-AcuVersionPath -versionNbr $versionNbr) "Data\ac.exe"
}

function Test-AcuVersionPath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $versionNbr
    )
    return Test-Path (Get-AcuConfigurationExe -versionNbr $versionNbr)
}

function Test-VersionFormat {
    param (
        [Parameter(Mandatory = $true)]
        [string] $version
    )
    
    Write-AcuDebug "Validating version format: $version"
    
    if ($version -notmatch '^\d{2}\.\d{3}\.\d{4}$') {
        Write-AcuError "Invalid version format: $version"
        throw "Version '$version' is invalid. Expected format is ##.###.####"
    }
    
    Write-AcuDebug "Version format is valid"
}

function PromptYesNo {
    param(
        [Parameter(Mandatory = $true)]
        [string] $message
    )

    do {
        Write-AcuPrompt -Message $message
        $response = Read-Host
        
        if ($response -match '^[YyNn]$') {
            $result = $response -match '^[Yy]$'
            Write-AcuDebug "User response: $(if ($result) { 'Yes' } else { 'No' })"
            return $result
        }
        
        Write-AcuWarning "Invalid input. Please enter Y or N"
    } while ($true)
}

function Invoke-AcuExe {
    param (
        [Parameter(Mandatory = $true)]
        [string[]] $arguments,
        
        [Parameter(Mandatory = $true)]
        [string] $version
    )

    if (Get-Command Write-AcuDebug -ErrorAction SilentlyContinue) {
        Write-AcuDebug "Preparing to execute Acumatica configuration utility"
    }
    
    # Verify admin rights
    $isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]"Administrator")
    if (!$isAdmin) {
        if (Get-Command Write-AcuError -ErrorAction SilentlyContinue) {
            Write-AcuError "Administrator privileges required"
        }
        throw "This script must be run as Administrator"
    }
    
    if (Get-Command Write-AcuDebug -ErrorAction SilentlyContinue) {
        Write-AcuDebug "Administrator privileges confirmed"
    }

    # Get ac.exe path
    $acExePath = Get-AcuConfigurationExe -versionNbr $version
    if (!(Test-Path $acExePath)) {
        if (Get-Command Write-AcuError -ErrorAction SilentlyContinue) {
            Write-AcuError "Configuration utility not found at: $acExePath"
        }
        throw "ac.exe not found at $acExePath"
    }
    
    if (Get-Command Write-AcuDebug -ErrorAction SilentlyContinue) {
        Write-AcuDebug "Configuration utility found at: $acExePath"
        Write-AcuDebug "Arguments: $($arguments -join ' ')"
    }
    
    if (Get-Command Write-AcuStep -ErrorAction SilentlyContinue) {
        Write-AcuStep "Executing Acumatica configuration utility"
    }
    
    try {
        # Execute with proper error handling
        $process = Start-Process -FilePath $acExePath -ArgumentList $arguments -Wait -PassThru -NoNewWindow -RedirectStandardOutput $env:TEMP\acu_output.txt -RedirectStandardError $env:TEMP\acu_error.txt
        
        # Read output files
        $output = if (Test-Path "$env:TEMP\acu_output.txt") { Get-Content "$env:TEMP\acu_output.txt" -Raw } else { "" }
        $errorOutput = if (Test-Path "$env:TEMP\acu_error.txt") { Get-Content "$env:TEMP\acu_error.txt" -Raw } else { "" }
        
        # Clean up temp files
        Remove-Item "$env:TEMP\acu_output.txt" -ErrorAction SilentlyContinue
        Remove-Item "$env:TEMP\acu_error.txt" -ErrorAction SilentlyContinue
        
        if ($process.ExitCode -ne 0) {
            if (Get-Command Write-AcuError -ErrorAction SilentlyContinue) {
                Write-AcuError "Configuration utility failed with exit code: $($process.ExitCode)"
                Write-AcuError "Error output: $errorOutput"
                Write-AcuError "Standard output: $output"
            }
            throw "Acumatica configuration failed with exit code $($process.ExitCode)`nError: $errorOutput`nOutput: $output"
        }
        
        if (Get-Command Write-AcuSuccess -ErrorAction SilentlyContinue) {
            Write-AcuSuccess "Configuration utility executed successfully"
        }
        
        if (Get-Command Write-AcuDebug -ErrorAction SilentlyContinue) {
            Write-AcuDebug "Process output: $output"
        }
        
        return $output
    }
    catch {
        if (Get-Command Write-AcuError -ErrorAction SilentlyContinue) {
            Write-AcuError "Failed to execute configuration utility: $_"
        }
        throw
    }
}

function Read-SitePathFromRegistryEntry {
    param(
        [Parameter(Mandatory = $true)]
        [string] $siteName
    )
    
    $registryPath = "HKLM:\SOFTWARE\Acumatica ERP\$siteName"
    
    Write-AcuDebug "Looking for site registry entry: $registryPath"
    
    if (!(Test-Path $registryPath)) {
        Write-AcuError "No registry entry found for site: $siteName"
        throw "No registry key found for site '$siteName'"
    }
    
    try {
        $path = Get-ItemPropertyValue -Path $registryPath -Name "Path"
        Write-AcuDebug "Site path found in registry: $path"
        return $path
    }
    catch {
        Write-AcuError "Failed to read site path from registry"
        throw "Failed to read site path from registry: $_"
    }
}

function Get-AcuSiteVersion {
    param(
        [string] $sitePath,
        [string] $siteName
    )

    Write-AcuDebug "Retrieving site version information"
    
    # Validate parameters
    if ([string]::IsNullOrWhiteSpace($sitePath) -and [string]::IsNullOrWhiteSpace($siteName)) {
        Write-AcuError "Missing required parameters"
        throw "Either SitePath or SiteName must be provided"
    }

    # Get site path from registry if not provided
    if ([string]::IsNullOrWhiteSpace($sitePath)) {
        Write-AcuDebug "Site path not provided, reading from registry"
        $sitePath = Read-SitePathFromRegistryEntry $siteName
    }

    # Read version from web.config
    $webConfigPath = Join-Path $sitePath "web.config"
    Write-AcuDebug "Checking web.config at: $webConfigPath"
    
    if (!(Test-Path $webConfigPath)) {
        Write-AcuError "web.config not found at: $webConfigPath"
        throw "web.config not found at $webConfigPath"
    }

    try {
        [xml]$webConfig = Get-Content $webConfigPath
        $versionNode = $webConfig.configuration.appSettings.add | Where-Object { $_.key -eq 'Version' }
        
        if (!$versionNode) {
            Write-AcuError "Version key not found in web.config"
            throw "Version key not found in web.config"
        }
        
        $version = $versionNode.value
        Write-AcuDebug "Version found in web.config: $version"
        return $version
    }
    catch {
        Write-AcuError "Failed to parse web.config"
        throw "Failed to read version from web.config: $_"
    }
}

function Build-AcuExeArgs {
    param (
        [Parameter(Mandatory = $true)]
        [string] $siteName,
        
        [string] $newClient,
        [string] $sitePath,
        
        [Alias("p")]
        [bool] $portal,
        
        [Alias("u")]
        [switch] $upgrade,
        
        [Alias("n")]
        [switch] $newInstance,
        
        [Alias("d")]
        [switch] $delete,
        
        [Alias("r")]
        [switch] $rename
    )
    
    Write-AcuDebug "Building configuration utility arguments"
    
    # Validate exactly one operation
    $operations = @($upgrade, $newInstance, $delete, $rename)
    $selectedOps = $operations | Where-Object { $_ }
    
    if ($selectedOps.Count -ne 1) {
        Write-AcuError "Invalid operation selection"
        throw "Specify exactly one operation: -upgrade, -newInstance, -delete, or -rename"
    }
    
    # Portal only valid for new instances
    if ($portal -and !$newInstance) {
        Write-AcuError "Portal configuration only valid for new instances"
        throw "Portal sites can only be created as new instances (use -portal with -newInstance)"
    }

    $virtDir = if ($portal) { "${siteName}Portal" } else { $siteName }
    Write-AcuDebug "Virtual directory name: $virtDir"

    # Build arguments based on operation
    switch ($true) {
        $newInstance {
            Write-AcuDebug "Building arguments for new instance creation"
            $exeArgs = @(
                "-configmode:`"NewInstance`"",
                "-dbsrvname:`"(local)`"",
                "-dbname:`"$siteName`"",
                "-output:Forced",
                "-iname:`"$virtDir`"",
                "-company:`"CompanyID=1;CompanyType=;LoginName=;`"",
                "-company:`"CompanyID=2;CompanyType=SalesDemo;ParentID=1;Visible=Yes;LoginName=Company;`"",
                "-ipath:`"$sitePath`"",
                "-swebsite:`"Default Web Site`"",
                "-svirtdir:`"$virtDir`"",
                "-spool:`"DefaultAppPool`""
            )
            
            if ($portal) {
                Write-AcuDebug "Adding portal-specific arguments"
                $exeArgs += "-portal:`"Yes`"", "-dbnew:`"No`""
            }
            
            return $exeArgs
        }
        
        $upgrade {
            Write-AcuDebug "Building arguments for site upgrade"
            return @(
                "-configmode:`"UpgradeSite`"",
                "-output:Forced",
                "-iname:`"$virtDir`""
            )
        }
        
        $delete {
            Write-AcuDebug "Building arguments for site deletion"
            return @(
                "-configmode:`"DeleteSite`"",
                "-output:Forced",
                "-iname:`"$virtDir`""
            )
        }
        
        $rename {
            Write-AcuDebug "Building arguments for site rename"
            if ([string]::IsNullOrWhiteSpace($newClient)) {
                Write-AcuError "NewClient parameter required for rename operation"
                throw "NewClient parameter required for rename operation"
            }
            
            return @(
                "-configmode:`"RenameSite`"",
                "-output:Forced",
                "-iname:`"$newClient`"",
                "-ioldname:`"$virtDir`""
            )
        }
    }
}

function Test-Url {
    param (
        [Parameter(Mandatory = $true)]
        [string] $Url
    )

    Write-AcuDebug "Testing URL accessibility: $Url"
    
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 10 -Method Head
        $isAccessible = $response.StatusCode -eq 200
        Write-AcuDebug "URL test result: $(if ($isAccessible) { 'Accessible' } else { 'Not accessible' })"
        return $isAccessible
    }
    catch {
        Write-AcuDebug "URL test failed: $_"
        return $false
    }
}