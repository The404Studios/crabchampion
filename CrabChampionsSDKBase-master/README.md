# Crab Champions - Gogurt Menu

**Gogurt** is an internal menu and runtime tool for *Crab Champions* built with ImGui and MinHook. It provides advanced player modifications, combat enhancements, and real-time HUD information.  

**FYI ChatGPT Helped A lot..**
---


## Features

### Main
- **Enemy ESP** – Highlights enemies with boxes and health info on-screen.
- **Godmode** – Instantly restores player health to maximum.
- **Infinite Ammo** – Ammo count never decreases.
- **No Reload** – Eliminates reload delays.
- **No Cooldowns** – Removes ability and melee cooldowns.
- **No Spread / Zero Recoil** – Removes weapon spread and recoil for precision shooting.

### Stats / Player Info
- **Override Rank** – Set your rank (`None` → `Diamond`).
- **Override Level** – Set your player level (1 → 100).
- **Override Keys** – Modify key count (0 → 999).
- **Override Crystals** – Modify crystal count (0 → 9999).

### Movement
- **Base Walk Speed** – Adjust movement speed (0 → 3000 units/s).
- **Air Control** – Adjust control while in the air (0 → 5).
- **Ground Friction** – Adjust friction affecting movement (0 → 10).
- **Flip Height** – Maximum flip height (0 → 3000 units).
- **Dash Height** – Maximum dash height (0 → 3000 units).
- **Max Acceleration** – Adjust acceleration (0 → 5000 units/s²).
- **Dash Cooldown** – Adjust dash cooldown (0 → 10 s).
- **Player Scale** – Scale character size (0.1 → 5×).

### Combat
- **Damage Multiplier** – Scale damage dealt (0 → 10×).
- **Health Multiplier** – Scale max health (0 → 10×).
- **Fire Rate** – Adjust weapon fire rate (0.01 → 10×).
- **Vertical Recoil** – Adjust vertical recoil (0 → 50).
- **Horizontal Recoil** – Adjust horizontal recoil (0 → 50).
- **Recoil Interp Speed** – Adjust recoil interpolation speed (0 → 50).
- **Recoil Recovery Speed** – Adjust recoil recovery speed (0 → 50).
- **Melee Range** – Adjust melee attack range (50 → 2000 units).
- **Melee Damage** – Adjust melee damage (50 → 2000 units).

### Misc / Utilities
- **Show Player HUD** – Displays runtime player info (location, speed, health, etc.) in console.
- **Unhook DLL** – Safely unloads the menu DLL.
- **Dump Offsets** – Outputs `GWorld`, `LocalPlayers`, and key offsets to console.

---

## Implementation Notes

- Uses **ImGui** for GUI rendering and sliders.
- Applies **per-frame updates** for persistent values like player scale.
- Includes **robust exception handling** to prevent crashes when accessing game objects.
- Works with **local player pointers** (`GWorld` → `UGameInstance` → `LocalPlayers`) for runtime modifications.

## 📸 Screenshots

### Main Menu
![Main Menu](https://raw.githubusercontent.com/Landan420/CrabChampionsSDKBase/refs/heads/master/screenshots/image.webp)

### Player Info Overrides - Movement
![Movement Overrides](https://raw.githubusercontent.com/Landan420/CrabChampionsSDKBase/refs/heads/master/screenshots/image1.webp)

### Player Info Overrides - Combat
![Combat Overrides](https://raw.githubusercontent.com/Landan420/CrabChampionsSDKBase/refs/heads/master/screenshots/image2.webp)


---

## Disclaimer

This tool is intended for educational purposes only. Modifying game behavior may be against the game's Terms of Service. Use responsibly and at your own risk.
