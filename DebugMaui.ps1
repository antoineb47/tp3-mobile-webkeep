param(
    [Alias('s')]
    [switch]$System
)

function Format-LogEntry {
    param([Parameter(Mandatory=$true)][string]$LogEntry)
    
    # Define color scheme
    $colors = @{
        Brackets   = 'White'    
        Separator  = 'White'    
        Timestamp  = 'DarkGray'  
        ProcessId  = 'DarkYellow'
        ThreadId   = 'Yellow'    
        Debug      = 'DarkGreen' 
        Error      = 'DarkRed'   
        ErrorBg    = 'Red'       
        AppTag     = 'White'     
        SysTag     = 'DarkCyan'  
        Function   = 'Magenta'   
        Message    = 'Gray'      
        ErrorMsg   = 'Red'       
    }    

    if ($LogEntry -match '^\d{2}-\d{2}') {
        $tokens = $LogEntry -split '\s+'
        
        # Parse components
        $timestamp = "$($tokens[0]) $($tokens[1])"
        $processId = $tokens[2]
        $threadId = $tokens[3]
        $level = $tokens[4]
        $tag = $tokens[5].TrimEnd(':')
        $messageParts = ($tokens[6..($tokens.Length-1)] -join ' ') -split ':\s*', 2
        $function = $messageParts[0]
        $message = if ($messageParts.Count -gt 1) { $messageParts[1] } else { '' }

        # Base structure
        Write-Host "[" -ForegroundColor $colors.Brackets -NoNewline
        Write-Host $timestamp -ForegroundColor $colors.Timestamp -NoNewline
        Write-Host "] (" -ForegroundColor $colors.Brackets -NoNewline
        Write-Host $processId -ForegroundColor $colors.ProcessId -NoNewline
        Write-Host "|" -ForegroundColor $colors.Brackets -NoNewline
        Write-Host $threadId -ForegroundColor $colors.ThreadId -NoNewline
        Write-Host ") " -ForegroundColor $colors.Brackets -NoNewline

        # Level indicator
        $levelBracketColor = if ($level -eq 'E') { $colors.ErrorBg } else { $colors.Brackets }
        $levelTextColor = if ($level -eq 'E') { $colors.Error } else { $colors.Debug }
        Write-Host "[" -ForegroundColor $levelBracketColor -NoNewline
        Write-Host $(if ($level -eq 'E') {"ERROR"} else {"DEBUG"}) -ForegroundColor $levelTextColor -NoNewline
        Write-Host "] " -ForegroundColor $levelBracketColor -NoNewline

        # Content
        $tagColor = if ($tag -eq "WebKeepAppDebug") { $colors.AppTag } else { $colors.SysTag }
        Write-Host $tag -ForegroundColor $tagColor -NoNewline
        Write-Host ": " -ForegroundColor $colors.Separator -NoNewline
        Write-Host $function -ForegroundColor $colors.Function -NoNewline
        
        if ($message) {
            Write-Host ": " -ForegroundColor $colors.Separator -NoNewline
            $messageColor = if ($level -eq 'E') { $colors.ErrorMsg } else { $colors.Message }
            Write-Host $message -ForegroundColor $messageColor
        } else {
            Write-Host ""
        }
    }
    else {
        Write-Host $LogEntry -ForegroundColor $colors.Separator
    }
}

function Launch-MauiLogs {
    param(
        [string]$Tag = "WebKeepAppDebug",
        [switch]$ClearLogs,
        [switch]$System
    )

    if ($ClearLogs) {
        Write-Host "Clearing logcat buffer..." -ForegroundColor Yellow
        adb logcat -c
    }

    Write-Host "`nStarting log watch for tag: $Tag" -ForegroundColor Yellow
    if ($System) {
        Write-Host "Including system error logs (*:E)" -ForegroundColor Yellow
    }
    Write-Host "Press Ctrl+C to stop watching logs`n" -ForegroundColor Yellow

    # Run logcat with filters as a single string
    if ($System) {
        adb logcat "$Tag`:V *:E" | ForEach-Object { Format-LogEntry $_ }
    }
    else {
        adb logcat -s $Tag | ForEach-Object { Format-LogEntry $_ }
    }
}

# Clear the console
Clear-Host
# Run the script
Launch-MauiLogs -ClearLogs -System:$System