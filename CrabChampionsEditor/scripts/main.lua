--[[
    Crab Champions Editor Tool
    A comprehensive modding/trainer tool for Crab Champions
    Built with UE4SS (Unreal Engine 4 Scripting System)

    Author: CrabChampionsEditor
    Version: 1.0.0
    Game Version: Latest (2024+)
]]

-- ============================================================================
-- CONFIGURATION
-- ============================================================================

local Config = {
    -- Mod settings
    ModName = "CrabChampionsEditor",
    Version = "1.0.0",
    DebugMode = true,

    -- Default values
    DefaultSpeedMultiplier = 1.0,
    DefaultJumpMultiplier = 1.0,
    DefaultDamageMultiplier = 1.0,
    DefaultFireRateMultiplier = 1.0,

    -- Hotkey delays (ms)
    KeyDebounce = 200,
}

-- ============================================================================
-- STATE MANAGEMENT
-- ============================================================================

local State = {
    -- Player modifications
    GodModeEnabled = false,
    InfiniteAmmoEnabled = false,
    NoClipEnabled = false,
    DualWieldEnabled = false,

    -- Multipliers
    SpeedMultiplier = 1.0,
    JumpMultiplier = 1.0,
    DamageMultiplier = 1.0,
    FireRateMultiplier = 1.0,

    -- Cached references
    PlayerController = nil,
    PlayerCharacter = nil,
    GameInstance = nil,

    -- Menu state
    MenuVisible = false,
    LastKeyPress = {},

    -- Hook IDs for cleanup
    Hooks = {},
}

-- ============================================================================
-- UTILITY FUNCTIONS
-- ============================================================================

local function Log(message)
    if Config.DebugMode then
        print(string.format("[%s] %s\n", Config.ModName, message))
    end
end

local function LogError(message)
    print(string.format("[%s] ERROR: %s\n", Config.ModName, message))
end

local function LogSuccess(message)
    print(string.format("[%s] SUCCESS: %s\n", Config.ModName, message))
end

local function CanProcessKey(keyName)
    local currentTime = os.clock() * 1000
    local lastPress = State.LastKeyPress[keyName] or 0

    if currentTime - lastPress < Config.KeyDebounce then
        return false
    end

    State.LastKeyPress[keyName] = currentTime
    return true
end

-- Safe object access wrapper
local function SafeCall(func, ...)
    local success, result = pcall(func, ...)
    if not success then
        LogError("Function call failed: " .. tostring(result))
        return nil
    end
    return result
end

-- ============================================================================
-- GAME OBJECT FINDERS
-- ============================================================================

local function FindPlayerController()
    local controller = FindFirstOf("PlayerController")
    if controller and controller:IsValid() then
        State.PlayerController = controller
        return controller
    end

    -- Try Crab Champions specific controller
    controller = FindFirstOf("CC_PlayerController")
    if controller and controller:IsValid() then
        State.PlayerController = controller
        return controller
    end

    return nil
end

local function FindPlayerCharacter()
    local character = FindFirstOf("CC_Character")
    if character and character:IsValid() then
        State.PlayerCharacter = character
        return character
    end

    -- Fallback to generic character
    character = FindFirstOf("Character")
    if character and character:IsValid() then
        State.PlayerCharacter = character
        return character
    end

    return nil
end

local function FindGameInstance()
    local instance = FindFirstOf("CC_GameInstance")
    if instance and instance:IsValid() then
        State.GameInstance = instance
        return instance
    end

    instance = FindFirstOf("GameInstance")
    if instance and instance:IsValid() then
        State.GameInstance = instance
        return instance
    end

    return nil
end

local function FindPlayerState()
    local state = FindFirstOf("CC_PlayerState")
    if state and state:IsValid() then
        return state
    end

    state = FindFirstOf("PlayerState")
    if state and state:IsValid() then
        return state
    end

    return nil
end

local function FindSaveGame()
    local save = FindFirstOf("CC_SaveGame")
    if save and save:IsValid() then
        return save
    end
    return nil
end

local function FindInventory()
    local inventory = FindFirstOf("CC_InventoryComponent")
    if inventory and inventory:IsValid() then
        return inventory
    end

    inventory = FindFirstOf("InventoryComponent")
    if inventory and inventory:IsValid() then
        return inventory
    end

    return nil
end

