# acumatica-installerHelper

PowerShell module for managing Acumatica ERP installations, sites, and versions.

## Overview

The AcuInstallerHelper module simplifies the process of downloading, installing, and managing Acumatica ERP versions and sites. It provides automated installation from Acumatica's S3 buckets, site creation/removal, and configuration management.

## Requirements

- Windows PowerShell 5.1 or higher
- Administrator privileges (required for site operations)
- Internet connection (for downloading installers)
- SQL Server instance accessible via `(local)` hostname
  - SQL Server must be installed and running
  - Mixed mode or Windows authentication enabled
  - The account running the PowerShell session must have SQL Server permissions to create databases

## Installation

### From PowerShell Gallery (Recommended)

```powershell
# Install the module from PowerShell Gallery
Install-Module -Name AcuInstallerHelper

# Import the module
Import-Module AcuInstallerHelper
```

### From Source

```powershell
# Clone the repository
git clone https://github.com/lekker-solutions/acumatica-installerHelper.git

# Import the module
Import-Module ./acumatica-installerHelper/AcuInstallerHelper.psd1
```

## Configuration

The module uses a configuration file (`AcuInstallerHelper_config.json`) to store default paths and settings:

```json
{
  "AcumaticaDir": "C:\\Acumatica",
  "AcumaticaSiteDir": "Sites",
  "AcumaticaVersionDir": "Versions",
  "SiteType": "Production",
  "InstallDebugTools": false
}
```

### SiteType Configuration

The `SiteType` setting controls the default behavior for new sites:
- **Production** (default): Sites are created with standard production settings
- **Dev**: Sites are automatically configured for development (compilation disabled, optimizations enabled)

You can override this default by using the `-devSite` parameter when creating a site.

## Functions

### Version Management

| Function | Parameters | Description |
|----------|------------|-------------|
| **Add-AcuVersion** | - version (alias: v) [string, Mandatory]<br>- debuggerTools (alias: dt) [switch]<br>- preview (alias: p) [switch] | Downloads and installs a specified Acumatica version from builds.acumatica.com. Supports both release and preview builds. |
| **Remove-AcuVersion** | - version [string, Mandatory] | Removes the specified Acumatica version from the system. |
| **Get-AcuVersions** | - majorRelease [string]<br>- preview [switch] | Lists available Acumatica versions from the S3 bucket. |
| **Get-InstalledAcuVersions** | *No parameters* | Displays all locally installed Acumatica versions with install date and size. |

### Site Management

| Function | Parameters | Description |
|----------|------------|-------------|
| **Add-AcuSite** | - version (alias: v) [string, Mandatory]<br>- siteName (alias: n) [string, Mandatory]<br>- siteInstallPath (alias: p) [string]<br>- installNewVersion (alias: nv) [switch]<br>- portal (alias: pt) [switch]<br>- devSite (alias: d) [switch]<br>- preview [switch]<br>- debuggerTools (alias: dt) [switch] | Creates a new Acumatica site. Automatically installs the version if not present (with prompt). The devSite flag configures web.config for development. |
| **Remove-AcuSite** | - siteName (alias: n) [string, Mandatory] | Removes a site registration using ac.exe. Does not remove the database. |
| **Update-AcuSite** | - siteName [string, Mandatory]<br>- newVersion [string, Mandatory] | Upgrades a site to a new version (currently not implemented). |

### Configuration Management

| Function | Parameters | Description |
|----------|------------|-------------|
| **Get-AcuDir** | *No parameters* | Returns the base directory where Acumatica installations are stored. |
| **Get-AcuSiteDir** | *No parameters* | Returns the name of the subdirectory for sites. |
| **Get-AcuVersionDir** | *No parameters* | Returns the name of the subdirectory for versions. |
| **Get-InstallDebugTools** | *No parameters* | Returns the default setting for installing debugger tools. |
| **Get-SiteType** | *No parameters* | Returns the default site type (Dev or Production). |
| **Set-AcuDir** | - NewPath [string, Mandatory] | Updates the base Acumatica directory path. |
| **Set-AcuSiteDir** | - NewPath [string, Mandatory] | Updates the sites subdirectory name. |
| **Set-AcuVersionDir** | - NewPath [string, Mandatory] | Updates the versions subdirectory name. |
| **Set-InstallDebugTools** | - Value [bool, Mandatory] | Sets the default for installing debugger tools with new versions. |
| **Set-SiteType** | - Value [string, Mandatory]<br>ValidateSet: "Dev", "Production" | Sets the default site type for new installations. |

