--[[
    ImGui Menu for Crab Champions Editor
    Press F1 to toggle menu visibility
]]

local Menu = {}

-- Menu state
local CurrentTab = "Player"
local Tabs = {"Player", "Movement", "Currency", "Items", "Weapons", "Prismatics", "Settings"}

-- Slider values
local SpeedSlider = 1.0
local JumpSlider = 1.0
local GravitySlider = 1.0
local KeysInput = 0
local CrystalsInput = 0

-----------------------------------------------------------
-- Main Render Function
-----------------------------------------------------------

function Menu.Render(PlayerCharacter, PlayerState, Config, Features, Database)
    -- Begin main window
    if not ImGui.Begin("Crab Champions Editor v1.0", true, ImGuiWindowFlags.MenuBar) then
        ImGui.End()
        return
    end

    -- Menu bar
    if ImGui.BeginMenuBar() then
        if ImGui.BeginMenu("File") then
            if ImGui.MenuItem("Reset All") then
                Features.ResetAll(PlayerCharacter, Config.Settings)
            end
            if ImGui.MenuItem("Close Menu") then
                -- Menu will be closed by hotkey handler
            end
            ImGui.EndMenu()
        end

        if ImGui.BeginMenu("Help") then
            if ImGui.MenuItem("Hotkeys") then
                print("[CrabEditor] Hotkeys:")
                print("  F1 - Toggle Menu")
                print("  F2 - God Mode")
                print("  F3 - Infinite Health")
                print("  F4 - Max Currency")
                print("  F5 - Give All Prismatics")
            end
            ImGui.EndMenu()
        end
        ImGui.EndMenuBar()
    end

    -- Status line
    local status = PlayerCharacter and PlayerCharacter:IsValid() and "CONNECTED" or "NOT IN GAME"
    local statusColor = PlayerCharacter and PlayerCharacter:IsValid()
        and ImVec4(0, 1, 0, 1) or ImVec4(1, 0, 0, 1)

    ImGui.TextColored(statusColor, "Status: " .. status)
    ImGui.Separator()

    -- Tab bar
    if ImGui.BeginTabBar("MainTabs") then
        for _, tabName in ipairs(Tabs) do
            if ImGui.BeginTabItem(tabName) then
                CurrentTab = tabName
                Menu.RenderTab(tabName, PlayerCharacter, PlayerState, Config, Features, Database)
                ImGui.EndTabItem()
            end
        end
        ImGui.EndTabBar()
    end

    ImGui.End()
end

-----------------------------------------------------------
-- Tab Renderers
-----------------------------------------------------------

function Menu.RenderTab(tabName, PlayerCharacter, PlayerState, Config, Features, Database)
    if tabName == "Player" then
        Menu.RenderPlayerTab(PlayerCharacter, Config, Features)
    elseif tabName == "Movement" then
        Menu.RenderMovementTab(PlayerCharacter, Config, Features)
    elseif tabName == "Currency" then
        Menu.RenderCurrencyTab(PlayerState, Config, Features)
    elseif tabName == "Items" then
        Menu.RenderItemsTab(Database, Features)
    elseif tabName == "Weapons" then
        Menu.RenderWeaponsTab(Database, Features)
    elseif tabName == "Prismatics" then
        Menu.RenderPrismaticsTab(Database, Features)
    elseif tabName == "Settings" then
        Menu.RenderSettingsTab(Config)
    end
end