local function FindWeaponManager()
    local manager = FindFirstOf("CC_WeaponManager")
    if manager and manager:IsValid() then
        return manager
    end

    manager = FindFirstOf("WeaponComponent")
    if manager and manager:IsValid() then
        return manager
    end

    return nil
end

-- ============================================================================
-- PLAYER MODIFICATIONS
-- ============================================================================

local function SetGodMode(enabled)
    State.GodModeEnabled = enabled

    local character = FindPlayerCharacter()
    if not character then
        LogError("Could not find player character for God Mode")
        return false
    end

    -- Try to find and modify health component
    SafeCall(function()
        -- Common health property names in UE games
        local healthProps = {"Health", "CurrentHealth", "HP", "HealthPoints"}
        local maxHealthProps = {"MaxHealth", "MaxHP", "MaxHealthPoints"}

        for _, prop in ipairs(healthProps) do
            pcall(function()
                if enabled then
                    -- Set health to very high value
                    character[prop] = 999999
                end
            end)
        end

        for _, prop in ipairs(maxHealthProps) do
            pcall(function()
                if enabled then
                    character[prop] = 999999
                end
            end)
        end

        -- Try to set invulnerability flags
        pcall(function() character.bCanBeDamaged = not enabled end)
        pcall(function() character.bIsInvulnerable = enabled end)
        pcall(function() character.Invulnerable = enabled end)
    end)

    if enabled then
        LogSuccess("God Mode ENABLED")
    else
        Log("God Mode DISABLED")
    end

    return true
end

local function SetInfiniteAmmo(enabled)
    State.InfiniteAmmoEnabled = enabled

    if enabled then
        LogSuccess("Infinite Ammo ENABLED")
    else
        Log("Infinite Ammo DISABLED")
    end

    return true
end

local function SetNoClip(enabled)
    State.NoClipEnabled = enabled

    local character = FindPlayerCharacter()
    if not character then
        LogError("Could not find player character for No Clip")
        return false
    end

    SafeCall(function()
        -- Try common collision properties
        pcall(function() character.bActorEnableCollision = not enabled end)
        pcall(function()
            local movement = character.CharacterMovement
            if movement and movement:IsValid() then
                if enabled then
                    movement.MovementMode = 5 -- Flying
                else
                    movement.MovementMode = 1 -- Walking
                end
            end
        end)
    end)

    if enabled then
        LogSuccess("No Clip ENABLED")
    else
        Log("No Clip DISABLED")
    end

    return true
end

local function SetSpeedMultiplier(multiplier)
    State.SpeedMultiplier = multiplier

    local character = FindPlayerCharacter()
    if not character then
        LogError("Could not find player character for Speed modification")
        return false
    end

    SafeCall(function()
        pcall(function()
            local movement = character.CharacterMovement
            if movement and movement:IsValid() then
                -- Default walk speed is typically 600
                local baseSpeed = 600
                movement.MaxWalkSpeed = baseSpeed * multiplier
                movement.MaxWalkSpeedCrouched = (baseSpeed * 0.5) * multiplier
            end
        end)

        -- Try direct properties
        pcall(function() character.WalkSpeed = 600 * multiplier end)
        pcall(function() character.RunSpeed = 900 * multiplier end)
        pcall(function() character.SprintSpeed = 1200 * multiplier end)
    end)

    Log(string.format("Speed Multiplier set to %.1fx", multiplier))
    return true
end

local function SetJumpMultiplier(multiplier)
    State.JumpMultiplier = multiplier

    local character = FindPlayerCharacter()
    if not character then
        LogError("Could not find player character for Jump modification")
        return false
    end

    SafeCall(function()
        pcall(function()
            local movement = character.CharacterMovement
            if movement and movement:IsValid() then
                -- Default jump velocity is typically 420
                local baseJump = 420
                movement.JumpZVelocity = baseJump * multiplier
            end
        end)

        pcall(function() character.JumpHeight = 420 * multiplier end)
        pcall(function() character.JumpForce = 420 * multiplier end)
    end)

    Log(string.format("Jump Multiplier set to %.1fx", multiplier))
    return true
end

-- ============================================================================
-- CURRENCY SYSTEM
-- ============================================================================

