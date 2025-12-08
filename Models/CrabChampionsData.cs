using System;
using System.Collections.Generic;

namespace UnrealSavEditor.Models
{
    /// <summary>
    /// Contains all known Crab Champions game data for the save editor
    /// </summary>
    public static class CrabChampionsData
    {
        // ============================================
        // RARITY/MASTERY LEVELS (from lowest to highest)
        // ============================================
        public static readonly string[] Rarities =
        {
            "Common",
            "Uncommon",
            "Rare",
            "Epic",
            "Legendary",
            "Prismatic"
        };

        public static readonly string[] DifficultyTiers =
        {
            "Bronze",
            "Silver",
            "Gold",
            "Sapphire",
            "Emerald",
            "Ruby",
            "Diamond",
            "Prismatic"
        };

        // ============================================
        // PRIMARY WEAPONS (18 total)
        // ============================================
        public static readonly WeaponInfo[] PrimaryWeapons =
        {
            // Starting weapons
            new("AutoRifle", "Auto Rifle", "Rifle", true),
            new("DualShotguns", "Dual Shotguns", "Shotgun", true),

            // Unlockable weapons
            new("Minigun", "Minigun", "LMG", false),
            new("Sniper", "Sniper", "Sniper", false),
            new("RocketLauncher", "Rocket Launcher", "Launcher", false),
            new("Flamethrower", "Flamethrower", "Special", false),
            new("AutoShotgun", "Auto Shotgun", "Shotgun", false),
            new("PumpShotgun", "Pump Shotgun", "Shotgun", false),
            new("DualPistols", "Dual Pistols", "Pistol", false),
            new("BurstPistol", "Burst Pistol", "Pistol", false),
            new("ShotgunPistol", "Shotgun Pistol", "Pistol", false),
            new("SMG", "SMG", "SMG", false),
            new("Crossbow", "Crossbow", "Sniper", false),
            new("BladeLauncher", "Blade Launcher", "Launcher", false),
            new("OrbLauncher", "Orb Launcher", "Launcher", false),
            new("ClusterLauncher", "Cluster Launcher", "Launcher", false),
            new("Wand", "Wand", "Special", false),
            new("IceStaff", "Ice Staff", "Special", false),
        };

        // ============================================
        // SECONDARY WEAPONS / ABILITIES (5 total)
        // ============================================
        public static readonly WeaponInfo[] SecondaryWeapons =
        {
            new("Grenade", "Grenade", "Explosive", true),
            new("GrapplingHook", "Grappling Hook", "Utility", false),
            new("LaserBeam", "Laser Beam", "Energy", false),
            new("ElectroGlobe", "Electro Globe", "Energy", false),
            new("Shockwave", "Shockwave", "Area", false),
        };

        // ============================================
        // MELEE WEAPONS (3 total)
        // ============================================
        public static readonly WeaponInfo[] MeleeWeapons =
        {
            new("Claws", "Claws", "Melee", true),
            new("Sword", "Sword", "Melee", false),
            new("Hammer", "Hammer", "Melee", false),
        };

        // ============================================
        // PERKS (common upgrade type)
        // ============================================
        public static readonly string[] PerkCategories =
        {
            "Damage",
            "Defense",
            "Utility",
            "Movement",
            "Luck"
        };

        // ============================================
        // RELICS
        // ============================================
        public static readonly string[] RelicTypes =
        {
            "Common",
            "Rare",
            "Legendary",
            "Cursed"
        };

        // ============================================
        // PROPERTY NAME PATTERNS FOR SAVE FILE
        // ============================================
        public static class PropertyPatterns
        {
            // Unlock patterns
            public static readonly string[] UnlockedWeapons = { "UnlockedWeapons", "WeaponUnlocks", "Weapons", "UnlockedPrimaryWeapons" };
            public static readonly string[] UnlockedAbilities = { "UnlockedAbilities", "AbilityUnlocks", "Abilities", "UnlockedSecondaryWeapons" };
            public static readonly string[] UnlockedMelee = { "UnlockedMelee", "MeleeUnlocks", "UnlockedMeleeWeapons" };

            // Mastery/Rarity patterns
            public static readonly string[] WeaponMastery = { "WeaponMastery", "Mastery", "WeaponLevels", "WeaponRarity" };
            public static readonly string[] AbilityMastery = { "AbilityMastery", "AbilityLevels", "AbilityRarity" };

            // Stats patterns
            public static readonly string[] TotalKills = { "TotalKills", "Kills", "EnemiesKilled" };
            public static readonly string[] TotalRuns = { "TotalRuns", "RunsCompleted", "GamesPlayed" };
            public static readonly string[] HighestWave = { "HighestWave", "MaxWave", "BestWave" };
            public static readonly string[] TotalPlayTime = { "TotalPlayTime", "PlayTime", "TimePlayed" };

            // Currency patterns
            public static readonly string[] Crystals = { "Crystals", "TotalCrystals", "Currency" };
            public static readonly string[] Keys = { "Keys", "TotalKeys", "KeyCount" };

            // Challenges patterns
            public static readonly string[] CompletedChallenges = { "CompletedChallenges", "Challenges", "Achievements" };
            public static readonly string[] UnlockedDifficulties = { "UnlockedDifficulties", "Difficulties", "DifficultyUnlocks" };
        }

        // ============================================
        // SAVE MODIFICATION HELPERS
        // ============================================

        /// <summary>
        /// Get the maximum mastery level value
        /// </summary>
        public static int GetMaxMasteryLevel() => 100;

        /// <summary>
        /// Get prismatic rarity index
        /// </summary>
        public static int GetPrismaticRarityIndex() => Rarities.Length - 1; // 5 = Prismatic

        /// <summary>
        /// Get all weapon IDs for unlocking
        /// </summary>
        public static List<string> GetAllWeaponIds()
        {
            var ids = new List<string>();
            foreach (var w in PrimaryWeapons) ids.Add(w.Id);
            return ids;
        }

        /// <summary>
        /// Get all ability IDs for unlocking
        /// </summary>
        public static List<string> GetAllAbilityIds()
        {
            var ids = new List<string>();
            foreach (var a in SecondaryWeapons) ids.Add(a.Id);
            return ids;
        }

        /// <summary>
        /// Get all melee IDs for unlocking
        /// </summary>
        public static List<string> GetAllMeleeIds()
        {
            var ids = new List<string>();
            foreach (var m in MeleeWeapons) ids.Add(m.Id);
            return ids;
        }
    }

    /// <summary>
    /// Represents a weapon/ability in the game
    /// </summary>
    public class WeaponInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public bool IsStarterWeapon { get; }

        public WeaponInfo(string id, string displayName, string category, bool isStarter)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            IsStarterWeapon = isStarter;
        }
    }
}