function Menu.RenderPlayerTab(PlayerCharacter, Config, Features)
    ImGui.Text("Player Cheats")
    ImGui.Separator()
    ImGui.Spacing()

    -- God Mode
    local godChanged, godValue = ImGui.Checkbox("God Mode (F2)", Config.Settings.GodMode)
    if godChanged then
        Config.Settings.GodMode = godValue
        print("[CrabEditor] God Mode: " .. (godValue and "ON" or "OFF"))
    end
    ImGui.SameLine()
    ImGui.TextDisabled("(?) Prevents all damage")

    -- Infinite Health
    local healthChanged, healthValue = ImGui.Checkbox("Infinite Health (F3)", Config.Settings.InfiniteHealth)
    if healthChanged then
        Config.Settings.InfiniteHealth = healthValue
        print("[CrabEditor] Infinite Health: " .. (healthValue and "ON" or "OFF"))
    end

    -- Infinite Shield
    local shieldChanged, shieldValue = ImGui.Checkbox("Infinite Shield", Config.Settings.InfiniteShield)
    if shieldChanged then
        Config.Settings.InfiniteShield = shieldValue
        print("[CrabEditor] Infinite Shield: " .. (shieldValue and "ON" or "OFF"))
    end

    ImGui.Spacing()
    ImGui.Separator()
    ImGui.Text("Current Stats:")

    -- Display current health/shield if available
    if PlayerCharacter and PlayerCharacter:IsValid() then
        local health = 0
        local maxHealth = 100
        local shield = 0

        pcall(function()
            if PlayerCharacter.Health then health = PlayerCharacter.Health end
            if PlayerCharacter.MaxHealth then maxHealth = PlayerCharacter.MaxHealth end
            if PlayerCharacter.Shield then shield = PlayerCharacter.Shield end
        end)

        ImGui.ProgressBar(health / maxHealth, ImVec2(200, 20), string.format("Health: %.0f/%.0f", health, maxHealth))
        ImGui.ProgressBar(shield / 100, ImVec2(200, 20), string.format("Shield: %.0f", shield))
    else
        ImGui.TextDisabled("Enter a game to see stats")
    end
end

function Menu.RenderMovementTab(PlayerCharacter, Config, Features)
    ImGui.Text("Movement Modifiers")
    ImGui.Separator()
    ImGui.Spacing()

    -- NoClip
    local noclipChanged, noclipValue = ImGui.Checkbox("NoClip / Fly Mode (F9)", Config.Settings.NoClip)
    if noclipChanged then
        Config.Settings.NoClip = noclipValue
        Features.ApplyNoClip(PlayerCharacter, noclipValue)
        print("[CrabEditor] NoClip: " .. (noclipValue and "ON" or "OFF"))
    end

    ImGui.Spacing()
    ImGui.Separator()
    ImGui.Spacing()

    -- Speed multiplier
    ImGui.Text("Speed Multiplier:")
    local speedChanged, speedValue = ImGui.SliderFloat("##Speed", Config.Settings.SpeedMultiplier, 0.1, 10.0, "%.1fx")
    if speedChanged then
        Config.Settings.SpeedMultiplier = speedValue
    end
    ImGui.SameLine()
    if ImGui.Button("Reset##Speed") then
        Config.Settings.SpeedMultiplier = 1.0
    end

    -- Jump multiplier
    ImGui.Text("Jump Multiplier:")
    local jumpChanged, jumpValue = ImGui.SliderFloat("##Jump", Config.Settings.JumpMultiplier, 0.1, 10.0, "%.1fx")
    if jumpChanged then
        Config.Settings.JumpMultiplier = jumpValue
    end
    ImGui.SameLine()
    if ImGui.Button("Reset##Jump") then
        Config.Settings.JumpMultiplier = 1.0
    end

    -- Gravity scale
    ImGui.Text("Gravity Scale:")
    local gravChanged, gravValue = ImGui.SliderFloat("##Gravity", Config.Settings.GravityScale, 0.0, 3.0, "%.1fx")
    if gravChanged then
        Config.Settings.GravityScale = gravValue
    end
    ImGui.SameLine()
    if ImGui.Button("Reset##Gravity") then
        Config.Settings.GravityScale = 1.0
    end

    ImGui.Spacing()
    if ImGui.Button("Reset All Movement") then
        Config.Settings.SpeedMultiplier = 1.0
        Config.Settings.JumpMultiplier = 1.0
        Config.Settings.GravityScale = 1.0
        Config.Settings.NoClip = false
        Features.ApplyNoClip(PlayerCharacter, false)
    end
end

