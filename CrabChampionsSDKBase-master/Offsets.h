#pragma once
#include <cstdint>

namespace Offsets
{
    // Global Objects / Names
    constexpr uintptr_t GObjects = 0x042AB770;
    constexpr uintptr_t GNames = 0x0426F480;
    constexpr uintptr_t AppendString = 0x00ECE1A0;

    // World & Gameplay
    constexpr uintptr_t GWorld = 0x043EAF20;

    // Function / Event
    constexpr uintptr_t ProcessEvent = 0x010C5F00;
    constexpr uintptr_t ProcessEventIdx = 0x44;
}
