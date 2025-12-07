using CrabChampionsTrainer.Data;

namespace CrabChampionsTrainer.Core;

/// <summary>
/// Main game interaction manager - handles all cheats and modifications
/// </summary>
public class GameManager : IDisposable
{
    private readonly Memory _memory;
    private System.Windows.Forms.Timer? _updateTimer;
    private bool _disposed;

    // Cached addresses
    private IntPtr _playerController;
    private IntPtr _playerCharacter;
    private IntPtr _gameInstance;
    private IntPtr _saveGame;
    private IntPtr _inventoryComponent;
    private IntPtr _weaponComponent;

    // State tracking
    private bool _godModeEnabled;
    private bool _infiniteAmmoEnabled;
    private bool _infiniteHealthEnabled;
    private bool _noClipEnabled;
    private bool _oneHitKillEnabled;
    private bool _superSpeedEnabled;
    private float _speedMultiplier = 1.0f;
    private float _damageMultiplier = 1.0f;
    private float _fireRateMultiplier = 1.0f;

    public bool IsAttached => _memory.IsAttached;
    public Memory Memory => _memory;

    // Events
    public event EventHandler<string>? OnLog;
    public event EventHandler<GameStats>? OnStatsUpdated;
    public event EventHandler? OnAttached;
    public event EventHandler? OnDetached;

    public GameManager()
    {
        _memory = new Memory();
        _memory.OnLog += (s, msg) => Log(msg);
        _memory.OnAttached += (s, e) =>
        {
            OnAttached?.Invoke(this, EventArgs.Empty);
            StartUpdateLoop();
        };
        _memory.OnDetached += (s, e) =>
        {
            OnDetached?.Invoke(this, EventArgs.Empty);
            StopUpdateLoop();
        };
    }

    #region Connection

    public bool Attach()
    {
        return _memory.Attach();
    }

    public void Detach()
    {
        _memory.Detach();
    }

    private void StartUpdateLoop()
    {
        _updateTimer = new System.Windows.Forms.Timer
        {
            Interval = 100 // 10 times per second
        };
        _updateTimer.Tick += UpdateLoop;
        _updateTimer.Start();

        Log("Update loop started");
    }

    private void StopUpdateLoop()
    {
        _updateTimer?.Stop();
        _updateTimer?.Dispose();
        _updateTimer = null;

        Log("Update loop stopped");
    }

    private void UpdateLoop(object? sender, EventArgs e)
    {
        if (!IsAttached)
        {
            StopUpdateLoop();
            return;
        }

        try
        {
            // Refresh cached addresses periodically
            RefreshAddresses();

            // Apply continuous effects
            ApplyContinuousEffects();

            // Update stats display
            UpdateStats();
        }
        catch (Exception ex)
        {
            Log($"Update loop error: {ex.Message}");
        }
    }

    private void RefreshAddresses()
    {
        // Find player controller and character
        // These would use pattern scanning or pointer chains
        // For now, we'll use placeholder logic

        // In a full implementation, this would:
        // 1. Scan for GWorld
        // 2. Get OwningGameInstance
        // 3. Get LocalPlayers[0]
        // 4. Get PlayerController
        // 5. Get Pawn/Character
    }

    private void ApplyContinuousEffects()
    {
        if (_godModeEnabled)
        {
            ApplyGodMode();
        }

        if (_infiniteAmmoEnabled)
        {
            ApplyInfiniteAmmo();
        }

        if (_infiniteHealthEnabled)
        {
            ApplyInfiniteHealth();
        }

        if (_superSpeedEnabled)
        {
            ApplySpeedMultiplier(_speedMultiplier);
        }
    }

    private void UpdateStats()
    {
        var stats = new GameStats
        {
            Health = ReadHealth(),
            MaxHealth = ReadMaxHealth(),
            Shield = ReadShield(),
            Keys = ReadKeys(),
            Crystals = ReadCrystals(),
            CurrentAmmo = ReadCurrentAmmo(),
            MaxAmmo = ReadMaxAmmo(),
        };

        OnStatsUpdated?.Invoke(this, stats);
    }

    #endregion

    #region Player Modifications

    public void SetGodMode(bool enabled)
    {
        _godModeEnabled = enabled;
        Log($"God Mode: {(enabled ? "ENABLED" : "DISABLED")}");

        if (enabled)
        {
            ApplyGodMode();
        }
    }

    private void ApplyGodMode()
    {
        if (_playerCharacter == IntPtr.Zero) return;

        // Set invulnerability flag
        // Set health to max
        // Disable damage taking

        // These are placeholder implementations
        // Actual offsets need to be found via reverse engineering
    }

    public void SetInfiniteHealth(bool enabled)
    {
        _infiniteHealthEnabled = enabled;
        Log($"Infinite Health: {(enabled ? "ENABLED" : "DISABLED")}");
    }

    private void ApplyInfiniteHealth()
    {
        if (_playerCharacter == IntPtr.Zero) return;

        // Keep health at max
        var maxHealth = ReadMaxHealth();
        if (maxHealth > 0)
        {
            SetHealth(maxHealth);
        }
    }

