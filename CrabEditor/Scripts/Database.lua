--[[
    Item, Weapon, and Prismatic Database for Crab Champions
    Data sourced from community resources and game files
]]

local Database = {}

-----------------------------------------------------------
-- Prismatics (Passive Upgrades)
-----------------------------------------------------------

Database.Prismatics = {
    -- Tier 1 (Green)
    { Name = "Damage Up", Class = "BP_Prismatic_DamageUp", Tier = 1, Effect = "+15% damage" },
    { Name = "Health Up", Class = "BP_Prismatic_HealthUp", Tier = 1, Effect = "+25 max health" },
    { Name = "Speed Up", Class = "BP_Prismatic_SpeedUp", Tier = 1, Effect = "+10% movement speed" },
    { Name = "Crit Chance", Class = "BP_Prismatic_CritChance", Tier = 1, Effect = "+10% crit chance" },
    { Name = "Attack Speed", Class = "BP_Prismatic_AttackSpeed", Tier = 1, Effect = "+15% attack speed" },
    { Name = "Jump Height", Class = "BP_Prismatic_JumpHeight", Tier = 1, Effect = "+20% jump height" },
    { Name = "Reload Speed", Class = "BP_Prismatic_ReloadSpeed", Tier = 1, Effect = "+20% reload speed" },
    { Name = "Shield Up", Class = "BP_Prismatic_ShieldUp", Tier = 1, Effect = "+25 max shield" },
    { Name = "Luck Up", Class = "BP_Prismatic_LuckUp", Tier = 1, Effect = "+10% luck" },
    { Name = "Range Up", Class = "BP_Prismatic_RangeUp", Tier = 1, Effect = "+15% range" },

    -- Tier 2 (Blue)
    { Name = "Vampirism", Class = "BP_Prismatic_Vampirism", Tier = 2, Effect = "Heal on kill" },
    { Name = "Explosive Rounds", Class = "BP_Prismatic_Explosive", Tier = 2, Effect = "Shots explode" },
    { Name = "Chain Lightning", Class = "BP_Prismatic_ChainLightning", Tier = 2, Effect = "Shots chain to enemies" },
    { Name = "Piercing", Class = "BP_Prismatic_Piercing", Tier = 2, Effect = "Shots pierce enemies" },
    { Name = "Bouncing", Class = "BP_Prismatic_Bouncing", Tier = 2, Effect = "Shots bounce" },
    { Name = "Homing", Class = "BP_Prismatic_Homing", Tier = 2, Effect = "Shots home to enemies" },
    { Name = "Double Jump", Class = "BP_Prismatic_DoubleJump", Tier = 2, Effect = "Extra jump" },
    { Name = "Dash", Class = "BP_Prismatic_Dash", Tier = 2, Effect = "Dash ability" },
    { Name = "Shield Regen", Class = "BP_Prismatic_ShieldRegen", Tier = 2, Effect = "Regenerate shield" },
    { Name = "Magnet", Class = "BP_Prismatic_Magnet", Tier = 2, Effect = "Attract pickups" },

    -- Tier 3 (Purple/Legendary)
    { Name = "Berserker", Class = "BP_Prismatic_Berserker", Tier = 3, Effect = "Damage scales with missing health" },
    { Name = "Glass Cannon", Class = "BP_Prismatic_GlassCannon", Tier = 3, Effect = "+100% damage, -50% health" },
    { Name = "Bullet Hell", Class = "BP_Prismatic_BulletHell", Tier = 3, Effect = "Triple shots" },
    { Name = "Time Warp", Class = "BP_Prismatic_TimeWarp", Tier = 3, Effect = "Slow time on kill" },
    { Name = "Nuclear", Class = "BP_Prismatic_Nuclear", Tier = 3, Effect = "Huge explosions" },
    { Name = "Immortality", Class = "BP_Prismatic_Immortality", Tier = 3, Effect = "Revive once per island" },
    { Name = "Ricochet Master", Class = "BP_Prismatic_RicochetMaster", Tier = 3, Effect = "Infinite bounces" },
    { Name = "Soul Collector", Class = "BP_Prismatic_SoulCollector", Tier = 3, Effect = "Collect souls for power" },
    { Name = "Overcharge", Class = "BP_Prismatic_Overcharge", Tier = 3, Effect = "Overflow damage chains" },
    { Name = "Golden Touch", Class = "BP_Prismatic_GoldenTouch", Tier = 3, Effect = "More crystals" },
}

-----------------------------------------------------------
-- Weapons
-----------------------------------------------------------

