@echo off
title Crab Champions Trainer - Build Script
color 0A

echo ============================================================
echo    CRAB CHAMPIONS TRAINER - BUILD SCRIPT
echo ============================================================
echo.

:: Check if .NET SDK is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [ERROR] .NET SDK not found!
    echo.
    echo Please install .NET 8.0 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    echo Or run install_dependencies.bat first.
    pause
    exit /b 1
)

echo [INFO] .NET SDK found
dotnet --version
echo.

:: Restore NuGet packages
echo [STEP 1/3] Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Failed to restore packages!
    pause
    exit /b 1
)
echo [OK] Packages restored successfully
echo.

:: Build the project
echo [STEP 2/3] Building project (Release mode)...
dotnet build -c Release
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)
echo [OK] Build completed successfully
echo.

:: Publish self-contained
echo [STEP 3/3] Publishing self-contained executable...
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Publish failed!
    pause
    exit /b 1
)
echo [OK] Published successfully
echo.

echo ============================================================
echo    BUILD COMPLETE!
echo ============================================================
echo.
echo Output location: %CD%\publish\
echo Executable: CrabChampionsTrainer.exe
echo.
echo You can now run the trainer from the publish folder.
echo.
pause
