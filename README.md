# Crab Champions Editor Tool

A comprehensive modding/trainer toolkit for Crab Champions with two components:

1. **UE4SS Lua Mod** - In-game mod that runs within the game
2. **C# Trainer Application** - Standalone Windows application with GUI

---

## Features

### Player Modifications
- God Mode (invincibility)
- Infinite Health / Infinite Ammo
- Speed, Jump, Damage Multipliers
- No Clip Mode
- One Hit Kill

### ⭐ Items & Prismatics
- **GIVE ALL PRISMATICS** - One button to unlock all prismatic abilities
- Give all items/upgrades
- Spawn by category (Combat, Defense, Movement, Utility)
- Spawn individual prismatics/items
- Random loadout generator

### Currency Editor
- Set/Add Keys and Crystals
- Quick MAX buttons
- Hotkey support

### Weapon Modifications
- Dual Wield any weapon
- Fire Rate / Damage Multipliers
- Infinite Magazine / No Reload
- No Recoil / No Spread
- Give all weapons

### Unlock System
- Unlock all skins
- Unlock all cosmetics
- Debug unlock everything

---

## Option 1: C# Trainer Application (Recommended)

The standalone trainer with a modern GUI interface.

### Requirements
- Windows 10/11
- .NET 8.0 Runtime
- Crab Champions (Steam version)

### Building
```bash
cd CrabChampionsTrainer
dotnet build
dotnet run
```

### Features
- Modern dark-themed GUI
- Auto-attach to game
- Real-time stats display
- Global hotkeys (work even when game is focused)
- Settings persistence
- Tabbed interface for organization

### Screenshot Features
- **Player Tab**: Toggles, multiplier sliders, quick actions
- **Items & Prismatics Tab**: One-click "Give All Prismatics" button!
- **Currency Tab**: Keys/Crystals editor with presets
- **Weapons Tab**: Weapon mods and give weapons
- **Unlocks Tab**: Unlock everything
- **Settings Tab**: Configuration options

### Hotkeys (Global)
| Key | Action |
|-----|--------|
| `F1` | God Mode |
| `F2` | Infinite Health |
| `F3` | Infinite Ammo |
| `F4` | No Clip |
| `F5` | Add 100 Keys |
| `F6` | Add 100 Crystals |
| `F7` | **Give ALL Prismatics** |
| `F8` | Give ALL Items |
| `F12` | Reset All |

---

## Option 2: UE4SS Lua Mod

In-game mod using the UE4SS framework.

### Requirements
- Crab Champions (Steam version)
- [UE4SS v3.0.0+](https://github.com/UE4SS-RE/RE-UE4SS/releases)

### Installation

1. **Install UE4SS**
   ```
   1. Download the latest UE4SS release (xinput version)
   2. Navigate to your Crab Champions install:
      C:\Program Files (x86)\Steam\steamapps\common\Crab Champions\CrabChampions\Binaries\Win64\
   3. Extract UE4SS contents into this folder
   ```

2. **Install This Mod**
   ```
   1. Copy the "CrabChampionsEditor" folder to:
      CrabChampions\Binaries\Win64\Mods\
   2. Open Mods\mods.txt
   3. Add this line: CrabChampionsEditor : 1
   ```

3. **Launch the Game**
   - The mod will load automatically
   - Check the UE4SS console for confirmation

### In-Game Keybinds

| Key | Action |
|-----|--------|
| `F1` | Toggle Mod Menu |
| `F2` | Toggle God Mode |
| `F3` | Toggle Infinite Ammo |
| `F4` | Toggle No Clip |
| `F5` | Add 100 Keys |
| `F6` | Add 100 Crystals |
| `F7` | Spawn Random Item |
| `F8` | Spawn Prismatic |
| `F9` | Toggle Dual Wield |
| `F10` | Reset All Modifications |
| `Numpad +` | Increase Speed |
| `Numpad -` | Decrease Speed |

### Console Commands

Open UE4SS console (`~` key or check settings) and type:

```lua
-- Player Mods
CrabEditor.GodMode(true/false)
CrabEditor.InfiniteAmmo(true/false)
CrabEditor.SetSpeed(multiplier)
CrabEditor.SetJumpHeight(multiplier)
CrabEditor.NoClip(true/false)

-- Currency
CrabEditor.SetKeys(amount)
CrabEditor.SetCrystals(amount)
CrabEditor.AddKeys(amount)
CrabEditor.AddCrystals(amount)

-- Items
CrabEditor.SpawnItem("ItemName")
CrabEditor.SpawnRandomItem()
CrabEditor.SpawnPrismatic()
CrabEditor.ClearItems()

-- Weapons
CrabEditor.DualWield(true/false)
CrabEditor.SetFireRate(multiplier)
CrabEditor.SetDamage(multiplier)

-- Unlocks
CrabEditor.UnlockAllSkins()
CrabEditor.UnlockAllCosmetics()
```

---

## Project Structure

```
crabchampion/
├── README.md
├── LICENSE
├── mods.txt.example
│
├── CrabChampionsEditor/          # UE4SS Lua Mod
│   ├── enabled.txt
│   └── scripts/
│       ├── main.lua              # Core mod functionality
│       ├── config.lua            # User configuration
│       ├── sdk_helper.lua        # SDK discovery tools
│       └── item_database.lua     # Item/weapon data
│
└── CrabChampionsTrainer/         # C# Trainer Application
    ├── CrabChampionsTrainer.csproj
    ├── Program.cs
    ├── Core/
    │   ├── Memory.cs             # Memory read/write
    │   ├── GameManager.cs        # Game interaction
    │   ├── HotkeyManager.cs      # Global hotkeys
    │   └── SettingsManager.cs    # Settings persistence
    ├── Data/
    │   ├── GameData.cs           # Items, weapons, prismatics
    │   └── GameOffsets.cs        # Memory offsets
    └── UI/
        └── MainForm.cs           # Main window
```

---

## Generating SDK Dump

To discover game classes and update offsets:

1. Launch the game with UE4SS installed
2. Press `Ctrl+J` to dump all objects to `UE4SS_ObjectDump.txt`
3. Press `Ctrl+H` to generate C++ headers
4. Check the `Mods\` folder for output files

---

## Updating Offsets

The C# trainer uses memory offsets that may change when the game updates. To find new offsets:

1. Use Cheat Engine to find values
2. Use UE4SS SDK dump to find class structures
3. Update `GameOffsets.cs` with new values

---

## Disclaimer

- This tool is for **single-player/solo use only**
- The developer (Noisestorm) tolerates modding but use responsibly
- Using cheats in multiplayer may affect other players' experience
- No warranty provided - use at your own risk

---

## Credits

- Built with [UE4SS](https://github.com/UE4SS-RE/RE-UE4SS)
- Crab Champions by [Noisestorm](https://store.steampowered.com/app/774801/Crab_Champions/)
- Community resources from FearLess Revolution and GameBanana

## License

MIT License - Feel free to modify and distribute
