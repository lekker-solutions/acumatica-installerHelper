# acumatica-installerHelper
Acumatica MSI Installer Helper

| Function Name               | Parameters                                                        |
|-----------------------------|-------------------------------------------------------------------|
| Add-AcuSiteVersion          | - version (alias: v) [string, Mandatory]                          |
|                             | - debuggerTools (alias: dt) [switch]                              |
| Remove-AcuSiteVersion       | - version [string]                                                |
| Add-AcuSite                 | - version (alias: v) [string, Mandatory]                          |
|                             | - siteName (alias: n) [string, Mandatory]                         |
|                             | - siteInstallPath (alias: p) [string]                             |
|                             | - installNewVersion (alias: nv) [switch]                          |
|                             | - portal (alias: pt) [switch]                                     |
|                             | - debuggerTools (alias: dt) [bool]                                |
| Remove-AcuSite              | - siteName (alias: n) [string]                                    |
| Update-AcuSite              | - siteName [string]                                               |
|                             | - newVersion [string]                                             |
| Get-ModuleBase              | *No parameters*                                                   |
| Get-AcumaticaDir            | *No parameters*                                                   |
| Get-AcumaticaSiteDir        | *No parameters*                                                   |
| Get-AcumaticaERPVersionDir  | *No parameters*                                                   |
| Set-AcumaticaDir            | - NewPath [string, Mandatory]                                     |
| Set-AcumaticaSiteDir        | - NewPath [string, Mandatory]                                     |
| Set-AcumaticaERPVersionDir  | - NewPath [string, Mandatory]    