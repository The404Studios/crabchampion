--[[
    Feature implementations for Crab Champions Editor
]]

local Features = {}
local Database = require("Database")
local Utils = require("Utils")

-----------------------------------------------------------
-- Player Features
-----------------------------------------------------------

function Features.ApplyFeatures(PlayerCharacter, PlayerState, Settings)
    if not PlayerCharacter or not PlayerCharacter:IsValid() then return end

    local success, err = pcall(function()
        -- God Mode
        if Settings.GodMode then
            Features.ApplyGodMode(PlayerCharacter, true)
        end

        -- Infinite Health
        if Settings.InfiniteHealth then
            Features.ApplyInfiniteHealth(PlayerCharacter)
        end

        -- Infinite Shield
        if Settings.InfiniteShield then
            Features.ApplyInfiniteShield(PlayerCharacter)
        end

        -- Movement modifiers
        if Settings.SpeedMultiplier ~= 1.0 or Settings.JumpMultiplier ~= 1.0 then
            Features.ApplyMovementMods(PlayerCharacter, Settings)
        end

        -- NoClip
        if Settings.NoClip then
            Features.ApplyNoClip(PlayerCharacter, true)
        end

        -- Currency
        if PlayerState and PlayerState:IsValid() then
            if Settings.InfiniteKeys then
                Features.SetKeys(PlayerState, 999999)
            end
            if Settings.InfiniteCrystals then
                Features.SetCrystals(PlayerState, 999999)
            end
        end
    end)

    if not success then
        -- Feature application failed, likely property not found
    end
end

function Features.ApplyGodMode(PlayerCharacter, enabled)
    local success, _ = pcall(function()
        -- Try to find and set the CanBeDamaged property
        if PlayerCharacter.bCanBeDamaged then
            PlayerCharacter.bCanBeDamaged = not enabled
        end

        -- Alternative: Set damage multiplier to 0
        if PlayerCharacter.DamageMultiplier then
            PlayerCharacter.DamageMultiplier = enabled and 0.0 or 1.0
        end
    end)
end

function Features.ApplyInfiniteHealth(PlayerCharacter)
    local success, _ = pcall(function()
        -- Find health properties
        if PlayerCharacter.Health and PlayerCharacter.MaxHealth then
            local maxHealth = PlayerCharacter.MaxHealth
            if maxHealth > 0 then
                PlayerCharacter.Health = maxHealth
            end
        end

        -- Alternative property names
        if PlayerCharacter.CurrentHealth and PlayerCharacter.MaximumHealth then
            PlayerCharacter.CurrentHealth = PlayerCharacter.MaximumHealth
        end
    end)
end

function Features.ApplyInfiniteShield(PlayerCharacter)
    local success, _ = pcall(function()
        if PlayerCharacter.Shield and PlayerCharacter.MaxShield then
            PlayerCharacter.Shield = PlayerCharacter.MaxShield
        end

        if PlayerCharacter.CurrentShield and PlayerCharacter.MaximumShield then
            PlayerCharacter.CurrentShield = PlayerCharacter.MaximumShield
        end
    end)
end

function Features.ApplyMovementMods(PlayerCharacter, Settings)
    local success, _ = pcall(function()
        local MovementComp = PlayerCharacter.CharacterMovement
        if not MovementComp or not MovementComp:IsValid() then return end

        local Config = require("Config")

        -- Speed
        if Settings.SpeedMultiplier ~= 1.0 then
            MovementComp.MaxWalkSpeed = Config.Defaults.WalkSpeed * Settings.SpeedMultiplier
            if MovementComp.MaxSprintSpeed then
                MovementComp.MaxSprintSpeed = Config.Defaults.SprintSpeed * Settings.SpeedMultiplier
            end
        end

        -- Jump
        if Settings.JumpMultiplier ~= 1.0 then
            MovementComp.JumpZVelocity = Config.Defaults.JumpZVelocity * Settings.JumpMultiplier
        end

        -- Gravity
        if Settings.GravityScale ~= 1.0 then
            MovementComp.GravityScale = Settings.GravityScale
        end
    end)
end

