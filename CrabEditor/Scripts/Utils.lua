--[[
    Utility functions for Crab Champions Editor
]]

local Utils = {}

-----------------------------------------------------------
-- Logging
-----------------------------------------------------------

function Utils.Log(message)
    print("[CrabEditor] " .. tostring(message))
end

function Utils.LogError(message)
    print("[CrabEditor ERROR] " .. tostring(message))
end

function Utils.LogWarning(message)
    print("[CrabEditor WARNING] " .. tostring(message))
end

-----------------------------------------------------------
-- Safe Execution
-----------------------------------------------------------

function Utils.SafeCall(func, ...)
    local success, result = pcall(func, ...)
    if not success then
        Utils.LogError("Function failed: " .. tostring(result))
        return nil
    end
    return result
end

-----------------------------------------------------------
-- UE4 Helpers
-----------------------------------------------------------

function Utils.GetPlayerController()
    local GameInstance = UEHelpers:GetGameInstance()
    if not GameInstance or not GameInstance:IsValid() then return nil end

    local Players = GameInstance:GetLocalPlayers()
    if #Players == 0 then return nil end

    local LocalPlayer = Players[1]
    if not LocalPlayer:IsValid() then return nil end

    return LocalPlayer:GetPlayerController()
end

function Utils.GetPlayerCharacter()
    local PC = Utils.GetPlayerController()
    if not PC or not PC:IsValid() then return nil end

    local Pawn = PC:GetPawn()
    if not Pawn or not Pawn:IsValid() then return nil end

    return Pawn
end

function Utils.GetPlayerState()
    local PC = Utils.GetPlayerController()
    if not PC or not PC:IsValid() then return nil end

    return PC:GetPlayerState()
end

function Utils.GetMovementComponent(Character)
    if not Character or not Character:IsValid() then return nil end

    local success, comp = pcall(function()
        return Character.CharacterMovement
    end)

    if success and comp and comp:IsValid() then
        return comp
    end
    return nil
end

-----------------------------------------------------------
-- Math Helpers
-----------------------------------------------------------

function Utils.Clamp(value, min, max)
    if value < min then return min end
    if value > max then return max end
    return value
end

function Utils.Lerp(a, b, t)
    return a + (b - a) * Utils.Clamp(t, 0, 1)
end

-----------------------------------------------------------
-- String Helpers
-----------------------------------------------------------

function Utils.Split(str, delimiter)
    local result = {}
    for match in (str .. delimiter):gmatch("(.-)" .. delimiter) do
        table.insert(result, match)
    end
    return result
end

function Utils.Trim(str)
    return str:match("^%s*(.-)%s*$")
end

-----------------------------------------------------------
-- Table Helpers
-----------------------------------------------------------

function Utils.TableContains(table, value)
    for _, v in pairs(table) do
        if v == value then return true end
    end
    return false
end

function Utils.TableLength(table)
    local count = 0
    for _ in pairs(table) do count = count + 1 end
    return count
end

function Utils.DeepCopy(orig)
    local copy
    if type(orig) == 'table' then
        copy = {}
        for k, v in next, orig, nil do
            copy[Utils.DeepCopy(k)] = Utils.DeepCopy(v)
        end
        setmetatable(copy, Utils.DeepCopy(getmetatable(orig)))
    else
        copy = orig
    end
    return copy
end

return Utils
