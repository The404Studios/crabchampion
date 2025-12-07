# Crab Champions Editor Tool

A comprehensive modding/trainer tool for Crab Champions built with UE4SS (Unreal Engine 4 Scripting System).

## Features

- **Player Modifications**
  - God Mode (invincibility)
  - Infinite Ammo
  - Speed Multiplier
  - Jump Height Modifier
  - No Clip Mode

- **Item & Prismatic System**
  - Spawn any item/upgrade
  - Spawn prismatics
  - Randomize loadout
  - Clear all items

- **Currency Editor**
  - Edit Keys
  - Edit Crystals
  - Add/Remove currency

- **Weapon Modifications**
  - Dual Wield any weapon
  - Fire Rate Multiplier
  - Damage Multiplier
  - Infinite Magazine

- **Unlock System**
  - Unlock all skins
  - Unlock all cosmetics
  - Debug unlock content

## Requirements

- Crab Champions (Steam version)
- [UE4SS v3.0.0+](https://github.com/UE4SS-RE/RE-UE4SS/releases)

## Installation

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

## Keybinds

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

## Console Commands

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

## Generating SDK Dump

To discover more game classes and functions:

1. Launch the game with UE4SS installed
2. Press `Ctrl+J` to dump all objects to `UE4SS_ObjectDump.txt`
3. Press `Ctrl+H` to generate C++ headers
4. Check the `Mods\` folder for output files

## Disclaimer

- This mod is for **single-player/solo use only**
- The developer (Noisestorm) tolerates modding but use responsibly
- Using mods in multiplayer may affect other players' experience
- No warranty provided - use at your own risk

## Credits

- Built with [UE4SS](https://github.com/UE4SS-RE/RE-UE4SS)
- Crab Champions by [Noisestorm](https://store.steampowered.com/app/774801/Crab_Champions/)
- Community modding resources from FearLess Revolution and GameBanana

## License

MIT License - Feel free to modify and distribute
