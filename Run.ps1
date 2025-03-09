param(
    [Parameter(HelpMessage = "Run debug logging after starting the app")]
    [Alias("d")]
    [switch]$ShowDebug
)

# Set console colors for better readability
Clear-Host

function Write-Message {
    param (
        [string]$Text,
        [string]$ForegroundColor = "White"
    )
    Write-Host $Text -ForegroundColor $ForegroundColor
}

Write-Message "WebKeep App Launcher" -ForegroundColor "White"
Write-Message "-------------------" -ForegroundColor "DarkGray"

# Check for Android emulator
Write-Message "[INFO] Checking for Android emulator..." -ForegroundColor "Cyan"
try {
    $devices = & adb devices | Select-Object -Skip 1 | Where-Object { $_ -match "\w+" }
    
    if (-not $devices -or $devices -match "no devices") {
        Write-Message "[ERROR] No Android emulator found. Please start an emulator first." -ForegroundColor "Red"
        exit 1
    }

    Write-Message "[INFO] Found device: $devices" -ForegroundColor "Green"
} 
catch {
    Write-Message "[ERROR] Failed to check for Android devices. Is ADB installed and in your PATH?" -ForegroundColor "Red"
    exit 1
}

# Store current directory path
$projectPath = $PWD.Path

# Launch the server first in a separate window
Write-Message "[INFO] Starting the WebKeep server..." -ForegroundColor "Cyan"
$serverCmd = "cd '$projectPath\BackupServer'; Write-Host '[INFO] Starting BackupServer...' -ForegroundColor Cyan; dotnet run"
Start-Process powershell -ArgumentList "-NoExit", "-Command", $serverCmd

# Build and run the MAUI project (will wait until this completes)
Write-Message "[INFO] Building and running the MAUI Android app..." -ForegroundColor "Cyan"
$mauiCmd = "cd '$projectPath\WebKeepApp'; Write-Host '[INFO] Building WebKeepApp for Android...' -ForegroundColor Cyan; dotnet build -t:Run -f net8.0-android"
# Start this process and wait for it to launch before continuing
$process = Start-Process powershell -ArgumentList "-NoExit", "-Command", $mauiCmd -PassThru

# Wait a moment to ensure build has started
Start-Sleep -Seconds 3

Write-Message "[INFO] Android app deploying..." -ForegroundColor "Green"

# Run DebugMaui.ps1 if debug parameter is provided, after app is building
if ($ShowDebug) {
    Write-Message "[INFO] Starting MAUI debug logging..." -ForegroundColor "Cyan"
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$projectPath'; .\DebugMaui.ps1"
}

Write-Message "[INFO] All components started successfully." -ForegroundColor "Green"
Write-Message "Use Ctrl+C in the respective windows to stop the processes." -ForegroundColor "Yellow"