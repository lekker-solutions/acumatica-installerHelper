# AcuInstallerHelper Pretty Logging System
# Add this file to your module and dot-source it in AcuInstallerHelper.psm1

# Color definitions for consistent theming
$script:LogColors = @{
    Info      = 'Cyan'
    Success   = 'Green'
    Warning   = 'Yellow'
    Error     = 'Red'
    Progress  = 'Magenta'
    Debug     = 'Gray'
    Highlight = 'White'
    Dim       = 'DarkGray'
}

# Icons for different log levels
$script:LogIcons = @{
    Info     = "ℹ"
    Success  = "✓"
    Warning  = "⚠"
    Error    = "✗"
    Progress = "→"
    Debug    = "●"
    Question = "?"
}

function Write-AcuLog {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Message,
        
        [Parameter()]
        [ValidateSet('Info', 'Success', 'Warning', 'Error', 'Progress', 'Debug')]
        [string] $Level = 'Info',
        
        [Parameter()]
        [switch] $NoNewline,
        
        [Parameter()]
        [string] $Indent = ""
    )
    
    $icon = $script:LogIcons[$Level]
    $color = $script:LogColors[$Level]
    $timestamp = Get-Date -Format "HH:mm:ss"
    
    $prefix = "$Indent[$timestamp] $icon "
    
    Write-Host $prefix -ForegroundColor $color -NoNewline
    Write-Host $Message -ForegroundColor $color -NoNewline:$NoNewline
}

function Write-AcuHeader {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Title,
        
        [Parameter()]
        [string] $Subtitle = $null
    )
    
    $border = "═" * ($Title.Length + 4)
    
    Write-Host ""
    Write-Host "╔$border╗" -ForegroundColor $script:LogColors.Highlight
    Write-Host "║  $Title  ║" -ForegroundColor $script:LogColors.Highlight
    if ($Subtitle) {
        $subtitlePadding = " " * [Math]::Max(0, ($Title.Length - $Subtitle.Length))
        Write-Host "║  $Subtitle$subtitlePadding  ║" -ForegroundColor $script:LogColors.Dim
    }
    Write-Host "╚$border╝" -ForegroundColor $script:LogColors.Highlight
    Write-Host ""
}

function Write-AcuSection {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Title
    )
    
    Write-Host ""
    Write-Host "▼ $Title" -ForegroundColor $script:LogColors.Progress
    Write-Host "─" * ($Title.Length + 2) -ForegroundColor $script:LogColors.Dim
}

function Write-AcuStep {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Message,
        
        [Parameter()]
        [int] $Step = 0,
        
        [Parameter()]
        [int] $Total = 0
    )
    
    $prefix = if ($Total -gt 0) {
        "[$Step/$Total] "
    } else {
        ""
    }
    
    Write-AcuLog -Message "$prefix$Message" -Level Progress -Indent "  "
}

function Write-AcuPrompt {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Message
    )
    
    $icon = $script:LogIcons.Question
    Write-Host "[$icon] " -ForegroundColor $script:LogColors.Warning -NoNewline
    Write-Host $Message -ForegroundColor $script:LogColors.Warning -NoNewline
    Write-Host " " -NoNewline
}

function Write-AcuTable {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable] $Data,
        
        [Parameter()]
        [string] $Title = $null
    )
    
    if ($Title) {
        Write-AcuSection -Title $Title
    }
    
    $maxKeyLength = ($Data.Keys | Measure-Object -Property Length -Maximum).Maximum
    
    foreach ($key in $Data.Keys) {
        $paddedKey = $key.PadRight($maxKeyLength)
        Write-Host "  $paddedKey : " -ForegroundColor $script:LogColors.Dim -NoNewline
        Write-Host $Data[$key] -ForegroundColor $script:LogColors.Info
    }
}

function Write-AcuProgress {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Activity,
        
        [Parameter()]
        [int] $PercentComplete = 0,
        
        [Parameter()]
        [string] $Status = "Processing..."
    )
    
    Write-Progress -Activity $Activity -Status $Status -PercentComplete $PercentComplete
    Write-AcuLog -Message "$Activity - $Status ($PercentComplete%)" -Level Progress
}

function Write-AcuSummary {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Operation,
        
        [Parameter()]
        [string] $Status = "Completed",
        
        [Parameter()]
        [hashtable] $Details = @{}
    )
    
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor $script:LogColors.Success
    Write-Host "║                                                                    SUMMARY                                                                    ║" -ForegroundColor $script:LogColors.Success
    Write-Host "╠═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╣" -ForegroundColor $script:LogColors.Success
    Write-Host "║  Operation: $Operation".PadRight(141) + "║" -ForegroundColor $script:LogColors.Success
    Write-Host "║  Status: $Status".PadRight(144) + "║" -ForegroundColor $script:LogColors.Success
    
    if ($Details.Count -gt 0) {
        Write-Host "║".PadRight(142) + "║" -ForegroundColor $script:LogColors.Success
        foreach ($key in $Details.Keys) {
            $line = "║  $key : $($Details[$key])"
            Write-Host $line.PadRight(141) + "║" -ForegroundColor $script:LogColors.Success
        }
    }
    
    Write-Host "╚═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝" -ForegroundColor $script:LogColors.Success
    Write-Host ""
}

# Convenience aliases for common operations
function Write-AcuInfo { param($Message) Write-AcuLog -Message $Message -Level Info }
function Write-AcuSuccess { param($Message) Write-AcuLog -Message $Message -Level Success }
function Write-AcuWarning { param($Message) Write-AcuLog -Message $Message -Level Warning }
function Write-AcuError { param($Message) Write-AcuLog -Message $Message -Level Error }
function Write-AcuDebug { param($Message) Write-AcuLog -Message $Message -Level Debug }