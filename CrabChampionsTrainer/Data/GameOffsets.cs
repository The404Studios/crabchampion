namespace CrabChampionsTrainer.Data;

/// <summary>
/// Memory offsets for Crab Champions (Unreal Engine 4)
/// Updated for Patch V2343+ (December 2024)
///
/// Sources:
/// - FearLess Revolution Cheat Tables
/// - UE4SS SDK Dumps
/// - Community research
///
/// Note: Offsets may need updating when game patches release
/// Use Cheat Engine or UE4SS to verify/update offsets
/// </summary>
public static class GameOffsets
{
    // Game info
    public const string GameVersion = "V2343+";
    public const string ProcessName = "CrabChampions-Win64-Shipping";

    // ===========================================
    // AOB PATTERNS (Array of Bytes Scan Patterns)
    // These are used to find base addresses dynamically
    // ===========================================

    public static class Patterns
    {
        /// <summary>
        /// GWorld pattern - finds the UWorld pointer
        /// Pattern: 48 8B 1D ?? ?? ?? ?? 48 85 DB 74 3B 41 B0 01
        /// </summary>
        public const string GWorld = "48 8B 1D ?? ?? ?? ?? 48 85 DB 74 3B 41 B0 01";

        /// <summary>
        /// GNames pattern - finds the FNamePool/GNames array
        /// Pattern: 48 8B 05 ?? ?? ?? ?? 48 85 C0 75 5F
        /// </summary>
        public const string GNames = "48 8B 05 ?? ?? ?? ?? 48 85 C0 75 5F";

        /// <summary>
        /// GObjects pattern - finds the TUObjectArray
        /// Pattern: 4C 8B 15 ?? ?? ?? ?? 8D 43 01
        /// </summary>
        public const string GObjects = "4C 8B 15 ?? ?? ?? ?? 8D 43 01";

        /// <summary>
        /// Player health decrease hook
        /// </summary>
        public const string HealthDecrease = "F3 0F 10 81 ?? ?? ?? ?? F3 0F 5C C1 F3 0F 11 81";

        /// <summary>
        /// Ammo usage hook
        /// </summary>
        public const string AmmoUsage = "44 89 ?? ?? ?? ?? ?? 48 8B ?? ?? ?? ?? ?? 4C 8B";

        /// <summary>
        /// Keys/Crystals change hook
        /// </summary>
        public const string CurrencyChange = "89 ?? ?? ?? ?? ?? 48 83 C4 20 5B C3";
    }

    // ===========================================
    // STATIC OFFSETS (from game base or GWorld)
    // These are relative to known base addresses
    // ===========================================

    /// <summary>
    /// Offsets from GWorld to game objects
    /// GWorld -> OwningGameInstance -> LocalPlayers[0] -> PlayerController -> etc
    /// </summary>
    public static class World
    {
        public const int OwningGameInstance = 0x1B8;
        public const int PersistentLevel = 0x30;
        public const int GameState = 0x120;
        public const int AuthorityGameMode = 0x118;
    }

    /// <summary>
    /// UGameInstance offsets
    /// </summary>
    public static class GameInstance
    {
        public const int LocalPlayers = 0x38;        // TArray<ULocalPlayer*>
        public const int TimerManager = 0x400;
        public const int WorldContext = 0x98;
    }

    /// <summary>
    /// ULocalPlayer -> APlayerController
    /// </summary>
    public static class LocalPlayer
    {
        public const int PlayerController = 0x30;
        public const int ViewportClient = 0x78;
    }

    /// <summary>
    /// APlayerController offsets
    /// </summary>
    public static class PlayerController
    {
        public const int AcknowledgedPawn = 0x338;   // The possessed pawn
        public const int PlayerState = 0x298;
        public const int PlayerCameraManager = 0x348;
        public const int MyHUD = 0x350;
        public const int InputComponent = 0x360;
        public const int SpawnLocation = 0x3E0;
    }

    /// <summary>
    /// ACharacter / APawn offsets
    /// These are for the player character (Crab)
    /// </summary>
    public static class Character
    {
        // Core components
        public const int RootComponent = 0x198;
        public const int CharacterMovement = 0x320;
        public const int Mesh = 0x318;
        public const int CapsuleComponent = 0x2F8;

        // Actor base
        public const int ActorLocation = 0x128;      // FVector
        public const int ActorRotation = 0x140;      // FRotator

        // Health (CC_Character specific - verify with UE4SS)
        public const int Health = 0x9D4;             // Current health float
        public const int MaxHealth = 0x9D8;          // Max health float
        public const int Shield = 0x9DC;             // Shield float
        public const int MaxShield = 0x9E0;          // Max shield float
        public const int Armor = 0x9E4;              // Armor value

        // Damage flags
        public const int bCanBeDamaged = 0x9F0;      // bool
        public const int bIsInvulnerable = 0x9F1;    // bool

        // Ability cooldowns
        public const int AbilityCooldown = 0xA10;
        public const int DashCooldown = 0xA14;
    }

