#include <pch.h>
#include <Engine.h>
#include <Menu.h>
#include "SDK.h"
#include "helper.h"
#include "SDKHelper.h"

using namespace SDK; // <-- makes UWorld, UPlayer, ACrabPS, etc. visible
using namespace SDKHelper;


namespace DX11Base
{
    // Global variable for player scale slider
    float g_overridePlayerScale = 1.0f;

    namespace Styles
    {
        void BaseStyle()
        {
            ImGuiStyle& style = ImGui::GetStyle();
            ImVec4* colors = style.Colors;

            // Start from dark base
            ImGui::StyleColorsDark();

            style.WindowRounding = 6.0f;
            style.ChildRounding = 6.0f;
            style.FrameRounding = 5.0f;
            style.ScrollbarRounding = 6.0f;

            style.WindowBorderSize = 1.0f;
            style.FrameBorderSize = 1.0f;

            style.WindowTitleAlign = ImVec2(0.5f, 0.5f);

            // Base backgrounds and borders
            colors[ImGuiCol_WindowBg] = ImVec4(0.10f, 0.10f, 0.10f, 1.00f);
            colors[ImGuiCol_ChildBg] = ImVec4(0.12f, 0.12f, 0.12f, 1.00f);
            colors[ImGuiCol_Border] = ImVec4(0.35f, 0.35f, 0.35f, 0.80f);
            colors[ImGuiCol_FrameBg] = ImVec4(0.20f, 0.20f, 0.20f, 1.00f);
            colors[ImGuiCol_FrameBgHovered] = ImVec4(0.25f, 0.25f, 0.25f, 1.00f);
            colors[ImGuiCol_FrameBgActive] = ImVec4(0.35f, 0.35f, 0.35f, 1.00f);

            // Buttons
            colors[ImGuiCol_Button] = ImVec4(0.20f, 0.20f, 0.20f, 1.00f);
            colors[ImGuiCol_ButtonHovered] = ImVec4(0.85f, 0.10f, 0.10f, 1.00f);
            colors[ImGuiCol_ButtonActive] = ImVec4(0.90f, 0.05f, 0.05f, 1.00f);

            // Headers / Collapsing / Tabs area accents
            colors[ImGuiCol_Header] = ImVec4(0.35f, 0.05f, 0.05f, 0.8f);
            colors[ImGuiCol_HeaderHovered] = ImVec4(0.85f, 0.10f, 0.10f, 0.8f);
            colors[ImGuiCol_HeaderActive] = ImVec4(0.90f, 0.05f, 0.05f, 1.00f);

            // Tabs
            colors[ImGuiCol_Tab] = ImVec4(0.30f, 0.05f, 0.05f, 0.8f);
            colors[ImGuiCol_TabHovered] = ImVec4(0.85f, 0.10f, 0.10f, 1.00f);
            colors[ImGuiCol_TabActive] = ImVec4(0.90f, 0.05f, 0.05f, 1.00f);
            colors[ImGuiCol_TabUnfocused] = ImVec4(0.25f, 0.05f, 0.05f, 0.6f);
            colors[ImGuiCol_TabUnfocusedActive] = ImVec4(0.40f, 0.08f, 0.08f, 1.00f);

            // Title / Menu bar accent (if ImGui uses “TitleBg” or “TitleBgActive”)
            colors[ImGuiCol_TitleBg] = ImVec4(0.15f, 0.05f, 0.05f, 1.00f);
            colors[ImGuiCol_TitleBgActive] = ImVec4(0.25f, 0.08f, 0.08f, 1.00f);
            colors[ImGuiCol_TitleBgCollapsed] = ImVec4(0.12f, 0.04f, 0.04f, 0.80f);

            // Misc accents (sliders, checkmarks, etc.)
            colors[ImGuiCol_CheckMark] = ImVec4(0.90f, 0.05f, 0.05f, 1.00f);
            colors[ImGuiCol_SliderGrab] = ImVec4(0.85f, 0.10f, 0.10f, 1.00f);
            colors[ImGuiCol_SliderGrabActive] = ImVec4(0.90f, 0.05f, 0.05f, 1.00f);
            colors[ImGuiCol_Separator] = ImVec4(0.35f, 0.10f, 0.10f, 0.60f);
            colors[ImGuiCol_SeparatorHovered] = ImVec4(0.85f, 0.10f, 0.10f, 1.00f);
            colors[ImGuiCol_SeparatorActive] = ImVec4(0.90f, 0.05f, 0.05f, 1.00f);
        }
    }