function Menu.RenderCurrencyTab(PlayerState, Config, Features)
    ImGui.Text("Currency Editor")
    ImGui.Separator()
    ImGui.Spacing()

    -- Get current values
    local currentKeys = Features.GetKeys(PlayerState)
    local currentCrystals = Features.GetCrystals(PlayerState)

    -- Display current
    ImGui.Text(string.format("Current Keys: %d", currentKeys))
    ImGui.Text(string.format("Current Crystals: %d", currentCrystals))

    ImGui.Spacing()
    ImGui.Separator()
    ImGui.Spacing()

    -- Infinite toggles
    local keysChanged, keysValue = ImGui.Checkbox("Infinite Keys", Config.Settings.InfiniteKeys)
    if keysChanged then
        Config.Settings.InfiniteKeys = keysValue
    end

    local crystalsChanged, crystalsValue = ImGui.Checkbox("Infinite Crystals", Config.Settings.InfiniteCrystals)
    if crystalsChanged then
        Config.Settings.InfiniteCrystals = crystalsValue
    end

    ImGui.Spacing()
    ImGui.Separator()
    ImGui.Spacing()

    -- Quick add buttons
    ImGui.Text("Quick Add:")

    if ImGui.Button("+100 Keys") then
        Features.SetKeys(PlayerState, currentKeys + 100)
    end
    ImGui.SameLine()
    if ImGui.Button("+1000 Keys") then
        Features.SetKeys(PlayerState, currentKeys + 1000)
    end
    ImGui.SameLine()
    if ImGui.Button("+10000 Keys") then
        Features.SetKeys(PlayerState, currentKeys + 10000)
    end

    if ImGui.Button("+100 Crystals") then
        Features.SetCrystals(PlayerState, currentCrystals + 100)
    end
    ImGui.SameLine()
    if ImGui.Button("+1000 Crystals") then
        Features.SetCrystals(PlayerState, currentCrystals + 1000)
    end
    ImGui.SameLine()
    if ImGui.Button("+10000 Crystals") then
        Features.SetCrystals(PlayerState, currentCrystals + 10000)
    end

    ImGui.Spacing()

    if ImGui.Button("MAX ALL CURRENCY (F4)", ImVec2(250, 30)) then
        Features.MaxCurrency()
    end
end

function Menu.RenderItemsTab(Database, Features)
    ImGui.Text("Items")
    ImGui.Separator()
    ImGui.Spacing()

    if ImGui.Button("Give ALL Items (F6)", ImVec2(200, 30)) then
        Features.GiveAllItems()
    end

    ImGui.Spacing()
    ImGui.Separator()
    ImGui.Text("Individual Items:")
    ImGui.Spacing()

    -- List items in scrollable region
    if ImGui.BeginChild("ItemsList", ImVec2(0, 300), true) then
        for _, item in ipairs(Database.Items) do
            if ImGui.Button(item.Name .. "##item", ImVec2(180, 0)) then
                Features.SpawnItem(item.Class or item.Name)
            end
            if item.Description then
                ImGui.SameLine()
                ImGui.TextDisabled(item.Description)
            end
        end
        ImGui.EndChild()
    end
end

function Menu.RenderWeaponsTab(Database, Features)
    ImGui.Text("Weapons")
    ImGui.Separator()
    ImGui.Spacing()

    if ImGui.Button("Give ALL Weapons (F7)", ImVec2(200, 30)) then
        Features.GiveAllWeapons()
    end

    ImGui.Spacing()
    ImGui.Separator()
    ImGui.Text("Individual Weapons:")
    ImGui.Spacing()

    -- Group by category
    local categories = {}
    for _, weapon in ipairs(Database.Weapons) do
        local cat = weapon.Category or "Other"
        if not categories[cat] then
            categories[cat] = {}
        end
        table.insert(categories[cat], weapon)
    end

    if ImGui.BeginChild("WeaponsList", ImVec2(0, 300), true) then
        for catName, weapons in pairs(categories) do
            if ImGui.CollapsingHeader(catName) then
                for _, weapon in ipairs(weapons) do
                    if ImGui.Button(weapon.Name .. "##weapon", ImVec2(180, 0)) then
                        Features.SpawnItem(weapon.Class or weapon.Name)
                    end
                    if weapon.Rarity then
                        ImGui.SameLine()
                        local rarityColor = Menu.GetRarityColor(weapon.Rarity)
                        ImGui.TextColored(rarityColor, "[" .. weapon.Rarity .. "]")
                    end
                end
            end
        end
        ImGui.EndChild()
    end
