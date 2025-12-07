--[[
    SDK Helper for Crab Champions
    Tools for discovering and documenting game objects
]]

SDKHelper = {}

-- ============================================================================
-- OBJECT DISCOVERY
-- ============================================================================

-- Find all objects of a specific type
SDKHelper.FindAll = function(className)
    print(string.format("\n[SDKHelper] Finding all: %s\n", className))

    local objects = FindAllOf(className)
    if objects then
        for i, obj in ipairs(objects) do
            if obj:IsValid() then
                print(string.format("  [%d] %s\n", i, obj:GetFullName()))
            end
        end
        print(string.format("\n[SDKHelper] Found %d objects\n", #objects))
    else
        print("[SDKHelper] No objects found\n")
    end

    return objects
end

-- Search for objects by partial name
SDKHelper.Search = function(pattern)
    print(string.format("\n[SDKHelper] Searching for pattern: %s\n", pattern))

    local searchClasses = {
        "Actor",
        "Component",
        "Object",
        "Widget",
        "GameMode",
        "PlayerState",
        "Character",
        "Pawn",
        "Controller",
    }

    local found = 0
    for _, className in ipairs(searchClasses) do
        local objects = FindAllOf(className)
        if objects then
            for _, obj in ipairs(objects) do
                if obj:IsValid() then
                    local fullName = obj:GetFullName()
                    if string.find(string.lower(fullName), string.lower(pattern)) then
                        print(string.format("  %s\n", fullName))
                        found = found + 1
                    end
                end
            end
        end
    end

    print(string.format("\n[SDKHelper] Found %d matching objects\n", found))
end

-- ============================================================================
-- PROPERTY INSPECTION
-- ============================================================================

-- Dump all properties of an object
SDKHelper.DumpProperties = function(className)
    print(string.format("\n[SDKHelper] Dumping properties of: %s\n", className))

    local obj = FindFirstOf(className)
    if not obj or not obj:IsValid() then
        print("[SDKHelper] Object not found\n")
        return
    end

    print(string.format("Full Name: %s\n", obj:GetFullName()))
    print("Properties:\n")

    pcall(function()
        local reflection = obj:Reflection()
        if reflection then
            for propName, propValue in pairs(reflection) do
                print(string.format("  %s = %s\n", tostring(propName), tostring(propValue)))
            end
        end
    end)

    -- Try ForEachProperty if available
    pcall(function()
        obj:ForEachProperty(function(prop)
            print(string.format("  [Property] %s\n", prop:GetFullName()))
        end)
    end)
end

-- Get specific property value
SDKHelper.GetProperty = function(className, propertyName)
    local obj = FindFirstOf(className)
    if not obj or not obj:IsValid() then
        print(string.format("[SDKHelper] Object not found: %s\n", className))
        return nil
    end

    local value = nil
    pcall(function()
        value = obj[propertyName]
    end)

    print(string.format("[SDKHelper] %s.%s = %s\n", className, propertyName, tostring(value)))
    return value
end

-- Set property value
SDKHelper.SetProperty = function(className, propertyName, value)
    local obj = FindFirstOf(className)
    if not obj or not obj:IsValid() then
        print(string.format("[SDKHelper] Object not found: %s\n", className))
        return false
    end

    local success = pcall(function()
        obj[propertyName] = value
    end)

    if success then
        print(string.format("[SDKHelper] Set %s.%s = %s\n", className, propertyName, tostring(value)))
    else
        print(string.format("[SDKHelper] Failed to set %s.%s\n", className, propertyName))
    end

    return success
end

-- ============================================================================
-- CLASS HIERARCHY
-- ============================================================================

-- List all classes matching a pattern
SDKHelper.ListClasses = function(pattern)
    print(string.format("\n[SDKHelper] Listing classes matching: %s\n", pattern or "*"))

    local classes = {
        -- Core Crab Champions classes (expected)
        "CC_Character",
        "CC_PlayerController",
        "CC_PlayerState",
        "CC_GameInstance",
        "CC_GameMode",
        "CC_SaveGame",
        "CC_HUD",

        -- Components
        "CC_HealthComponent",
        "CC_WeaponComponent",
        "CC_InventoryComponent",
        "CC_MovementComponent",

        -- Gameplay
        "CC_Item",
        "CC_Weapon",
        "CC_Upgrade",
        "CC_Prismatic",
        "CC_Perk",
        "CC_Mod",

        -- Managers
        "CC_LootManager",
        "CC_ItemSpawner",
        "CC_EnemySpawner",
        "CC_WaveManager",
        "CC_RewardManager",
        "CC_PrismaticManager",

        -- World
        "CC_Island",
        "CC_Portal",
        "CC_Totem",
        "CC_Chest",
    }

    for _, className in ipairs(classes) do
        if not pattern or string.find(string.lower(className), string.lower(pattern)) then
            local obj = FindFirstOf(className)
            local status = (obj and obj:IsValid()) and "FOUND" or "not loaded"
            print(string.format("  %s: %s\n", className, status))
        end
    end
end

-- ============================================================================
-- GAME STATE INSPECTION
-- ============================================================================

-- Get current game state
SDKHelper.GetGameState = function()
    print("\n[SDKHelper] Current Game State:\n")

    -- Player info
    local character = FindFirstOf("CC_Character")
    if character and character:IsValid() then
        print("  Player Character: FOUND\n")
        pcall(function()
            print(string.format("    Health: %s\n", tostring(character.Health)))
            print(string.format("    MaxHealth: %s\n", tostring(character.MaxHealth)))
        end)
    else
        character = FindFirstOf("Character")
        print(string.format("  Generic Character: %s\n", (character and character:IsValid()) and "FOUND" or "NOT FOUND"))
    end

    -- Controller
    local controller = FindFirstOf("CC_PlayerController")
    if not controller or not controller:IsValid() then
        controller = FindFirstOf("PlayerController")
    end
    print(string.format("  Player Controller: %s\n", (controller and controller:IsValid()) and "FOUND" or "NOT FOUND"))

    -- Game Instance
    local instance = FindFirstOf("CC_GameInstance")
    if not instance or not instance:IsValid() then
        instance = FindFirstOf("GameInstance")
    end
    print(string.format("  Game Instance: %s\n", (instance and instance:IsValid()) and "FOUND" or "NOT FOUND"))

    -- Save Game
    local save = FindFirstOf("CC_SaveGame")
    print(string.format("  Save Game: %s\n", (save and save:IsValid()) and "FOUND" or "NOT FOUND"))

    print("\n")
end

-- ============================================================================
-- FUNCTION DISCOVERY
-- ============================================================================

-- Try to call a function on an object
SDKHelper.TryCall = function(className, functionName, ...)
    local obj = FindFirstOf(className)
    if not obj or not obj:IsValid() then
        print(string.format("[SDKHelper] Object not found: %s\n", className))
        return nil
    end

    local result = nil
    local success = pcall(function()
        result = obj[functionName](obj, ...)
    end)

    if success then
        print(string.format("[SDKHelper] %s:%s() = %s\n", className, functionName, tostring(result)))
    else
        print(string.format("[SDKHelper] Failed to call %s:%s()\n", className, functionName))
    end

    return result
end

-- ============================================================================
-- EXPORT FUNCTIONS
-- ============================================================================

-- Export discovered data to console
SDKHelper.Export = function()
    print("\n==========================================================\n")
    print("SDK HELPER EXPORT\n")
    print("==========================================================\n")

    SDKHelper.GetGameState()
    SDKHelper.ListClasses()

    print("==========================================================\n")
    print("For full SDK dump, press Ctrl+J in game\n")
    print("For C++ headers, press Ctrl+H in game\n")
    print("==========================================================\n")
end

-- Help
SDKHelper.Help = function()
    print("\n==========================================================\n")
    print("SDK HELPER COMMANDS\n")
    print("==========================================================\n")
    print("\n")
    print("DISCOVERY:\n")
    print("  SDKHelper.FindAll('ClassName')      - Find all objects of type\n")
    print("  SDKHelper.Search('pattern')         - Search objects by name\n")
    print("  SDKHelper.ListClasses('pattern')    - List known class names\n")
    print("\n")
    print("INSPECTION:\n")
    print("  SDKHelper.DumpProperties('Class')   - Dump object properties\n")
    print("  SDKHelper.GetProperty('Class','Prop') - Get property value\n")
    print("  SDKHelper.SetProperty('Class','Prop',val) - Set property\n")
    print("\n")
    print("FUNCTIONS:\n")
    print("  SDKHelper.TryCall('Class','Func',...) - Try calling function\n")
    print("\n")
    print("STATE:\n")
    print("  SDKHelper.GetGameState()            - Show current game state\n")
    print("  SDKHelper.Export()                  - Export all discovered info\n")
    print("\n")
    print("==========================================================\n")
end

print("[SDKHelper] SDK Helper loaded. Use SDKHelper.Help() for commands.\n")