    // ------------------------------
    // Draw and Loops
    // ------------------------------

    void Menu::Draw()
    {
        if (g_Engine->bShowMenu)
            MainMenu();

        if (g_Engine->bShowESP)
            ESP();

            
    }

    void DX11Base::Menu::Loops()
    {
        if (!g_Engine)
            return;

        if (!g_Engine->pWorld)
        {
            uintptr_t moduleBase = (uintptr_t)GetModuleHandle(L"CrabChampions-Win64-Shipping.exe");
            if (!moduleBase)
                moduleBase = (uintptr_t)GetModuleHandle(nullptr);

            __try
            {
                g_Engine->pWorld = *reinterpret_cast<SDK::UWorld**>(moduleBase + ::Offsets::GWorld);
            }
            __except (EXCEPTION_EXECUTE_HANDLER)
            {
                g_Engine->pWorld = nullptr;
            }

            if (!g_Engine->pWorld)
                return;
        }

        SDK::UGameInstance* gi = nullptr;
        __try { gi = g_Engine->pWorld->OwningGameInstance; }
        __except (EXCEPTION_EXECUTE_HANDLER) { gi = nullptr; }

        if (!gi || gi->LocalPlayers.Num() == 0)
            return;

        SDK::ACrabC* playerPawn = nullptr;
        __try
        {
            SDK::APawn* pawn = gi->LocalPlayers[0]->PlayerController->AcknowledgedPawn;
            playerPawn = static_cast<SDK::ACrabC*>(pawn);
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
            playerPawn = nullptr;
        }

        if (!playerPawn || !playerPawn->HC)
            return;

        SDK::ACrabPlayerC* player = static_cast<SDK::ACrabPlayerC*>(playerPawn);

        // -----------------------------
        // GODMODE
        // -----------------------------
        if (g_Engine->bGodMode)
        {
            __try
            {
                playerPawn->HC->HealthInfo.CurrentHealth = playerPawn->HC->HealthInfo.CurrentMaxHealth;
                playerPawn->HC->HealthInfo.PreviousHealth = playerPawn->HC->HealthInfo.CurrentMaxHealth;
            }
            __except (EXCEPTION_EXECUTE_HANDLER) {}
        }

        // -----------------------------
        // Infinite Ammo / No Reload
        // -----------------------------
        if (g_Engine->bInfiniteAmmo || g_Engine->bNoReload)
        {
            __try
            {
                TArray<SDK::ACrabWeapon*> weapons = *(TArray<SDK::ACrabWeapon*>*)((uintptr_t)playerPawn + 0x538);
                int32_t count = weapons.Num();

                for (int i = 0; i < count; i++)
                {
                    SDK::ACrabWeapon* weap = nullptr;
                    __try { weap = weapons[i]; }
                    __except (EXCEPTION_EXECUTE_HANDLER) { weap = nullptr; }
                    if (!weap) continue;

                    SDK::UCrabWeaponDA* weaponDA = nullptr;
                    __try { weaponDA = weap->WeaponDA; }
                    __except (EXCEPTION_EXECUTE_HANDLER) { weaponDA = nullptr; }
                    if (!weaponDA) continue;

                    __try
                    {
                        if (g_Engine->bInfiniteAmmo)
                            weaponDA->bInfiniteClipSize = true;

                        if (g_Engine->bNoReload)
                            weaponDA->ReloadDuration = 0.f;
                    }
                    __except (EXCEPTION_EXECUTE_HANDLER) {}
                }
            }
            __except (EXCEPTION_EXECUTE_HANDLER) {}
        }

        // -----------------------------
        // Optional: No Cooldowns
        // -----------------------------
        if (g_Engine->bNoCooldowns)
        {
            if (!playerPawn || !playerPawn->PlayerState)
                return;

            SDK::ACrabPS* ps = static_cast<SDK::ACrabPS*>(playerPawn->PlayerState);

            __try
            {
                if (ps->AbilityDA)
                    ps->AbilityDA->Cooldown = 0.f;
            }
            __except (EXCEPTION_EXECUTE_HANDLER) {}

            __try
            {
                if (ps->MeleeDA)
                    ps->MeleeDA->Cooldown = 0.f;
            }
            __except (EXCEPTION_EXECUTE_HANDLER) {}
        }

        // -----------------------------
        // Apply Player Scale Every Frame (Persistent)
        // -----------------------------
        SDK::USceneComponent* rootComp = nullptr;
        __try { rootComp = playerPawn->RootComponent; }
        __except (EXCEPTION_EXECUTE_HANDLER) { rootComp = nullptr; }

        if (rootComp)
        {
            __try
            {
                SDK::FVector* scale = reinterpret_cast<SDK::FVector*>(uintptr_t(rootComp) + 0x134);
                *scale = SDK::FVector{ g_overridePlayerScale, g_overridePlayerScale, g_overridePlayerScale };
            }
            __except (EXCEPTION_EXECUTE_HANDLER) {}
        }

        // -----------------------------
        // No Spread / Zero Recoil
        // -----------------------------
        if (g_Engine->g_bNoSpread)
        {
            TArray<SDK::ACrabWeapon*> weapons = *(TArray<SDK::ACrabWeapon*>*)((uintptr_t)playerPawn + 0x538);
            int32_t count = weapons.Num();

            for (int i = 0; i < count; i++)
            {
                SDK::ACrabWeapon* weap = nullptr;
                __try { weap = weapons[i]; }
                __except (EXCEPTION_EXECUTE_HANDLER) { weap = nullptr; }
                if (!weap) continue;

                SDK::UCrabWeaponDA* weaponDA = nullptr;
                __try { weaponDA = weap->WeaponDA; }
                __except (EXCEPTION_EXECUTE_HANDLER) { weaponDA = nullptr; }
                if (!weaponDA) continue;

                __try
                {
                    weaponDA->BaseSpread = 0.f;
                    weaponDA->FiringSpreadIncrement = 0.f;
                    weaponDA->MaxSpread = 0.f;
                    weaponDA->SpreadRecovery = 0.f;
                    weaponDA->AimingSpreadMultiplier = 0.f;
                    weaponDA->VerticalRecoil = 0.f;
                    weaponDA->HorizontalRecoil = 0.f;
                    weaponDA->RecoilInterpSpeed = 0.f;
                    weaponDA->RecoilRecoveryInterpSpeed = 0.f;
                }
                __except (EXCEPTION_EXECUTE_HANDLER) {}
            }
        }
    }

