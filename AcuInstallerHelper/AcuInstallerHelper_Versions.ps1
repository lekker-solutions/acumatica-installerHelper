# -------- Version Module Methods --------
#          Add-AcuVersion
#          Get-AcuVersions
#          Get-InstalledAcuVersions
#          Remove-AcuVersion
# ----------------------------------------

function Add-AcuVersion {
    param (
        [Parameter(Mandatory = $true)]
        [Alias("v")]
        [string] $version,
        
        [Alias("dt")]
        [switch] $debuggerTools,
        
        [Alias("p")]
        [switch] $preview
    )

    Write-AcuHeader -Title "Acumatica Version Installation" -Subtitle "Version $version"
    
    Write-AcuSection -Title "Validating Version"
    
    Test-VersionFormat $version
    Write-AcuSuccess "Version format validated: $version"
    
    # Check if already installed
    if (Test-AcuVersionPath -versionNbr $version) {
        $existingPath = Get-AcuVersionPath -versionNbr $version
        Write-AcuWarning "Version $version already installed at $existingPath"
        
        Write-AcuSummary -Operation "Version Installation" -Status "Already Installed" -Details @{
            "Version" = $version
            "Path"    = $existingPath
        }
        return
    }
    
    Write-AcuSection -Title "Locating Installation Package"
    
    # Build download URL
    $downloadUrl = Get-AcumaticaDownloadUrl -version $version -preview $preview
    if (!$downloadUrl) {
        Write-AcuError "No build found for version $version"
        throw "No build found for version $version"
    }
    
    Write-AcuSuccess "Installation package located: $downloadUrl"
    
    # Download and install
    $tempInstaller = Join-Path $env:TEMP "AcumaticaERPInstall_$version.msi"
    
    Write-AcuSection -Title "Downloading and Installing"
    
    try {
        Write-AcuStep "Downloading installer package" -Step 1 -Total 2
        Start-BitsTransfer $downloadUrl $tempInstaller
        Write-AcuSuccess "Download completed successfully"
        
        Write-AcuStep "Installing Acumatica version" -Step 2 -Total 2
        Install-AcumaticaVersion -installerPath $tempInstaller -version $version -debuggerTools $debuggerTools
    }
    finally {
        if (Test-Path $tempInstaller) {
            Write-AcuDebug "Cleaning up temporary installer file"
            Remove-Item $tempInstaller -Force
        }
    }
    
    Write-AcuSummary -Operation "Version Installation" -Status "Completed Successfully" -Details @{
        "Version"     = $version
        "Path"        = Get-AcuVersionPath -versionNbr $version
        "Build Type"  = $(if ($preview) { "Preview" } else { "Release" })
        "Debug Tools" = $(if ($debuggerTools) { "Included" } else { "Not included" })
    }
}

function Get-AcumaticaDownloadUrl {
    param (
        [string] $version,
        [bool] $preview
    )
    
    Write-AcuDebug "Building download URL for version: $version"
    
    $majorRelease = $version.Substring(0, 4)
    if ($majorRelease -match "\.0$") {
        $majorRelease = $majorRelease -replace "\.0$", ".1"
    }
    
    Write-AcuDebug "Major release determined: $majorRelease"
    
    # Try preview first if requested
    if ($preview) {
        Write-AcuDebug "Preview build requested, checking preview URLs first"
        $urls = @(
            "https://acumatica-builds.s3.amazonaws.com/builds/preview/$majorRelease/$version/AcumaticaERP/AcumaticaERPInstall.msi",
            "https://acumatica-builds.s3.amazonaws.com/builds/$majorRelease/$version/AcumaticaERP/AcumaticaERPInstall.msi"
        )
    }
    else {
        Write-AcuDebug "Release build requested, checking release URLs first"
        $urls = @(
            "https://acumatica-builds.s3.amazonaws.com/builds/$majorRelease/$version/AcumaticaERP/AcumaticaERPInstall.msi",
            "https://acumatica-builds.s3.amazonaws.com/builds/preview/$majorRelease/$version/AcumaticaERP/AcumaticaERPInstall.msi"
        )
    }
    
    foreach ($url in $urls) {
        Write-AcuDebug "Testing URL: $url"
        if (Test-Url -Url $url) {
            $isPreview = $url -match "/preview/"
            Write-AcuDebug "URL accessible, build type: $(if ($isPreview) { 'Preview' } else { 'Release' })"
            
            $prompt = if ($preview -and !$isPreview) {
                "No preview build found. Use normal build?"
            }
            elseif (!$preview -and $isPreview) {
                "No normal build found. Use preview build?"
            }
            else {
                $null
            }
            
            if (!$prompt -or (PromptYesNo $prompt)) {
                Write-AcuDebug "Selected URL: $url"
                return $url
            }
        }
    }
    
    Write-AcuDebug "No accessible URLs found"
    return $null
}

