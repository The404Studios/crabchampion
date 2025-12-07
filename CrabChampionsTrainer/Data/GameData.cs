namespace CrabChampionsTrainer.Data;

/// <summary>
/// Static game data including items, weapons, and prismatics
/// </summary>
public static class GameData
{
    #region Weapons

    public static readonly Dictionary<string, WeaponInfo> Weapons = new()
    {
        // Pistols
        { "Pistol", new WeaponInfo("Pistol", WeaponCategory.Pistol, Rarity.Common) },
        { "Revolver", new WeaponInfo("Revolver", WeaponCategory.Pistol, Rarity.Uncommon) },
        { "Deagle", new WeaponInfo("Deagle", WeaponCategory.Pistol, Rarity.Rare) },
        { "Dual Pistols", new WeaponInfo("Dual Pistols", WeaponCategory.Pistol, Rarity.Rare) },
        { "Auto Pistol", new WeaponInfo("Auto Pistol", WeaponCategory.Pistol, Rarity.Uncommon) },

        // SMGs
        { "SMG", new WeaponInfo("SMG", WeaponCategory.SMG, Rarity.Common) },
        { "Vector", new WeaponInfo("Vector", WeaponCategory.SMG, Rarity.Rare) },
        { "P90", new WeaponInfo("P90", WeaponCategory.SMG, Rarity.Rare) },
        { "MAC-10", new WeaponInfo("MAC-10", WeaponCategory.SMG, Rarity.Uncommon) },
        { "Dual SMGs", new WeaponInfo("Dual SMGs", WeaponCategory.SMG, Rarity.Epic) },

        // Shotguns
        { "Shotgun", new WeaponInfo("Shotgun", WeaponCategory.Shotgun, Rarity.Common) },
        { "Pump Shotgun", new WeaponInfo("Pump Shotgun", WeaponCategory.Shotgun, Rarity.Uncommon) },
        { "Auto Shotgun", new WeaponInfo("Auto Shotgun", WeaponCategory.Shotgun, Rarity.Rare) },
        { "Double Barrel", new WeaponInfo("Double Barrel", WeaponCategory.Shotgun, Rarity.Rare) },
        { "Quad Shotgun", new WeaponInfo("Quad Shotgun", WeaponCategory.Shotgun, Rarity.Epic) },

        // Rifles
        { "Assault Rifle", new WeaponInfo("Assault Rifle", WeaponCategory.Rifle, Rarity.Common) },
        { "AK-47", new WeaponInfo("AK-47", WeaponCategory.Rifle, Rarity.Uncommon) },
        { "M4", new WeaponInfo("M4", WeaponCategory.Rifle, Rarity.Rare) },
        { "Burst Rifle", new WeaponInfo("Burst Rifle", WeaponCategory.Rifle, Rarity.Uncommon) },
        { "Battle Rifle", new WeaponInfo("Battle Rifle", WeaponCategory.Rifle, Rarity.Rare) },

        // Snipers
        { "Sniper Rifle", new WeaponInfo("Sniper Rifle", WeaponCategory.Sniper, Rarity.Uncommon) },
        { "AWP", new WeaponInfo("AWP", WeaponCategory.Sniper, Rarity.Rare) },
        { "Scout", new WeaponInfo("Scout", WeaponCategory.Sniper, Rarity.Uncommon) },
        { "Anti-Material Rifle", new WeaponInfo("Anti-Material Rifle", WeaponCategory.Sniper, Rarity.Epic) },

        // Heavy
        { "Minigun", new WeaponInfo("Minigun", WeaponCategory.Heavy, Rarity.Rare) },
        { "LMG", new WeaponInfo("LMG", WeaponCategory.Heavy, Rarity.Uncommon) },
        { "Grenade Launcher", new WeaponInfo("Grenade Launcher", WeaponCategory.Heavy, Rarity.Rare) },
        { "Rocket Launcher", new WeaponInfo("Rocket Launcher", WeaponCategory.Heavy, Rarity.Epic) },
        { "Flamethrower", new WeaponInfo("Flamethrower", WeaponCategory.Heavy, Rarity.Rare) },

        // Special
        { "Laser Gun", new WeaponInfo("Laser Gun", WeaponCategory.Special, Rarity.Rare) },
        { "Rail Gun", new WeaponInfo("Rail Gun", WeaponCategory.Special, Rarity.Epic) },
        { "Plasma Rifle", new WeaponInfo("Plasma Rifle", WeaponCategory.Special, Rarity.Epic) },
        { "Crossbow", new WeaponInfo("Crossbow", WeaponCategory.Special, Rarity.Uncommon) },
        { "Harpoon Gun", new WeaponInfo("Harpoon Gun", WeaponCategory.Special, Rarity.Rare) },
    };

