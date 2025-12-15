using System;
using System.Collections.Generic;

namespace CrabChampionsSaveEditor.Models
{
    /// <summary>
    /// Reference information for Crab Champions cheat tables
    /// Based on publicly available cheat tables from FearLess Revolution, GuidedHacking, etc.
    ///
    /// IMPORTANT: Offsets change with each game update. These are for reference only.
    /// Always download the latest cheat table from the sources below.
    /// </summary>
    public static class CheatTableReference
    {
        // =============================================================
        // CHEAT TABLE SOURCES (most up-to-date)
        // =============================================================
        public static readonly Dictionary<string, string> CheatTableSources = new()
        {
            ["FearLess Revolution"] = "https://fearlessrevolution.com/viewtopic.php?t=28134",
            ["GuidedHacking"] = "https://guidedhacking.com/resources/crab-champions-cheat-engine-table.1186/",
            ["OpenCheatTables"] = "https://opencheattables.org/viewtopic.php?t=110229",
            ["GamePressure"] = "https://www.gamepressure.com/download.asp?ID=83682",
        };

        // =============================================================
        // KNOWN CHEAT TABLE FEATURES (FearLess Revolution table)
        // =============================================================
        public static readonly string[] FearLessFeatures = new[]
        {
            // Player
            "Infinite Health",
            "Infinite Armor",
            "No Cooldowns",
            "Infinite Jump",
            "Super Speed",
            "Super Jump",

            // Weapons
            "Infinite Ammo",
            "No Reload",
            "Rapid Fire",
            "One Hit Kill",

            // Currency
            "Infinite Crystals",
            "Infinite Keys",

            // Items/Perks (Live Modification)
            "Unlock All Weapons",
            "Unlock All Abilities",
            "Unlock All Melee",
            "Give Any Perk",
            "Give Any Weapon Mod",
            "Give Any Ability Mod",
            "Give Any Melee Mod",
            "Give Any Relic",

            // Game
            "Instant Win Wave",
            "Skip To Boss",
            "Freeze Enemies",
            "Kill All Enemies",

            // Cosmetics
            "Unlock All Skins",
            "Change Skin Mid-Game",
        };

        // =============================================================
        // KNOWN POINTER PATTERNS (for Cheat Engine)
        // These patterns help find game data in memory
        // =============================================================
        public static readonly Dictionary<string, string> AOBPatterns = new()
        {
            // UWorld pointer (common starting point)
            ["UWorld"] = "48 8B 1D ?? ?? ?? ?? 48 85 DB 74 ?? 41 B0 01",

            // GEngine pointer
            ["GEngine"] = "48 8B 05 ?? ?? ?? ?? 48 8B 88 ?? 00 00 00 48 85 C9",

            // Player Controller
            ["PlayerController"] = "48 8B 05 ?? ?? ?? ?? 48 8B 88 ?? ?? ?? ?? 48 85 C9 74",

            // Health Pattern (float comparison)
            ["Health"] = "F3 0F 10 ?? ?? ?? 00 00 0F 2F ?? F3 0F 10",

            // Ammo Pattern
            ["Ammo"] = "89 ?? ?? 00 00 00 83 ?? ?? 00 00 00 00 7E",

            // Currency Pattern (crystal/key pickup)
            ["Currency"] = "01 ?? ?? ?? 00 00 8B ?? ?? ?? 00 00",
        };

        // =============================================================
        // ITEM SPAWN CODES (for console/UE4 commands if accessible)
        // =============================================================
        public static readonly Dictionary<string, string> SpawnCommands = new()
        {
            // Weapons
            ["AutoRifle"] = "SpawnItem DA_Weapon_AutoRifle",
            ["Sniper"] = "SpawnItem DA_Weapon_Sniper",
            ["RocketLauncher"] = "SpawnItem DA_Weapon_RocketLauncher",

            // Perks (example format)
            ["GlassCannon"] = "GivePerk DA_Perk_GlassCannon",
            ["GodMode"] = "GivePerk DA_Perk_Juggernaut",

            // Mods
            ["BouncingShot"] = "GiveWeaponMod DA_WeaponMod_BouncingShot",
            ["FireShot"] = "GiveWeaponMod DA_WeaponMod_FireShot",
        };

        // =============================================================
        // OFFSET STRUCTURE (example from CT file)
        // Offsets are version-specific and change frequently
        // =============================================================
        public class GameOffsets
        {
            public string GameVersion { get; set; } = "";
            public long UWorldOffset { get; set; }
            public long GEngineOffset { get; set; }
            public long PlayerControllerOffset { get; set; }

            // Player offsets (from PlayerController)
            public long HealthOffset { get; set; }
            public long MaxHealthOffset { get; set; }
            public long ArmorOffset { get; set; }
            public long CrystalsOffset { get; set; }
            public long KeysOffset { get; set; }

