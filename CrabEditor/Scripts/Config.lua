--[[
    Configuration for Crab Champions Editor
]]

local Config = {}

-- Update interval in milliseconds
Config.UpdateInterval = 50

-- Hotkeys (UE4SS key names)
Config.Hotkeys = {
    ToggleMenu = Key.F1,
    GodMode = Key.F2,
    InfiniteHealth = Key.F3,
    MaxCurrency = Key.F4,
    GiveAllPrismatics = Key.F5,
    GiveAllItems = Key.F6,
    GiveAllWeapons = Key.F7,
    SpeedBoost = Key.F8,
    NoClip = Key.F9,
}

-- Feature settings (modified via menu or hotkeys)
Config.Settings = {
    -- Player
    GodMode = false,
    InfiniteHealth = false,
    InfiniteShield = false,
    NoClip = false,

    -- Movement
    SpeedMultiplier = 1.0,
    JumpMultiplier = 1.0,
    GravityScale = 1.0,

    -- Combat
    InfiniteAmmo = false,
    NoRecoil = false,
    RapidFire = false,
    DamageMultiplier = 1.0,

    -- Resources
    InfiniteKeys = false,
    InfiniteCrystals = false,
}

-- Default values for reset
Config.Defaults = {
    WalkSpeed = 600.0,
    SprintSpeed = 900.0,
    JumpZVelocity = 600.0,
    GravityScale = 1.0,
    MaxHealth = 100.0,
    MaxShield = 100.0,
}

return Config
