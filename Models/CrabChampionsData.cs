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
        // PERKS / UPGRADES (In-game powerups)
        // ============================================
        public static readonly PerkInfo[] Perks =
        {
            // Damage perks
            new("DamageUp", "Damage Up", "Damage", "Increases damage dealt"),
            new("CritChance", "Critical Chance", "Damage", "Increases critical hit chance"),
            new("CritDamage", "Critical Damage", "Damage", "Increases critical hit damage"),
            new("FireRate", "Fire Rate", "Damage", "Increases attack speed"),
            new("Multishot", "Multishot", "Damage", "Chance to fire additional projectiles"),
            new("Piercing", "Piercing", "Damage", "Projectiles pierce through enemies"),
            new("ExplosiveRounds", "Explosive Rounds", "Damage", "Attacks explode on impact"),
            new("ChainLightning", "Chain Lightning", "Damage", "Attacks chain to nearby enemies"),

            // Defense perks
            new("MaxHealth", "Max Health", "Defense", "Increases maximum health"),
            new("HealthRegen", "Health Regen", "Defense", "Regenerate health over time"),
            new("Armor", "Armor", "Defense", "Reduces damage taken"),
            new("DodgeChance", "Dodge Chance", "Defense", "Chance to avoid damage"),
            new("Shield", "Shield", "Defense", "Gain a protective shield"),
            new("Lifesteal", "Lifesteal", "Defense", "Heal on dealing damage"),
            new("DamageReduction", "Damage Reduction", "Defense", "Flat damage reduction"),

            // Movement perks
            new("MoveSpeed", "Move Speed", "Movement", "Increases movement speed"),
            new("JumpHeight", "Jump Height", "Movement", "Increases jump height"),
            new("DoubleJump", "Double Jump", "Movement", "Gain an extra jump"),
            new("DashDistance", "Dash Distance", "Movement", "Increases dash range"),
            new("DashCooldown", "Dash Cooldown", "Movement", "Reduces dash cooldown"),

            // Utility perks
            new("CooldownReduction", "Cooldown Reduction", "Utility", "Reduces ability cooldowns"),
            new("LuckUp", "Luck Up", "Utility", "Increases item drop quality"),
            new("XPGain", "XP Gain", "Utility", "Increases experience gained"),
            new("GoldFind", "Gold Find", "Utility", "Increases crystal drops"),
            new("MagnetRange", "Magnet Range", "Utility", "Increases pickup range"),
            new("ReviveChance", "Second Wind", "Utility", "Chance to revive on death"),
        };

        // ============================================
        // GAME MODES
        // ============================================
        public static readonly string[] GameModes =
        {
            "Classic",
            "Endless",
            "Challenge",
            "Daily",
            "Weekly"
        };

        // ============================================
        // ISLAND BIOMES
        // ============================================
        public static readonly string[] Biomes =
        {
            "Beach",
            "Jungle",
            "Volcano",
            "Crystal",
            "Ice",
            "Void"
        };

        // ============================================
        // BOSS TYPES
        // ============================================
        public static readonly string[] Bosses =
        {
            "KingCrab",
            "GiantSquid",
            "MegaShark",
            "LavaGolem",
            "IceDragon",
            "VoidKraken"
        };

        // ============================================
        // STAT TRACKING CATEGORIES
        // ============================================
        public static readonly StatInfo[] TrackedStats =
        {
            new("TotalKills", "Total Kills", "Combat", "Total enemies killed"),
            new("TotalDeaths", "Total Deaths", "Combat", "Total times died"),
            new("BossesKilled", "Bosses Killed", "Combat", "Total bosses defeated"),
            new("DamageDealt", "Damage Dealt", "Combat", "Total damage inflicted"),
            new("DamageTaken", "Damage Taken", "Combat", "Total damage received"),
            new("CriticalHits", "Critical Hits", "Combat", "Total critical hits landed"),

            new("TotalRuns", "Total Runs", "Progress", "Total runs started"),
            new("RunsCompleted", "Runs Completed", "Progress", "Successful run completions"),
            new("HighestWave", "Highest Wave", "Progress", "Best wave reached"),
            new("HighestDifficulty", "Highest Difficulty", "Progress", "Hardest difficulty beaten"),

            new("TotalPlayTime", "Play Time", "General", "Total time played (seconds)"),
            new("CrystalsCollected", "Crystals Collected", "General", "Total crystals earned"),
            new("KeysCollected", "Keys Collected", "General", "Total keys found"),
            new("ItemsCollected", "Items Collected", "General", "Total items picked up"),
            new("PerksObtained", "Perks Obtained", "General", "Total perks collected"),

            new("ShotsFired", "Shots Fired", "Misc", "Total projectiles fired"),
            new("Accuracy", "Accuracy", "Misc", "Hit percentage"),
            new("DistanceTraveled", "Distance Traveled", "Misc", "Total distance moved"),
            new("JumpsPerformed", "Jumps", "Misc", "Total jumps"),
        };

        // ============================================
        // PRESET PROFILES
        // ============================================
        public static readonly PresetProfile[] Presets =
        {
            new("GodMode", "God Mode", "Unlock everything, max stats, prismatic rarity",
                unlockAll: true, prismatic: true, maxCurrency: true, maxMastery: true),
            new("AllUnlocks", "All Unlocks", "Unlock all weapons and difficulties only",
                unlockAll: true, prismatic: false, maxCurrency: false, maxMastery: false),
            new("Prismatic", "Prismatic Collection", "Set all items to prismatic rarity",
                unlockAll: false, prismatic: true, maxCurrency: false, maxMastery: false),
            new("RichCrab", "Rich Crab", "Max out currency only",
                unlockAll: false, prismatic: false, maxCurrency: true, maxMastery: false),
            new("FreshStart", "Fresh Start", "Reset to default (starter weapons only)",
                unlockAll: false, prismatic: false, maxCurrency: false, maxMastery: false, isReset: true),
        };

        // ============================================
        // PROPERTY NAME PATTERNS FOR SAVE FILE
        // Based on actual Crab Champions save structure
        // ============================================
        public static class PropertyPatterns
        {
            // Unlock patterns - lowercase variants common in UE4/UE5
            public static readonly string[] UnlockedWeapons = {
                "UnlockedWeapons", "unlockedweapons", "WeaponUnlocks", "weaponunlocks",
                "Weapons", "weapons", "UnlockedPrimaryWeapons", "unlockedprimaryweapons",
                "PrimaryWeapons", "primaryweapons", "ownedweapons", "OwnedWeapons"
            };
            public static readonly string[] UnlockedAbilities = {
                "UnlockedAbilities", "unlockedabilities", "AbilityUnlocks", "abilityunlocks",
                "Abilities", "abilities", "UnlockedSecondaryWeapons", "unlockedsecondaryweapons",
                "SecondaryWeapons", "secondaryweapons", "ownedabilities", "OwnedAbilities",
                "grenades", "Grenades"
            };
            public static readonly string[] UnlockedMelee = {
                "UnlockedMelee", "unlockedmelee", "MeleeUnlocks", "meleeunlocks",
                "UnlockedMeleeWeapons", "unlockedmeleeweapons", "MeleeWeapons", "meleeweapons",
                "ownedmelee", "OwnedMelee"
            };
            public static readonly string[] UnlockedPerks = {
                "UnlockedPerks", "unlockedperks", "PerkUnlocks", "perkunlocks",
                "Perks", "perks", "AvailablePerks", "availableperks"
            };

            // Mastery/Rarity patterns
            public static readonly string[] WeaponMastery = {
                "WeaponMastery", "weaponmastery", "Mastery", "mastery",
                "WeaponLevels", "weaponlevels", "WeaponRarity", "weaponrarity",
                "weapondata", "WeaponData", "weaponstats", "WeaponStats"
            };
            public static readonly string[] AbilityMastery = {
                "AbilityMastery", "abilitymastery", "AbilityLevels", "abilitylevels",
                "AbilityRarity", "abilityrarity"
            };

            // Difficulty/Wins patterns - actual Crab Champions naming
            public static readonly string[] DifficultyWins = {
                "bronzewins", "BronzeWins", "silverwins", "SilverWins",
                "goldwins", "GoldWins", "sapphirewins", "SapphireWins",
                "emeraldwins", "EmeraldWins", "rubywins", "RubyWins",
                "diamondwins", "DiamondWins", "prismaticwins", "PrismaticWins"
            };

            // Stats patterns
            public static readonly string[] TotalKills = { "TotalKills", "totalkills", "Kills", "kills", "EnemiesKilled", "enemieskilled" };
            public static readonly string[] TotalDeaths = { "TotalDeaths", "totaldeaths", "Deaths", "deaths", "DeathCount", "deathcount" };
            public static readonly string[] TotalRuns = { "TotalRuns", "totalruns", "RunsCompleted", "runscompleted", "GamesPlayed", "gamesplayed" };
            public static readonly string[] HighestWave = { "HighestWave", "highestwave", "MaxWave", "maxwave", "BestWave", "bestwave" };
            public static readonly string[] TotalPlayTime = { "TotalPlayTime", "totalplaytime", "PlayTime", "playtime", "TimePlayed", "timeplayed" };
            public static readonly string[] BossesKilled = { "BossesKilled", "bosseskilled", "BossKills", "bosskills", "BossesDefeated", "bossesdefeated" };

            // Currency patterns
            public static readonly string[] Crystals = { "Crystals", "crystals", "TotalCrystals", "totalcrystals", "Currency", "currency", "gold", "Gold" };
            public static readonly string[] Keys = { "Keys", "keys", "TotalKeys", "totalkeys", "KeyCount", "keycount" };

            // Challenges patterns
            public static readonly string[] CompletedChallenges = { "CompletedChallenges", "completedchallenges", "Challenges", "challenges", "Achievements", "achievements" };
            public static readonly string[] UnlockedDifficulties = { "UnlockedDifficulties", "unlockeddifficulties", "Difficulties", "difficulties", "DifficultyUnlocks", "difficultyunlocks" };
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

        /// <summary>
        /// Get all perk IDs for unlocking
        /// </summary>
        public static List<string> GetAllPerkIds()
        {
            var ids = new List<string>();
            foreach (var p in Perks) ids.Add(p.Id);
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

    /// <summary>
    /// Represents a perk/upgrade in the game
    /// </summary>
    public class PerkInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public string Description { get; }

        public PerkInfo(string id, string displayName, string category, string description)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            Description = description;
        }
    }

    /// <summary>
    /// Represents a tracked statistic
    /// </summary>
    public class StatInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public string Description { get; }

        public StatInfo(string id, string displayName, string category, string description)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            Description = description;
        }
    }

    /// <summary>
    /// Represents a preset profile for quick modifications
    /// </summary>
    public class PresetProfile
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public bool UnlockAll { get; }
        public bool SetPrismatic { get; }
        public bool MaxCurrency { get; }
        public bool MaxMastery { get; }
        public bool IsReset { get; }

        public PresetProfile(string id, string displayName, string description,
            bool unlockAll = false, bool prismatic = false, bool maxCurrency = false,
            bool maxMastery = false, bool isReset = false)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            UnlockAll = unlockAll;
            SetPrismatic = prismatic;
            MaxCurrency = maxCurrency;
            MaxMastery = maxMastery;
            IsReset = isReset;
        }
    }
}
