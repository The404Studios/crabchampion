@echo off
title Crab Champions Trainer - Dependency Installer
color 0B

echo ============================================================
echo    CRAB CHAMPIONS TRAINER - DEPENDENCY INSTALLER
echo ============================================================
echo.
echo This script will download and install all required dependencies.
echo.
echo Requirements:
echo   - Windows 10/11 (64-bit)
echo   - Internet connection
echo   - Administrator privileges (for some installations)
echo.
pause

:: Create temp directory
if not exist "%TEMP%\CrabTrainerSetup" mkdir "%TEMP%\CrabTrainerSetup"
cd /d "%TEMP%\CrabTrainerSetup"

:: Check for winget
where winget >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo [INFO] Windows Package Manager (winget) found
    set USE_WINGET=1
) else (
    echo [INFO] winget not found, will use direct downloads
    set USE_WINGET=0
)

echo.
echo ============================================================
echo    STEP 1: Installing .NET 8.0 SDK
echo ============================================================
echo.

:: Check if .NET 8 is already installed
dotnet --list-sdks 2>nul | findstr "8.0" >nul
if %ERRORLEVEL% equ 0 (
    echo [OK] .NET 8.0 SDK is already installed
    goto :step2
)

if "%USE_WINGET%"=="1" (
    echo [INFO] Installing via winget...
    winget install Microsoft.DotNet.SDK.8 --accept-source-agreements --accept-package-agreements
) else (
    echo [INFO] Downloading .NET 8.0 SDK...
    echo.
    echo Please download manually from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    echo Or install via PowerShell:
    echo   Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile "dotnet-install.ps1"
    echo   .\dotnet-install.ps1 -Channel 8.0
    echo.

    :: Try PowerShell download
    powershell -Command "& { try { Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile 'dotnet-install.ps1'; .\dotnet-install.ps1 -Channel 8.0 } catch { Write-Host 'Failed to auto-install. Please install manually.' } }"
)

:step2
echo.
echo ============================================================
echo    STEP 2: Installing Visual C++ Redistributable
echo ============================================================
echo.

:: Check if VC++ is installed
reg query "HKLM\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64" >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo [OK] Visual C++ Redistributable is already installed
    goto :step3
)

if "%USE_WINGET%"=="1" (
    echo [INFO] Installing via winget...
    winget install Microsoft.VCRedist.2015+.x64 --accept-source-agreements --accept-package-agreements
) else (
    echo [INFO] Please download VC++ Redistributable manually from:
    echo https://aka.ms/vs/17/release/vc_redist.x64.exe
    echo.

    :: Try to download
    powershell -Command "& { try { Invoke-WebRequest -Uri 'https://aka.ms/vs/17/release/vc_redist.x64.exe' -OutFile 'vc_redist.x64.exe'; Start-Process -FilePath 'vc_redist.x64.exe' -ArgumentList '/install /quiet /norestart' -Wait } catch { Write-Host 'Failed to auto-install. Please install manually.' } }"
)

:step3
echo.
echo ============================================================
echo    STEP 3: Verifying Installation
echo ============================================================
echo.

:: Verify .NET
where dotnet >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo [OK] .NET SDK installed:
    dotnet --version
) else (
    echo [WARNING] .NET SDK not found in PATH
    echo You may need to restart your terminal or computer.
)

echo.
echo ============================================================
echo    STEP 4: Restoring NuGet Packages
echo ============================================================
echo.

:: Go back to project directory
cd /d "%~dp0"

:: Restore packages
if exist "CrabChampionsTrainer.csproj" (
    echo [INFO] Restoring NuGet packages...
    dotnet restore
    if %ERRORLEVEL% equ 0 (
        echo [OK] Packages restored successfully
    ) else (
        echo [WARNING] Failed to restore packages
    )
) else (
    echo [INFO] Run this from the CrabChampionsTrainer directory to restore packages
)

echo.
echo ============================================================
echo    INSTALLATION COMPLETE!
echo ============================================================
echo.
echo Next steps:
echo   1. Run build.bat to compile the trainer
echo   2. Run run.bat to start the trainer
echo.
echo If you encounter issues:
echo   - Restart your terminal/computer
echo   - Run as Administrator
echo   - Check https://dotnet.microsoft.com for manual install
echo.
pause
