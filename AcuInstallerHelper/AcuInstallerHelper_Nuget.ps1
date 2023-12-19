# Functions for managing Nuget Packages
function Push-AcuDllToNuget {
  param (
    [string][Parameter(Mandatory = $true)][Alias("v")]  $version,
    [string][Parameter(Mandatory = $true)][Alias("dll")]  $dllName,
    [string][Parameter()][Alias("k")]  $apiKey,
    [string][Parameter()][Alias("url")]  $repositoryURL
  )
    
  Test-VersionFormat $version
  # if (Test-AcuVersionPath($version) -eq $false){
  #     Write-Error "Version $($version) has not been installed"
  #     return
  # }
  $versionDir = Get-AcuVersionPath($version)
  # Step 1: Define the source DLL file and the target NuGet package details
  $sourceDll = Join-Path $versionDir 'Files\Bin' $dllName 
  $description = "Acumatica DLL $dllName" # Replace with your package description

  # Step 2: Create a temporary directory for NuGet package creation
  $tempDirRoot = "$env:TEMP\NuGetPackage\"
  $tempDir = "$tempDirRoot$version\$dllName\"
  New-Item -ItemType Directory -Path $tempDir -Force

  # Step 3: Copy the DLL to the temporary directory
  Copy-Item -Path $sourceDll -Destination $tempDir
  $dllName = $dllName.Replace(".dll", "");
  # Step 4: Create the .nuspec file required for the NuGet package
  $nuspecContent = @"
<?xml version="1.0"?>
<package >
  <metadata>
    <id>$dllName</id>
    <version>$version</version>
    <authors>AcuInstallerHelper</authors>
    <owners></owners>
    <description>$description</description>
  </metadata>
  <files>
    <file src="*.dll" target="lib" />
    <file src="*.pdb" target="lib" />
  </files>
</package>
"@

  $nuspecFileName = "$($dllName).nuspec"
  $nuspecFilePath = Join-Path $tempDir $nuspecFileName
  New-Item $nuspecFilePath
  $nuspecContent | Set-Content -Path $nuspecFilePath

  # Step 5: Create the NuGet package
  nuget pack $nuspecFilePath -OutputDirectory $tempDirRoot -OutputFileNamesWithoutVersion

  # Step 6: Define NuGet repository details
  if (!$repositoryURL) {
    $repositoryURL = Get-AcuNugetRepository
  }

  # Step 7: Push the package to the NuGet repository
  nuget push (Join-Path $tempDirRoot ($dllName + ".nupkg")) -src $repositoryURL -apikey $apiKey

  # Step 8: Clean up - remove the temporary directory
  Remove-Item -Path $tempDir -Recurse -Force
}