# Acumatica Installer Helper - PowerShell Module

This PowerShell module provides cmdlets for managing Acumatica ERP installations and sites.

## Installation

1. Build the project:
   ```bash
   dotnet build src/AcumaticaInstallerHelper.CLI/AcumaticaInstallerHelper.CLI.csproj
   ```

2. Import the module in PowerShell:
   ```powershell
   Import-Module .\src\AcumaticaInstallerHelper.CLI\bin\Debug\net9.0\AcumaticaInstallerHelper.PowerShell.dll
   ```

## Available Cmdlets

### Version Management

- **Install-AcumaticaVersion** - Install a new Acumatica version
  ```powershell
  Install-AcumaticaVersion -Version "2023.2.0001" -Preview -DebugTools
  ```

- **Uninstall-AcumaticaVersion** - Remove an installed Acumatica version
  ```powershell
  Uninstall-AcumaticaVersion -Version "2023.2.0001"
  ```

- **Get-AcumaticaVersion** - List installed or available versions
  ```powershell
  Get-AcumaticaVersion                    # List installed versions
  Get-AcumaticaVersion -Available         # List available versions
  Get-AcumaticaVersion -Available -Preview -MajorRelease "2023.2"
  ```

### Site Management

- **New-AcumaticaSite** - Create a new Acumatica site
  ```powershell
  New-AcumaticaSite -Version "2023.2.0001" -Name "MyTestSite" -Development
  New-AcumaticaSite -Version "2023.2.0001" -Name "MyPortalSite" -Portal -Path "C:\Sites\Portal"
  ```

- **Remove-AcumaticaSite** - Remove an Acumatica site
  ```powershell
  Remove-AcumaticaSite -Name "MyTestSite"
  ```

- **Get-AcumaticaSite** - List installed sites
  ```powershell
  Get-AcumaticaSite                       # List site names
  Get-AcumaticaSite -IncludeVersion       # Include version information
  ```

- **Update-AcumaticaSite** - Update a site to a new version
  ```powershell
  Update-AcumaticaSite -Name "MyTestSite" -Version "2023.2.0002"
  ```

### Configuration Management

- **Get-AcumaticaConfig** - Show current configuration
  ```powershell
  Get-AcumaticaConfig
  ```

- **Set-AcumaticaDirectory** - Set base Acumatica directory
  ```powershell
  Set-AcumaticaDirectory -Path "C:\Acumatica"
  ```

- **Set-AcumaticaSiteDirectory** - Set sites directory
  ```powershell
  Set-AcumaticaSiteDirectory -Path "C:\Sites"
  ```

- **Set-AcumaticaVersionDirectory** - Set versions directory
  ```powershell
  Set-AcumaticaVersionDirectory -Path "C:\Acumatica\Versions"
  ```

- **Set-AcumaticaDefaultSiteType** - Set default site type
  ```powershell
  Set-AcumaticaDefaultSiteType -SiteType Development
  Set-AcumaticaDefaultSiteType -SiteType Production
  ```

- **Set-AcumaticaInstallDebugTools** - Set debug tools installation preference
  ```powershell
  Set-AcumaticaInstallDebugTools -InstallDebugTools $true
  ```

## Examples

### Complete Site Setup Workflow
```powershell
# Configure directories
Set-AcumaticaDirectory -Path "C:\Acumatica"
Set-AcumaticaDefaultSiteType -SiteType Development

# Install a version
Install-AcumaticaVersion -Version "2023.2.0001" -DebugTools

# Create a development site
New-AcumaticaSite -Version "2023.2.0001" -Name "DevSite" -Development

# List all sites with versions
Get-AcumaticaSite -IncludeVersion

# Update the site to a newer version
Install-AcumaticaVersion -Version "2023.2.0002"
Update-AcumaticaSite -Name "DevSite" -Version "2023.2.0002"
```

### Error Handling
All cmdlets return boolean values for success/failure and write detailed error information to the PowerShell error stream. You can capture and handle errors:

```powershell
try {
    $success = Install-AcumaticaVersion -Version "2023.2.0001"
    if ($success) {
        Write-Host "Version installed successfully"
    }
} catch {
    Write-Error "Failed to install version: $($_.Exception.Message)"
}
```

## Requirements

- PowerShell 5.1 or later
- .NET 9.0 or later
- Windows operating system
- Administrator privileges (for some operations)

## Migration from Console App

The PowerShell cmdlets provide the same functionality as the previous console application but with better PowerShell integration:

| Console Command | PowerShell Cmdlet |
|----------------|-------------------|
| `acuhelper version add --version X` | `Install-AcumaticaVersion -Version X` |
| `acuhelper version remove --version X` | `Uninstall-AcumaticaVersion -Version X` |
| `acuhelper version list` | `Get-AcumaticaVersion` |
| `acuhelper site add --version X --name Y` | `New-AcumaticaSite -Version X -Name Y` |
| `acuhelper site remove --name Y` | `Remove-AcumaticaSite -Name Y` |
| `acuhelper site list` | `Get-AcumaticaSite` |
| `acuhelper config show` | `Get-AcumaticaConfig` |