function Features.ApplyNoClip(PlayerCharacter, enabled)
    local success, _ = pcall(function()
        local MovementComp = PlayerCharacter.CharacterMovement
        if not MovementComp or not MovementComp:IsValid() then return end

        if enabled then
            -- Set to flying mode
            MovementComp.MovementMode = 5 -- Flying
            if PlayerCharacter.SetActorEnableCollision then
                PlayerCharacter:SetActorEnableCollision(false)
            end
        else
            -- Reset to walking
            MovementComp.MovementMode = 1 -- Walking
            if PlayerCharacter.SetActorEnableCollision then
                PlayerCharacter:SetActorEnableCollision(true)
            end
        end
    end)
end

-----------------------------------------------------------
-- Currency Features
-----------------------------------------------------------

function Features.GetKeys(PlayerState)
    if not PlayerState or not PlayerState:IsValid() then return 0 end

    local keys = 0
    pcall(function()
        if PlayerState.Keys then
            keys = PlayerState.Keys
        elseif PlayerState.CurrentKeys then
            keys = PlayerState.CurrentKeys
        end
    end)
    return keys
end

function Features.SetKeys(PlayerState, amount)
    if not PlayerState or not PlayerState:IsValid() then return end

    pcall(function()
        if PlayerState.Keys then
            PlayerState.Keys = amount
        elseif PlayerState.CurrentKeys then
            PlayerState.CurrentKeys = amount
        end
    end)
end

function Features.GetCrystals(PlayerState)
    if not PlayerState or not PlayerState:IsValid() then return 0 end

    local crystals = 0
    pcall(function()
        if PlayerState.Crystals then
            crystals = PlayerState.Crystals
        elseif PlayerState.CurrentCrystals then
            crystals = PlayerState.CurrentCrystals
        end
    end)
    return crystals
end

function Features.SetCrystals(PlayerState, amount)
    if not PlayerState or not PlayerState:IsValid() then return end

    pcall(function()
        if PlayerState.Crystals then
            PlayerState.Crystals = amount
        elseif PlayerState.CurrentCrystals then
            PlayerState.CurrentCrystals = amount
        end
    end)
end

function Features.MaxCurrency()
    local GameInstance = UEHelpers:GetGameInstance()
    if not GameInstance or not GameInstance:IsValid() then return end

    local Players = GameInstance:GetLocalPlayers()
    if #Players == 0 then return end

    local LocalPlayer = Players[1]
    if not LocalPlayer:IsValid() then return end

    local PC = LocalPlayer:GetPlayerController()
    if not PC:IsValid() then return end

    local PS = PC:GetPlayerState()
    if PS and PS:IsValid() then
        Features.SetKeys(PS, 999999)
        Features.SetCrystals(PS, 999999)
    end
end

-----------------------------------------------------------
-- Item/Prismatic Features
-----------------------------------------------------------

function Features.GiveAllPrismatics()
    print("[CrabEditor] Giving all Prismatics...")

    local GameInstance = UEHelpers:GetGameInstance()
    if not GameInstance or not GameInstance:IsValid() then
        print("[CrabEditor] GameInstance not found")
        return
    end

    -- Get player character
    local Players = GameInstance:GetLocalPlayers()
    if #Players == 0 then return end

    local LocalPlayer = Players[1]
    local PC = LocalPlayer:GetPlayerController()
    local Pawn = PC:GetPawn()

    if not Pawn or not Pawn:IsValid() then
        print("[CrabEditor] Player pawn not found")
        return
    end

    -- Find the inventory/item manager component
    local success, err = pcall(function()
        -- Try to find inventory component
        local InventoryComp = nil

        -- Common component names
        local compNames = {"InventoryComponent", "ItemManager", "PrismaticManager", "UpgradeManager"}

        for _, name in ipairs(compNames) do
            local comp = Pawn:GetComponentByClass(FindClass(name))
            if comp and comp:IsValid() then
                InventoryComp = comp
                break
            end
        end

        if InventoryComp then
            -- Try to add prismatics
            for _, prismatic in ipairs(Database.Prismatics) do
                if InventoryComp.AddItem then
                    InventoryComp:AddItem(prismatic.Class)
                elseif InventoryComp.GiveItem then
                    InventoryComp:GiveItem(prismatic.Class)
                elseif InventoryComp.AddPrismatic then
                    InventoryComp:AddPrismatic(prismatic.Class)
                end
            end
        else
            -- Alternative: Execute console command if available
            if PC.ConsoleCommand then
                for _, prismatic in ipairs(Database.Prismatics) do
                    PC:ConsoleCommand("give " .. prismatic.Name, false)
                end
            end
        end
    end)

    if success then
        print("[CrabEditor] Prismatics given!")
    else
        print("[CrabEditor] Could not give prismatics: " .. tostring(err))
        print("[CrabEditor] Try using the spawn menu instead")
    end