end

function Menu.RenderPrismaticsTab(Database, Features)
    ImGui.Text("Prismatics")
    ImGui.Separator()
    ImGui.Spacing()

    -- Big button for all prismatics
    ImGui.PushStyleColor(ImGuiCol.Button, ImVec4(0.8, 0.2, 0.8, 1))
    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImVec4(0.9, 0.3, 0.9, 1))
    if ImGui.Button("GIVE ALL PRISMATICS (F5)", ImVec2(280, 40)) then
        Features.GiveAllPrismatics()
    end
    ImGui.PopStyleColor(2)

    ImGui.Spacing()
    ImGui.Separator()
    ImGui.Text("Individual Prismatics:")
    ImGui.Spacing()

    if ImGui.BeginChild("PrismaticsList", ImVec2(0, 300), true) then
        for _, prismatic in ipairs(Database.Prismatics) do
            -- Color based on tier
            local color = Menu.GetPrismaticColor(prismatic.Tier)
            ImGui.PushStyleColor(ImGuiCol.Button, color)

            if ImGui.Button(prismatic.Name .. "##prismatic", ImVec2(200, 0)) then
                Features.SpawnItem(prismatic.Class or prismatic.Name)
            end

            ImGui.PopStyleColor()

            if prismatic.Effect then
                ImGui.SameLine()
                ImGui.TextDisabled(prismatic.Effect)
            end
        end
        ImGui.EndChild()
    end
end

function Menu.RenderSettingsTab(Config)
    ImGui.Text("Settings")
    ImGui.Separator()
    ImGui.Spacing()

    ImGui.Text("Hotkeys:")
    ImGui.BulletText("F1 - Toggle Menu")
    ImGui.BulletText("F2 - God Mode")
    ImGui.BulletText("F3 - Infinite Health")
    ImGui.BulletText("F4 - Max Currency")
    ImGui.BulletText("F5 - Give All Prismatics")
    ImGui.BulletText("F6 - Give All Items")
    ImGui.BulletText("F7 - Give All Weapons")
    ImGui.BulletText("F9 - NoClip")

    ImGui.Spacing()
    ImGui.Separator()
    ImGui.Spacing()

    ImGui.Text("About:")
    ImGui.TextWrapped("Crab Champions Editor v1.0")
    ImGui.TextWrapped("A UE4SS Lua mod for Crab Champions")
    ImGui.Spacing()
    ImGui.TextDisabled("Use responsibly in single-player!")
end

-----------------------------------------------------------
-- Helper Functions
-----------------------------------------------------------

function Menu.GetRarityColor(rarity)
    local colors = {
        Common = ImVec4(0.7, 0.7, 0.7, 1),
        Uncommon = ImVec4(0.2, 0.8, 0.2, 1),
        Rare = ImVec4(0.2, 0.4, 1, 1),
        Epic = ImVec4(0.8, 0.2, 0.8, 1),
        Legendary = ImVec4(1, 0.8, 0, 1),
    }
    return colors[rarity] or ImVec4(1, 1, 1, 1)
end

function Menu.GetPrismaticColor(tier)
    local colors = {
        [1] = ImVec4(0.3, 0.8, 0.3, 0.7),
        [2] = ImVec4(0.3, 0.5, 1, 0.7),
        [3] = ImVec4(0.8, 0.3, 0.8, 0.7),
    }
    return colors[tier] or ImVec4(0.5, 0.5, 0.5, 0.7)
end

return Menu