            // Inventory offsets
            public long InventoryOffset { get; set; }
            public long WeaponsArrayOffset { get; set; }
            public long PerksArrayOffset { get; set; }
            public long ModsArrayOffset { get; set; }
            public long RelicsArrayOffset { get; set; }
        }

        // Example offsets (OUTDATED - use CT file)
        public static readonly GameOffsets SampleOffsets_V2003 = new()
        {
            GameVersion = "V2003",
            UWorldOffset = 0x04B89D68,
            GEngineOffset = 0x04C04100,

            // These are example offsets - NOT guaranteed to work
            HealthOffset = 0x348,
            MaxHealthOffset = 0x34C,
            ArmorOffset = 0x350,
            CrystalsOffset = 0x2F0,
            KeysOffset = 0x2F4,

            InventoryOffset = 0x500,
            WeaponsArrayOffset = 0x510,
            PerksArrayOffset = 0x520,
            ModsArrayOffset = 0x530,
            RelicsArrayOffset = 0x540,
        };

        // =============================================================
        // HOW TO UPDATE OFFSETS
        // =============================================================
        public static readonly string[] UpdateInstructions = new[]
        {
            "1. Download Cheat Engine from https://cheatengine.org/",
            "2. Download the latest CT file from FearLess Revolution",
            "3. Open Crab Champions and attach Cheat Engine",
            "4. Load the CT file in Cheat Engine",
            "5. If pointers are broken, use AOB scanning:",
            "   - Search for the AOB patterns above",
            "   - Follow the pointer chain to find values",
            "   - Update the offsets in the CT file or this code",
            "6. For item injection:",
            "   - Find the inventory array structure",
            "   - Identify how items are stored (UObject pointers)",
            "   - Use Lua scripts or code injection to add items",
        };

        // =============================================================
        // ITEM DATA ASSET PATHS (for UE4 item spawning)
        // These are the actual asset paths used by the game
        // =============================================================
        public static string GetWeaponPath(string weaponId)
            => $"/Game/Blueprint/Weapon/{weaponId}/DA_Weapon_{weaponId}.DA_Weapon_{weaponId}";

        public static string GetAbilityPath(string abilityId)
            => $"/Game/Blueprint/Ability/DA_Ability_{abilityId}.DA_Ability_{abilityId}";

        public static string GetMeleePath(string meleeId)
            => $"/Game/Blueprint/Melee/DA_Melee_{meleeId}.DA_Melee_{meleeId}";

        public static string GetPerkPath(string perkId, string rarity)
            => $"/Game/Blueprint/Pickup/Perk/{rarity}/DA_Perk_{perkId}.DA_Perk_{perkId}";

        public static string GetWeaponModPath(string modId, string rarity)
            => $"/Game/Blueprint/Pickup/WeaponMod/{rarity}/DA_WeaponMod_{modId}.DA_WeaponMod_{modId}";

        public static string GetAbilityModPath(string modId, string rarity)
            => $"/Game/Blueprint/Pickup/AbilityMod/{rarity}/DA_AbilityMod_{modId}.DA_AbilityMod_{modId}";

        public static string GetMeleeModPath(string modId, string rarity)
            => $"/Game/Blueprint/Pickup/MeleeMod/{rarity}/DA_MeleeMod_{modId}.DA_MeleeMod_{modId}";

        public static string GetRelicPath(string relicId, string rarity)
            => $"/Game/Blueprint/Pickup/Relic/{rarity}/DA_Relic_{relicId}.DA_Relic_{relicId}";

        public static string GetSkinPath(string skinId)
            => $"/Game/Character/Crab/Texture/SkinPrototype/MI_{skinId}.MI_{skinId}";

        // =============================================================
        // LUA SCRIPT TEMPLATES (for Cheat Engine)
        // =============================================================
        public static readonly string LuaGiveItemTemplate = @"
-- Lua script template for giving items in Crab Champions
-- This requires finding the correct addresses first

function GiveItem(itemPath)
    -- Find inventory component
    local inventoryAddr = readQword(getAddress('InventoryPointer'))
    if inventoryAddr == 0 then return false end

    -- Find item array
    local itemArrayAddr = readQword(inventoryAddr + 0x10)
    local itemCount = readInteger(inventoryAddr + 0x18)

    -- Add item to array (simplified - actual implementation more complex)
    -- This would call the game's native AddItem function

    print('Giving item: ' .. itemPath)
    return true
end

-- Usage: GiveItem('/Game/Blueprint/Pickup/Perk/Epic/DA_Perk_MegaCrit.DA_Perk_MegaCrit')
";

        public static readonly string LuaUnlockAllTemplate = @"
-- Lua script template for unlocking all items
-- This modifies the save data pointers in memory

function UnlockAllItems()
    local saveDataAddr = getAddress('SaveDataPointer')
    if saveDataAddr == 0 then
        print('Save data not found')
        return
    end

    -- Find unlock arrays and add all items
    -- Implementation depends on game version

    print('Unlocking all items...')
end
";
    }
}
