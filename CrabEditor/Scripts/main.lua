--[[
    Crab Champions Editor
    A UE4SS Lua mod for Crab Champions

    Features:
    - God Mode, Infinite Health
    - Speed/Jump modifiers
    - Currency editing (Keys, Crystals)
    - Give all Prismatics, Items, Weapons
    - In-game ImGui menu (F1 to toggle)
]]

local ModName = "CrabEditor"
local Version = "1.0.0"

-- Module loading
local Config = require("Config")
local Features = require("Features")
local Menu = require("Menu")
local Database = require("Database")
local Utils = require("Utils")

-- State
local Initialized = false
local PlayerController = nil
local PlayerCharacter = nil
local PlayerState = nil
local GameInstance = nil

-- Hotkey states
local MenuVisible = false

print(string.format("[%s] Loading v%s...", ModName, Version))

-----------------------------------------------------------
-- Core Functions
-----------------------------------------------------------

local function FindPlayer()
    local success, err = pcall(function()
        -- Get the game instance
        GameInstance = UEHelpers:GetGameInstance()
        if not GameInstance:IsValid() then return end

        -- Get local player controller
        local Players = GameInstance:GetLocalPlayers()
        if #Players == 0 then return end

        local LocalPlayer = Players[1]
        if not LocalPlayer:IsValid() then return end

        PlayerController = LocalPlayer:GetPlayerController()
        if not PlayerController:IsValid() then return end

        -- Get the pawn/character
        PlayerCharacter = PlayerController:GetPawn()
        if not PlayerCharacter:IsValid() then
            PlayerCharacter = nil
            return
        end

        -- Try to get player state
        PlayerState = PlayerController:GetPlayerState()
    end)

    if not success then
        -- Silently fail - player may not be in game yet
    end
end

local function Initialize()
    if Initialized then return end

    -- Register keybinds
    RegisterKeyBind(Config.Hotkeys.ToggleMenu, function()
        MenuVisible = not MenuVisible
        print(string.format("[%s] Menu %s", ModName, MenuVisible and "OPENED" or "CLOSED"))
    end)

    RegisterKeyBind(Config.Hotkeys.GodMode, function()
        Config.Settings.GodMode = not Config.Settings.GodMode
        print(string.format("[%s] God Mode: %s", ModName, Config.Settings.GodMode and "ON" or "OFF"))
    end)

    RegisterKeyBind(Config.Hotkeys.InfiniteHealth, function()
        Config.Settings.InfiniteHealth = not Config.Settings.InfiniteHealth
        print(string.format("[%s] Infinite Health: %s", ModName, Config.Settings.InfiniteHealth and "ON" or "OFF"))
    end)

    RegisterKeyBind(Config.Hotkeys.MaxCurrency, function()
        Features.MaxCurrency()
        print(string.format("[%s] Currency MAXED!", ModName))
    end)

    RegisterKeyBind(Config.Hotkeys.GiveAllPrismatics, function()
        Features.GiveAllPrismatics()
        print(string.format("[%s] All Prismatics given!", ModName))
    end)

    Initialized = true
    print(string.format("[%s] Initialized! Press %s to open menu.", ModName, Config.Hotkeys.ToggleMenu))
end

-----------------------------------------------------------
-- Update Loop
-----------------------------------------------------------

local TickCount = 0

RegisterHook("/Script/Engine.PlayerController:ClientRestart", function(Context)
    -- Player respawned, refresh references
    PlayerCharacter = nil
    PlayerState = nil
    FindPlayer()
end)

-- Main tick loop
LoopAsync(Config.UpdateInterval, function()
    TickCount = TickCount + 1

    -- Initialize on first tick
    if not Initialized then
        Initialize()
    end

    -- Refresh player references periodically
    if TickCount % 60 == 0 or PlayerCharacter == nil then
        FindPlayer()
    end

    -- Apply active features
    if PlayerCharacter and PlayerCharacter:IsValid() then
        Features.ApplyFeatures(PlayerCharacter, PlayerState, Config.Settings)
    end

    return false -- Keep looping
end)

-----------------------------------------------------------
-- ImGui Menu Rendering
-----------------------------------------------------------

RegisterHook("/Script/Engine.GameViewportClient:Tick", function()
    if not MenuVisible then return end

    -- Find player if needed
    if not PlayerCharacter or not PlayerCharacter:IsValid() then
        FindPlayer()
    end

    Menu.Render(PlayerCharacter, PlayerState, Config, Features, Database)
end)

print(string.format("[%s] Loaded successfully!", ModName))
