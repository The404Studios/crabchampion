using System;
using System.Collections.Generic;

namespace CrabTrainer.Cheats
{
    /// <summary>
    /// Defines known memory offsets and cheat configurations for Crab Champions
    /// Note: These offsets may need to be updated when the game updates
    /// </summary>
    public static class CheatDefinitions
    {
        // ============================================
        // COMMON PERKS IN CRAB CHAMPIONS
        // These are the in-run upgrades/perks you can pick up
        // ============================================
        public static readonly PerkDefinition[] Perks =
        {
            // Damage Perks
            new("DamageUp", "Damage Up", "Increases damage dealt by 10%", PerkCategory.Damage),
            new("CritChance", "Critical Chance", "Increases crit chance by 5%", PerkCategory.Damage),
            new("CritDamage", "Critical Damage", "Increases crit damage by 25%", PerkCategory.Damage),
            new("AttackSpeed", "Attack Speed", "Increases attack speed by 10%", PerkCategory.Damage),
            new("Multishot", "Multishot", "Chance to fire extra projectiles", PerkCategory.Damage),
            new("Piercing", "Piercing Rounds", "Projectiles pierce enemies", PerkCategory.Damage),
            new("Explosive", "Explosive Rounds", "Attacks explode on impact", PerkCategory.Damage),
            new("Chain", "Chain Lightning", "Attacks chain to nearby enemies", PerkCategory.Damage),
            new("Burn", "Burn Damage", "Attacks apply burn damage", PerkCategory.Damage),
            new("Freeze", "Freeze", "Attacks slow enemies", PerkCategory.Damage),
            new("Poison", "Poison", "Attacks apply poison damage", PerkCategory.Damage),
            new("Bleed", "Bleed", "Attacks cause bleeding", PerkCategory.Damage),

            // Defense Perks
            new("MaxHP", "Max Health", "Increases maximum health", PerkCategory.Defense),
            new("HealthRegen", "Health Regen", "Regenerate health over time", PerkCategory.Defense),
            new("Armor", "Armor", "Reduces damage taken", PerkCategory.Defense),
            new("DodgeChance", "Dodge", "Chance to avoid damage", PerkCategory.Defense),
            new("Shield", "Shield", "Gain a protective shield", PerkCategory.Defense),
            new("Lifesteal", "Lifesteal", "Heal on dealing damage", PerkCategory.Defense),
            new("DamageReduction", "Damage Reduction", "Flat damage reduction", PerkCategory.Defense),
            new("Thorns", "Thorns", "Reflect damage to attackers", PerkCategory.Defense),

            // Movement Perks
            new("MoveSpeed", "Move Speed", "Increases movement speed", PerkCategory.Movement),
            new("JumpHeight", "Jump Height", "Increases jump height", PerkCategory.Movement),
            new("DoubleJump", "Double Jump", "Gain an extra jump", PerkCategory.Movement),
            new("DashDistance", "Dash Distance", "Increases dash range", PerkCategory.Movement),
            new("DashCooldown", "Dash Cooldown", "Reduces dash cooldown", PerkCategory.Movement),
            new("AirControl", "Air Control", "Better control in air", PerkCategory.Movement),

            // Utility Perks
            new("CooldownReduction", "Cooldown Reduction", "Reduces ability cooldowns", PerkCategory.Utility),
            new("LuckUp", "Luck", "Increases item quality", PerkCategory.Utility),
            new("XPGain", "XP Boost", "Increases experience gained", PerkCategory.Utility),
            new("GoldFind", "Gold Find", "Increases crystal drops", PerkCategory.Utility),
            new("MagnetRange", "Magnet", "Increases pickup range", PerkCategory.Utility),
            new("ExtraLife", "Extra Life", "Revive on death", PerkCategory.Utility),
            new("Reroll", "Reroll", "Free reroll per shop", PerkCategory.Utility),
        };

        // ============================================
        // CHEAT PRESETS
        // ============================================
        public static readonly CheatPreset[] Presets =
        {
            new("GodMode", "God Mode", "Infinite health, max damage", new[] {
                new CheatValue("Health", CheatValueType.Float, 99999f),
                new CheatValue("MaxHealth", CheatValueType.Float, 99999f),
                new CheatValue("Damage", CheatValueType.Float, 9999f),
            }),
            new("InfiniteAmmo", "Infinite Ammo", "Never reload", new[] {
                new CheatValue("Ammo", CheatValueType.Int32, 999),
                new CheatValue("MaxAmmo", CheatValueType.Int32, 999),
            }),
            new("SpeedHack", "Speed Boost", "Move faster", new[] {
                new CheatValue("MoveSpeed", CheatValueType.Float, 2000f),
            }),
            new("MaxCurrency", "Max Currency", "Lots of crystals", new[] {
                new CheatValue("Crystals", CheatValueType.Int32, 999999),
            }),
            new("MaxPerks", "Max Perks", "All perks at max level", new[] {
                new CheatValue("PerkLevel", CheatValueType.Int32, 99),
            }),
        };

        // ============================================
        // COMMON VALUE NAMES TO SEARCH FOR
        // ============================================
        public static readonly SearchPattern[] CommonSearchPatterns =
        {
            new("Health", "Current health value", CheatValueType.Float, 100f),
            new("MaxHealth", "Maximum health", CheatValueType.Float, 100f),
            new("Crystals", "Crystal currency", CheatValueType.Int32, 0),
            new("Damage", "Damage multiplier", CheatValueType.Float, 1f),
            new("MoveSpeed", "Movement speed", CheatValueType.Float, 600f),
            new("Ammo", "Current ammo", CheatValueType.Int32, 30),
            new("Wave", "Current wave number", CheatValueType.Int32, 1),
        };
    }

    public enum PerkCategory
    {
        Damage,
        Defense,
        Movement,
        Utility
    }

    public class PerkDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public PerkCategory Category { get; }

        public PerkDefinition(string id, string name, string description, PerkCategory category)
        {
            Id = id;
            Name = name;
            Description = description;
            Category = category;
        }
    }

    public enum CheatValueType
    {
        Int32,
        Int64,
        Float,
        Double,
        Byte
    }

    public class CheatValue
    {
        public string Name { get; }
        public CheatValueType Type { get; }
        public object Value { get; }

        public CheatValue(string name, CheatValueType type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }

    public class CheatPreset
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public CheatValue[] Values { get; }

        public CheatPreset(string id, string name, string description, CheatValue[] values)
        {
            Id = id;
            Name = name;
            Description = description;
            Values = values;
        }
    }

    public class SearchPattern
    {
        public string Name { get; }
        public string Description { get; }
        public CheatValueType Type { get; }
        public object DefaultValue { get; }

        public SearchPattern(string name, string description, CheatValueType type, object defaultValue)
        {
            Name = name;
            Description = description;
            Type = type;
            DefaultValue = defaultValue;
        }
    }

    /// <summary>
    /// Represents a found memory address with its value
    /// </summary>
    public class FoundAddress
    {
        public IntPtr Address { get; set; }
        public string Name { get; set; } = string.Empty;
        public CheatValueType Type { get; set; }
        public object? CurrentValue { get; set; }
        public object? FrozenValue { get; set; }
        public bool IsFrozen { get; set; }
        public DateTime LastUpdated { get; set; }

        public string AddressHex => $"0x{Address.ToInt64():X}";
        public string ValueDisplay => CurrentValue?.ToString() ?? "?";
    }
}