function Install-AcumaticaVersion {
    param (
        [string] $installerPath,
        [string] $version,
        [bool] $debuggerTools
    )
    
    Write-AcuDebug "Starting installation process"
    
    $targetDir = Get-AcuVersionPath -versionNbr $version
    $null = [System.IO.Directory]::CreateDirectory($targetDir)
    
    Write-AcuInfo "Installing Acumatica ERP to: $targetDir"
    
    $features = ""
    if ($debuggerTools) {
        Write-AcuInfo "Including Debugger Tools in installation"
        $features = "ADDLOCAL=DEBUGGERTOOLS"
    }

    $argumentList = "/a `"$installerPath`" $features /qn TARGETDIR=`"$targetDir`" /passive /l `"$targetDir\install.log`""
    
    Write-AcuDebug "MSI arguments: $argumentList"
    Write-AcuStep "Executing MSI installer"
    
    $process = Start-Process -FilePath "msiexec.exe" -ArgumentList $argumentList -Wait -PassThru
    
    if ($process.ExitCode -ne 0) {
        Write-AcuError "Installation failed with exit code: $($process.ExitCode)"
        Write-AcuDebug "Check installation log at: $targetDir\install.log"
        throw "Installation failed with exit code $($process.ExitCode). Check log at $targetDir\install.log"
    }
    
    Write-AcuSuccess "MSI installation completed successfully"
    
    # Handle nested installation directory
    $nestedDir = Join-Path $targetDir "Acumatica ERP"
    if (Test-Path $nestedDir) {
        Write-AcuStep "Reorganizing installation directory structure"
        Write-AcuDebug "Moving files from nested directory: $nestedDir"
        robocopy $nestedDir $targetDir /E /MOVE /NFL /NDL /NJH /NJS
        Remove-Item $nestedDir -Force -ErrorAction SilentlyContinue
        Write-AcuSuccess "Directory structure reorganized"
    }
    
    Write-AcuSuccess "Installation completed successfully"
}

function Get-AcuVersions {
    param (
        [string] $majorRelease,
        [switch] $preview
    )

    Write-AcuHeader -Title "Available Acumatica Versions" -Subtitle $(if ($preview) { "Preview Builds" } else { "Release Builds" })
    
    Write-AcuSection -Title "Retrieving Version Information"
    
    $allVersions = @{}
    $token = $null
    $pageCount = 0
    
    do {
        $pageCount++
        Write-AcuStep "Fetching page $pageCount of version data"
        
        $result = Get-S3Keys -ContinuationToken $token -Preview:$preview
        
        foreach ($key in $result.Keys) {
            if (!$allVersions.ContainsKey($key.MinorVersion)) {
                $allVersions[$key.MinorVersion] = $key
            }
        }
        
        $token = $result.NextToken
    } while ($token)
    
    Write-AcuSuccess "Retrieved $($allVersions.Count) versions"
    
    Write-AcuSection -Title "Version List"
    
    # Sort and display
    $sortedVersions = $allVersions.Values | 
    Sort-Object Type, 
    @{Expression = { [Version]($_.MajorVersion -replace '\.', '.0.') } }, 
    @{Expression = { [Version]$_.MinorVersion } }
    
    $sortedVersions | Format-Table -AutoSize
    
    Write-AcuInfo "Total versions found: $($allVersions.Count)"
}