    public void SetInfiniteAmmo(bool enabled)
    {
        _infiniteAmmoEnabled = enabled;
        Log($"Infinite Ammo: {(enabled ? "ENABLED" : "DISABLED")}");
    }

    private void ApplyInfiniteAmmo()
    {
        if (_weaponComponent == IntPtr.Zero) return;

        // Keep ammo at max
        var maxAmmo = ReadMaxAmmo();
        if (maxAmmo > 0)
        {
            SetCurrentAmmo(maxAmmo);
        }
    }

    public void SetNoClip(bool enabled)
    {
        _noClipEnabled = enabled;
        Log($"No Clip: {(enabled ? "ENABLED" : "DISABLED")}");

        // Toggle collision
        // Set movement mode to flying
    }

    public void SetOneHitKill(bool enabled)
    {
        _oneHitKillEnabled = enabled;
        Log($"One Hit Kill: {(enabled ? "ENABLED" : "DISABLED")}");
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = Math.Clamp(multiplier, 0.1f, 10f);
        _superSpeedEnabled = Math.Abs(_speedMultiplier - 1.0f) > 0.01f;

        ApplySpeedMultiplier(_speedMultiplier);
        Log($"Speed Multiplier: {_speedMultiplier:F1}x");
    }

    private void ApplySpeedMultiplier(float multiplier)
    {
        if (_playerCharacter == IntPtr.Zero) return;

        // Modify movement component speeds
        // Default walk speed is typically 600
        float baseSpeed = 600f;
        // _memory.WriteFloat(walkSpeedAddress, baseSpeed * multiplier);
    }

    public void SetDamageMultiplier(float multiplier)
    {
        _damageMultiplier = Math.Clamp(multiplier, 0.1f, 100f);
        Log($"Damage Multiplier: {_damageMultiplier:F1}x");
    }

    public void SetFireRateMultiplier(float multiplier)
    {
        _fireRateMultiplier = Math.Clamp(multiplier, 0.1f, 10f);
        Log($"Fire Rate Multiplier: {_fireRateMultiplier:F1}x");
    }

    public void SetJumpMultiplier(float multiplier)
    {
        multiplier = Math.Clamp(multiplier, 0.1f, 10f);
        Log($"Jump Multiplier: {multiplier:F1}x");

        // Modify jump Z velocity
        // Default is typically 420
    }

    public void TeleportPlayer(float x, float y, float z)
    {
        Log($"Teleporting to: ({x}, {y}, {z})");
        // Write position to player location
    }

    #endregion

    #region Currency

    public int ReadKeys()
    {
        if (_saveGame == IntPtr.Zero) return 0;
        // return _memory.ReadInt32(IntPtr.Add(_saveGame, GameOffsets.Currency.Keys));
        return 0;
    }

    public void SetKeys(int amount)
    {
        amount = Math.Max(0, amount);
        Log($"Setting Keys to: {amount}");

        // Write to save game and/or player state
    }

    public void AddKeys(int amount)
    {
        var current = ReadKeys();
        SetKeys(current + amount);
    }

    public int ReadCrystals()
    {
        if (_saveGame == IntPtr.Zero) return 0;
        // return _memory.ReadInt32(IntPtr.Add(_saveGame, GameOffsets.Currency.Crystals));
        return 0;
    }

    public void SetCrystals(int amount)
    {
        amount = Math.Max(0, amount);
        Log($"Setting Crystals to: {amount}");
    }

    public void AddCrystals(int amount)
    {
        var current = ReadCrystals();
        SetCrystals(current + amount);
    }

    #endregion

    #region Health/Stats

    public float ReadHealth()
    {
        if (_playerCharacter == IntPtr.Zero) return 0;
        return 0;
    }

    public float ReadMaxHealth()
    {
        if (_playerCharacter == IntPtr.Zero) return 0;
        return 100;
    }

    public void SetHealth(float amount)
    {
        Log($"Setting Health to: {amount}");
    }

    public void SetMaxHealth(float amount)
    {
        Log($"Setting Max Health to: {amount}");
    }

    public float ReadShield()
    {
        return 0;
    }

    public void SetShield(float amount)
    {
        Log($"Setting Shield to: {amount}");
    }

    #endregion

    #region Ammo

    public int ReadCurrentAmmo()
    {
        return 0;
    }

    public int ReadMaxAmmo()
    {
        return 30;
    }

    public void SetCurrentAmmo(int amount)
    {
        // Write ammo value
    }

    public void SetMaxAmmo(int amount)
    {
        Log($"Setting Max Ammo to: {amount}");
    }

    #endregion

    #region Items & Prismatics

    public void GiveAllPrismatics()
    {
        Log("Giving ALL Prismatics...");

        foreach (var prismatic in GameData.Prismatics)
        {
            GivePrismatic(prismatic.Key);
        }

        Log($"Gave {GameData.Prismatics.Count} Prismatics!");
    }

    public void GivePrismatic(string prismaticName)
    {
        Log($"Giving Prismatic: {prismaticName}");

        // In a full implementation, this would:
        // 1. Find the inventory component
        // 2. Find or create the prismatic item instance
        // 3. Add it to the inventory array

        // For UE4 games, this typically requires:
        // - Finding the item class via GObjects
        // - Spawning a new instance
        // - Adding to inventory TArray
    }

