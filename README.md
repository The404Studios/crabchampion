# Crab Champions Editor

A UE4SS Lua mod for Crab Champions that provides an in-game menu with cheats, item spawning, and more.

## Features

### Player Cheats
- **God Mode** - Prevent all damage
- **Infinite Health** - Keep health at maximum
- **Infinite Shield** - Keep shield at maximum
- **NoClip** - Fly through walls

### Movement Modifiers
- Speed multiplier (0.1x to 10x)
- Jump height multiplier (0.1x to 10x)
- Gravity scale (0x to 3x)

### Currency
- Set/add Keys
- Set/add Crystals
- Infinite Keys toggle
- Infinite Crystals toggle
- Max Currency button

### Items & Equipment
- Give all Prismatics
- Give all Items
- Give all Weapons
- Spawn individual items from menu

## Requirements

- [Crab Champions](https://store.steampowered.com/app/774801/Crab_Champions/) (Steam)
- [UE4SS](https://github.com/UE4SS-RE/RE-UE4SS/releases) (Unreal Engine Scripting System)

## Installation

### Automatic (Recommended)

1. Download this repository
2. Run `install.bat` (Windows) or `install.ps1` (PowerShell)
3. The installer will:
   - Find your Crab Champions installation
   - Check for UE4SS (download if needed)
   - Install the mod files
   - Configure settings

### Manual Installation

1. **Install UE4SS:**
   - Download the latest [UE4SS release](https://github.com/UE4SS-RE/RE-UE4SS/releases)
   - Extract to: `[Game Folder]\CrabChampions\Binaries\Win64\`

2. **Install the Mod:**
   - Copy the `CrabEditor` folder to: `[Game Folder]\CrabChampions\Binaries\Win64\Mods\`

3. **Configure UE4SS:**
   - Edit `UE4SS-settings.ini` in the Win64 folder
   - Set `EnableImGui = 1`
   - Set `GuiConsoleEnabled = true`

## Usage

1. Launch Crab Champions
2. Press **F1** to open the mod menu
3. Use the tabs to navigate between features

## Hotkeys

| Key | Function |
|-----|----------|
| F1 | Toggle Menu |
| F2 | Toggle God Mode |
| F3 | Toggle Infinite Health |
| F4 | Max Currency |
| F5 | Give All Prismatics |
| F6 | Give All Items |
| F7 | Give All Weapons |
| F9 | Toggle NoClip |

## Troubleshooting

### Mod not loading
1. Verify UE4SS is installed correctly
2. Check that `enabled.txt` contains `1` in the mod folder
3. Press `~` (tilde) to open UE4SS console and check for errors

### Menu not appearing
1. Verify `EnableImGui = 1` in UE4SS-settings.ini
2. Try pressing F1 multiple times
3. Check if other mods are conflicting

### Features not working
- Some features require being in an active game (not main menu)
- Property names may change between game updates
- Check the UE4SS console for error messages

## File Structure

```
CrabEditor/
├── enabled.txt          # Enables the mod (contains "1")
└── Scripts/
    ├── main.lua         # Entry point, initialization
    ├── Config.lua       # Settings and hotkey configuration
    ├── Features.lua     # Cheat implementations
    ├── Menu.lua         # ImGui menu rendering
    ├── Database.lua     # Items, weapons, prismatics data
    └── Utils.lua        # Helper functions
```

## Modifying the Mod

### Adding new prismatics/items
Edit `Database.lua` and add entries to the appropriate table:
```lua
{ Name = "My Item", Class = "BP_Item_MyItem", Description = "Does something" },
```

### Adding new hotkeys
Edit `Config.lua`:
```lua
Config.Hotkeys.MyFeature = Key.F10
```
Then register in `main.lua`:
```lua
RegisterKeyBind(Config.Hotkeys.MyFeature, function()
    -- Your code here
end)
```

### Adding new features
1. Add the feature function to `Features.lua`
2. Add a toggle to `Config.Settings` in `Config.lua`
3. Add UI in `Menu.lua` under the appropriate tab
4. Call the feature in `Features.ApplyFeatures()` if it needs continuous updates

## Disclaimer

This mod is for **single-player use only**. Use responsibly.

The developer of Crab Champions (Noisestorm) tolerates modding for single-player. Please respect this by not using mods to gain unfair advantages in multiplayer modes.

## Credits

- **UE4SS Team** - For the amazing modding framework
- **Crab Champions Community** - For game data and testing

## License

This project is provided as-is for educational and personal use.