function Get-S3Keys {
    param (
        [string] $ContinuationToken,
        [switch] $Preview
    )

    Write-AcuDebug "Querying S3 for version information"
    
    $baseUrl = "http://acumatica-builds.s3.amazonaws.com/?list-type=2"
    
    if ($ContinuationToken) {
        $baseUrl += "&continuation-token=$([System.Web.HttpUtility]::UrlEncode($ContinuationToken))"
        Write-AcuDebug "Using continuation token"
    }
    
    if ($Preview) {
        $baseUrl += "&prefix=builds/preview/"
        Write-AcuDebug "Searching preview builds"
    }
    else {
        Write-AcuDebug "Searching release builds"
    }

    try {
        $response = Invoke-WebRequest -Uri $baseUrl
        [xml]$xml = $response.Content
        
        Write-AcuDebug "Successfully retrieved S3 listing"
    }
    catch {
        Write-AcuError "Failed to retrieve version listing from S3"
        throw "Failed to retrieve version listing: $_"
    }

    $pattern = if ($Preview) {
        'builds/preview/(?<major>\d{2}\.\d)/(?<minor>\d{2}\.\d{3}\.\d{4})/AcumaticaERP/AcumaticaERPInstall.msi'
    }
    else {
        'builds/(?<major>\d{2}\.\d)/(?<minor>\d{2}\.\d{3}\.\d{4})/AcumaticaERP/AcumaticaERPInstall.msi'
    }

    $keys = $xml.ListBucketResult.Contents.Key | ForEach-Object {
        if ($_ -match $pattern) {
            [PSCustomObject]@{
                Type         = if ($Preview) { 'preview' } else { 'release' }
                MajorVersion = $matches['major']
                MinorVersion = $matches['minor']
            }
        }
    }
    
    Write-AcuDebug "Parsed $($keys.Count) version entries from current page"
    
    @{
        Keys      = $keys
        NextToken = $xml.ListBucketResult.NextContinuationToken
    }
}

function Get-InstalledAcuVersions {
    Write-AcuHeader -Title "Installed Acumatica Versions"
    
    Write-AcuSection -Title "Scanning Installation Directory"
    
    $versionDir = Join-Path (Get-AcuDir) (Get-AcuVersionDir)
    
    if (!(Test-Path $versionDir)) {
        Write-AcuWarning "Version directory not found: $versionDir"
        Write-AcuInfo "No versions are currently installed"
        return
    }
    
    Write-AcuDebug "Scanning directory: $versionDir"
    
    $installedVersions = Get-ChildItem -Directory -Path $versionDir
    
    if ($installedVersions.Count -eq 0) {
        Write-AcuInfo "No versions are currently installed"
        return
    }
    
    Write-AcuSuccess "Found $($installedVersions.Count) installed versions"
    
    Write-AcuSection -Title "Version Details"
    
    $versionDetails = $installedVersions | 
    Select-Object @{L = 'Installed Version'; E = { $_.Name } }, 
    @{L = 'Install Date'; E = { $_.CreationTime } },
    @{L = 'Size (GB)'; E = { [math]::Round((Get-ChildItem $_.FullName -Recurse | Measure-Object -Property Length -Sum).Sum / 1GB, 2) } }
    
    $versionDetails | Format-Table -AutoSize
    
    $totalSize = ($versionDetails | Measure-Object -Property "Size (GB)" -Sum).Sum
    Write-AcuInfo "Total disk space used: $([math]::Round($totalSize, 2)) GB"
}

function Remove-AcuVersion {
    param (
        [Parameter(Mandatory = $true)]
        [string] $version
    )
    
    Write-AcuHeader -Title "Acumatica Version Removal" -Subtitle "Version: $version"
    
    Write-AcuSection -Title "Validating Version"
    
    Test-VersionFormat -version $version
    Write-AcuSuccess "Version format validated: $version"
    
    $versionPath = Get-AcuVersionPath -versionNbr $version
    if (!(Test-Path $versionPath)) {
        Write-AcuError "Version $version not found at: $versionPath"
        Write-AcuInfo "Use Get-InstalledAcuVersions to see available versions"
        return
    }
    
    Write-AcuSuccess "Version found at: $versionPath"
    
    # Calculate size before removal
    $sizeGB = [math]::Round((Get-ChildItem $versionPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1GB, 2)
    
    Write-AcuTable -Data @{
        "Version" = $version
        "Path"    = $versionPath
        "Size"    = "$sizeGB GB"
    } -Title "Version to Remove"
    
    Write-AcuSection -Title "Confirmation"
    
    if (PromptYesNo "Are you sure you want to remove version $version?") {
        Write-AcuStep "Removing version files"
        
        try {
            Remove-Item -Recurse -Force $versionPath
            Write-AcuSuccess "Version files removed successfully"
            
            Write-AcuSummary -Operation "Version Removal" -Status "Completed Successfully" -Details @{
                "Version"     = $version
                "Space Freed" = "$sizeGB GB"
            }
        }
        catch {
            Write-AcuError "Failed to remove version files: $_"
            throw
        }
    }
    else {
        Write-AcuInfo "Version removal cancelled by user"
    }
}