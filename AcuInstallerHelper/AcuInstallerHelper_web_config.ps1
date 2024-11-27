function Update-WebConfigForDev {
    param (
        [Parameter(Mandatory = $true)]
        [string]$webConfigPath
    )
    

    try {
        # Load the XML file
        [xml]$xml = Get-Content -Path $webConfigPath

        # Update <add> tag with key="CompilePages"
        $addTag = $xml.configuration.appSettings.add | Where-Object { $_.key -eq "CompilePages" }
        if ($addTag) {
            $addTag.value = "False"
            Write-Output "Updated <add key='CompilePages' value='False' />"
        } else {
            Write-Output "Tag <add key='CompilePages' /> not found."
        }

        # Update <compilation> tag attributes
        $compilationTag = $xml.SelectNodes("//compilation")
        if ($compilationTag) {
            $compilationTag.SetAttribute("optimizeCompilations", "true")
            $compilationTag.SetAttribute("batch", "false")
            Write-Output "Updated <compilation> tag with optimizeCompilations='true' and batch='false'."
        } else {
            Write-Output "Tag <compilation> not found."
        }

        # Save the updated XML back to the file
        $xml.Save($webConfigPath)
        Write-Output "Changes saved to $webConfigPath"

    } catch {
        Write-Output "Error: $($_.Exception.Message)"
    }
}