    /// <summary>
    /// UCharacterMovementComponent offsets
    /// Used for speed, jump, and movement hacks
    /// </summary>
    public static class Movement
    {
        // Speeds (float values)
        public const int MaxWalkSpeed = 0x2C4;           // Default: 600
        public const int MaxWalkSpeedCrouched = 0x2C8;   // Default: 300
        public const int MaxSwimSpeed = 0x2CC;
        public const int MaxFlySpeed = 0x2D0;
        public const int MaxAcceleration = 0x2DC;

        // Jump
        public const int JumpZVelocity = 0x3A0;          // Default: 420
        public const int AirControl = 0x3A8;
        public const int GravityScale = 0x3AC;

        // Movement mode
        public const int MovementMode = 0x250;           // EMovementMode enum
        // 0 = None, 1 = Walking, 2 = NavWalking, 3 = Falling, 4 = Swimming, 5 = Flying

        // Velocity
        public const int Velocity = 0x168;               // FVector
    }

    /// <summary>
    /// Weapon component offsets
    /// </summary>
    public static class Weapon
    {
        // Ammo
        public const int CurrentAmmo = 0x2C0;            // int32
        public const int MaxAmmo = 0x2C4;                // int32
        public const int ReserveAmmo = 0x2C8;            // int32
        public const int MaxReserveAmmo = 0x2CC;         // int32

        // Weapon stats
        public const int BaseDamage = 0x2D0;             // float
        public const int FireRate = 0x2D4;               // float (shots per second)
        public const int ReloadTime = 0x2D8;             // float (seconds)
        public const int Range = 0x2DC;                  // float
        public const int Spread = 0x2E0;                 // float
        public const int Recoil = 0x2E4;                 // float

        // Flags
        public const int bInfiniteAmmo = 0x2F0;          // bool
        public const int bNoReload = 0x2F1;              // bool
        public const int bDualWield = 0x2F2;             // bool
    }

    /// <summary>
    /// Currency/Save data offsets
    /// Found in PlayerState or SaveGame object
    /// </summary>
    public static class Currency
    {
        // These offsets are relative to the save game or player state
        public const int Keys = 0x350;                   // int32 - current keys
        public const int Crystals = 0x354;               // int32 - current crystals
        public const int TotalKeysCollected = 0x358;     // int32
        public const int TotalCrystalsCollected = 0x35C; // int32
    }

    /// <summary>
    /// Inventory/Item offsets
    /// </summary>
    public static class Inventory
    {
        public const int Items = 0x280;                  // TArray<UItem*>
        public const int ItemCount = 0x288;              // int32
        public const int MaxSlots = 0x28C;               // int32
        public const int Weapons = 0x290;                // TArray<UWeapon*>
        public const int CurrentWeaponIndex = 0x298;
        public const int Upgrades = 0x2A0;               // TArray<UUpgrade*>
        public const int Prismatics = 0x2B0;             // TArray<UPrismatic*>
        public const int Perks = 0x2C0;                  // TArray<UPerk*>
    }

    /// <summary>
    /// Unlocks and cosmetics
    /// </summary>
    public static class Unlocks
    {
        public const int UnlockedSkins = 0x400;          // TArray<int32> or TSet
        public const int UnlockedCosmetics = 0x410;
        public const int bDebugUnlockAll = 0x420;        // bool - unlocks everything
    }

    /// <summary>
    /// Common UE4 UObject offsets
    /// </summary>
    public static class UObject
    {
        public const int VTable = 0x0;
        public const int ObjectFlags = 0x8;
        public const int InternalIndex = 0xC;
        public const int ClassPrivate = 0x10;
        public const int NamePrivate = 0x18;
        public const int OuterPrivate = 0x20;
    }

    /// <summary>
    /// TArray structure (UE4 dynamic array)
    /// </summary>
    public static class TArray
    {
        public const int Data = 0x0;      // Pointer to array data
        public const int Count = 0x8;     // Number of elements (int32)
        public const int Max = 0xC;       // Allocated capacity (int32)
    }

    // ===========================================
    // POINTER CHAIN HELPERS
    // ===========================================

    /// <summary>
    /// Full pointer chain from GWorld to player health
    /// GWorld -> GameInstance -> LocalPlayers[0] -> PlayerController -> Pawn -> Health
    /// </summary>
    public static readonly int[] HealthPointerChain = new int[]
    {
        World.OwningGameInstance,
        GameInstance.LocalPlayers,
        0x0,  // First element of TArray
        LocalPlayer.PlayerController,
        PlayerController.AcknowledgedPawn,
        Character.Health
    };

    /// <summary>
    /// Pointer chain to player movement component
    /// </summary>
    public static readonly int[] MovementPointerChain = new int[]
    {
        World.OwningGameInstance,
        GameInstance.LocalPlayers,
        0x0,
        LocalPlayer.PlayerController,
        PlayerController.AcknowledgedPawn,
        Character.CharacterMovement
    };

    /// <summary>
    /// Pointer chain to player currency
    /// </summary>
    public static readonly int[] CurrencyPointerChain = new int[]
    {
        World.OwningGameInstance,
        GameInstance.LocalPlayers,
        0x0,
        LocalPlayer.PlayerController,
        PlayerController.PlayerState,
        Currency.Keys
    };
}
