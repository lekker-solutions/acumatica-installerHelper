function Update-WebConfigForDev {
    param (
        [Parameter(Mandatory = $true)]
        [string] $webConfigPath
    )
    
    Write-AcuDebug "Applying development configuration to: $webConfigPath"
    
    if (!(Test-Path $webConfigPath)) {
        Write-AcuError "web.config not found at: $webConfigPath"
        throw "web.config not found at $webConfigPath"
    }

    Write-AcuStep "Loading web.config file"
    
    try {
        # Load XML
        [xml]$xml = Get-Content -Path $webConfigPath
        Write-AcuDebug "web.config loaded successfully"
        
        $changesApplied = @()

        # Update CompilePages setting
        $appSettings = $xml.configuration.appSettings
        if ($appSettings) {
            $compilePagesNode = $appSettings.add | Where-Object { $_.key -eq "CompilePages" }
            
            if ($compilePagesNode) {
                $compilePagesNode.value = "False"
                Write-AcuDebug "Updated existing CompilePages setting to False"
                $changesApplied += "Updated CompilePages to False"
            }
            else {
                # Add the setting if it doesn't exist
                $newNode = $xml.CreateElement("add")
                $newNode.SetAttribute("key", "CompilePages")
                $newNode.SetAttribute("value", "False")
                $appSettings.AppendChild($newNode) | Out-Null
                Write-AcuDebug "Added new CompilePages setting with value False"
                $changesApplied += "Added CompilePages setting (False)"
            }
        }
        else {
            Write-AcuWarning "appSettings section not found in web.config"
        }

        # Update compilation settings
        $compilationNode = $xml.configuration.'system.web'.compilation
        if ($compilationNode) {
            $compilationNode.SetAttribute("optimizeCompilations", "true")
            $compilationNode.SetAttribute("batch", "false")
            Write-AcuDebug "Updated compilation settings for development"
            $changesApplied += "Updated compilation settings (optimizeCompilations=true, batch=false)"
        }
        else {
            Write-AcuWarning "Compilation node not found in web.config"
        }

        # Save changes
        Write-AcuStep "Saving configuration changes"
        $xml.Save($webConfigPath)
        Write-AcuDebug "web.config saved successfully"

        Write-AcuSuccess "Development configuration applied successfully"
        
        # Log all changes applied
        foreach ($change in $changesApplied) {
            Write-AcuInfo "  • $change"
        }
        
        Write-AcuDebug "Total changes applied: $($changesApplied.Count)"
    }
    catch {
        Write-AcuError "Failed to update web.config: $_"
        throw "Failed to update web.config: $_"
    }
}