    #endregion

    #region Prismatics

    public static readonly Dictionary<string, PrismaticInfo> Prismatics = new()
    {
        // Damage Prismatics
        { "Prismatic Damage", new PrismaticInfo("Prismatic Damage", PrismaticCategory.Combat, "Massively increases all damage dealt") },
        { "Prismatic Crit", new PrismaticInfo("Prismatic Crit", PrismaticCategory.Combat, "Greatly increases critical hit chance and damage") },
        { "Prismatic Fire Rate", new PrismaticInfo("Prismatic Fire Rate", PrismaticCategory.Combat, "Drastically increases fire rate") },
        { "Prismatic Explosion", new PrismaticInfo("Prismatic Explosion", PrismaticCategory.Combat, "All attacks cause explosions") },
        { "Prismatic Chain", new PrismaticInfo("Prismatic Chain", PrismaticCategory.Combat, "Attacks chain to nearby enemies") },
        { "Prismatic Piercing", new PrismaticInfo("Prismatic Piercing", PrismaticCategory.Combat, "All projectiles pierce through enemies") },
        { "Prismatic Homing", new PrismaticInfo("Prismatic Homing", PrismaticCategory.Combat, "Projectiles home in on enemies") },
        { "Prismatic Ricochet", new PrismaticInfo("Prismatic Ricochet", PrismaticCategory.Combat, "Projectiles bounce between enemies") },

        // Defense Prismatics
        { "Prismatic Health", new PrismaticInfo("Prismatic Health", PrismaticCategory.Defense, "Massively increases max health") },
        { "Prismatic Shield", new PrismaticInfo("Prismatic Shield", PrismaticCategory.Defense, "Gain a powerful regenerating shield") },
        { "Prismatic Regen", new PrismaticInfo("Prismatic Regen", PrismaticCategory.Defense, "Rapidly regenerate health over time") },
        { "Prismatic Lifesteal", new PrismaticInfo("Prismatic Lifesteal", PrismaticCategory.Defense, "Heal for a portion of damage dealt") },
        { "Prismatic Armor", new PrismaticInfo("Prismatic Armor", PrismaticCategory.Defense, "Take significantly reduced damage") },
        { "Prismatic Invulnerability", new PrismaticInfo("Prismatic Invulnerability", PrismaticCategory.Defense, "Periodically become invulnerable") },

        // Movement Prismatics
        { "Prismatic Speed", new PrismaticInfo("Prismatic Speed", PrismaticCategory.Movement, "Massively increases movement speed") },
        { "Prismatic Dash", new PrismaticInfo("Prismatic Dash", PrismaticCategory.Movement, "Dash cooldown greatly reduced, increased range") },
        { "Prismatic Jump", new PrismaticInfo("Prismatic Jump", PrismaticCategory.Movement, "Gain multiple extra jumps") },
        { "Prismatic Flight", new PrismaticInfo("Prismatic Flight", PrismaticCategory.Movement, "Gain the ability to fly") },

        // Utility Prismatics
        { "Prismatic Luck", new PrismaticInfo("Prismatic Luck", PrismaticCategory.Utility, "Greatly increases luck for better loot") },
        { "Prismatic XP", new PrismaticInfo("Prismatic XP", PrismaticCategory.Utility, "Gain massively increased experience") },
        { "Prismatic Currency", new PrismaticInfo("Prismatic Currency", PrismaticCategory.Utility, "Find much more keys and crystals") },
        { "Prismatic Magnet", new PrismaticInfo("Prismatic Magnet", PrismaticCategory.Utility, "Pickups are attracted from far away") },
        { "Prismatic Cooldown", new PrismaticInfo("Prismatic Cooldown", PrismaticCategory.Utility, "All cooldowns greatly reduced") },
    };

    #endregion

