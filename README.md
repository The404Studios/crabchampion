# Crab Champions Save Editor

A powerful, modern save file editor for **Crab Champions** built with WPF and .NET 8.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?style=flat-square&logo=windows)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

---

## Features

### Unlock Everything
- **Unlock All Weapons** - Instantly unlock all 20 primary weapons
- **Unlock All Abilities** - Unlock all 7 abilities
- **Unlock All Melee** - Unlock all 5 melee weapons
- **Unlock All Difficulties** - Access all difficulty tiers

### Set All to Prismatic
- Upgrade all your weapons, abilities, and melee to **Prismatic** rank (the highest tier!)
- Works on all items in your RankedWeapons array

### God Mode
One-click to:
- Unlock everything
- Set all items to Prismatic
- Max out currency
- Max all mastery levels

### Additional Features
- **Max Currency** - Set crystals and keys to 999,999
- **Max Mastery** - Max out all mastery levels
- **Stats Editor** - Modify game statistics
- **Preset Profiles** - Quick-apply common configurations
- **Full Property Browser** - View and edit any property in the save file
- **Hex Viewer** - Inspect raw binary data
- **Auto Backup** - Automatically creates backups before saving

---

## Supported Items

### Weapons (20)
| Weapon | Weapon | Weapon | Weapon |
|--------|--------|--------|--------|
| Auto Rifle | Dual Shotguns | Dual Pistols | Auto Shotgun |
| Burst Pistol | Sniper | Crossbow | Orb Launcher |
| Rocket Launcher | Minigun | Blade Launcher | Cluster Launcher |
| Flamethrower | Arcane Wand | Laser Cannons | Seagle |
| Marksman Rifle | Ice Staff | Lightning Scepter | Poison Cannon |

### Abilities (7)
| Ability | Ability | Ability | Ability |
|---------|---------|---------|---------|
| Grenade | Grappling Hook | Black Hole | Laser Beam |
| Ice Blast | Electro Globe | Air Strike | |

### Melee (5)
| Melee | Melee | Melee | Melee | Melee |
|-------|-------|-------|-------|-------|
| Claw | Dagger | Hammer | Pickaxe | Katana |

### Ranks
Bronze → Silver → Gold → Sapphire → Emerald → Ruby → Diamond → **Prismatic**

---

## Installation

### Prerequisites
- Windows 10/11
- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

### Download & Run
1. Download the latest release
2. Extract the zip file
3. Run `UnrealSavEditor.exe`

### Build from Source
```bash
git clone https://github.com/The404Studios/crabchampion.git
cd crabchampion
dotnet build -c Release
```

---

## Usage

### Quick Start
1. Launch the editor
2. Click **"Open Game Save"** (auto-detects save location)
3. Use the quick action buttons to modify your save
4. Click **"Save"** to save changes

### Save File Location
```
%LocalAppData%\CrabChampions\Saved\SaveGames\SaveSlot.sav
```

### Tips
- **Always backup your save** before making changes
- The editor automatically creates a `.backup` file when saving
- Use **God Mode** for a quick full unlock
- The property browser lets you edit any value manually

---

## Screenshots

The editor features a modern dark theme UI with:
- Orange accent colors matching Crab Champions branding
- Gradient "God Mode" button
- Real-time unlock status display
- Organized action cards for different feature categories

---

## Technical Details

### GVAS Parser
This editor includes a custom GVAS (Unreal Engine Save Game) parser that handles:
- Compressed saves (GZip/Zlib)
- All standard property types (Int, Float, Bool, String, Struct, Array, Map, etc.)
- Nested structures and arrays
- Binary serialization/deserialization

### Save Structure
The Crab Champions save file uses:
- `UnlockedWeapons` - Array of ObjectProperty paths
- `UnlockedAbilities` - Array of ObjectProperty paths
- `UnlockedMeleeWeapons` - Array of ObjectProperty paths
- `RankedWeapons` - Array of structs with Weapon + Rank (ECrabRank enum)

---

## Troubleshooting

### Save not loading?
- Make sure you've played Crab Champions at least once
- Check if the save file exists at the expected location
- Try running the editor as administrator

### Changes not working in game?
- Make sure the game is closed when editing
- Verify you clicked "Save" after making changes
- Check the backup file to restore if needed

### Items still locked after unlock?
- Some items may require being in the RankedWeapons array
- Try using "God Mode" which handles all arrays

---

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

---

## Disclaimer

This tool is for personal use only. Use at your own risk. Always backup your save files before making modifications. This project is not affiliated with or endorsed by the developers of Crab Champions.

---

## License

MIT License - See [LICENSE](LICENSE) for details.

---

Made with :crab: by The404Studios