    // ------------------------------
    // Main menu
    // ------------------------------
    void DX11Base::Menu::MainMenu()
    {
        Styles::BaseStyle();
        ImGui::SetNextWindowSize(ImVec2(500, 400), ImGuiCond_FirstUseEver);

        if (!ImGui::Begin("Crab Champions 0.1", &g_Engine->bShowMenu,
            ImGuiWindowFlags_MenuBar | ImGuiWindowFlags_NoCollapse | ImGuiWindowFlags_AlwaysAutoResize))
        {
            ImGui::End();
            return;
        }

        if (!g_Engine || !g_Engine->pWorld)
        {
            ImGui::End();
            return;
        }

        SDK::ACrabC* playerPawn = nullptr;
        __try
        {
            SDK::APawn* pawn = g_Engine->pWorld->OwningGameInstance->LocalPlayers[0]->PlayerController->AcknowledgedPawn;
            playerPawn = static_cast<SDK::ACrabC*>(pawn);
        }
        __except (EXCEPTION_EXECUTE_HANDLER) { playerPawn = nullptr; }

        if (!playerPawn || !playerPawn->PlayerState)
        {
            ImGui::TextColored(ImVec4(1.f, 0.f, 0.f, 1.f), "PlayerState not valid");
            ImGui::End();
            return;
        }

        SDK::ACrabPS* ps = static_cast<SDK::ACrabPS*>(playerPawn->PlayerState);
        SDK::ACrabPlayerC* player = static_cast<SDK::ACrabPlayerC*>(playerPawn);

        if (ImGui::BeginTabBar("MainTabs"))
        {
            // ---------------- Main Tab ----------------
            if (ImGui::BeginTabItem("Main"))
            {
                ImGui::Checkbox("Enemy ESP", &g_Engine->bShowESP);
                ImGui::Checkbox("Godmode", &g_Engine->bGodMode);
                ImGui::Checkbox("Infinite Ammo", &g_Engine->bInfiniteAmmo);
                ImGui::Checkbox("No Reload", &g_Engine->bNoReload);
                ImGui::Checkbox("No Cooldowns", &g_Engine->bNoCooldowns);
                ImGui::Checkbox("No Spread", &g_Engine->g_bNoSpread);

                ImGui::EndTabItem();
            }

            // ---------------- Stats Tab ----------------
            if (ImGui::BeginTabItem("Stats"))
            {
                static int overrideRank = static_cast<int>(ps->AccountRank);
                const char* rankNames[] = { "None", "Bronze", "Silver", "Gold", "Sapphire", "Emerald", "Ruby", "Diamond" };
                if (ImGui::Combo("Override Rank", &overrideRank, rankNames, IM_ARRAYSIZE(rankNames)))
                    ps->AccountRank = static_cast<SDK::ECrabRank>(overrideRank);

                static int overrideLevel = ps->AccountLevel;
                if (ImGui::SliderInt("Override Level", &overrideLevel, 1, 100))
                    ps->AccountLevel = overrideLevel;

                static int overrideKeys = ps->Keys;
                if (ImGui::SliderInt("Override Keys", &overrideKeys, 0, 999))
                    ps->Keys = overrideKeys;

                static int overrideCrystals = ps->Crystals;
                if (ImGui::SliderInt("Override Crystals", &overrideCrystals, 0, 9999))
                    ps->Crystals = overrideCrystals;

                ImGui::EndTabItem();
            }

            // ---------------- Movement Tab ----------------
            if (ImGui::BeginTabItem("Movement"))
            {
                static float overrideWalkSpeed = playerPawn->BaseWalkSpeed;
                if (ImGui::SliderFloat("Base Walk Speed", &overrideWalkSpeed, 0.f, 10000.f))
                    playerPawn->BaseWalkSpeed = overrideWalkSpeed;

                static float overrideAirControl = playerPawn->AirControl;
                if (ImGui::SliderFloat("Air Control", &overrideAirControl, 0.f, 10000.f))
                    playerPawn->AirControl = overrideAirControl;

                static float overrideFriction = playerPawn->GroundFriction;
                if (ImGui::SliderFloat("Ground Friction", &overrideFriction, 0.f, 10000.f))
                    playerPawn->GroundFriction = overrideFriction;

                static float overrideFlipHeight = player->FlipHeight;
                if (ImGui::SliderFloat("Flip Height", &overrideFlipHeight, 0.f, 10000.f))
                    player->FlipHeight = overrideFlipHeight;

                static float overrideDashHeight = player->DashHeight;
                if (ImGui::SliderFloat("Dash Height", &overrideDashHeight, 0.f, 10000.f))
                    player->DashHeight = overrideDashHeight;

                static float overrideAcceleration = playerPawn->MaxAcceleration;
                if (ImGui::SliderFloat("Max Acceleration", &overrideAcceleration, 0.f, 10000.f))
                    playerPawn->MaxAcceleration = overrideAcceleration;

                static float overrideDashCooldown = player->BaseDashCooldown;
                if (ImGui::SliderFloat("Dash Cooldown", &overrideDashCooldown, 0.f, 10000.f))
                    player->BaseDashCooldown = overrideDashCooldown;

                ImGui::SliderFloat("Player Scale", &g_overridePlayerScale, 0.1f, 10000.f);

                ImGui::EndTabItem();
            }

            // ---------------- Combat Tab ----------------
            if (ImGui::BeginTabItem("Combat"))
            {
                static float overrideDamageMultiplier = ps->DamageMultiplier;
                if (ImGui::SliderFloat("Damage Multiplier", &overrideDamageMultiplier, 0.f, 10000.f))
                    ps->DamageMultiplier = overrideDamageMultiplier;

                static float overrideHealthMultiplier = ps->MaxHealthMultiplier;
                if (ImGui::SliderFloat("Health Multiplier", &overrideHealthMultiplier, 0.f, 10000.f))
                    ps->MaxHealthMultiplier = overrideHealthMultiplier;

                if (ps->WeaponDA)
                {
                    static float overrideFireRate = ps->WeaponDA->BaseFireRate;
                    if (ImGui::SliderFloat("Fire Rate", &overrideFireRate, 0.01f, 10000.f))
                        ps->WeaponDA->BaseFireRate = overrideFireRate;

                    static float overrideChamberDelay = ps->WeaponDA->PostFireClearChamberDelay;
                    if (ImGui::SliderFloat("Chamber Delay", &overrideChamberDelay, 0.f, 10000.f))
                        ps->WeaponDA->PostFireClearChamberDelay = overrideChamberDelay;

                    static float overrideBurstDelay = ps->WeaponDA->TimeBetweenBurstShots;
                    if (ImGui::SliderFloat("Burst Shot Delay", &overrideBurstDelay, 0.f, 10000.f))
                        ps->WeaponDA->TimeBetweenBurstShots = overrideBurstDelay;
                   
                    static float overrideReloadDuration = ps->WeaponDA->ReloadDuration;
                    if (ImGui::SliderFloat("Reload Duration", &overrideReloadDuration, 0.f, 10000.f))
                        ps->WeaponDA->ReloadDuration = overrideReloadDuration;

                    static float overrideVerticalRecoil = ps->WeaponDA->VerticalRecoil;
                    if (ImGui::SliderFloat("Vertical Recoil", &overrideVerticalRecoil, 0.f, 10000.f))
                        ps->WeaponDA->VerticalRecoil = overrideVerticalRecoil;

                    static float overrideHorizontalRecoil = ps->WeaponDA->HorizontalRecoil;
                    if (ImGui::SliderFloat("Horizontal Recoil", &overrideHorizontalRecoil, 0.f, 10000.f))
                        ps->WeaponDA->HorizontalRecoil = overrideHorizontalRecoil;

                    static float overrideRecoilInterp = ps->WeaponDA->RecoilInterpSpeed;
                    if (ImGui::SliderFloat("Recoil Interp Speed", &overrideRecoilInterp, 0.f, 10000.f))
                        ps->WeaponDA->RecoilInterpSpeed = overrideRecoilInterp;

                    static float overrideRecoveryInterp = ps->WeaponDA->RecoilRecoveryInterpSpeed;
                    if (ImGui::SliderFloat("Recoil Recovery Speed", &overrideRecoveryInterp, 0.f, 10000.f))
                        ps->WeaponDA->RecoilRecoveryInterpSpeed = overrideRecoveryInterp;
                }

                if (ps->MeleeDA)
                {
                    static float meleeRange = ps->MeleeDA->Range;
                    if (ImGui::SliderFloat("Melee Range", &meleeRange, 50.f, 10000.f))
                        ps->MeleeDA->Range = meleeRange;

                    static float meleeDamage = ps->MeleeDA->Damage;
                    if (ImGui::SliderFloat("Melee DMG", &meleeDamage, 50.f, 10000.f))
                        ps->MeleeDA->Damage = meleeDamage;
                }

                ImGui::EndTabItem();
            }

            // ---------------- Misc Tab ----------------
            if (ImGui::BeginTabItem("Misc"))
            {
                if (ImGui::Button("UNHOOK DLL", ImVec2(ImGui::GetContentRegionAvail().x, 25)))
                    g_KillSwitch = TRUE;

                if (ImGui::Button("DUMP OFFSETS", ImVec2(ImGui::GetContentRegionAvail().x, 25)))
                    DumpLocalPlayerOffsets();

                ImGui::EndTabItem();
            }

            ImGui::EndTabBar();
        }

        ImGui::End();
    }

