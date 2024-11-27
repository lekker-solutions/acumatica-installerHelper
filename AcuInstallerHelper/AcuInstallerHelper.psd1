#
# Module manifest for module 'AcuInstallerHelper'
#
# Generated by: kvanderstoep
#
# Generated on: 5/8/2023
#

@{

# Script module or binary module file associated with this manifest.
RootModule = 'AcuInstallerHelper.psm1'

# Version number of this module.
ModuleVersion = '0.6.8'

# Supported PSEditions
# CompatiblePSEditions = @()

# ID used to uniquely identify this module
GUID = '1b6c3d2f-b6df-48c8-8c6b-d324e89badf7'

# Author of this module
Author = 'Kyle Vanderstoep'

# Company or vendor of this module
CompanyName = 'Lekker Solutions LLC'

# Copyright statement for this module
Copyright = '(c) Kyle Vanderstoep. All rights reserved.'

# Description of the functionality provided by this module
Description = 'Assists with the installation and removal of Acu Sites and Versions'

# Minimum version of the PowerShell engine required by this module
# PowerShellVersion = ''

# Name of the PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
# DotNetFrameworkVersion = ''

# Minimum version of the common language runtime (CLR) required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
# ClrVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
# RequiredAssemblies = @()

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
# TypesToProcess = @()

# Format files (.ps1xml) to be loaded when importing this module
# FormatsToProcess = @()

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
# NestedModules = @()

# Functions to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no functions to export.
FunctionsToExport = @(
    'Add-AcuVersion',
    'Remove-AcuVersion',
    'Get-AcuVersions',
    'Get-InstalledAcuVersions',
    'Add-AcuSite',
    'Remove-AcuSite',
    'Update-AcuSite',
    'Set-AcuDir',
    'Set-AcuSiteDir',
    'Set-AcuVersionDir',
    'Get-AcuDir',
    'Get-AcuSiteDir',
    'Get-AcuVersionDir')

# Cmdlets to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no cmdlets to export.
CmdletsToExport = @()

# Variables to export from this module
VariablesToExport = @()

# Aliases to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no aliases to export.
AliasesToExport = @()

# DSC resources to export from this module
# DscResourcesToExport = @()

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
# FileList = @()

# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{

    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        Tags = @('Acumatica')

        # A URL to the license for this module.
        LicenseUri = 'https://www.gnu.org/licenses/gpl-3.0.html#license-text'

        # A URL to the main website for this project.
        ProjectUri = 'https://github.com/lekker-solutions/acumatica-installerHelper'

        # A URL to an icon representing this module.
        # IconUri = ''

        # ReleaseNotes of this module
        # ReleaseNotes = ''

        # Prerelease string of this module
        # Prerelease = ''

        # Flag to indicate whether the module requires explicit user acceptance for install/update/save
        # RequireLicenseAcceptance = $false

        # External dependent modules of this module
        # ExternalModuleDependencies = @()

    } # End of PSData hashtable

} # End of PrivateData hashtable

# HelpInfo URI of this module
HelpInfoURI = 'https://github.com/lekker-solutions/acumatica-installerHelper/blob/main/README.md'

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}

