# WebKeep App

A mobile application for managing and organizing website bookmarks with a modern dark/grey/yellow theme.

## Prerequisites

Before running the application, make sure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Android Studio](https://developer.android.com/studio) (for the emulator)
- [ADB (Android Debug Bridge)](https://developer.android.com/studio/command-line/adb) (included with Android Studio)

## Setup Instructions

### Setting up the Android Emulator

1. Open Android Studio
2. Click on "More Actions" → "Virtual Device Manager"
3. Click "Create Device"
4. Select "Pixel" from the list and click "Next"
5. Select "API 34" image and click "Next"
6. Click "Finish"

### Launch the Application from PowerShell

1. Ensure your Pixel emulator is running and fully booted.
2. Open PowerShell and navigate to the project's root directory (`c:\Dev\tp3-mobile`).
3. Run the provided script that launches both the server and the app:

   ```powershell
   .\Run.ps1
   ```

4. Watch the terminal output. When the build completes successfully, the application will launch on the connected emulator.
