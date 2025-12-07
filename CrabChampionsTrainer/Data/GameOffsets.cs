namespace CrabChampionsTrainer.Data;

/// <summary>
/// Memory offsets for Crab Champions
/// These offsets may need to be updated when the game updates
/// Use the SDK dump feature or Cheat Engine to find new offsets
/// </summary>
public static class GameOffsets
{
    // Game version these offsets are for
    public const string GameVersion = "Latest (2024)";

    // ===========================================
    // POINTER PATTERNS (AOB Scans)
    // ===========================================

    /// <summary>
    /// Pattern to find GWorld pointer
    /// </summary>
    public const string GWorldPattern = "48 8B 1D ?? ?? ?? ?? 48 85 DB 74 ?? 41 B0 01";

    /// <summary>
    /// Pattern to find GNames pointer
    /// </summary>
    public const string GNamesPattern = "48 8B 05 ?? ?? ?? ?? 48 85 C0 75 ?? B9 ?? ?? ?? ?? 48 89";

    /// <summary>
    /// Pattern to find GObjects pointer
    /// </summary>
    public const string GObjectsPattern = "48 8B 05 ?? ?? ?? ?? 48 8B 0C ?? 48 8D 04 ?? 48 85 C0";

    // ===========================================
    // BASE OFFSETS (from game base address)
    // ===========================================

    // These are placeholder offsets - need to be updated via CE or SDK dump
    public const int GWorld = 0x0;
    public const int GNames = 0x0;
    public const int GObjects = 0x0;

    // ===========================================
    // PLAYER OFFSETS
    // ===========================================

    public static class Player
    {
        // From PlayerController
        public const int PlayerController = 0x0;
        public const int AcknowledgedPawn = 0x338;
        public const int PlayerState = 0x298;

        // From Character/Pawn
        public const int Health = 0x0;
        public const int MaxHealth = 0x4;
        public const int Shield = 0x8;
        public const int MaxShield = 0xC;
        public const int Armor = 0x10;

        // Movement
        public const int MovementComponent = 0x320;
        public const int WalkSpeed = 0x0;
        public const int SprintSpeed = 0x4;
        public const int JumpZVelocity = 0x8;

        // Position
        public const int Location = 0x0;
        public const int Rotation = 0x0;

        // Flags
        public const int bCanBeDamaged = 0x0;
        public const int bIsInvulnerable = 0x0;
        public const int bGodMode = 0x0;
    }

    // ===========================================
    // CURRENCY OFFSETS
    // ===========================================

    public static class Currency
    {
        // From SaveGame or PlayerState
        public const int Keys = 0x0;
        public const int Crystals = 0x4;
        public const int TotalKeys = 0x8;
        public const int TotalCrystals = 0xC;
    }

    // ===========================================
    // WEAPON OFFSETS
    // ===========================================

    public static class Weapon
    {
        public const int WeaponComponent = 0x0;
        public const int CurrentWeapon = 0x0;
        public const int WeaponList = 0x0;

        // Current weapon stats
        public const int Ammo = 0x0;
        public const int MaxAmmo = 0x4;
        public const int ReserveAmmo = 0x8;
        public const int MaxReserveAmmo = 0xC;

        public const int Damage = 0x0;
        public const int FireRate = 0x4;
        public const int ReloadSpeed = 0x8;
        public const int Range = 0xC;

        // Flags
        public const int bInfiniteAmmo = 0x0;
        public const int bDualWield = 0x0;
    }

    // ===========================================
    // INVENTORY OFFSETS
    // ===========================================

    public static class Inventory
    {
        public const int InventoryComponent = 0x0;
        public const int Items = 0x0;
        public const int ItemCount = 0x0;
        public const int MaxSlots = 0x0;

        public const int Upgrades = 0x0;
        public const int Prismatics = 0x0;
        public const int Perks = 0x0;
    }

    // ===========================================
    // GAME STATE OFFSETS
    // ===========================================

    public static class GameState
    {
        public const int GameInstance = 0x0;
        public const int GameMode = 0x0;
        public const int CurrentLevel = 0x0;
        public const int CurrentIsland = 0x0;
        public const int Difficulty = 0x0;

        // Flags
        public const int bDebugMode = 0x0;
        public const int bUnlockAll = 0x0;
    }

    // ===========================================
    // SAVE DATA OFFSETS
    // ===========================================

    public static class SaveData
    {
        public const int SaveGame = 0x0;
        public const int UnlockedSkins = 0x0;
        public const int UnlockedCosmetics = 0x0;
        public const int Statistics = 0x0;
        public const int Achievements = 0x0;
    }

    // ===========================================
    // COMMON UE4 OFFSETS
    // ===========================================

    public static class UE4
    {
        // UObject
        public const int VTable = 0x0;
        public const int ObjectFlags = 0x8;
        public const int InternalIndex = 0xC;
        public const int ClassPrivate = 0x10;
        public const int NamePrivate = 0x18;
        public const int OuterPrivate = 0x20;

        // AActor
        public const int RootComponent = 0x198;
        public const int ActorLocation = 0x1E0;
        public const int ActorRotation = 0x1E8;

        // APawn
        public const int Controller = 0x2A0;

        // ACharacter
        public const int CharacterMovement = 0x320;

        // UCharacterMovementComponent
        public const int MaxWalkSpeed = 0x2C4;
        public const int MaxWalkSpeedCrouched = 0x2C8;
        public const int JumpZVelocity_CM = 0x3A0;
    }
}
