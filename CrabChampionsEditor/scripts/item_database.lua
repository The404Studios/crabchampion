--[[
    Item Database for Crab Champions
    Contains known item names, categories, and spawn helpers

    NOTE: These names are based on community research.
    Use SDKHelper.Search() or Ctrl+J dump to find exact class names.
]]

ItemDB = {}

-- ============================================================================
-- WEAPON CATEGORIES
-- ============================================================================

ItemDB.Weapons = {
    -- Pistols
    Pistol = {
        "Pistol",
        "Revolver",
        "Deagle",
        "DualPistols",
    },

    -- SMGs
    SMG = {
        "SMG",
        "Vector",
        "P90",
        "MAC10",
    },

    -- Shotguns
    Shotgun = {
        "Shotgun",
        "PumpShotgun",
        "AutoShotgun",
        "DoubleBarrel",
    },

    -- Rifles
    Rifle = {
        "AssaultRifle",
        "AK47",
        "M4",
        "BurstRifle",
    },

    -- Snipers
    Sniper = {
        "SniperRifle",
        "AWP",
        "Scout",
    },

    -- Heavy
    Heavy = {
        "Minigun",
        "LMG",
        "GrenadeLauncher",
        "RocketLauncher",
    },

    -- Special
    Special = {
        "LaserGun",
        "RailGun",
        "PlasmaRifle",
        "Crossbow",
        "Flamethrower",
    },
}

-- ============================================================================
-- ITEM CATEGORIES
-- ============================================================================

ItemDB.Items = {
    -- Damage Items
    Damage = {
        "DamageUp",
        "CritChance",
        "CritDamage",
        "ArmorPiercing",
        "ExplosiveDamage",
        "FireDamage",
        "IceDamage",
        "LightningDamage",
        "PoisonDamage",
    },

    -- Fire Rate / Attack Speed
    AttackSpeed = {
        "FireRateUp",
        "AttackSpeedUp",
        "ReloadSpeedUp",
        "MagazineSize",
        "AmmoCapacity",
    },

    -- Defense Items
    Defense = {
        "HealthUp",
        "MaxHealthUp",
        "ShieldUp",
        "ArmorUp",
        "DamageResist",
        "HealthRegen",
        "Lifesteal",
    },

    -- Movement Items
    Movement = {
        "SpeedUp",
        "JumpHeightUp",
        "DoubleJump",
        "TripleJump",
        "DashRange",
        "DashCooldown",
        "AirControl",
    },

    -- Utility Items
    Utility = {
        "LuckUp",
        "XPBoost",
        "CurrencyBoost",
        "CooldownReduction",
        "RangeUp",
        "ProjectileSpeed",
        "ProjectileSize",
    },

    -- Special Items
    Special = {
        "Magnet",
        "AutoAim",
        "Piercing",
        "Ricochet",
        "Homing",
        "Explosion",
        "Chain",
        "Split",
    },
}

-- ============================================================================
-- PRISMATIC TYPES
-- ============================================================================

ItemDB.Prismatics = {
    -- Combat Prismatics
    "PrismaticDamage",
    "PrismaticCrit",
    "PrismaticFireRate",
    "PrismaticExplosion",
    "PrismaticChain",

    -- Defense Prismatics
    "PrismaticHealth",
    "PrismaticShield",
    "PrismaticRegen",
    "PrismaticLifesteal",

    -- Movement Prismatics
    "PrismaticSpeed",
    "PrismaticDash",
    "PrismaticJump",

    -- Utility Prismatics
    "PrismaticLuck",
    "PrismaticXP",
    "PrismaticCurrency",
}

-- ============================================================================
-- PERKS
-- ============================================================================

ItemDB.Perks = {
    "GlassCannon",
    "Tank",
    "Speedster",
    "Sniper",
    "Berserker",
    "Vampire",
    "Lucky",
    "Greedy",
    "Explosive",
    "Electric",
    "Frost",
    "Fire",
    "Poison",
    "Ninja",
    "Heavy",
}

-- ============================================================================
-- MODS (In-game modifiers)
-- ============================================================================

ItemDB.Mods = {
    "DoubleDamage",
    "HalfDamage",
    "DoubleHealth",
    "HalfHealth",
    "FastEnemies",
    "SlowEnemies",
    "MoreEnemies",
    "LessEnemies",
    "EliteEnemies",
    "NightmareMode",
}

-- ============================================================================
-- TOTEMS
-- ============================================================================

