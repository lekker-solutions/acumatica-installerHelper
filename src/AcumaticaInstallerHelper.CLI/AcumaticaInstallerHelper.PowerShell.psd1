@{
    # Module manifest for AcumaticaInstallerHelper PowerShell Module
    
    RootModule = 'AcumaticaInstallerHelper.PowerShell.dll'
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    
    Author = 'Acumatica Installer Helper Team'
    CompanyName = 'Unknown'
    Copyright = '(c) 2024 Acumatica Installer Helper Team. All rights reserved.'
    
    Description = 'PowerShell module for managing Acumatica ERP installations and sites'
    
    # Minimum version of the Windows PowerShell engine required by this module
    PowerShellVersion = '5.1'
    
    # Minimum version of the .NET Framework required by this module
    DotNetFrameworkVersion = '4.7.2'
    
    # Minimum version of the common language runtime (CLR) required by this module
    CLRVersion = '4.0'
    
    # Functions to export from this module
    FunctionsToExport = @()
    
    # Cmdlets to export from this module
    CmdletsToExport = @(
        'Install-AcumaticaVersion',
        'Uninstall-AcumaticaVersion', 
        'Get-AcumaticaVersion',
        'New-AcumaticaSite',
        'Remove-AcumaticaSite',
        'Get-AcumaticaSite',
        'Update-AcumaticaSite',
        'Get-AcumaticaConfig',
        'Set-AcumaticaDirectory',
        'Set-AcumaticaSiteDirectory',
        'Set-AcumaticaVersionDirectory',
        'Set-AcumaticaDefaultSiteType',
        'Set-AcumaticaInstallDebugTools'
    )
    
    # Variables to export from this module
    VariablesToExport = @()
    
    # Aliases to export from this module
    AliasesToExport = @()
    
    # List of all files packaged with this module
    FileList = @(
        'AcumaticaInstallerHelper.PowerShell.dll',
        'AcumaticaInstallerHelper.PowerShell.psd1'
    )
    
    # Private data to pass to the module specified in RootModule/ModuleToProcess
    PrivateData = @{
        PSData = @{
            # Tags applied to this module
            Tags = @('Acumatica', 'ERP', 'Installation', 'Management', 'PowerShell')
            
            # A URL to the license for this module
            LicenseUri = ''
            
            # A URL to the main website for this project
            ProjectUri = ''
            
            # A URL to an icon representing this module
            IconUri = ''
            
            # Release notes for this module
            ReleaseNotes = 'Initial release of Acumatica Installer Helper PowerShell module'
        }
    }
}