    void Menu::ESP()
    {
        if (!g_Engine)
            return;

        // Get world
        SDK::UWorld* world = g_Engine->pWorld;
        if (!world)
            return;

        SDK::UGameInstance* gi = world->OwningGameInstance;
        if (!gi)
            return;

        // Get local player controller and pawn
        SDK::APlayerController* localPC = nullptr;
        SDK::APawn* localPawn = nullptr;
        for (int i = 0; i < gi->LocalPlayers.Num(); ++i)
        {
            SDK::ULocalPlayer* local = gi->LocalPlayers[i];
            if (!local || !local->PlayerController) continue;

            localPC = local->PlayerController;
            localPawn = localPC->AcknowledgedPawn;
            break;
        }

        if (!localPC || !localPawn)
            return;

        SDK::FVector localLoc = localPawn->RootComponent ? localPawn->RootComponent->RelativeLocation : SDK::FVector{ 0,0,0 };

        // Get actors in level
        SDK::ULevel* level = world->PersistentLevel;
        if (!level)
            return;

        TArray<SDK::AActor*>& actors = *reinterpret_cast<TArray<SDK::AActor*>*>((uintptr_t)level + 0x98);

        // Clear previous enemy cache
        gCachedEnemies.clear();

        for (auto* actor : actors)
        {
            if (!actor || actor == localPawn || !actor->Class) continue;

            std::string className = actor->Class->GetName();
            if (className.find("BP_Enemy_") == std::string::npos &&
                className.find("BP_EQC_") == std::string::npos)
                continue;

            gCachedEnemies.push_back(static_cast<SDK::ACrabEnemyC*>(actor));

            SDK::FVector loc = actor->RootComponent ? actor->RootComponent->RelativeLocation : SDK::FVector{ 0,0,0 };

            // Project to screen
            SDK::FVector2D screenPos;
            if (!localPC->ProjectWorldLocationToScreen(loc, &screenPos, false))
                continue;

            // Distance-based scaling
            float distance = sqrtf(
                powf(loc.X - localLoc.X, 2) +
                powf(loc.Y - localLoc.Y, 2) +
                powf(loc.Z - localLoc.Z, 2)
            );

            float scale = 1000.f / (distance + 1.f);
            if (scale < 0.5f) scale = 0.5f;
            if (scale > 1.0f) scale = 1.0f;

            float boxWidth = 40.f * scale;
            float boxHeight = 80.f * scale;

            ImVec2 tl(screenPos.X - boxWidth / 2.f, screenPos.Y - boxHeight / 2.f);
            ImVec2 br(screenPos.X + boxWidth / 2.f, screenPos.Y + boxHeight / 2.f);

            // Draw box
            ImGui::GetForegroundDrawList()->AddRect(tl, br, IM_COL32(255, 0, 0, 255), 0.0f, 0, 2.0f);

            // Draw proper enemy name
            SDK::ACrabEnemyC* enemyPawn = static_cast<SDK::ACrabEnemyC*>(actor);
            std::string enemyName = "Enemy";

            if (enemyPawn)
            {
                enemyName = enemyPawn->EnemyName.ToString();
            }

            ImGui::GetForegroundDrawList()->AddText(ImVec2(tl.x, tl.y - 12.f), IM_COL32(255, 255, 255, 255), enemyName.c_str());
        }
    }


