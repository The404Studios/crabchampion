@echo off
setlocal enabledelayedexpansion
title Crab Champions Editor - Installer
color 0E

echo.
echo  ============================================================
echo   CRAB CHAMPIONS EDITOR - INSTALLER
echo  ============================================================
echo.

:: Check for admin rights (needed for some operations)
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo  [WARNING] Not running as Administrator.
    echo            Some features may not work correctly.
    echo.
)

:: Try to find Crab Champions installation
echo  [1/4] Searching for Crab Champions...
echo.

set "GAME_PATH="

:: Check common Steam locations
set "STEAM_PATHS=C:\Program Files (x86)\Steam\steamapps\common\Crab Champions;D:\SteamLibrary\steamapps\common\Crab Champions;E:\SteamLibrary\steamapps\common\Crab Champions;C:\Steam\steamapps\common\Crab Champions"

for %%p in (%STEAM_PATHS%) do (
    if exist "%%p\CrabChampions.exe" (
        set "GAME_PATH=%%p"
        echo  [OK] Found: %%p
        goto :found_game
    )
)

:: Check Steam registry for install location
for /f "tokens=2*" %%a in ('reg query "HKLM\SOFTWARE\WOW6432Node\Valve\Steam" /v InstallPath 2^>nul') do (
    set "STEAM_DIR=%%b"
)

if defined STEAM_DIR (
    if exist "!STEAM_DIR!\steamapps\common\Crab Champions\CrabChampions.exe" (
        set "GAME_PATH=!STEAM_DIR!\steamapps\common\Crab Champions"
        echo  [OK] Found: !GAME_PATH!
        goto :found_game
    )
)

:: Not found - ask user
echo  [INFO] Crab Champions not found in common locations.
echo.
set /p GAME_PATH="  Enter the full path to Crab Champions folder: "

if not exist "%GAME_PATH%\CrabChampions.exe" (
    echo.
    echo  [ERROR] CrabChampions.exe not found in specified path!
    echo          Please verify the path and try again.
    pause
    exit /b 1
)

:found_game
echo.
echo  Game folder: %GAME_PATH%
echo.

:: Create Mods folder structure
echo  [2/4] Setting up mod folders...

set "MODS_PATH=%GAME_PATH%\CrabChampions\Binaries\Win64\Mods"

if not exist "%MODS_PATH%" (
    mkdir "%MODS_PATH%"
    echo  [OK] Created Mods folder
)

:: Check if UE4SS is installed
echo.
echo  [3/4] Checking UE4SS installation...

set "UE4SS_DLL=%GAME_PATH%\CrabChampions\Binaries\Win64\dwmapi.dll"
set "UE4SS_SETTINGS=%GAME_PATH%\CrabChampions\Binaries\Win64\UE4SS-settings.ini"

if exist "%UE4SS_DLL%" (
    echo  [OK] UE4SS appears to be installed
) else (
    echo.
    echo  [INFO] UE4SS not detected!
    echo.
    echo  UE4SS is required for this mod to work.
    echo.
    echo  Please download UE4SS from:
    echo    https://github.com/UE4SS-RE/RE-UE4SS/releases
    echo.
    echo  Download the latest release (UE4SS_v*.zip) and:
    echo    1. Extract the contents to:
    echo       %GAME_PATH%\CrabChampions\Binaries\Win64\
    echo    2. Run this installer again
    echo.

    set /p DOWNLOAD_NOW="  Open download page in browser? (Y/N): "
    if /i "!DOWNLOAD_NOW!"=="Y" (
        start https://github.com/UE4SS-RE/RE-UE4SS/releases/latest
    )

    echo.
    echo  After installing UE4SS, run this installer again.
    pause
    exit /b 0
)

:: Install the mod
echo.
echo  [4/4] Installing Crab Champions Editor...

set "MOD_DEST=%MODS_PATH%\CrabEditor"

:: Remove old version if exists
if exist "%MOD_DEST%" (
    echo  [INFO] Removing old version...
    rmdir /s /q "%MOD_DEST%"
)

:: Copy mod files
mkdir "%MOD_DEST%"
mkdir "%MOD_DEST%\Scripts"

xcopy /s /y "%~dp0CrabEditor\*" "%MOD_DEST%\" >nul

echo  [OK] Mod files copied

:: Enable ImGui in UE4SS settings
if exist "%UE4SS_SETTINGS%" (
    echo  [INFO] Configuring UE4SS settings...

    :: Backup settings
    copy /y "%UE4SS_SETTINGS%" "%UE4SS_SETTINGS%.backup" >nul 2>&1

    :: Enable necessary features using PowerShell
    powershell -Command "(Get-Content '%UE4SS_SETTINGS%') -replace 'bEnableMod = false', 'bEnableMod = true' -replace 'GuiConsoleEnabled = false', 'GuiConsoleEnabled = true' -replace 'EnableImGui = 0', 'EnableImGui = 1' | Set-Content '%UE4SS_SETTINGS%'"

    echo  [OK] Settings configured
)

echo.
echo  ============================================================
echo   INSTALLATION COMPLETE!
echo  ============================================================
echo.
echo  The mod has been installed to:
echo    %MOD_DEST%
echo.
echo  To use the mod:
echo    1. Launch Crab Champions
echo    2. Press F1 to open the menu
echo.
echo  Hotkeys:
echo    F1 - Toggle Menu
echo    F2 - God Mode
echo    F3 - Infinite Health
echo    F4 - Max Currency
echo    F5 - Give All Prismatics
echo    F6 - Give All Items
echo    F7 - Give All Weapons
echo    F9 - NoClip
echo.
echo  If the mod doesn't work:
echo    1. Verify UE4SS is installed correctly
echo    2. Check the UE4SS console (~ key) for errors
echo    3. Make sure the game is updated to the latest version
echo.

pause
