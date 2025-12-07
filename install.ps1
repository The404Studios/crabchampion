#Requires -Version 5.1
<#
.SYNOPSIS
    Crab Champions Editor - PowerShell Installer

.DESCRIPTION
    Automated installer for the Crab Champions Editor UE4SS mod.
    Downloads UE4SS if needed and installs the mod.

.NOTES
    Run as: powershell -ExecutionPolicy Bypass -File install.ps1
#>

$ErrorActionPreference = "Stop"

# Colors
function Write-Header { param($text) Write-Host "`n$text" -ForegroundColor Cyan }
function Write-Success { param($text) Write-Host "[OK] $text" -ForegroundColor Green }
function Write-Info { param($text) Write-Host "[INFO] $text" -ForegroundColor Yellow }
function Write-Err { param($text) Write-Host "[ERROR] $text" -ForegroundColor Red }

Write-Host "`n============================================================" -ForegroundColor Cyan
Write-Host "  CRAB CHAMPIONS EDITOR - POWERSHELL INSTALLER" -ForegroundColor Cyan
Write-Host "============================================================`n" -ForegroundColor Cyan

# Find Crab Champions
Write-Header "Step 1: Finding Crab Champions..."

$searchPaths = @(
    "C:\Program Files (x86)\Steam\steamapps\common\Crab Champions",
    "D:\SteamLibrary\steamapps\common\Crab Champions",
    "E:\SteamLibrary\steamapps\common\Crab Champions",
    "C:\Steam\steamapps\common\Crab Champions"
)

# Check Steam registry
try {
    $steamPath = (Get-ItemProperty -Path "HKLM:\SOFTWARE\WOW6432Node\Valve\Steam" -Name InstallPath -ErrorAction SilentlyContinue).InstallPath
    if ($steamPath) {
        $searchPaths += "$steamPath\steamapps\common\Crab Champions"
    }
} catch {}

$gamePath = $null
foreach ($path in $searchPaths) {
    if (Test-Path "$path\CrabChampions.exe") {
        $gamePath = $path
        Write-Success "Found: $gamePath"
        break
    }
}

if (-not $gamePath) {
    Write-Info "Crab Champions not found automatically."
    $gamePath = Read-Host "Enter the full path to Crab Champions folder"

    if (-not (Test-Path "$gamePath\CrabChampions.exe")) {
        Write-Err "CrabChampions.exe not found in specified path!"
        exit 1
    }
}

$binPath = "$gamePath\CrabChampions\Binaries\Win64"
$modsPath = "$binPath\Mods"

# Create folders
Write-Header "Step 2: Creating mod folders..."

if (-not (Test-Path $modsPath)) {
    New-Item -ItemType Directory -Path $modsPath -Force | Out-Null
    Write-Success "Created Mods folder"
} else {
    Write-Success "Mods folder exists"
}

# Check/Install UE4SS
Write-Header "Step 3: Checking UE4SS..."

$ue4ssDll = "$binPath\dwmapi.dll"
$ue4ssZip = "$env:TEMP\UE4SS.zip"

if (-not (Test-Path $ue4ssDll)) {
    Write-Info "UE4SS not found. Downloading..."

    # Get latest release from GitHub
    try {
        $releases = Invoke-RestMethod -Uri "https://api.github.com/repos/UE4SS-RE/RE-UE4SS/releases/latest"
        $asset = $releases.assets | Where-Object { $_.name -like "UE4SS_v*.zip" -and $_.name -notlike "*symbols*" } | Select-Object -First 1

        if ($asset) {
            Write-Info "Downloading $($asset.name)..."
            Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $ue4ssZip

            Write-Info "Extracting UE4SS..."
            Expand-Archive -Path $ue4ssZip -DestinationPath $binPath -Force

            Remove-Item $ue4ssZip -Force
            Write-Success "UE4SS installed"
        } else {
            throw "Could not find UE4SS download"
        }
    } catch {
        Write-Err "Failed to download UE4SS: $_"
        Write-Info "Please download manually from: https://github.com/UE4SS-RE/RE-UE4SS/releases"
        Start-Process "https://github.com/UE4SS-RE/RE-UE4SS/releases/latest"
        exit 1
    }
} else {
    Write-Success "UE4SS is installed"
}

# Install mod
Write-Header "Step 4: Installing Crab Champions Editor..."

$modDest = "$modsPath\CrabEditor"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$modSource = "$scriptDir\CrabEditor"

# Remove old version
if (Test-Path $modDest) {
    Write-Info "Removing old version..."
    Remove-Item -Path $modDest -Recurse -Force
}

# Copy mod
Write-Info "Copying mod files..."
Copy-Item -Path $modSource -Destination $modDest -Recurse -Force
Write-Success "Mod files installed"

# Configure UE4SS
$settingsFile = "$binPath\UE4SS-settings.ini"
if (Test-Path $settingsFile) {
    Write-Info "Configuring UE4SS settings..."

    # Backup
    Copy-Item $settingsFile "$settingsFile.backup" -Force

    # Update settings
    $content = Get-Content $settingsFile -Raw
    $content = $content -replace 'bEnableMod\s*=\s*false', 'bEnableMod = true'
    $content = $content -replace 'GuiConsoleEnabled\s*=\s*false', 'GuiConsoleEnabled = true'
    $content = $content -replace 'EnableImGui\s*=\s*0', 'EnableImGui = 1'
    Set-Content $settingsFile $content

    Write-Success "Settings configured"
}

# Done
Write-Host "`n============================================================" -ForegroundColor Green
Write-Host "  INSTALLATION COMPLETE!" -ForegroundColor Green
Write-Host "============================================================`n" -ForegroundColor Green

Write-Host "Mod installed to: $modDest`n" -ForegroundColor White

Write-Host "To use the mod:" -ForegroundColor Yellow
Write-Host "  1. Launch Crab Champions"
Write-Host "  2. Press F1 to open the menu`n"

Write-Host "Hotkeys:" -ForegroundColor Yellow
Write-Host "  F1 - Toggle Menu"
Write-Host "  F2 - God Mode"
Write-Host "  F3 - Infinite Health"
Write-Host "  F4 - Max Currency"
Write-Host "  F5 - Give All Prismatics"
Write-Host "  F6 - Give All Items"
Write-Host "  F7 - Give All Weapons"
Write-Host "  F9 - NoClip`n"

Read-Host "Press Enter to exit"