    #region Items/Upgrades

    public static readonly Dictionary<string, ItemInfo> Items = new()
    {
        // Damage
        { "Damage Up", new ItemInfo("Damage Up", ItemCategory.Damage, "+10% Damage") },
        { "Crit Chance", new ItemInfo("Crit Chance", ItemCategory.Damage, "+5% Critical Hit Chance") },
        { "Crit Damage", new ItemInfo("Crit Damage", ItemCategory.Damage, "+25% Critical Hit Damage") },
        { "Armor Piercing", new ItemInfo("Armor Piercing", ItemCategory.Damage, "Ignore enemy armor") },
        { "Fire Damage", new ItemInfo("Fire Damage", ItemCategory.Damage, "Add fire damage to attacks") },
        { "Ice Damage", new ItemInfo("Ice Damage", ItemCategory.Damage, "Add ice damage, slow enemies") },
        { "Lightning Damage", new ItemInfo("Lightning Damage", ItemCategory.Damage, "Add chain lightning to attacks") },
        { "Poison Damage", new ItemInfo("Poison Damage", ItemCategory.Damage, "Poison enemies on hit") },
        { "Explosive Rounds", new ItemInfo("Explosive Rounds", ItemCategory.Damage, "Shots explode on impact") },

        // Attack Speed
        { "Fire Rate Up", new ItemInfo("Fire Rate Up", ItemCategory.AttackSpeed, "+15% Fire Rate") },
        { "Attack Speed Up", new ItemInfo("Attack Speed Up", ItemCategory.AttackSpeed, "+10% Attack Speed") },
        { "Reload Speed Up", new ItemInfo("Reload Speed Up", ItemCategory.AttackSpeed, "+20% Reload Speed") },
        { "Magazine Size", new ItemInfo("Magazine Size", ItemCategory.AttackSpeed, "+30% Magazine Size") },
        { "Ammo Capacity", new ItemInfo("Ammo Capacity", ItemCategory.AttackSpeed, "+50% Ammo Reserves") },

        // Defense
        { "Health Up", new ItemInfo("Health Up", ItemCategory.Defense, "+25 Max Health") },
        { "Max Health Up", new ItemInfo("Max Health Up", ItemCategory.Defense, "+15% Max Health") },
        { "Shield Up", new ItemInfo("Shield Up", ItemCategory.Defense, "+20 Shield") },
        { "Armor Up", new ItemInfo("Armor Up", ItemCategory.Defense, "+5 Armor") },
        { "Damage Resist", new ItemInfo("Damage Resist", ItemCategory.Defense, "+10% Damage Resistance") },
        { "Health Regen", new ItemInfo("Health Regen", ItemCategory.Defense, "Regenerate health over time") },
        { "Lifesteal", new ItemInfo("Lifesteal", ItemCategory.Defense, "Heal 5% of damage dealt") },

        // Movement
        { "Speed Up", new ItemInfo("Speed Up", ItemCategory.Movement, "+10% Movement Speed") },
        { "Jump Height Up", new ItemInfo("Jump Height Up", ItemCategory.Movement, "+15% Jump Height") },
        { "Double Jump", new ItemInfo("Double Jump", ItemCategory.Movement, "Gain an extra jump") },
        { "Triple Jump", new ItemInfo("Triple Jump", ItemCategory.Movement, "Gain two extra jumps") },
        { "Dash Range", new ItemInfo("Dash Range", ItemCategory.Movement, "+25% Dash Range") },
        { "Dash Cooldown", new ItemInfo("Dash Cooldown", ItemCategory.Movement, "-20% Dash Cooldown") },
        { "Air Control", new ItemInfo("Air Control", ItemCategory.Movement, "Better control in air") },

        // Utility
        { "Luck Up", new ItemInfo("Luck Up", ItemCategory.Utility, "+10% Luck") },
        { "XP Boost", new ItemInfo("XP Boost", ItemCategory.Utility, "+15% Experience Gain") },
        { "Currency Boost", new ItemInfo("Currency Boost", ItemCategory.Utility, "+20% Currency Drop") },
        { "Cooldown Reduction", new ItemInfo("Cooldown Reduction", ItemCategory.Utility, "-10% All Cooldowns") },
        { "Range Up", new ItemInfo("Range Up", ItemCategory.Utility, "+15% Weapon Range") },
        { "Projectile Speed", new ItemInfo("Projectile Speed", ItemCategory.Utility, "+20% Projectile Speed") },
        { "Projectile Size", new ItemInfo("Projectile Size", ItemCategory.Utility, "+15% Projectile Size") },

        // Special
        { "Magnet", new ItemInfo("Magnet", ItemCategory.Special, "Attract nearby pickups") },
        { "Auto Aim", new ItemInfo("Auto Aim", ItemCategory.Special, "Slight aim assist") },
        { "Piercing", new ItemInfo("Piercing", ItemCategory.Special, "Projectiles pierce enemies") },
        { "Ricochet", new ItemInfo("Ricochet", ItemCategory.Special, "Projectiles bounce off walls") },
        { "Homing", new ItemInfo("Homing", ItemCategory.Special, "Projectiles track enemies") },
        { "Split Shot", new ItemInfo("Split Shot", ItemCategory.Special, "Fire multiple projectiles") },
        { "Chain Shot", new ItemInfo("Chain Shot", ItemCategory.Special, "Hits jump to nearby enemies") },
    };

