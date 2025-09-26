# Acumatica Installer Helper - C# Version

This is the C# conversion of the PowerShell Acumatica Installer Helper module. It provides the same functionality for managing Acumatica ERP installations, sites, and versions but as a .NET application.

## Project Structure

```
├── AcumaticaInstallerHelper.sln           # Solution file
├── src/
│   ├── AcumaticaInstallerHelper/           # Core library
│   │   ├── Models/                         # Data models
│   │   ├── Services/                       # Business logic services
│   │   ├── AcumaticaManager.cs            # Main facade class
│   │   └── ServiceCollectionExtensions.cs # DI registration
│   └── AcumaticaInstallerHelper.CLI/       # Command-line interface
│       └── Program.cs                      # CLI application
└── tests/
    └── AcumaticaInstallerHelper.Tests/     # Unit tests
        ├── Models/                         # Model tests
        └── Services/                       # Service tests
```

## Technology Stack

- **.NET 6.0**: Target framework
- **Microsoft.Extensions.*****: Dependency injection, configuration, logging
- **System.CommandLine**: Modern CLI framework
- **AWSSDK.S3**: AWS S3 integration for version downloads
- **xUnit**: Unit testing framework
- **FluentAssertions**: Fluent assertion library
- **Moq**: Mocking framework

## Key Components

### Models
- `AcumaticaConfig`: Configuration settings
- `AcumaticaVersion`: Version information
- `SiteConfiguration`: Site setup parameters
- `SiteInstallOptions`: Installation options

### Services
- `ConfigurationService`: Manages JSON configuration
- `VersionService`: Handles version downloads and installations
- `SiteService`: Manages site creation and removal
- `ConsoleLoggingService`: Rich console output with colored logging

### CLI Application
- Built with System.CommandLine
- Supports all original PowerShell commands
- Modern argument parsing and help system

## Building and Running

### Prerequisites
- .NET 6.0 SDK or later
- Windows (for IIS and registry operations)
- Administrator privileges (for site operations)
- SQL Server instance accessible via `(local)`

### Build
```bash
dotnet build
```

### Run
```bash
# Install a version
dotnet run --project src/AcumaticaInstallerHelper.CLI -- version add --version 24.100.0023

# Create a site
dotnet run --project src/AcumaticaInstallerHelper.CLI -- site add --version 24.100.0023 --name MyTestSite

# List installed versions
dotnet run --project src/AcumaticaInstallerHelper.CLI -- version list

# Show configuration
dotnet run --project src/AcumaticaInstallerHelper.CLI -- config show
```

### Test
```bash
dotnet test
```

## Key Differences from PowerShell Version

### Architecture Improvements
1. **Dependency Injection**: Proper DI container with service registration
2. **Async/Await**: Modern asynchronous programming patterns
3. **Structured Logging**: Integration with Microsoft.Extensions.Logging
4. **Type Safety**: Strong typing throughout the application
5. **Testability**: Interfaces and mocking for comprehensive testing

### Feature Parity
- ✅ Version management (install, remove, list)
- ✅ Site management (create, remove, list)
- ✅ Configuration management (JSON-based)
- ✅ Development site configuration
- ✅ Administrator privilege checking
- ✅ Version format validation
- ⚠️ S3 version listing (placeholder - needs AWS SDK implementation)
- ⚠️ Site updates (not implemented yet)

### CLI Improvements
- Modern argument parsing with System.CommandLine
- Built-in help system with `--help`
- Tab completion support
- Better error handling and exit codes

## Configuration

The application uses the same JSON configuration format as the PowerShell version:

```json
{
  "AcumaticaDir": "C:\\Acumatica",
  "AcumaticaSiteDir": "Sites",
  "AcumaticaVersionDir": "Versions",
  "SiteType": "Production",
  "InstallDebugTools": true
}
```

## Examples

### Version Management
```bash
# Install a release version
acuhelper version add -v 24.100.0023

# Install a preview version with debug tools
acuhelper version add -v 24.105.0012 --preview --debug-tools

# List installed versions
acuhelper version list

# Remove a version
acuhelper version remove -v 23.200.0045
```

### Site Management
```bash
# Create a standard site
acuhelper site add -v 24.100.0023 -n MyERPSite

# Create a development site
acuhelper site add -v 24.100.0023 -n DevSite --dev

# Create a portal site
acuhelper site add -v 24.100.0023 -n CustomerPortal --portal

# List sites
acuhelper site list

# Remove a site
acuhelper site remove -n OldSite
```

### Configuration
```bash
# Show current configuration
acuhelper config show

# Set Acumatica base directory
acuhelper config set-acudir --path "D:\\AcumaticaERP"

# Set default site type to Development
acuhelper config set-sitetype --type Development
```

## Deployment

### As a Global Tool
1. Pack the CLI project:
   ```bash
   dotnet pack src/AcumaticaInstallerHelper.CLI -c Release
   ```

2. Install globally:
   ```bash
   dotnet tool install -g AcumaticaInstallerHelper.CLI
   ```

### As a Standalone Executable
```bash
dotnet publish src/AcumaticaInstallerHelper.CLI -c Release -r win-x64 --self-contained
```

## Contributing

1. Follow the existing code patterns and architecture
2. Add unit tests for new functionality
3. Update documentation for API changes
4. Ensure administrator privilege requirements are handled properly

## Migration from PowerShell

The C# version maintains compatibility with the PowerShell configuration file, so existing users can migrate seamlessly. The command structure is similar but follows modern CLI conventions:

| PowerShell | C# CLI |
|------------|---------|
| `Add-AcuVersion -version "24.100.0023"` | `acuhelper version add -v 24.100.0023` |
| `Add-AcuSite -version "24.100.0023" -siteName "Test"` | `acuhelper site add -v 24.100.0023 -n Test` |
| `Get-InstalledAcuVersions` | `acuhelper version list` |
| `Get-AcuDir` | `acuhelper config show` |

The C# version provides better performance, type safety, and maintainability while preserving all the core functionality of the PowerShell module.