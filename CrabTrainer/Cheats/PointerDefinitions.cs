using System;
using System.Collections.Generic;

namespace CrabTrainer.Cheats
{
    /// <summary>
    /// Defines pointer chains for Crab Champions values.
    /// These need to be discovered via Cheat Engine pointer scanning.
    ///
    /// How pointer chains work:
    /// 1. Game has a base address (exe module base)
    /// 2. Add static offset to get first pointer
    /// 3. Follow pointer chain (dereference + add offset) until final value
    ///
    /// Example: BaseAddress + 0x1234 -> [+0x10] -> [+0x48] -> +0x1C = Health
    /// </summary>
    public static class PointerDefinitions
    {
        // Module name for base address
        public const string GameModule = "CrabChampions-Win64-Shipping.exe";

        // ============================================
        // POINTER CHAINS
        // Format: (StaticOffset, Offsets[])
        //
        // TO FIND THESE:
        // 1. Use Cheat Engine to find the value
        // 2. Do a pointer scan on the address
        // 3. Find a path starting from game module base
        // 4. Test it works after game restart
        // ============================================

        /// <summary>
        /// Player health pointer chain
        /// Unreal Engine typically: GEngine -> GameInstance -> LocalPlayer -> PlayerController -> Character -> Health
        /// </summary>
        public static readonly PointerChain PlayerHealth = new(
            "Player Health",
            CheatValueType.Float,
            0x0, // Static offset from module base - NEEDS TO BE FOUND
            new int[] { 0x0, 0x0, 0x0 } // Offset chain - NEEDS TO BE FOUND
        );

        /// <summary>
        /// Player max health
        /// </summary>
        public static readonly PointerChain PlayerMaxHealth = new(
            "Max Health",
            CheatValueType.Float,
            0x0,
            new int[] { 0x0, 0x0, 0x0 }
        );

        /// <summary>
        /// Crystal/currency count
        /// </summary>
        public static readonly PointerChain Crystals = new(
            "Crystals",
            CheatValueType.Int32,
            0x0,
            new int[] { 0x0, 0x0, 0x0 }
        );

        /// <summary>
        /// Current wave number
        /// </summary>
        public static readonly PointerChain WaveNumber = new(
            "Wave",
            CheatValueType.Int32,
            0x0,
            new int[] { 0x0, 0x0, 0x0 }
        );

        /// <summary>
        /// Player movement speed
        /// </summary>
        public static readonly PointerChain MoveSpeed = new(
            "Move Speed",
            CheatValueType.Float,
            0x0,
            new int[] { 0x0, 0x0, 0x0 }
        );

        /// <summary>
        /// Damage multiplier
        /// </summary>
        public static readonly PointerChain DamageMultiplier = new(
            "Damage Multiplier",
            CheatValueType.Float,
            0x0,
            new int[] { 0x0, 0x0, 0x0 }
        );

        /// <summary>
        /// All defined pointer chains
        /// </summary>
        public static readonly PointerChain[] AllChains = new[]
        {
            PlayerHealth,
            PlayerMaxHealth,
            Crystals,
            WaveNumber,
            MoveSpeed,
            DamageMultiplier
        };

        // ============================================
        // COMMON UNREAL ENGINE 5 PATTERNS
        // These are typical offsets found in UE5 games
        // ============================================

        /// <summary>
        /// Common UE5 GWorld offset patterns to try
        /// </summary>
        public static readonly long[] CommonGWorldOffsets = new long[]
        {
            0x05A8C8F8,  // Common UE5 pattern
            0x05A8C900,
            0x058A0000,
            0x05900000,
        };

        /// <summary>
        /// Common offset from GWorld to PlayerController
        /// GWorld -> GameInstance -> LocalPlayers[0] -> PlayerController
        /// </summary>
        public static readonly int[] GWorldToPlayerController = new int[]
        {
            0x180,  // GWorld -> OwningGameInstance
            0x38,   // GameInstance -> LocalPlayers
            0x0,    // LocalPlayers[0]
            0x30,   // ULocalPlayer -> PlayerController
        };

        /// <summary>
        /// Common offset from PlayerController to Pawn/Character
        /// </summary>
        public static readonly int[] PlayerControllerToPawn = new int[]
        {
            0x338,  // APlayerController -> AcknowledgedPawn (varies by UE version)
        };
    }

    /// <summary>
    /// Represents a pointer chain to a game value
    /// </summary>
    public class PointerChain
    {
        public string Name { get; }
        public CheatValueType ValueType { get; }
        public long StaticOffset { get; set; }
        public int[] Offsets { get; set; }
        public bool IsValid => StaticOffset != 0 && Offsets.Length > 0;

        public PointerChain(string name, CheatValueType valueType, long staticOffset, int[] offsets)
        {
            Name = name;
            ValueType = valueType;
            StaticOffset = staticOffset;
            Offsets = offsets;
        }

        /// <summary>
        /// Create a copy with updated offsets
        /// </summary>
        public PointerChain WithOffsets(long staticOffset, int[] offsets)
        {
            return new PointerChain(Name, ValueType, staticOffset, offsets);
        }
    }

    /// <summary>
    /// Configuration file for storing discovered pointer chains
    /// </summary>
    public class PointerConfig
    {
        public string GameVersion { get; set; } = "";
        public DateTime LastUpdated { get; set; }
        public Dictionary<string, PointerChainConfig> Chains { get; set; } = new();
    }

    public class PointerChainConfig
    {
        public long StaticOffset { get; set; }
        public int[] Offsets { get; set; } = Array.Empty<int>();
        public string ValueType { get; set; } = "Int32";
    }
}