    // ----------------------------------------------------------------
    // Function to dump local player and main offsets to console
    // ----------------------------------------------------------------
    void DX11Base::Menu::DumpLocalPlayerOffsets()
    {
        char buffer[512];

        g_Console->cLog("[*] Dumping LocalPlayer Offsets (runtime)\n", Console::EColor_dark_yellow);

        uintptr_t moduleBase = (uintptr_t)GetModuleHandle(L"CrabChampions-Win64-Shipping.exe");
        if (!moduleBase)
            moduleBase = (uintptr_t)GetModuleHandle(nullptr);

        if (!moduleBase)
        {
            g_Console->cLog("[!] Unable to get module base.\n", Console::EColor_dark_red);
            return;
        }

        UWorld* GWorldPtr = nullptr;
        __try
        {
            GWorldPtr = *reinterpret_cast<UWorld**>(moduleBase + ::Offsets::GWorld);
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
            GWorldPtr = nullptr;
        }

        if (!GWorldPtr)
        {
            std::snprintf(buffer, sizeof(buffer), "[!] GWorld is null. Check ::Offsets::GWorld (0x%zX) and module name.\n", ::Offsets::GWorld);
            g_Console->cLog(buffer, Console::EColor_dark_red);
            return;
        }

        std::snprintf(buffer, sizeof(buffer), "[+] GWorld: 0x%p\n", (void*)GWorldPtr);
        g_Console->cLog(buffer, Console::EColor_dark_green);

        UGameInstance* GameInstance = nullptr;
        __try { GameInstance = GWorldPtr->OwningGameInstance; }
        __except (EXCEPTION_EXECUTE_HANDLER) { GameInstance = nullptr; }

        std::snprintf(buffer, sizeof(buffer), "[+] GameInstance: 0x%p\n", (void*)GameInstance);
        g_Console->cLog(buffer, Console::EColor_dark_green);

        if (!GameInstance)
        {
            g_Console->cLog("[!] GameInstance is null. Aborting further derefs.\n", Console::EColor_dark_red);
            return;
        }

        int32 countLocalPlayers = 0;
        __try { countLocalPlayers = GameInstance->LocalPlayers.Num(); }
        __except (EXCEPTION_EXECUTE_HANDLER) { countLocalPlayers = 0; }

        std::snprintf(buffer, sizeof(buffer), "[+] LocalPlayers count: %d\n", countLocalPlayers);
        g_Console->cLog(buffer, Console::EColor_dark_green);

        if (countLocalPlayers <= 0)
        {
            g_Console->cLog("[!] No local players found (count == 0)\n", Console::EColor_dark_yellow);
        }

        for (int i = 0; i < countLocalPlayers; ++i)
        {
            ULocalPlayer* local = nullptr;
            __try { local = GameInstance->LocalPlayers[i]; }
            __except (EXCEPTION_EXECUTE_HANDLER) { local = nullptr; }

            std::snprintf(buffer, sizeof(buffer), "    [+] LocalPlayer[%d]: 0x%p\n", i, (void*)local);
            g_Console->cLog(buffer, Console::EColor_dark_green);

            if (!local)
                continue;

            APlayerController* pc = nullptr;
            __try { pc = local->PlayerController; }
            __except (EXCEPTION_EXECUTE_HANDLER) { pc = nullptr; }

            std::snprintf(buffer, sizeof(buffer), "        [+] PlayerController: 0x%p\n", (void*)pc);
            g_Console->cLog(buffer, Console::EColor_dark_green);

            if (!pc)
                continue;

            APawn* pawn = nullptr;
            __try { pawn = pc->AcknowledgedPawn; }
            __except (EXCEPTION_EXECUTE_HANDLER) { pawn = nullptr; }

            std::snprintf(buffer, sizeof(buffer), "            [+] Pawn: 0x%p\n", (void*)pawn);
            g_Console->cLog(buffer, Console::EColor_dark_green);

            if (!pawn)
                continue;

            USceneComponent* root = nullptr;
            __try { root = pawn->RootComponent; }
            __except (EXCEPTION_EXECUTE_HANDLER) { root = nullptr; }

            std::snprintf(buffer, sizeof(buffer), "                [+] RootComponent: 0x%p\n", (void*)root);
            g_Console->cLog(buffer, Console::EColor_dark_green);

            DX11Base::Vector3 loc = { 0.0f, 0.0f, 0.0f };
            if (root)
            {
                __try
                {
                    loc.x = root->RelativeLocation.X;
                    loc.y = root->RelativeLocation.Y;
                    loc.z = root->RelativeLocation.Z;
                }
                __except (EXCEPTION_EXECUTE_HANDLER)
                {
                    loc.x = loc.y = loc.z = 0.0f;
                }
            }

            std::snprintf(buffer, sizeof(buffer), "                [+] Location: X=%.2f Y=%.2f Z=%.2f\n", loc.x, loc.y, loc.z);
            g_Console->cLog(buffer, Console::EColor_dark_green);
        }

        std::snprintf(buffer, sizeof(buffer), "[*] Offsets summary: GWorld=0x%zX, GObjects=0x%zX, GNames=0x%zX, ProcessEvent=0x%zX\n",
            ::Offsets::GWorld, ::Offsets::GObjects, ::Offsets::GNames, ::Offsets::ProcessEvent);
        g_Console->cLog(buffer, Console::EColor_dark_yellow);
    }

