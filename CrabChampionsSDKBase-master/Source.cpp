#pragma once
#include <pch.h>
#include <Engine.h>
#include <Menu.h>
#include "SDK.h"
using namespace DX11Base;

void ClientBGThread()
{
    while (g_Running)
    {
        Menu::Loops();

        if (g_KillSwitch)
        {
            g_D3D11Window->UnhookD3D();
            g_Hooking->Shutdown();
            g_Engine.release();     // releases all created class instances
            g_Running = false;
        }

        std::this_thread::sleep_for(1ms);
        std::this_thread::yield();
    }
}

DWORD WINAPI MainThread_Initialize(LPVOID dwModule)
{
    UNREFERENCED_PARAMETER(dwModule);

    g_Engine = std::make_unique<Engine>();
    g_Console->InitializeConsole("DEBUG CONSOLE", true);
    g_Engine->Init();
    g_D3D11Window->HookD3D();
    g_Hooking->Initialize();

    // INITIALIZE BACKGROUND THREAD
    std::thread WCMUpdate(ClientBGThread);

    // RENDER LOOP
    g_Running = true;
    static int LastTick = 0;

    while (g_Running)
    {
        // Only toggle menu on INSERT key with debounce
        if (Engine::GetKeyState(VK_INSERT, 0) && (GetTickCount64() - LastTick > 500))
        {
            g_Engine->bShowMenu = !g_Engine->bShowMenu; // toggle menu visibility
            LastTick = GetTickCount64();
        }

        std::this_thread::sleep_for(1ms);
        std::this_thread::yield();
    }

    // EXIT
    WCMUpdate.join();                          // Exit Loops Thread
    FreeLibraryAndExitThread(g_hModule, EXIT_SUCCESS);
    return EXIT_SUCCESS;
}