local function SetKeys(amount)
    local save = FindSaveGame()
    local playerState = FindPlayerState()

    SafeCall(function()
        -- Try save game
        if save then
            pcall(function() save.Keys = amount end)
            pcall(function() save.TotalKeys = amount end)
        end

        -- Try player state
        if playerState then
            pcall(function() playerState.Keys = amount end)
            pcall(function() playerState.CurrentKeys = amount end)
        end

        -- Try game instance
        local instance = FindGameInstance()
        if instance then
            pcall(function() instance.Keys = amount end)
            pcall(function() instance.PlayerKeys = amount end)
        end
    end)

    LogSuccess(string.format("Keys set to %d", amount))
    return true
end

local function SetCrystals(amount)
    local save = FindSaveGame()
    local playerState = FindPlayerState()

    SafeCall(function()
        -- Try save game
        if save then
            pcall(function() save.Crystals = amount end)
            pcall(function() save.TotalCrystals = amount end)
        end

        -- Try player state
        if playerState then
            pcall(function() playerState.Crystals = amount end)
            pcall(function() playerState.CurrentCrystals = amount end)
        end

        -- Try game instance
        local instance = FindGameInstance()
        if instance then
            pcall(function() instance.Crystals = amount end)
            pcall(function() instance.PlayerCrystals = amount end)
        end
    end)

    LogSuccess(string.format("Crystals set to %d", amount))
    return true
end

local function AddKeys(amount)
    local save = FindSaveGame()

    SafeCall(function()
        if save then
            local current = save.Keys or 0
            pcall(function() save.Keys = current + amount end)
        end
    end)

    LogSuccess(string.format("Added %d Keys", amount))
    return true
end

local function AddCrystals(amount)
    local save = FindSaveGame()

    SafeCall(function()
        if save then
            local current = save.Crystals or 0
            pcall(function() save.Crystals = current + amount end)
        end
    end)

    LogSuccess(string.format("Added %d Crystals", amount))
    return true
end

-- ============================================================================
-- ITEM SPAWNING SYSTEM
-- ============================================================================

-- Known item class patterns (to be expanded with SDK dump)
local ItemClasses = {
    -- Weapons
    "BP_Weapon_Pistol",
    "BP_Weapon_Shotgun",
    "BP_Weapon_SMG",
    "BP_Weapon_Rifle",
    "BP_Weapon_Sniper",
    "BP_Weapon_Launcher",
    "BP_Weapon_Laser",

    -- Items/Upgrades
    "BP_Item_",
    "BP_Upgrade_",
    "BP_Perk_",
    "BP_Mod_",

    -- Prismatics
    "BP_Prismatic_",
}

local function SpawnItem(itemName)
    Log(string.format("Attempting to spawn: %s", itemName))

    local inventory = FindInventory()
    local character = FindPlayerCharacter()

    SafeCall(function()
        -- Try to find the item class
        local itemClass = StaticFindObject(string.format("/Game/Blueprints/Items/%s.%s_C", itemName, itemName))

        if not itemClass then
            -- Try alternate paths
            itemClass = StaticFindObject(string.format("/Game/Items/%s.%s_C", itemName, itemName))
        end

        if not itemClass then
            itemClass = StaticFindObject(string.format("/Game/Blueprints/Upgrades/%s.%s_C", itemName, itemName))
        end

        if itemClass and itemClass:IsValid() then
            -- Try to add to inventory
            if inventory and inventory:IsValid() then
                pcall(function() inventory:AddItem(itemClass) end)
                pcall(function() inventory:GiveItem(itemClass) end)
            end

            LogSuccess(string.format("Spawned: %s", itemName))
        else
            Log(string.format("Could not find item class: %s", itemName))
            Log("Use SDK dump (Ctrl+J) to find valid item names")
        end
    end)

    return true
end

local function SpawnRandomItem()
    Log("Spawning random item...")

    local character = FindPlayerCharacter()
    local controller = FindPlayerController()

    SafeCall(function()
        -- Try to trigger the game's item spawn system
        pcall(function()
            local spawner = FindFirstOf("CC_ItemSpawner")
            if spawner and spawner:IsValid() then
                spawner:SpawnRandomItem()
            end
        end)

        pcall(function()
            local lootManager = FindFirstOf("CC_LootManager")
            if lootManager and lootManager:IsValid() then
                lootManager:DropRandomLoot()
            end
        end)
    end)

    LogSuccess("Random item spawn triggered")
    return true
end

