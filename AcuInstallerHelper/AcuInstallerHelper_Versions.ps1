# -------- Version Module Methods --------
#          Add-AcuVersion
#          Get-AcuVersions
#          Get-InstalledAcuVersions
#          Remove-AcuVersion
# ----------------------------------------


function Add-AcuVersion{
    param (
        [string] [Parameter(Mandatory=$true)] [Alias("v")] $version,
        [switch] [Alias("dt")] $debuggerTools,
        [switch] $preview
    )

    Test-VersionFormat $version

    $dir = Get-AcuVersionPath($version)
    $majRel = $version.Substring(0,4)

    <#-- Download Installer --#>
    if($true -eq $preview){
        $site = "https://acumatica-builds.s3.amazonaws.com/builds/preview/"
    }
    else {
        $site = "https://acumatica-builds.s3.amazonaws.com/builds/"
    }
    
    $downloadUrl = "{0}{1}/{2}/AcumaticaERP/AcumaticaERPInstall.msi" -f $site, $majRel, $version
    $tempInstaller = Join-Path $env:TEMP "install.msi"

    <# --  Extract MSI into appropriate folder and rename -- #>
    Write-Output "Checking for existing Acu Install"
    if (Test-AcuVersionPath -version $version){
        Write-Output "EXISTING INSTALL FOR THIS VERSION AT $($dir)"
        return
    }
    else {
        Write-Output "No Existing Install at $($dir), Downloading Installer from $downloadUrl"
        Start-BitsTransfer $downloadUrl $tempInstaller
        $null = [System.IO.Directory]::CreateDirectory($dir)
        Write-Output "Directory Created for new Install: $dir"
    }

    # Install Acu and Remove temp installer
    Write-Output "Installing Acu ERP to $($dir)"
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
            robocopy $possibleDir $dir /E /COPY:DA /NFL /NJH /NJS /NDL /NC /NS
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
        Write-Output "Cleaning up Acu Installer"
        Remove-Item $tempInstaller
    }
}

function Get-AcuVersions {
    param (
        [string] $majorRelease,
        [switch] $preview
    )

    # Initialize dictionary
    $allKeys = @{}

    do {
        # Get keys from S3
        $result = Get-S3Keys -ContinuationToken $token
        foreach ($key in $result.Keys) {
            # Create a unique identifier for the dictionary
            $dictKey = $key.MinorVersion

            # Check if the key already exists to avoid duplicates
            if (-not $allKeys.ContainsKey($dictKey)) {
                $allKeys[$dictKey] = $key
            }
        }

        $token = $result.NextToken
    } while ($token)


    # Custom function to convert version string to a Version object for sorting
    function ConvertTo-Version([string]$versionString) {
        $versionParts = $versionString -split '\.' | ForEach-Object { [int]$_ }
        while ($versionParts.Count -lt 4) {
            $versionParts += 0
        }
        return [Version]::new($versionParts[0], $versionParts[1], $versionParts[2], $versionParts[3])
    }

    # Convert dictionary values to an array
    $valuesArray = $allKeys.Values

    # Sort by Type, then MajorVersion, then MinorVersion
    $sortedValues = $valuesArray | Sort-Object Type, @{Expression={ConvertTo-Version $_.MajorVersion}}, @{Expression={ConvertTo-Version $_.MinorVersion}}

    # Output to table
    $sortedValues | Format-Table -AutoSize   
}

function Get-S3Keys {
    param (
        [string]$ContinuationToken
    )

    # Base URL
    $baseUrl = "http://Acu-builds.s3.amazonaws.com/?list-type=2"

    # Add continuation token to URL if provided
    if ($ContinuationToken) {
        $baseUrl += "&continuation-token=$([System.Web.HttpUtility]::UrlEncode($ContinuationToken))"
    }
    if($preview){
        $baseUrl += "&prefix=builds/preview/"
    }

    # Send GET request
    $response = Invoke-WebRequest -Uri $baseUrl

    # Parse XML response
    [xml]$xml = $response.Content

    # Extract keys and continuation token
    if ($preview){
        $match = '(?<prefix>builds/preview/)?(?<major>\d{2}\.\d)/(?<minor>\d{2}\.\d{3}\.\d{4})/AcuERP/AcuERPInstall.msi'
    }
    else{
        $match = '(?<prefix>builds/)?(?<major>\d{2}\.\d)/(?<minor>\d{2}\.\d{3}\.\d{4})/AcuERP/AcuERPInstall.msi'
    }

    $keys = $xml.ListBucketResult.Contents.Key | ForEach-Object {
        if ($_ -match $match) {
            [PSCustomObject]@{
                Type = if ($matches['prefix'] -eq 'builds/preview/') { 'builds/preview' } else { 'builds' }
                MajorVersion = $matches['major']
                MinorVersion = $matches['minor']
            }
        }
     }
    $nextToken = $xml.ListBucketResult.NextContinuationToken

    return @{ "Keys" = $keys; "NextToken" = $nextToken }
}


function Get-InstalledAcuVersions {
    $dir = Get-AcuVersionDir;
    Get-ChildItem -Directory -Path $dir | Format-Table @{L=’Installed Version’;E={$_.Name}} -AutoSize
}

function Remove-AcuVersion{
    param (
        [string] $version
    )
    Test-VersionFormat -version $version
    $dir = Get-AcuVersionPath($version)
    Remove-Item -Recurse -Force $dir
}