ItemDB.Totems = {
    "ChanceTotem",
    "CrystalTotem",
    "FuseTotem",
    "GambleTotem",
    "GlassTotem",
    "GoldTotem",
    "GreedTotem",
    "HealthTotem",
    "LootTotem",
    "RandomTotem",
    "RerollTotem",
}

-- ============================================================================
-- HELPER FUNCTIONS
-- ============================================================================

-- List all items in a category
ItemDB.ListCategory = function(category)
    print(string.format("\n[ItemDB] Category: %s\n", category))

    local items = ItemDB.Items[category] or ItemDB.Weapons[category]
    if items then
        for i, item in ipairs(items) do
            print(string.format("  %d. %s\n", i, item))
        end
    else
        print("  Category not found\n")
    end
end

-- List all categories
ItemDB.ListCategories = function()
    print("\n[ItemDB] Available Categories:\n")

    print("\nWeapon Categories:\n")
    for category, _ in pairs(ItemDB.Weapons) do
        print(string.format("  - Weapons.%s\n", category))
    end

    print("\nItem Categories:\n")
    for category, _ in pairs(ItemDB.Items) do
        print(string.format("  - Items.%s\n", category))
    end

    print("\nOther:\n")
    print("  - Prismatics\n")
    print("  - Perks\n")
    print("  - Mods\n")
    print("  - Totems\n")
end

-- Search for item by name
ItemDB.Search = function(query)
    print(string.format("\n[ItemDB] Searching for: %s\n", query))

    local found = {}
    query = string.lower(query)

    -- Search weapons
    for category, items in pairs(ItemDB.Weapons) do
        for _, item in ipairs(items) do
            if string.find(string.lower(item), query) then
                table.insert(found, {category = "Weapons." .. category, name = item})
            end
        end
    end

    -- Search items
    for category, items in pairs(ItemDB.Items) do
        for _, item in ipairs(items) do
            if string.find(string.lower(item), query) then
                table.insert(found, {category = "Items." .. category, name = item})
            end
        end
    end

    -- Search prismatics
    for _, item in ipairs(ItemDB.Prismatics) do
        if string.find(string.lower(item), query) then
            table.insert(found, {category = "Prismatics", name = item})
        end
    end

    -- Search perks
    for _, item in ipairs(ItemDB.Perks) do
        if string.find(string.lower(item), query) then
            table.insert(found, {category = "Perks", name = item})
        end
    end

    -- Display results
    if #found > 0 then
        for _, result in ipairs(found) do
            print(string.format("  [%s] %s\n", result.category, result.name))
        end
        print(string.format("\nFound %d results\n", #found))
    else
        print("  No results found\n")
    end

    return found
end

-- Get random item from category
ItemDB.GetRandom = function(category)
    local items = ItemDB.Items[category] or ItemDB.Weapons[category]
    if items and #items > 0 then
        local index = math.random(1, #items)
        return items[index]
    end
    return nil
end

-- Get all items flattened
ItemDB.GetAllItems = function()
    local all = {}

    for _, items in pairs(ItemDB.Weapons) do
        for _, item in ipairs(items) do
            table.insert(all, item)
        end
    end

    for _, items in pairs(ItemDB.Items) do
        for _, item in ipairs(items) do
            table.insert(all, item)
        end
    end

    for _, item in ipairs(ItemDB.Prismatics) do
        table.insert(all, item)
    end

    return all
end

-- Help
ItemDB.Help = function()
    print("\n==========================================================\n")
    print("ITEM DATABASE COMMANDS\n")
    print("==========================================================\n")
    print("\n")
    print("BROWSING:\n")
    print("  ItemDB.ListCategories()        - Show all categories\n")
    print("  ItemDB.ListCategory('Damage')  - Show items in category\n")
    print("  ItemDB.Search('crit')          - Search for items\n")
    print("\n")
    print("DATA ACCESS:\n")
    print("  ItemDB.Weapons.Pistol          - Access weapon list\n")
    print("  ItemDB.Items.Damage            - Access item list\n")
    print("  ItemDB.Prismatics              - Access prismatic list\n")
    print("  ItemDB.Perks                   - Access perk list\n")
    print("  ItemDB.Totems                  - Access totem list\n")
    print("\n")
    print("UTILITIES:\n")
    print("  ItemDB.GetRandom('Damage')     - Get random item\n")
    print("  ItemDB.GetAllItems()           - Get all items\n")
    print("\n")
    print("NOTE: Use CrabEditor.SpawnItem('name') to spawn items\n")
    print("      Use SDKHelper to find exact class names if needed\n")
    print("==========================================================\n")
end

print("[ItemDB] Item Database loaded. Use ItemDB.Help() for commands.\n")