end

function Features.GiveAllItems()
    print("[CrabEditor] Giving all Items...")

    local success, err = pcall(function()
        local GameInstance = UEHelpers:GetGameInstance()
        local Players = GameInstance:GetLocalPlayers()
        local PC = Players[1]:GetPlayerController()

        if PC.ConsoleCommand then
            for _, item in ipairs(Database.Items) do
                PC:ConsoleCommand("give " .. item.Name, false)
            end
        end
    end)

    if success then
        print("[CrabEditor] Items given!")
    else
        print("[CrabEditor] Could not give items: " .. tostring(err))
    end
end

function Features.GiveAllWeapons()
    print("[CrabEditor] Giving all Weapons...")

    local success, err = pcall(function()
        local GameInstance = UEHelpers:GetGameInstance()
        local Players = GameInstance:GetLocalPlayers()
        local PC = Players[1]:GetPlayerController()

        if PC.ConsoleCommand then
            for _, weapon in ipairs(Database.Weapons) do
                PC:ConsoleCommand("give " .. weapon.Name, false)
            end
        end
    end)

    if success then
        print("[CrabEditor] Weapons given!")
    else
        print("[CrabEditor] Could not give weapons: " .. tostring(err))
    end
end

function Features.SpawnItem(itemName)
    print("[CrabEditor] Spawning: " .. itemName)

    local success, err = pcall(function()
        local GameInstance = UEHelpers:GetGameInstance()
        local Players = GameInstance:GetLocalPlayers()
        local PC = Players[1]:GetPlayerController()
        local Pawn = PC:GetPawn()

        if not Pawn:IsValid() then return end

        -- Get player location for spawn
        local Location = Pawn:GetActorLocation()

        -- Try console command
        if PC.ConsoleCommand then
            PC:ConsoleCommand("summon " .. itemName, false)
        end
    end)
end

-----------------------------------------------------------
-- Reset
-----------------------------------------------------------

function Features.ResetAll(PlayerCharacter, Settings)
    local Config = require("Config")

    -- Reset settings
    Settings.GodMode = false
    Settings.InfiniteHealth = false
    Settings.InfiniteShield = false
    Settings.NoClip = false
    Settings.SpeedMultiplier = 1.0
    Settings.JumpMultiplier = 1.0
    Settings.GravityScale = 1.0
    Settings.InfiniteAmmo = false
    Settings.InfiniteKeys = false
    Settings.InfiniteCrystals = false

    -- Reset player properties
    if PlayerCharacter and PlayerCharacter:IsValid() then
        pcall(function()
            -- Reset god mode
            if PlayerCharacter.bCanBeDamaged then
                PlayerCharacter.bCanBeDamaged = true
            end

            -- Reset movement
            local MovementComp = PlayerCharacter.CharacterMovement
            if MovementComp and MovementComp:IsValid() then
                MovementComp.MaxWalkSpeed = Config.Defaults.WalkSpeed
                MovementComp.JumpZVelocity = Config.Defaults.JumpZVelocity
                MovementComp.GravityScale = Config.Defaults.GravityScale
                MovementComp.MovementMode = 1
            end

            -- Reset collision
            if PlayerCharacter.SetActorEnableCollision then
                PlayerCharacter:SetActorEnableCollision(true)
            end
        end)
    end

    print("[CrabEditor] All features reset!")
end

return Features