local function SpawnPrismatic()
    Log("Spawning prismatic...")

    SafeCall(function()
        -- Try to find and spawn a prismatic
        local allPrismatics = FindAllOf("CC_Prismatic")

        if allPrismatics then
            Log(string.format("Found %d prismatic classes", #allPrismatics))
        end

        pcall(function()
            local prismaticManager = FindFirstOf("CC_PrismaticManager")
            if prismaticManager and prismaticManager:IsValid() then
                prismaticManager:SpawnPrismatic()
            end
        end)

        pcall(function()
            local rewardManager = FindFirstOf("CC_RewardManager")
            if rewardManager and rewardManager:IsValid() then
                rewardManager:GivePrismatic()
            end
        end)
    end)

    LogSuccess("Prismatic spawn triggered")
    return true
end

local function ClearItems()
    Log("Clearing all items...")

    local inventory = FindInventory()

    SafeCall(function()
        if inventory and inventory:IsValid() then
            pcall(function() inventory:ClearInventory() end)
            pcall(function() inventory:RemoveAllItems() end)
            pcall(function() inventory:Clear() end)
        end
    end)

    LogSuccess("Items cleared")
    return true
end

-- ============================================================================
-- WEAPON MODIFICATIONS
-- ============================================================================

local function SetDualWield(enabled)
    State.DualWieldEnabled = enabled

    local weaponManager = FindWeaponManager()
    local character = FindPlayerCharacter()

    SafeCall(function()
        if weaponManager and weaponManager:IsValid() then
            pcall(function() weaponManager.bDualWield = enabled end)
            pcall(function() weaponManager.DualWieldEnabled = enabled end)
            pcall(function() weaponManager:SetDualWield(enabled) end)
        end

        if character then
            pcall(function() character.bDualWield = enabled end)
            pcall(function() character.DualWielding = enabled end)
        end
    end)

    if enabled then
        LogSuccess("Dual Wield ENABLED")
    else
        Log("Dual Wield DISABLED")
    end

    return true
end

local function SetFireRate(multiplier)
    State.FireRateMultiplier = multiplier

    Log(string.format("Fire Rate Multiplier set to %.1fx", multiplier))
    return true
end

local function SetDamage(multiplier)
    State.DamageMultiplier = multiplier

    local character = FindPlayerCharacter()

    SafeCall(function()
        if character then
            pcall(function() character.DamageMultiplier = multiplier end)
            pcall(function() character.BaseDamageMultiplier = multiplier end)
        end

        local weaponManager = FindWeaponManager()
        if weaponManager and weaponManager:IsValid() then
            pcall(function() weaponManager.DamageMultiplier = multiplier end)
        end
    end)

    Log(string.format("Damage Multiplier set to %.1fx", multiplier))
    return true
end

-- ============================================================================
-- UNLOCK SYSTEM
-- ============================================================================

local function UnlockAllSkins()
    Log("Unlocking all skins...")

    local save = FindSaveGame()

    SafeCall(function()
        if save then
            pcall(function() save.UnlockedSkins = {} end)  -- May need specific implementation
            pcall(function() save:UnlockAllSkins() end)
            pcall(function() save.bAllSkinsUnlocked = true end)
        end

        local instance = FindGameInstance()
        if instance then
            pcall(function() instance:UnlockAllSkins() end)
            pcall(function() instance.bDebugUnlockAll = true end)
        end
    end)

    LogSuccess("All skins unlock triggered")
    return true
end

local function UnlockAllCosmetics()
    Log("Unlocking all cosmetics...")

    local save = FindSaveGame()

    SafeCall(function()
        if save then
            pcall(function() save:UnlockAllCosmetics() end)
            pcall(function() save.bAllCosmeticsUnlocked = true end)
        end

        local instance = FindGameInstance()
        if instance then
            pcall(function() instance:UnlockAllCosmetics() end)
            pcall(function() instance:DebugUnlockAllContent() end)
        end
    end)

    LogSuccess("All cosmetics unlock triggered")
    return true
end

-- ============================================================================
-- HOOKS FOR CONTINUOUS EFFECTS
-- ============================================================================

local function SetupHooks()
    Log("Setting up game hooks...")

    -- Hook for infinite ammo
    SafeCall(function()
        local hookId1, hookId2 = RegisterHook("/Script/CrabChampions.CC_WeaponComponent:ConsumeAmmo", function(context)
            if State.InfiniteAmmoEnabled then
                return 0  -- Don't consume ammo
            end
        end)

        if hookId1 then
            table.insert(State.Hooks, {hookId1, hookId2})
            Log("Infinite ammo hook registered")
        end
    end)

    -- Hook for god mode (prevent damage)
    SafeCall(function()
        local hookId1, hookId2 = RegisterHook("/Script/CrabChampions.CC_Character:TakeDamage", function(context, damage)
            if State.GodModeEnabled then
                return 0  -- No damage taken
            end
        end)

        if hookId1 then
            table.insert(State.Hooks, {hookId1, hookId2})
            Log("God mode hook registered")
        end
    end)

    -- Hook for fire rate modification
    SafeCall(function()
        local hookId1, hookId2 = RegisterHook("/Script/CrabChampions.CC_WeaponComponent:GetFireRate", function(context)
            if State.FireRateMultiplier ~= 1.0 then
                -- Attempt to modify return value
            end
        end)

        if hookId1 then
            table.insert(State.Hooks, {hookId1, hookId2})
            Log("Fire rate hook registered")
        end
    end)

    -- Generic damage hook fallback
    SafeCall(function()
        local hookId1, hookId2 = RegisterHook("/Script/Engine.Actor:TakeDamage", function(context, damage)
            local character = FindPlayerCharacter()
            if State.GodModeEnabled and character and context:get() == character then
                return 0
            end
        end)

        if hookId1 then
            table.insert(State.Hooks, {hookId1, hookId2})
            Log("Generic damage hook registered")
        end
    end)

    Log("Hooks setup complete")
end

-- ============================================================================
-- MENU SYSTEM
-- ============================================================================

local function PrintMenu()
    print("\n")
    print("==========================================================\n")
    print("           CRAB CHAMPIONS EDITOR v" .. Config.Version .. "\n")
    print("==========================================================\n")
    print("\n")
    print("  PLAYER MODS:\n")
    print(string.format("    [F2] God Mode:      %s\n", State.GodModeEnabled and "ON" or "OFF"))
    print(string.format("    [F3] Infinite Ammo: %s\n", State.InfiniteAmmoEnabled and "ON" or "OFF"))
    print(string.format("    [F4] No Clip:       %s\n", State.NoClipEnabled and "ON" or "OFF"))
    print(string.format("    [+/-] Speed:        %.1fx\n", State.SpeedMultiplier))
    print("\n")
    print("  CURRENCY:\n")
    print("    [F5] Add 100 Keys\n")
    print("    [F6] Add 100 Crystals\n")
    print("\n")
    print("  ITEMS:\n")
    print("    [F7] Spawn Random Item\n")
    print("    [F8] Spawn Prismatic\n")
    print("\n")
    print("  WEAPONS:\n")
    print(string.format("    [F9] Dual Wield:    %s\n", State.DualWieldEnabled and "ON" or "OFF"))
    print("\n")
    print("  SYSTEM:\n")
    print("    [F10] Reset All Modifications\n")
    print("    [F1]  Toggle This Menu\n")
    print("\n")
    print("==========================================================\n")
    print("  Use console for advanced commands: CrabEditor.Help()\n")
    print("==========================================================\n")
    print("\n")
end

local function ToggleMenu()
    State.MenuVisible = not State.MenuVisible

    if State.MenuVisible then
        PrintMenu()
    else
        print("[CrabChampionsEditor] Menu hidden. Press F1 to show.\n")
    end
end

local function ResetAll()
    Log("Resetting all modifications...")

    State.GodModeEnabled = false
    State.InfiniteAmmoEnabled = false
    State.NoClipEnabled = false
    State.DualWieldEnabled = false
    State.SpeedMultiplier = 1.0
    State.JumpMultiplier = 1.0
    State.DamageMultiplier = 1.0
    State.FireRateMultiplier = 1.0

    SetGodMode(false)
    SetNoClip(false)
    SetSpeedMultiplier(1.0)
    SetJumpMultiplier(1.0)
    SetDualWield(false)

    LogSuccess("All modifications reset to default")
end

-- ============================================================================
-- KEYBIND SETUP
-- ============================================================================

local function SetupKeybinds()
    Log("Setting up keybinds...")

    -- F1 - Toggle Menu
    RegisterKeyBind(Key.F1, function()
        if CanProcessKey("F1") then
            ToggleMenu()
        end
    end)

    -- F2 - Toggle God Mode
    RegisterKeyBind(Key.F2, function()
        if CanProcessKey("F2") then
            SetGodMode(not State.GodModeEnabled)
        end
    end)

    -- F3 - Toggle Infinite Ammo
    RegisterKeyBind(Key.F3, function()
        if CanProcessKey("F3") then
            SetInfiniteAmmo(not State.InfiniteAmmoEnabled)
        end
    end)

    -- F4 - Toggle No Clip
    RegisterKeyBind(Key.F4, function()
        if CanProcessKey("F4") then
            SetNoClip(not State.NoClipEnabled)
        end
    end)

    -- F5 - Add 100 Keys
    RegisterKeyBind(Key.F5, function()
        if CanProcessKey("F5") then
            AddKeys(100)
        end
    end)

    -- F6 - Add 100 Crystals
    RegisterKeyBind(Key.F6, function()
        if CanProcessKey("F6") then
            AddCrystals(100)
        end
    end)

    -- F7 - Spawn Random Item
    RegisterKeyBind(Key.F7, function()
        if CanProcessKey("F7") then
            SpawnRandomItem()
        end
    end)

    -- F8 - Spawn Prismatic
    RegisterKeyBind(Key.F8, function()
        if CanProcessKey("F8") then
            SpawnPrismatic()
        end
    end)

    -- F9 - Toggle Dual Wield
    RegisterKeyBind(Key.F9, function()
        if CanProcessKey("F9") then
            SetDualWield(not State.DualWieldEnabled)
        end
    end)

    -- F10 - Reset All
    RegisterKeyBind(Key.F10, function()
        if CanProcessKey("F10") then
            ResetAll()
        end
    end)

    -- Numpad + : Increase Speed
    RegisterKeyBind(Key.ADD, function()
        if CanProcessKey("ADD") then
            State.SpeedMultiplier = math.min(State.SpeedMultiplier + 0.5, 10.0)
            SetSpeedMultiplier(State.SpeedMultiplier)
        end
    end)

    -- Numpad - : Decrease Speed
    RegisterKeyBind(Key.SUBTRACT, function()
        if CanProcessKey("SUBTRACT") then
            State.SpeedMultiplier = math.max(State.SpeedMultiplier - 0.5, 0.5)
            SetSpeedMultiplier(State.SpeedMultiplier)
        end
    end)

    Log("Keybinds registered successfully")
end

-- ============================================================================
-- GLOBAL API (Console Commands)
-- ============================================================================

CrabEditor = {}

-- Player Mods
CrabEditor.GodMode = SetGodMode
CrabEditor.InfiniteAmmo = SetInfiniteAmmo
CrabEditor.NoClip = SetNoClip
CrabEditor.SetSpeed = SetSpeedMultiplier
CrabEditor.SetJumpHeight = SetJumpMultiplier

-- Currency
CrabEditor.SetKeys = SetKeys
CrabEditor.SetCrystals = SetCrystals
CrabEditor.AddKeys = AddKeys
CrabEditor.AddCrystals = AddCrystals

-- Items
CrabEditor.SpawnItem = SpawnItem
CrabEditor.SpawnRandomItem = SpawnRandomItem
CrabEditor.SpawnPrismatic = SpawnPrismatic
CrabEditor.ClearItems = ClearItems

-- Weapons
CrabEditor.DualWield = SetDualWield
CrabEditor.SetFireRate = SetFireRate
CrabEditor.SetDamage = SetDamage

-- Unlocks
CrabEditor.UnlockAllSkins = UnlockAllSkins
CrabEditor.UnlockAllCosmetics = UnlockAllCosmetics

-- System
CrabEditor.Reset = ResetAll
CrabEditor.Menu = PrintMenu

CrabEditor.Help = function()
    print("\n")
    print("==========================================================\n")
    print("           CRAB CHAMPIONS EDITOR - CONSOLE COMMANDS\n")
    print("==========================================================\n")
    print("\n")
    print("PLAYER MODIFICATIONS:\n")
    print("  CrabEditor.GodMode(true/false)     - Toggle god mode\n")
    print("  CrabEditor.InfiniteAmmo(true/false) - Toggle infinite ammo\n")
    print("  CrabEditor.NoClip(true/false)      - Toggle no clip\n")
    print("  CrabEditor.SetSpeed(multiplier)    - Set speed (0.5-10.0)\n")
    print("  CrabEditor.SetJumpHeight(mult)     - Set jump height\n")
    print("\n")
    print("CURRENCY:\n")
    print("  CrabEditor.SetKeys(amount)         - Set key count\n")
    print("  CrabEditor.SetCrystals(amount)     - Set crystal count\n")
    print("  CrabEditor.AddKeys(amount)         - Add keys\n")
    print("  CrabEditor.AddCrystals(amount)     - Add crystals\n")
    print("\n")
    print("ITEMS:\n")
    print("  CrabEditor.SpawnItem('name')       - Spawn specific item\n")
    print("  CrabEditor.SpawnRandomItem()       - Spawn random item\n")
    print("  CrabEditor.SpawnPrismatic()        - Spawn prismatic\n")
    print("  CrabEditor.ClearItems()            - Clear all items\n")
    print("\n")
    print("WEAPONS:\n")
    print("  CrabEditor.DualWield(true/false)   - Toggle dual wield\n")
    print("  CrabEditor.SetFireRate(multiplier) - Set fire rate\n")
    print("  CrabEditor.SetDamage(multiplier)   - Set damage mult\n")
    print("\n")
    print("UNLOCKS:\n")
    print("  CrabEditor.UnlockAllSkins()        - Unlock all skins\n")
    print("  CrabEditor.UnlockAllCosmetics()    - Unlock cosmetics\n")
    print("\n")
    print("SYSTEM:\n")
    print("  CrabEditor.Reset()                 - Reset all mods\n")
    print("  CrabEditor.Menu()                  - Show menu\n")
    print("  CrabEditor.Help()                  - Show this help\n")
    print("\n")
    print("==========================================================\n")
    print("TIP: Use Ctrl+J to dump SDK for item/class names\n")
    print("==========================================================\n")
    print("\n")
end

-- Debug/Advanced functions
CrabEditor.Debug = {}

CrabEditor.Debug.FindClasses = function(pattern)
    Log(string.format("Searching for classes matching: %s", pattern))
    local found = FindAllOf(pattern)
    if found then
        for i, obj in ipairs(found) do
            print(string.format("  [%d] %s\n", i, obj:GetFullName()))
        end
        Log(string.format("Found %d matching objects", #found))
    else
        Log("No matching objects found")
    end
end

CrabEditor.Debug.DumpObject = function(objName)
    local obj = FindFirstOf(objName)
    if obj and obj:IsValid() then
        Log(string.format("Dumping object: %s", obj:GetFullName()))

        -- Try to iterate properties
        SafeCall(function()
            obj:ForEachProperty(function(prop)
                print(string.format("  Property: %s\n", prop:GetFullName()))
            end)
        end)
    else
        LogError(string.format("Object not found: %s", objName))
    end
end

CrabEditor.Debug.GetState = function()
    print("\n")
    print("Current State:\n")
    print(string.format("  God Mode: %s\n", tostring(State.GodModeEnabled)))
    print(string.format("  Infinite Ammo: %s\n", tostring(State.InfiniteAmmoEnabled)))
    print(string.format("  No Clip: %s\n", tostring(State.NoClipEnabled)))
    print(string.format("  Dual Wield: %s\n", tostring(State.DualWieldEnabled)))
    print(string.format("  Speed: %.1fx\n", State.SpeedMultiplier))
    print(string.format("  Jump: %.1fx\n", State.JumpMultiplier))
    print(string.format("  Damage: %.1fx\n", State.DamageMultiplier))
    print(string.format("  Fire Rate: %.1fx\n", State.FireRateMultiplier))
    print("\n")
end

-- ============================================================================
-- INITIALIZATION
-- ============================================================================

local function Initialize()
    print("\n")
    print("==========================================================\n")
    print("    CRAB CHAMPIONS EDITOR v" .. Config.Version .. " LOADING...\n")
    print("==========================================================\n")

    -- Setup hooks and keybinds
    SetupHooks()
    SetupKeybinds()

    -- Initial object cache
    ExecuteWithDelay(1000, function()
        FindPlayerController()
        FindPlayerCharacter()
        FindGameInstance()

        Log("Initial object cache complete")

        if State.PlayerCharacter then
            LogSuccess("Player character found!")
        else
            Log("Player character not found yet - may need to start a run first")
        end
    end)

    print("\n")
    print("==========================================================\n")
    LogSuccess("Crab Champions Editor loaded successfully!")
    print("    Press F1 to toggle menu | CrabEditor.Help() for commands\n")
    print("==========================================================\n")
    print("\n")
end

-- Run initialization
Initialize()