    #endregion

    #region Perks

    public static readonly Dictionary<string, PerkInfo> Perks = new()
    {
        { "Glass Cannon", new PerkInfo("Glass Cannon", "Double damage, half health") },
        { "Tank", new PerkInfo("Tank", "Double health, reduced speed") },
        { "Speedster", new PerkInfo("Speedster", "Much faster movement and fire rate") },
        { "Berserker", new PerkInfo("Berserker", "Damage increases as health decreases") },
        { "Vampire", new PerkInfo("Vampire", "Lifesteal but no natural regen") },
        { "Lucky", new PerkInfo("Lucky", "Increased luck and better loot") },
        { "Greedy", new PerkInfo("Greedy", "More currency, but reduced damage") },
        { "Explosive Expert", new PerkInfo("Explosive Expert", "All weapons cause explosions") },
        { "Shock Trooper", new PerkInfo("Shock Trooper", "Chain lightning on all hits") },
        { "Frost Mage", new PerkInfo("Frost Mage", "Freeze enemies on hit") },
        { "Pyromancer", new PerkInfo("Pyromancer", "Set enemies on fire") },
        { "Assassin", new PerkInfo("Assassin", "Huge crit damage, low health") },
        { "Ninja", new PerkInfo("Ninja", "Very fast, dash through enemies") },
        { "Heavy Gunner", new PerkInfo("Heavy Gunner", "Huge magazines, slow reload") },
    };

    #endregion

    #region Totems

    public static readonly string[] Totems =
    {
        "Chance Totem",
        "Crystal Totem",
        "Fuse Totem",
        "Gamble Totem",
        "Glass Totem",
        "Gold Totem",
        "Greed Totem",
        "Health Totem",
        "Loot Totem",
        "Random Totem",
        "Reroll Totem",
    };

    #endregion

    #region Skins

    public static readonly string[] Skins =
    {
        "Default",
        "Golden Crab",
        "Diamond Crab",
        "Prismatic Crab",
        "Fire Crab",
        "Ice Crab",
        "Lightning Crab",
        "Shadow Crab",
        "Rainbow Crab",
        "Void Crab",
        "Neon Crab",
        "Crystal Crab",
        "Lava Crab",
        "Ocean Crab",
        "Galaxy Crab",
        "Zombie Crab",
        "Robot Crab",
        "King Crab",
        "Warrior Crab",
        "Ninja Crab",
    };

    #endregion
}

#region Data Classes

public enum WeaponCategory
{
    Pistol,
    SMG,
    Shotgun,
    Rifle,
    Sniper,
    Heavy,
    Special
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Prismatic
}

public enum PrismaticCategory
{
    Combat,
    Defense,
    Movement,
    Utility
}

public enum ItemCategory
{
    Damage,
    AttackSpeed,
    Defense,
    Movement,
    Utility,
    Special
}

public record WeaponInfo(string Name, WeaponCategory Category, Rarity Rarity);
public record PrismaticInfo(string Name, PrismaticCategory Category, string Description);
public record ItemInfo(string Name, ItemCategory Category, string Description);
public record PerkInfo(string Name, string Description);

#endregion