## Examples

### Installing a Version

```powershell
# Install a release version
Add-AcuVersion -version "24.100.0023"

# Install a preview version with debugger tools
Add-AcuVersion -version "24.105.0012" -preview -debuggerTools

# Set default to always install debugger tools
Set-InstallDebugTools -Value $true
```

### Creating Sites

```powershell
# Create a standard site
Add-AcuSite -version "24.100.0023" -siteName "MyERPSite"

# Create a development site with custom path
Add-AcuSite -version "24.100.0023" -siteName "DevSite" -siteInstallPath "D:\DevSites\MyDevSite" -devSite

# Create a portal site
Add-AcuSite -version "24.100.0023" -siteName "CustomerPortal" -portal

# Set default site type to Dev (all new sites will be dev sites)
Set-SiteType -Value "Dev"
Add-AcuSite -version "24.100.0023" -siteName "AutoDevSite"  # Automatically configured as dev

# Override the default site type
Set-SiteType -Value "Dev"
Add-AcuSite -version "24.100.0023" -siteName "ProdSite" -devSite:$false  # Force production settings
```

### Managing Installations

```powershell
# List installed versions
Get-InstalledAcuVersions

# List available versions online
Get-AcuVersions

# Remove a version
Remove-AcuVersion -version "23.200.0045"

# Remove a site
Remove-AcuSite -siteName "OldSite"
```

### Configuration

```powershell
# Change base installation directory
Set-AcuDir -NewPath "D:\AcumaticaERP"

# Set default site type for all new sites
Set-SiteType -Value "Dev"

# Check current configuration
Get-AcuDir
Get-AcuSiteDir
Get-AcuVersionDir
Get-InstallDebugTools
Get-SiteType
```

## Features

- **Automatic Version Detection**: When creating a site, the module checks if the required version is installed and offers to download it
- **Preview Build Support**: Access to both release and preview builds from Acumatica's S3 bucket
- **Development Mode**: The `-devSite` flag automatically configures web.config for development (disables compilation, enables optimizations)
- **Default Site Type**: Configure all new sites to be development or production sites by default
- **Portal Support**: Create portal sites with the `-portal` flag
- **Debugger Tools**: Option to install Acumatica debugger tools with any version
- **Default Settings**: Configure default behavior for debugger tools installation and site types

## Directory Structure

By default, the module organizes installations as follows:

```
C:\Acumatica\
├── Versions\
│   ├── 24.100.0023\
│   ├── 24.105.0012\
│   └── ...
└── Sites\
    ├── MyERPSite\
    ├── DevSite\
    └── ...
```

## Version Format

Acumatica versions must follow the format: `##.###.####` (e.g., "24.100.0023")

## Notes

- Administrator privileges are required for most operations
- The module uses the local SQL Server instance `(local)` for new sites
- SQL Server must be installed and running on the machine
- The `(local)` hostname must resolve to your SQL Server instance (this is typically `localhost\SQLEXPRESS` or the default instance)
- Database names match the site names by default
- Sites are created in IIS under "Default Web Site" using "DefaultAppPool"
- The account running the PowerShell session needs appropriate SQL Server permissions to create databases

## Troubleshooting

### SQL Server Connection Issues

If you encounter errors when creating sites, ensure:

1. SQL Server is installed and running:
   ```powershell
   Get-Service -Name 'MSSQL*' | Where-Object {$_.Status -eq 'Running'}
   ```

2. You can connect to `(local)`:
   ```powershell
   sqlcmd -S "(local)" -Q "SELECT @@VERSION"
   ```

3. If using SQL Express, you may need to use `(local)\SQLEXPRESS` instead. Currently, the module hardcodes `(local)` in the connection string.

### Assembly Loading Warnings

When creating sites with newer Acumatica versions (2024 R1+), you may see warnings about missing assemblies like:
```
[OEM] Error while loading the [Assembly].dll. Message: Could not find assembly 'netstandard, Version=2.0.0.0...
```

These are cosmetic warnings from ac.exe as it scans assemblies and can be safely ignored. Your site will still be created successfully.

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](https://www.gnu.org/licenses/gpl-3.0.html#license-text) file for details.

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/lekker-solutions/acumatica-installerHelper).