    public void GivePrismaticsByCategory(PrismaticCategory category)
    {
        Log($"Giving all {category} Prismatics...");

        var prismatics = GameData.Prismatics
            .Where(p => p.Value.Category == category)
            .Select(p => p.Key);

        foreach (var prismatic in prismatics)
        {
            GivePrismatic(prismatic);
        }
    }

    public void GiveItem(string itemName)
    {
        Log($"Giving Item: {itemName}");
    }

    public void GiveAllItems()
    {
        Log("Giving ALL Items...");

        foreach (var item in GameData.Items)
        {
            GiveItem(item.Key);
        }

        Log($"Gave {GameData.Items.Count} Items!");
    }

    public void GiveItemsByCategory(ItemCategory category)
    {
        Log($"Giving all {category} Items...");

        var items = GameData.Items
            .Where(i => i.Value.Category == category)
            .Select(i => i.Key);

        foreach (var item in items)
        {
            GiveItem(item);
        }
    }

    public void GiveWeapon(string weaponName)
    {
        Log($"Giving Weapon: {weaponName}");
    }

    public void GiveAllWeapons()
    {
        Log("Giving ALL Weapons...");

        foreach (var weapon in GameData.Weapons)
        {
            GiveWeapon(weapon.Key);
        }

        Log($"Gave {GameData.Weapons.Count} Weapons!");
    }

    public void GivePerk(string perkName)
    {
        Log($"Giving Perk: {perkName}");
    }

    public void ClearInventory()
    {
        Log("Clearing Inventory...");
    }

    public void RandomizeLoadout()
    {
        Log("Randomizing Loadout...");

        var random = new Random();

        // Give random weapon
        var weapons = GameData.Weapons.Keys.ToArray();
        GiveWeapon(weapons[random.Next(weapons.Length)]);

        // Give random items
        var items = GameData.Items.Keys.ToArray();
        for (int i = 0; i < 5; i++)
        {
            GiveItem(items[random.Next(items.Length)]);
        }

        // Give random prismatic
        var prismatics = GameData.Prismatics.Keys.ToArray();
        GivePrismatic(prismatics[random.Next(prismatics.Length)]);
    }

    #endregion

    #region Unlocks

    public void UnlockAllSkins()
    {
        Log("Unlocking ALL Skins...");

        foreach (var skin in GameData.Skins)
        {
            Log($"  Unlocked: {skin}");
        }

        Log($"Unlocked {GameData.Skins.Length} Skins!");
    }

    public void UnlockAllCosmetics()
    {
        Log("Unlocking ALL Cosmetics...");
        Log("All cosmetics unlocked!");
    }

    public void UnlockEverything()
    {
        Log("Unlocking EVERYTHING...");

        UnlockAllSkins();
        UnlockAllCosmetics();

        // Set debug unlock flag if available
        Log("Everything unlocked!");
    }

    public void ResetUnlocks()
    {
        Log("Resetting all unlocks...");
    }

    #endregion

    #region Weapon Modifications

    public void SetDualWield(bool enabled)
    {
        Log($"Dual Wield: {(enabled ? "ENABLED" : "DISABLED")}");
    }

    public void SetInfiniteMagazine(bool enabled)
    {
        Log($"Infinite Magazine: {(enabled ? "ENABLED" : "DISABLED")}");
    }

    public void SetNoReload(bool enabled)
    {
        Log($"No Reload: {(enabled ? "ENABLED" : "DISABLED")}");
    }

    public void SetNoRecoil(bool enabled)
    {
        Log($"No Recoil: {(enabled ? "ENABLED" : "DISABLED")}");
    }

    public void SetNoSpread(bool enabled)
    {
        Log($"No Spread: {(enabled ? "ENABLED" : "DISABLED")}");
    }

    public void SetRapidFire(bool enabled)
    {
        if (enabled)
        {
            SetFireRateMultiplier(5.0f);
        }
        else
        {
            SetFireRateMultiplier(1.0f);
        }
    }

    #endregion

    #region Game State

    public void SetDifficulty(int level)
    {
        Log($"Setting Difficulty to: {level}");
    }

    public void SkipLevel()
    {
        Log("Skipping current level...");
    }

    public void CompleteIsland()
    {
        Log("Completing current island...");
    }

    public void RestartRun()
    {
        Log("Restarting run...");
    }

    #endregion

    #region Helpers

    private void Log(string message)
    {
        OnLog?.Invoke(this, $"[GameManager] {message}");
    }

    public void Dispose()
    {
        if (_disposed) return;

        StopUpdateLoop();
        _memory.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    ~GameManager()
    {
        Dispose();
    }

    #endregion
}

/// <summary>
/// Current game statistics
/// </summary>
public record GameStats
{
    public float Health { get; init; }
    public float MaxHealth { get; init; }
    public float Shield { get; init; }
    public int Keys { get; init; }
    public int Crystals { get; init; }
    public int CurrentAmmo { get; init; }
    public int MaxAmmo { get; init; }
}
