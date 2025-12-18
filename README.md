# AcuInstallerHelper

PowerShell module for managing Acumatica ERP installations, sites, versions, and patches. Provides automated installation, site creation/removal, configuration management, and patch operations.

## Overview

The AcuInstallerHelper module simplifies the process of downloading, installing, and managing Acumatica ERP versions, sites, and patches. It provides automated installation from Acumatica's download sources, comprehensive site management, and patch operations for maintaining your Acumatica installations.

## Requirements

- **PowerShell 7.0 or higher** (PowerShell Core)
- Administrator privileges (required for site operations)
- Internet connection (for downloading installers and patches)
- SQL Server instance accessible via `(local)` hostname
  - SQL Server must be installed and running
  - Mixed mode or Windows authentication enabled
  - The account running the PowerShell session must have SQL Server permissions to create databases

> **Note**: This module requires PowerShell 7.0 or higher and is not compatible with Windows PowerShell 5.1. To install PowerShell 7, visit [Installing PowerShell on Windows](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows).

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
Import-Module ./acumatica-installerHelper/AcuInstallerHelper/AcuInstallerHelper.psd1
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
- **Development**: Sites are automatically configured for development (compilation disabled, optimizations enabled)

## Available Cmdlets

### Version Management

| Cmdlet | Description |
|--------|-------------|
| **Install-AcumaticaVersion** | Downloads and installs a specified Acumatica version |
| **Uninstall-AcumaticaVersion** | Removes the specified Acumatica version from the system |
| **Get-AcumaticaVersion** | Lists installed or available Acumatica versions |

### Site Management

| Cmdlet | Description |
|--------|-------------|
| **New-AcumaticaSite** | Creates a new Acumatica site with specified version and configuration |
| **Remove-AcumaticaSite** | Removes an existing Acumatica site |
| **Get-AcumaticaSite** | Lists installed Acumatica sites with optional version information |
| **Update-AcumaticaSite** | Updates an existing site to a new version |

### Configuration Management

| Cmdlet | Description |
|--------|-------------|
| **Get-AcumaticaConfig** | Returns the current module configuration |
| **Set-AcumaticaDirectory** | Sets the base directory for Acumatica installations |
| **Set-AcumaticaSiteDirectory** | Sets the directory for Acumatica sites |
| **Set-AcumaticaVersionDirectory** | Sets the directory for Acumatica versions |
| **Set-AcumaticaDefaultSiteType** | Sets the default site type (Production or Development) |
| **Set-AcumaticaInstallDebugTools** | Sets whether to install debug tools by default |

### Patch Management

| Cmdlet | Description |
|--------|-------------|
| **Test-AcumaticaPatch** | Checks if patches are available for a specified site |
| **Install-AcumaticaPatch** | Applies patches to an Acumatica site with optional backup |
| **Restore-AcumaticaPatch** | Rolls back a patch from an Acumatica site |
| **Test-AcumaticaPatchTool** | Verifies if the patch tool is available for a specific version |

## Examples

### Version Management

```powershell
# Install a release version
Install-AcumaticaVersion -Version "24.100.0023"

# Install a preview version with debug tools
Install-AcumaticaVersion -Version "24.105.0012" -Preview -DebugTools

# List installed versions
Get-AcumaticaVersion

# List available versions online
Get-AcumaticaVersion -Available

# List available versions for specific major release
Get-AcumaticaVersion -Available -MajorRelease "2024.2"

# Include preview versions
Get-AcumaticaVersion -Available -Preview

# Remove a version
Uninstall-AcumaticaVersion -Version "23.200.0045"
```

### Site Management

```powershell
# Create a standard production site
New-AcumaticaSite -Version "24.100.0023" -Name "MyERPSite"

# Create a development site with custom path
New-AcumaticaSite -Version "24.100.0023" -Name "DevSite" -Path "D:\DevSites\MyDevSite" -Development

# Create a portal site
New-AcumaticaSite -Version "24.100.0023" -Name "CustomerPortal" -Portal

# Install version automatically if not present
New-AcumaticaSite -Version "24.100.0023" -Name "AutoInstallSite" -InstallVersion

# List all sites
Get-AcumaticaSite

# List sites with version information
Get-AcumaticaSite -IncludeVersion

# Update a site to a new version
Update-AcumaticaSite -Name "MyERPSite" -Version "24.200.0001"

# Remove a site
Remove-AcumaticaSite -Name "OldSite"
```

### Configuration Management

```powershell
# View current configuration
Get-AcumaticaConfig

# Change base installation directory
Set-AcumaticaDirectory -Path "D:\AcumaticaERP"

# Set site directory
Set-AcumaticaSiteDirectory -Path "MySites"

# Set version directory
Set-AcumaticaVersionDirectory -Path "MyVersions"

# Set default site type for all new sites
Set-AcumaticaDefaultSiteType -SiteType Development

# Enable debug tools installation by default
Set-AcumaticaInstallDebugTools -InstallDebugTools $true
```

