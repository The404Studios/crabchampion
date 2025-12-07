--[[
    Configuration for Crab Champions Editor
    Modify these values to customize the mod behavior
]]

-- ============================================================================
-- USER CONFIGURATION
-- ============================================================================

-- Enable/disable features on startup
STARTUP_CONFIG = {
    -- Auto-enable features when mod loads
    AutoGodMode = false,
    AutoInfiniteAmmo = false,
    AutoNoClip = false,
    AutoDualWield = false,

    -- Starting multipliers
    StartSpeedMultiplier = 1.0,
    StartJumpMultiplier = 1.0,
    StartDamageMultiplier = 1.0,
    StartFireRateMultiplier = 1.0,

    -- UI Settings
    ShowMenuOnLoad = false,
    DebugMode = true,

    -- Auto-add currency on load
    AutoAddKeys = 0,       -- Set to > 0 to auto-add keys on load
    AutoAddCrystals = 0,   -- Set to > 0 to auto-add crystals on load
}

-- ============================================================================
-- KEYBIND CONFIGURATION
-- ============================================================================

-- Modify these to change keybinds
-- Available keys: F1-F12, A-Z, 0-9, ADD, SUBTRACT, MULTIPLY, DIVIDE
-- See UE4SS documentation for full key list

KEYBIND_CONFIG = {
    ToggleMenu = "F1",
    ToggleGodMode = "F2",
    ToggleInfiniteAmmo = "F3",
    ToggleNoClip = "F4",
    AddKeys = "F5",
    AddCrystals = "F6",
    SpawnRandomItem = "F7",
    SpawnPrismatic = "F8",
    ToggleDualWield = "F9",
    ResetAll = "F10",
    IncreaseSpeed = "ADD",      -- Numpad +
    DecreaseSpeed = "SUBTRACT", -- Numpad -
}

-- ============================================================================
-- GAMEPLAY CONFIGURATION
-- ============================================================================

GAMEPLAY_CONFIG = {
    -- Currency amounts per keypress
    KeysPerPress = 100,
    CrystalsPerPress = 100,

    -- Speed limits
    MinSpeedMultiplier = 0.5,
    MaxSpeedMultiplier = 10.0,
    SpeedIncrement = 0.5,

    -- Jump limits
    MinJumpMultiplier = 0.5,
    MaxJumpMultiplier = 10.0,
    JumpIncrement = 0.5,

    -- Damage limits
    MinDamageMultiplier = 0.1,
    MaxDamageMultiplier = 100.0,

    -- Fire rate limits
    MinFireRateMultiplier = 0.1,
    MaxFireRateMultiplier = 10.0,

    -- Debounce time for keys (ms)
    KeyDebounceTime = 200,
}

-- ============================================================================
-- ADVANCED CONFIGURATION
-- ============================================================================

ADVANCED_CONFIG = {
    -- Hook configuration
    EnableDamageHook = true,
    EnableAmmoHook = true,
    EnableFireRateHook = true,

    -- Retry settings for object finding
    MaxRetries = 5,
    RetryDelay = 500,  -- ms

    -- Logging
    LogToConsole = true,
    LogToFile = false,
    LogFileName = "CrabEditor.log",

    -- Experimental features
    EnableExperimental = false,
}

-- ============================================================================
-- APPLY CONFIGURATION
-- ============================================================================

-- This function is called by main.lua to apply config
function ApplyStartupConfig()
    if not CrabEditor then
        print("[Config] Warning: CrabEditor not loaded yet\n")
        return
    end

    -- Apply auto-enable features
    if STARTUP_CONFIG.AutoGodMode then
        CrabEditor.GodMode(true)
    end

    if STARTUP_CONFIG.AutoInfiniteAmmo then
        CrabEditor.InfiniteAmmo(true)
    end

    if STARTUP_CONFIG.AutoNoClip then
        CrabEditor.NoClip(true)
    end

    if STARTUP_CONFIG.AutoDualWield then
        CrabEditor.DualWield(true)
    end

    -- Apply starting multipliers
    if STARTUP_CONFIG.StartSpeedMultiplier ~= 1.0 then
        CrabEditor.SetSpeed(STARTUP_CONFIG.StartSpeedMultiplier)
    end

    if STARTUP_CONFIG.StartJumpMultiplier ~= 1.0 then
        CrabEditor.SetJumpHeight(STARTUP_CONFIG.StartJumpMultiplier)
    end

    -- Auto-add currency
    if STARTUP_CONFIG.AutoAddKeys > 0 then
        CrabEditor.AddKeys(STARTUP_CONFIG.AutoAddKeys)
    end

    if STARTUP_CONFIG.AutoAddCrystals > 0 then
        CrabEditor.AddCrystals(STARTUP_CONFIG.AutoAddCrystals)
    end

    -- Show menu if configured
    if STARTUP_CONFIG.ShowMenuOnLoad then
        CrabEditor.Menu()
    end

    print("[Config] Startup configuration applied\n")
end

print("[Config] Configuration loaded. Edit config.lua to customize.\n")
