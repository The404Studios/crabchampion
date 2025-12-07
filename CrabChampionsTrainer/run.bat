@echo off
title Crab Champions Trainer
color 0E

echo ============================================================
echo    CRAB CHAMPIONS TRAINER - LAUNCHER
echo ============================================================
echo.

:: Check if published build exists
if exist "publish\CrabChampionsTrainer.exe" (
    echo [INFO] Starting trainer from published build...
    cd publish
    start "" "CrabChampionsTrainer.exe"
    exit
)

:: Check if we can run with dotnet
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [ERROR] .NET SDK not found and no published build exists!
    echo.
    echo Please run install_dependencies.bat first,
    echo then run build.bat to compile the trainer.
    echo.
    pause
    exit /b 1
)

:: Run with dotnet
echo [INFO] Starting trainer with dotnet run...
echo.
dotnet run -c Release