    //----------------------------------------------------------------------------------------------------
    //										GUI
    //-----------------------------------------------------------------------------------

    void GUI::TextCentered(const char* pText)
    {
        ImVec2 textSize = ImGui::CalcTextSize(pText);
        ImVec2 windowSize = ImGui::GetWindowSize();
        ImVec2 textPos = ImVec2((windowSize.x - textSize.x) * 0.5f, (windowSize.y - textSize.y) * 0.5f);
        ImGui::SetCursorPos(textPos);
        ImGui::Text("%s", pText);
    }

    void GUI::TextCenteredf(const char* pText, ...)
    {
        va_list args;
        va_start(args, pText);
        char buffer[256];
        vsnprintf(buffer, sizeof(buffer), pText, args);
        va_end(args);

        TextCentered(buffer);
    }

    void GUI::DrawText_(ImVec2 pos, ImColor color, const char* pText, float fontSize)
    {
        ImGui::GetWindowDrawList()->AddText(ImGui::GetFont(), fontSize, pos, color, pText, pText + strlen(pText), 800, 0);
    }

    void GUI::DrawTextf(ImVec2 pos, ImColor color, const char* pText, float fontSize, ...)
    {
        va_list args;
        va_start(args, fontSize);
        char buffer[256];
        vsnprintf(buffer, sizeof(buffer), pText, args);
        va_end(args);

        DrawText_(pos, color, buffer, fontSize);
    }

    void GUI::DrawTextCentered(ImVec2 pos, ImColor color, const char* pText, float fontSize)
    {
        float textSize = ImGui::CalcTextSize(pText).x;
        ImVec2 textPosition = ImVec2(pos.x - (textSize * 0.5f), pos.y);
        DrawText_(textPosition, color, pText, fontSize);
    }

    void GUI::DrawTextCenteredf(ImVec2 pos, ImColor color, const char* pText, float fontSize, ...)
    {
        va_list args;
        va_start(args, fontSize);
        char buffer[256];
        vsnprintf(buffer, sizeof(buffer), pText, args);
        va_end(args);

        DrawTextCentered(pos, color, buffer, fontSize);
    }
}
