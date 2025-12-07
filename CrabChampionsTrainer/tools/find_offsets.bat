@echo off
title Crab Champions - Offset Finder Helper
color 0B

echo ============================================================
echo    CRAB CHAMPIONS - OFFSET FINDER HELPER
echo ============================================================
echo.
echo This guide will help you find memory offsets when the game updates.
echo.
echo ============================================================
echo    REQUIRED TOOLS
echo ============================================================
echo.
echo 1. Cheat Engine (CE) - https://cheatengine.org/
echo 2. UE4SS - https://github.com/UE4SS-RE/RE-UE4SS/releases
echo.
echo ============================================================
echo    METHOD 1: USING CHEAT ENGINE
echo ============================================================
echo.
echo FINDING HEALTH:
echo   1. Start Crab Champions and start a run
echo   2. Attach Cheat Engine to "CrabChampions-Win64-Shipping.exe"
echo   3. Search for your current health value (float)
echo   4. Take damage, search for new value
echo   5. Repeat until you find the address
echo   6. Right-click address ^> "Find what writes to this address"
echo   7. Note the instruction and offset
echo.
echo FINDING KEYS/CRYSTALS:
echo   1. Press TAB in-game to show stats
echo   2. Search for your key count (4 bytes)
echo   3. Collect/spend keys, search for new value
echo   4. Find the stable address
echo.
echo FINDING BASE POINTERS (GWorld):
echo   1. In CE, go to Memory View ^> Tools ^> Dissect Code/Data
echo   2. Search for pattern: 48 8B 1D ?? ?? ?? ?? 48 85 DB 74 3B
echo   3. This finds GWorld pointer
echo.
echo ============================================================
echo    METHOD 2: USING UE4SS
echo ============================================================
echo.
echo 1. Download UE4SS (xinput version)
echo 2. Extract to: CrabChampions\Binaries\Win64\
echo 3. Launch game, press Ctrl+J to dump objects
echo 4. Check UE4SS_ObjectDump.txt for class names
echo 5. Press Ctrl+H to generate C++ headers
echo 6. Use Live View in UE4SS console to browse objects
echo.
echo ============================================================
echo    COMMON OFFSET PATTERNS
echo ============================================================
echo.
echo UE4 Standard Offsets (may vary by version):
echo   - GWorld to OwningGameInstance: 0x1B8
echo   - GameInstance to LocalPlayers: 0x38
echo   - LocalPlayer to PlayerController: 0x30
echo   - PlayerController to Pawn: 0x338
echo   - Movement Component: +0x320 from Character
echo   - MaxWalkSpeed: +0x2C4 from MovementComponent
echo   - JumpZVelocity: +0x3A0 from MovementComponent
echo.
echo ============================================================
echo    RESOURCES
echo ============================================================
echo.
echo - FearLess Revolution: https://fearlessrevolution.com/
echo - Guided Hacking: https://guidedhacking.com/
echo - OpenCheatTables: https://opencheattables.org/
echo - UE4SS Docs: https://docs.ue4ss.com/
echo.
echo After finding new offsets, update:
echo   CrabChampionsTrainer\Data\GameOffsets.cs
echo.
pause
