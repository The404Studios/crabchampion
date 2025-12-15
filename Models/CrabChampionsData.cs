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
        // PRIMARY WEAPONS (20 total - from actual save file)
        // Path format: /Game/Blueprint/Weapon/{Id}/DA_Weapon_{Id}.DA_Weapon_{Id}
        // ============================================
        public static readonly WeaponInfo[] PrimaryWeapons =
        {
            // Starting weapons
            new("AutoRifle", "Auto Rifle", "Rifle", true),
            new("DualShotguns", "Dual Shotguns", "Shotgun", true),

            // Unlockable weapons (IDs from actual save file)
            new("DualPistols", "Dual Pistols", "Pistol", false),
            new("AutoShotgun", "Auto Shotgun", "Shotgun", false),
            new("BurstPistol", "Burst Pistol", "Pistol", false),
            new("Sniper", "Sniper", "Sniper", false),
            new("Crossbow", "Crossbow", "Sniper", false),
            new("OrbLauncher", "Orb Launcher", "Launcher", false),
            new("RocketLauncher", "Rocket Launcher", "Launcher", false),
            new("Minigun", "Minigun", "LMG", false),
            new("BladeLauncher", "Blade Launcher", "Launcher", false),
            new("ClusterLauncher", "Cluster Launcher", "Launcher", false),
            new("Flamethrower", "Flamethrower", "Special", false),
            new("ArcaneWand", "Arcane Wand", "Special", false),
            new("LaserCannons", "Laser Cannons", "Special", false),
            new("Seagle", "Seagle", "Rifle", false),
            new("MarksmanRifle", "Marksman Rifle", "Rifle", false),
            new("IceStaff", "Ice Staff", "Special", false),
            new("LightningScepter", "Lightning Scepter", "Special", false),
            new("PoisonCannon", "Poison Cannon", "Special", false),
        };

        // ============================================
        // ABILITIES (7 total - from actual save file)
        // Path format: /Game/Blueprint/Ability/DA_Ability_{Id}.DA_Ability_{Id}
        // ============================================
        public static readonly WeaponInfo[] SecondaryWeapons =
        {
            new("Grenade", "Grenade", "Explosive", true, "Ability"),
            new("GrapplingHook", "Grappling Hook", "Utility", false, "Ability"),
            new("BlackHole", "Black Hole", "Special", false, "Ability"),
            new("LaserBeam", "Laser Beam", "Energy", false, "Ability"),
            new("IceBlast", "Ice Blast", "Ice", false, "Ability"),
            new("ElectroGlobe", "Electro Globe", "Energy", false, "Ability"),
            new("AirStrike", "Air Strike", "Explosive", false, "Ability"),
        };

        // ============================================
        // MELEE WEAPONS (5 total - from actual save file)
        // Path format: /Game/Blueprint/Melee/DA_Melee_{Id}.DA_Melee_{Id}
        // ============================================
        public static readonly WeaponInfo[] MeleeWeapons =
        {
            new("Claw", "Claw", "Melee", true, "Melee"),
            new("Dagger", "Dagger", "Melee", false, "Melee"),
            new("Hammer", "Hammer", "Melee", false, "Melee"),
            new("Pickaxe", "Pickaxe", "Melee", false, "Melee"),
            new("Katana", "Katana", "Melee", false, "Melee"),
        };

        // ============================================
        // CHARACTER SKINS (from actual save file)
        // Path format: /Game/Character/Crab/Texture/SkinPrototype/MI_{Id}.MI_{Id}
        // ============================================
        public static readonly SkinInfo[] CharacterSkins =
        {
            // Default and basic skins
            new("Default", "Default", "Default", true, "/Game/Character/Crab/Texture/Default/MI_Crab_Default.MI_Crab_Default"),
            new("BlueIce", "Blue Ice", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_BlueIce.MI_BlueIce"),
            new("Glimmer", "Glimmer", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Glimmer.MI_Glimmer"),
            new("Flow", "Flow", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Flow.MI_Flow"),
            new("Aqua", "Aqua", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Aqua.MI_Aqua"),
            new("Iridescent", "Iridescent", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Iridescent.MI_Iridescent"),
            new("Warped", "Warped", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Warped.MI_Warped"),
            new("Amber", "Amber", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Amber.MI_Amber"),
            new("Watermelon", "Watermelon", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Watermelon.MI_Watermelon"),
            new("Chemical", "Chemical", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Chemical.MI_Chemical"),
            new("Tiger", "Tiger", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Tiger.MI_Tiger"),
            new("Snow", "Snow", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Snow.MI_Snow"),
            new("Cheetah", "Cheetah", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Cheetah.MI_Cheetah"),
            new("Geometric", "Geometric", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Geometric.MI_Geometric"),
            new("Orange", "Orange", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Orange.MI_Orange"),
            new("Pink", "Pink", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Pink.MI_Pink"),
            new("Waves", "Waves", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Waves.MI_Waves"),
            new("Swirl", "Swirl", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Swirl.MI_Swirl"),
            new("Jelly", "Jelly", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Jelly.MI_Jelly"),
            new("Obsidian", "Obsidian", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Obsidian.MI_Obsidian"),
            new("HotRod", "Hot Rod", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_HotRod.MI_HotRod"),
            new("Toxic", "Toxic", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Toxic.MI_Toxic"),
            new("Grid", "Grid", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Grid.MI_Grid"),
            new("PurpleTiger", "Purple Tiger", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_PurpleTiger.MI_PurpleTiger"),
            new("BlueTiger", "Blue Tiger", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_BlueTiger.MI_BlueTiger"),
            new("RedTiger", "Red Tiger", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_RedTiger.MI_RedTiger"),
            new("JetBlack", "Jet Black", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_JetBlack.MI_JetBlack"),
            new("Ocean", "Ocean", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Ocean.MI_Ocean"),
            new("Focus", "Focus", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Focus.MI_Focus"),
            new("Vibrations", "Vibrations", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Vibrations.MI_Vibrations"),
            new("Chrome", "Chrome", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Chrome.MI_Chrome"),
            new("Vampire", "Vampire", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Vampire.MI_Vampire"),
            new("Reptile", "Reptile", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Reptile.MI_Reptile"),
            new("Damascus", "Damascus", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Damascus.MI_Damascus"),
            new("Current", "Current", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Current.MI_Current"),
            new("Regal", "Regal", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Regal.MI_Regal"),
            new("Heat", "Heat", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Heat.MI_Heat"),
            new("Festive", "Festive", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Festive.MI_Festive"),

            // Rank skins
            new("Silver", "Silver", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Silver.MI_Silver"),
            new("FakeGold", "Fake Gold", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_FakeGold.MI_FakeGold"),
            new("Gold", "Gold", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Gold.MI_Gold"),
            new("Sapphire", "Sapphire", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Sapphire.MI_Sapphire"),
            new("Emerald", "Emerald", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Emerald.MI_Emerald"),
            new("Ruby", "Ruby", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Ruby.MI_Ruby"),
            new("Kaleidoscopic", "Kaleidoscopic", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Kaleidoscopic.MI_Kaleidoscopic"),
            new("Prismatic", "Prismatic", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Prismatic.MI_Prismatic"),
        };

        // ============================================
        // WEAPON SKINS
        // Path format: /Game/Blueprint/Cosmetics/WeaponSkins/DA_WeaponSkin_{WeaponId}_{SkinId}.DA_WeaponSkin_{WeaponId}_{SkinId}
        // ============================================
        public static readonly SkinInfo[] WeaponSkins =
        {
            // Universal weapon skins (apply to all weapons)
            new("Default", "Default", "Default", true),
            new("Gold", "Golden", "Rare", false),
            new("Silver", "Silver", "Uncommon", false),
            new("Obsidian", "Obsidian", "Rare", false),
            new("Crystal", "Crystal", "Rare", false),
            new("Neon", "Neon", "Epic", false),
            new("Prismatic", "Prismatic", "Legendary", false),
            new("Flame", "Flame", "Epic", false),
            new("Ice", "Ice", "Epic", false),
            new("Void", "Void", "Legendary", false),
            new("Champion", "Champion", "Legendary", false),
        };

        // ============================================
        // EMOTES
        // Path format: /Game/Blueprint/Cosmetics/Emotes/DA_Emote_{Id}.DA_Emote_{Id}
        // ============================================
        public static readonly CosmeticInfo[] Emotes =
        {
            new("Wave", "Wave", "Default"),
            new("Dance", "Dance", "Rare"),
            new("Flex", "Flex", "Uncommon"),
            new("Clap", "Clap", "Common"),
            new("Bow", "Bow", "Uncommon"),
            new("Victory", "Victory Pose", "Rare"),
            new("Taunt", "Taunt", "Rare"),
            new("Crabwalk", "Crab Walk", "Epic"),
            new("Moonwalk", "Moonwalk", "Epic"),
            new("Spin", "Spin", "Legendary"),
        };

        // ============================================
        // BANNERS / PROFILE COSMETICS
        // Path format: /Game/Blueprint/Cosmetics/Banners/DA_Banner_{Id}.DA_Banner_{Id}
        // ============================================
        public static readonly CosmeticInfo[] Banners =
        {
            new("Default", "Default Banner", "Default"),
            new("Bronze", "Bronze Banner", "Common"),
            new("Silver", "Silver Banner", "Uncommon"),
            new("Gold", "Gold Banner", "Rare"),
            new("Diamond", "Diamond Banner", "Epic"),
            new("Prismatic", "Prismatic Banner", "Legendary"),
            new("Champion", "Champion Banner", "Legendary"),
            new("Slayer", "Slayer Banner", "Rare"),
            new("Speedrunner", "Speedrunner Banner", "Rare"),
            new("Collector", "Collector Banner", "Epic"),
        };

        // ============================================
        // TITLES
        // ============================================
        public static readonly string[] Titles =
        {
            "Crab Champion",
            "Shell Shocker",
            "Wave Rider",
            "Island Hopper",
            "Boss Slayer",
            "Prismatic Master",
            "Speed Demon",
            "Collector",
            "Completionist",
            "Legend",
            "Champion",
            "Immortal",
            "Untouchable",
            "Destroyer",
            "Survivor",
        };

        // ============================================
        // CHALLENGES / ACHIEVEMENTS
        // ============================================
        public static readonly ChallengeInfo[] Challenges =
        {
            new("FirstWin", "First Victory", "Win your first run", 1),
            new("TenWins", "Veteran", "Win 10 runs", 10),
            new("HundredWins", "Champion", "Win 100 runs", 100),
            new("NoDamage", "Untouchable", "Complete a run without taking damage", 1),
            new("SpeedRun", "Speed Demon", "Complete a run in under 30 minutes", 1),
            new("AllWeapons", "Arsenal", "Unlock all weapons", 32),
            new("AllSkins", "Fashionista", "Unlock all character skins", 30),
            new("MaxMastery", "Master", "Max out weapon mastery", 100),
            new("AllDifficulties", "Conqueror", "Beat all difficulty tiers", 8),
            new("BossRush", "Boss Hunter", "Defeat 100 bosses", 100),
            new("Millionaire", "Rich Crab", "Collect 1,000,000 crystals total", 1000000),
            new("Perfectionist", "Perfectionist", "Get 100% accuracy in a run", 100),
            new("Marathon", "Marathon Runner", "Travel 1,000,000 distance", 1000000),
            new("AllPrismatic", "Prismatic Collector", "Get all items to Prismatic rarity", 32),
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
            new("GodMode", "God Mode", "Unlock everything, max stats, prismatic rarity, all skins",
                unlockAll: true, prismatic: true, maxCurrency: true, maxMastery: true,
                unlockSkins: true, unlockCosmetics: true),
            new("AllUnlocks", "All Unlocks", "Unlock all weapons, skins and difficulties",
                unlockAll: true, prismatic: false, maxCurrency: false, maxMastery: false,
                unlockSkins: true),
            new("Prismatic", "Prismatic Collection", "Set all items to prismatic rarity",
                unlockAll: false, prismatic: true, maxCurrency: false, maxMastery: false),
            new("RichCrab", "Rich Crab", "Max out currency only",
                unlockAll: false, prismatic: false, maxCurrency: true, maxMastery: false),
            new("Fashionista", "Fashionista", "Unlock all skins and cosmetics only",
                unlockAll: false, prismatic: false, maxCurrency: false, maxMastery: false,
                unlockSkins: true, unlockCosmetics: true),
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

        /// <summary>
        /// Get all weapon asset paths for unlocking
        /// </summary>
        public static List<string> GetAllWeaponAssetPaths()
        {
            var paths = new List<string>();
            foreach (var w in PrimaryWeapons) paths.Add(w.AssetPath);
            return paths;
        }

        /// <summary>
        /// Get all ability asset paths for unlocking
        /// </summary>
        public static List<string> GetAllAbilityAssetPaths()
        {
            var paths = new List<string>();
            foreach (var a in SecondaryWeapons) paths.Add(a.AssetPath);
            return paths;
        }

        /// <summary>
        /// Get all melee asset paths for unlocking
        /// </summary>
        public static List<string> GetAllMeleeAssetPaths()
        {
            var paths = new List<string>();
            foreach (var m in MeleeWeapons) paths.Add(m.AssetPath);
            return paths;
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
        public string AssetPath { get; }

        // For weapons: /Game/Blueprint/Weapon/{Id}/DA_Weapon_{Id}.DA_Weapon_{Id}
        public WeaponInfo(string id, string displayName, string category, bool isStarter, string assetType = "Weapon")
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            IsStarterWeapon = isStarter;
            // Weapons have a subfolder, abilities/melee don't
            if (assetType == "Weapon")
                AssetPath = $"/Game/Blueprint/Weapon/{id}/DA_Weapon_{id}.DA_Weapon_{id}";
            else if (assetType == "Ability")
                AssetPath = $"/Game/Blueprint/Ability/DA_Ability_{id}.DA_Ability_{id}";
            else if (assetType == "Melee")
                AssetPath = $"/Game/Blueprint/Melee/DA_Melee_{id}.DA_Melee_{id}";
            else
                AssetPath = $"/Game/Blueprint/{assetType}/{id}/DA_{assetType}_{id}.DA_{assetType}_{id}";
        }

        public WeaponInfo(string id, string displayName, string category, bool isStarter, string assetPath, bool customPath)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            IsStarterWeapon = isStarter;
            AssetPath = assetPath;
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
        public bool UnlockSkins { get; }
        public bool UnlockCosmetics { get; }

        public PresetProfile(string id, string displayName, string description,
            bool unlockAll = false, bool prismatic = false, bool maxCurrency = false,
            bool maxMastery = false, bool isReset = false, bool unlockSkins = false,
            bool unlockCosmetics = false)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            UnlockAll = unlockAll;
            SetPrismatic = prismatic;
            MaxCurrency = maxCurrency;
            MaxMastery = maxMastery;
            IsReset = isReset;
            UnlockSkins = unlockSkins;
            UnlockCosmetics = unlockCosmetics;
        }
    }

    /// <summary>
    /// Represents a character or weapon skin
    /// </summary>
    public class SkinInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Rarity { get; }
        public bool IsDefault { get; }
        public string AssetPath { get; }

        public SkinInfo(string id, string displayName, string rarity, bool isDefault)
        {
            Id = id;
            DisplayName = displayName;
            Rarity = rarity;
            IsDefault = isDefault;
            AssetPath = $"/Game/Blueprint/Cosmetics/Skins/DA_Skin_{id}.DA_Skin_{id}";
        }

        public SkinInfo(string id, string displayName, string rarity, bool isDefault, string customPath)
        {
            Id = id;
            DisplayName = displayName;
            Rarity = rarity;
            IsDefault = isDefault;
            AssetPath = customPath;
        }
    }

    /// <summary>
    /// Represents an emote, banner, or other cosmetic item
    /// </summary>
    public class CosmeticInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Rarity { get; }
        public string AssetPath { get; }

        public CosmeticInfo(string id, string displayName, string rarity)
        {
            Id = id;
            DisplayName = displayName;
            Rarity = rarity;
            AssetPath = $"/Game/Blueprint/Cosmetics/{id}/DA_{id}.DA_{id}";
        }
    }

    /// <summary>
    /// Represents a challenge/achievement
    /// </summary>
    public class ChallengeInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int RequiredValue { get; }
        public string AssetPath { get; }

        public ChallengeInfo(string id, string displayName, string description, int requiredValue)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            RequiredValue = requiredValue;
            AssetPath = $"/Game/Blueprint/Challenges/DA_Challenge_{id}.DA_Challenge_{id}";
        }
    }
}
