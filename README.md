# acumatica-installerHelper
Acumatica MSI Installer Helper

| Function Name               | Parameters                                                        | Description
|-----------------------------|-------------------------------------------------------------------| -----------------------------------
| Add-AcuSiteVersion          | - version (alias: v) [string, Mandatory]                          | Downloades a specified acumatica installer from builds.acumatica.com, installs it inside of the AcuERPVersionDir. Debugger tools can be installed with a swtich.
|                             | - debuggerTools (alias: dt) [switch]                              |
| Remove-AcuSiteVersion       | - version [string]                                                | Deletes all contents of the specified installed acumatica version from the AcuERPVersionDir
| Add-AcuSite                 | - version (alias: v) [string, Mandatory]                          | Creates a site that is by default installed inside of the AcuSiteDir. A path can be specified to override this behavior. If the current version does not exist, there is an option to install it.
|                             | - siteName (alias: n) [string, Mandatory]                         |
|                             | - siteInstallPath (alias: p) [string]                             |
|                             | - installNewVersion (alias: nv) [switch]                          |
|                             | - portal (alias: pt) [switch]                                     |
|                             | - debuggerTools (alias: dt) [bool]                                |
| Remove-AcuSite              | - siteName (alias: n) [string]                                    | Deletes a site using ac.exe. Does not remove the database
| Update-AcuSite              | - siteName [string]                                               | Upgrades a site based off of ac.exe
|                             | - newVersion [string]                                             |
| Get-AcuDir                  | *No parameters*                                                   | Gets the directory path of where versions and sites are installed
| Get-AcuSiteDir              | *No parameters*                                                   | Gets the name of the site directory that sites are installed into. This directory falls inside of the AcuDir.
| Get-AcuERPVersionDir        | *No parameters*                                                   | Gets the name of the site directory that versions are installed into. This directory falls inside of the AcuDir.
| Set-AcuDir                  | - NewPath [string, Mandatory]                                     | Sets the configuration file for the AcuDir
| Set-AcuSiteDir              | - NewPath [string, Mandatory]                                     | Sets the configuration file for the AcuSiteDir
| Set-AcuERPVersionDir        | - NewPath [string, Mandatory]                                     | Sets the configuration file for the AcuERPVersionDir   