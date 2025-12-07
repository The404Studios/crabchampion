@echo off
title Crab Champions Trainer - Quick Start
color 0A

echo ============================================================
echo       CRAB CHAMPIONS TRAINER - QUICK START
echo ============================================================
echo.
echo    This will install dependencies, build, and run the trainer.
echo.
echo ============================================================
echo.
pause

:: Step 1: Check/Install .NET
echo.
echo [STEP 1/4] Checking .NET SDK...
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [INFO] .NET SDK not found. Installing...
    call install_dependencies.bat
) else (
    echo [OK] .NET SDK found
    dotnet --version
)

:: Step 2: Restore packages
echo.
echo [STEP 2/4] Restoring packages...
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo [WARNING] Package restore had issues, continuing anyway...
)

:: Step 3: Build
echo.
echo [STEP 3/4] Building trainer...
dotnet build -c Release
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Build failed!
    echo Please check for errors above.
    pause
    exit /b 1
)

:: Step 4: Run
echo.
echo [STEP 4/4] Starting trainer...
echo.
echo ============================================================
echo    TRAINER IS NOW RUNNING
echo ============================================================
echo.
echo Hotkeys:
echo   F1: God Mode          F5: Add Keys
echo   F2: Infinite Health   F6: Add Crystals
echo   F3: Infinite Ammo     F7: ALL PRISMATICS
echo   F4: No Clip           F8: All Items
echo   INSERT: Toggle Menu   HOME: Toggle Overlay
echo.
echo Close this window to stop the trainer.
echo ============================================================
echo.

dotnet run -c Release