### Patch Management

```powershell
# Check for available patches for a site
Test-AcumaticaPatch -SiteName "MyERPSite"

# Apply patches to a site
Install-AcumaticaPatch -SiteName "MyERPSite"

# Apply patches with custom backup location
Install-AcumaticaPatch -SiteName "MyERPSite" -BackupPath "D:\Backups\MyERPSite_backup.zip"

# Apply patch from a local archive
Install-AcumaticaPatch -SiteName "MyERPSite" -ArchivePath "C:\Patches\patch.zip"

# Restore/rollback a patch
Restore-AcumaticaPatch -SiteName "MyERPSite"

# Restore with custom backup path
Restore-AcumaticaPatch -SiteName "MyERPSite" -BackupPath "D:\Backups\MyERPSite_backup.zip"

# Check if patch tool is available for a version
Test-AcumaticaPatchTool -Version "24.100.0023"
```

## Features

- **Automated Version Management**: Download and install Acumatica versions with optional debug tools and preview support
- **Comprehensive Site Management**: Create, update, remove, and list sites with flexible configuration options
- **Advanced Patch Operations**: Check for patches, apply them with backup support, and rollback when needed
- **Configuration Persistence**: Store and manage default settings for consistent behavior
- **Development Mode Support**: Automatic configuration for development sites
- **Portal Site Support**: Create portal sites with the `-Portal` flag
- **Preview Build Support**: Access to both release and preview builds
- **Backup Integration**: Automatic backup creation during patch operations

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
- Database names match the site names by default
- Sites are created in IIS under "Default Web Site" using "DefaultAppPool"
- The account running the PowerShell session needs appropriate SQL Server permissions to create databases
- Patch operations create backups by default for safety

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

3. The `(local)` hostname must resolve to your SQL Server instance (this is typically `localhost\SQLEXPRESS` or the default instance). If using SQL Express, you may need to use `(local)\SQLEXPRESS` instead. Currently, the module hardcodes `(local)` in the connection string.

### Assembly Loading Warnings

When creating sites with newer Acumatica versions (2024 R1+), you may see warnings about missing assemblies like:
```
[OEM] Error while loading the [Assembly].dll. Message: Could not find assembly 'netstandard, Version=2.0.0.0...
```

These are cosmetic warnings from ac.exe as it scans assemblies and can be safely ignored. Your site will still be created successfully.

### Patch Tool Availability

Patch operations require the Acumatica patch tool to be available for the specific version. Use `Test-AcumaticaPatchTool` to verify availability before attempting patch operations.

## Testing

The AcuInstallerHelper module includes comprehensive Pester tests to ensure reliability and functionality.

### Running Tests

The easiest way to run tests is using the provided test runner script:

```powershell
# Run all tests (installs Pester automatically if needed)
.\RunTests.ps1

# Run tests with detailed output
.\RunTests.ps1 -OutputFormat Detailed

# Run tests in CI format
.\RunTests.ps1 -OutputFormat CI

# Run tests with minimal output
.\RunTests.ps1 -OutputFormat Minimal
```

### Manual Test Execution

If you prefer to run tests manually:

```powershell
# Install Pester if needed
Install-Module -Name Pester -Force -SkipPublisherCheck

# Run all tests
Invoke-Pester .\tests\ -Recurse

# Run specific test file
Invoke-Pester .\tests\AcumaticaConfigCmdlets.Tests.ps1
```

### Test Files

The test suite is organized into separate files by cmdlet category:

- **`AcumaticaConfigCmdlets.Tests.ps1`**: Tests configuration management cmdlets (Get-AcumaticaConfig, Set-AcumaticaDirectory, etc.)
- **`AcumaticaVersionCmdlets.Tests.ps1`**: Tests version management cmdlets (Install-AcumaticaVersion, Get-AcumaticaVersion, etc.)
- **`AcumaticaSiteCmdlets.Tests.ps1`**: Tests site management cmdlets (New-AcumaticaSite, Remove-AcumaticaSite, etc.)
- **`AcumaticaPatchCmdlets.Tests.ps1`**: Tests patch management cmdlets (Test-AcumaticaPatch, Install-AcumaticaPatch, etc.)

### Test Coverage

The comprehensive test suite covers:

- **Parameter Validation**: Ensures all cmdlets properly validate required parameters and reject invalid input
- **Parameter Acceptance**: Verifies optional parameters and switches work correctly
- **Return Types**: Confirms cmdlets return expected data types and structures
- **Error Handling**: Validates graceful error handling for various failure scenarios
- **Module Loading**: Tests proper module import and cmdlet availability

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](https://www.gnu.org/licenses/gpl-3.0.html#license-text) file for details.

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/lekker-solutions/acumatica-installerHelper).