Database.Weapons = {
    -- Pistols
    { Name = "Pistol", Class = "BP_Weapon_Pistol", Category = "Pistols", Rarity = "Common" },
    { Name = "Revolver", Class = "BP_Weapon_Revolver", Category = "Pistols", Rarity = "Uncommon" },
    { Name = "Dual Pistols", Class = "BP_Weapon_DualPistols", Category = "Pistols", Rarity = "Rare" },
    { Name = "Hand Cannon", Class = "BP_Weapon_HandCannon", Category = "Pistols", Rarity = "Epic" },
    { Name = "Golden Gun", Class = "BP_Weapon_GoldenGun", Category = "Pistols", Rarity = "Legendary" },

    -- Rifles
    { Name = "Assault Rifle", Class = "BP_Weapon_AssaultRifle", Category = "Rifles", Rarity = "Common" },
    { Name = "Burst Rifle", Class = "BP_Weapon_BurstRifle", Category = "Rifles", Rarity = "Uncommon" },
    { Name = "Sniper Rifle", Class = "BP_Weapon_Sniper", Category = "Rifles", Rarity = "Rare" },
    { Name = "Railgun", Class = "BP_Weapon_Railgun", Category = "Rifles", Rarity = "Epic" },
    { Name = "Laser Rifle", Class = "BP_Weapon_LaserRifle", Category = "Rifles", Rarity = "Legendary" },

    -- Shotguns
    { Name = "Shotgun", Class = "BP_Weapon_Shotgun", Category = "Shotguns", Rarity = "Common" },
    { Name = "Double Barrel", Class = "BP_Weapon_DoubleBarrel", Category = "Shotguns", Rarity = "Uncommon" },
    { Name = "Combat Shotgun", Class = "BP_Weapon_CombatShotgun", Category = "Shotguns", Rarity = "Rare" },
    { Name = "Flak Cannon", Class = "BP_Weapon_FlakCannon", Category = "Shotguns", Rarity = "Epic" },

    -- SMGs
    { Name = "SMG", Class = "BP_Weapon_SMG", Category = "SMGs", Rarity = "Common" },
    { Name = "Dual SMGs", Class = "BP_Weapon_DualSMG", Category = "SMGs", Rarity = "Rare" },
    { Name = "Minigun", Class = "BP_Weapon_Minigun", Category = "SMGs", Rarity = "Epic" },

    -- Explosives
    { Name = "Grenade Launcher", Class = "BP_Weapon_GrenadeLauncher", Category = "Explosives", Rarity = "Uncommon" },
    { Name = "Rocket Launcher", Class = "BP_Weapon_RocketLauncher", Category = "Explosives", Rarity = "Rare" },
    { Name = "Missile Launcher", Class = "BP_Weapon_MissileLauncher", Category = "Explosives", Rarity = "Epic" },
    { Name = "Nuke Launcher", Class = "BP_Weapon_NukeLauncher", Category = "Explosives", Rarity = "Legendary" },

    -- Special
    { Name = "Crossbow", Class = "BP_Weapon_Crossbow", Category = "Special", Rarity = "Uncommon" },
    { Name = "Bow", Class = "BP_Weapon_Bow", Category = "Special", Rarity = "Rare" },
    { Name = "Flamethrower", Class = "BP_Weapon_Flamethrower", Category = "Special", Rarity = "Epic" },
    { Name = "Lightning Gun", Class = "BP_Weapon_LightningGun", Category = "Special", Rarity = "Legendary" },
    { Name = "Crab Cannon", Class = "BP_Weapon_CrabCannon", Category = "Special", Rarity = "Legendary" },

    -- Melee
    { Name = "Sword", Class = "BP_Weapon_Sword", Category = "Melee", Rarity = "Common" },
    { Name = "Hammer", Class = "BP_Weapon_Hammer", Category = "Melee", Rarity = "Rare" },
    { Name = "Claws", Class = "BP_Weapon_Claws", Category = "Melee", Rarity = "Epic" },
}

-----------------------------------------------------------
-- Items (Consumables and Pickups)
-----------------------------------------------------------

Database.Items = {
    -- Health
    { Name = "Small Health", Class = "BP_Item_HealthSmall", Description = "Restore 25 HP" },
    { Name = "Large Health", Class = "BP_Item_HealthLarge", Description = "Restore 50 HP" },
    { Name = "Full Health", Class = "BP_Item_HealthFull", Description = "Full heal" },

    -- Shield
    { Name = "Small Shield", Class = "BP_Item_ShieldSmall", Description = "Restore 25 shield" },
    { Name = "Large Shield", Class = "BP_Item_ShieldLarge", Description = "Restore 50 shield" },
    { Name = "Full Shield", Class = "BP_Item_ShieldFull", Description = "Full shield" },

    -- Currency
    { Name = "Crystal", Class = "BP_Item_Crystal", Description = "Currency" },
    { Name = "Key", Class = "BP_Item_Key", Description = "Open chests" },
    { Name = "Golden Key", Class = "BP_Item_GoldenKey", Description = "Open golden chests" },

    -- Power-ups
    { Name = "Damage Boost", Class = "BP_Item_DamageBoost", Description = "Temporary damage up" },
    { Name = "Speed Boost", Class = "BP_Item_SpeedBoost", Description = "Temporary speed up" },
    { Name = "Invincibility", Class = "BP_Item_Invincibility", Description = "Temporary invincibility" },

    -- Chests
    { Name = "Common Chest", Class = "BP_Chest_Common", Description = "Basic loot" },
    { Name = "Rare Chest", Class = "BP_Chest_Rare", Description = "Better loot" },
    { Name = "Epic Chest", Class = "BP_Chest_Epic", Description = "Great loot" },
    { Name = "Legendary Chest", Class = "BP_Chest_Legendary", Description = "Best loot" },
    { Name = "Prismatic Chest", Class = "BP_Chest_Prismatic", Description = "Contains prismatic" },
}

-----------------------------------------------------------
-- Enemies (for spawning)
-----------------------------------------------------------

Database.Enemies = {
    { Name = "Crab Scout", Class = "BP_Enemy_CrabScout" },
    { Name = "Crab Soldier", Class = "BP_Enemy_CrabSoldier" },
    { Name = "Crab Heavy", Class = "BP_Enemy_CrabHeavy" },
    { Name = "Crab Sniper", Class = "BP_Enemy_CrabSniper" },
    { Name = "Crab Boss", Class = "BP_Enemy_CrabBoss" },
    { Name = "Mini Boss", Class = "BP_Enemy_MiniBoss" },
}

return Database
