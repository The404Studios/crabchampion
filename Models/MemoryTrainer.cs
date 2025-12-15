using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace CrabChampionsSaveEditor.Models
{
    /// <summary>
    /// Memory trainer for live game modification of Crab Champions.
    /// Provides utilities for reading/writing game memory and injecting items.
    ///
    /// ARCHITECTURE NOTES:
    /// - Crab Champions is built on Unreal Engine 4
    /// - Game uses UE4 reflection system for enums (ECrabPerkType, ECrabRank, etc.)
    /// - Items are stored as UObject pointers in TArray structures
    /// - Perks/Mods/Relics use data asset references (DA_Perk_*, DA_WeaponMod_*, etc.)
    /// </summary>
    public class MemoryTrainer : IDisposable
    {
        #region Win32 API Imports

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_OPERATION = 0x0008;
        private const uint MEM_COMMIT = 0x1000;
        private const uint PAGE_READWRITE = 0x04;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        #endregion

        #region Properties

        public Process? GameProcess { get; private set; }
        public IntPtr ProcessHandle { get; private set; }
        public bool IsAttached => GameProcess != null && !GameProcess.HasExited && ProcessHandle != IntPtr.Zero;
        public IntPtr BaseAddress => GameProcess?.MainModule?.BaseAddress ?? IntPtr.Zero;
        public string GameVersion { get; private set; } = "Unknown";

        // Cached addresses (found during initialization)
        public IntPtr GEngineAddress { get; private set; }
        public IntPtr UWorldAddress { get; private set; }
        public IntPtr GNamesAddress { get; private set; }
        public IntPtr PlayerControllerAddress { get; private set; }

        // Events
        public event EventHandler<string>? StatusChanged;
        public event EventHandler<string>? ErrorOccurred;
        public event EventHandler? GameAttached;
        public event EventHandler? GameDetached;

        #endregion

        #region UE4 Enum Definitions (from Reverse Engineering)

        /// <summary>
        /// ECrabPerkType enum values from game binary (.rdata section)
        /// These are used by UE4 reflection system for perk identification
        /// </summary>
        public static class ECrabPerkType
        {
            // Common Perks
            public const string Mango = "ECrabPerkType::Mango";
            public const string Banana = "ECrabPerkType::Banana";
            public const string GlassCannon = "ECrabPerkType::GlassCannon";
            public const string Juggernaut = "ECrabPerkType::Juggernaut";
            public const string SpeedDemon = "ECrabPerkType::SpeedDemon";
            public const string Regenerator = "ECrabPerkType::Regenerator";
            public const string Bulletproof = "ECrabPerkType::Bulletproof";
            public const string Sharpshooter = "ECrabPerkType::Sharpshooter";
            public const string HeavyHitter = "ECrabPerkType::HeavyHitter";
            public const string Firestarter = "ECrabPerkType::Firestarter";
            public const string IceCold = "ECrabPerkType::IceCold";
            public const string HighVoltage = "ECrabPerkType::HighVoltage";
            public const string Toxic = "ECrabPerkType::Toxic";
            public const string PotentMagic = "ECrabPerkType::PotentMagic";
            public const string Fortitude = "ECrabPerkType::Fortitude";
            public const string Vitality = "ECrabPerkType::Vitality";
            public const string Endurance = "ECrabPerkType::Endurance";
            public const string Stamina = "ECrabPerkType::Stamina";

            // Epic Perks
            public const string MegaCrit = "ECrabPerkType::MegaCrit";
            public const string Assassin = "ECrabPerkType::Assassin";
            public const string Survivor = "ECrabPerkType::Survivor";
            public const string Collector = "ECrabPerkType::Collector";
            public const string DoubleVision = "ECrabPerkType::DoubleVision";
            public const string ExplodingEnemies = "ECrabPerkType::ExplodingEnemies";
            public const string HealthIsPower = "ECrabPerkType::HealthIsPower";
            public const string MoneyIsPower = "ECrabPerkType::MoneyIsPower";
            public const string SpeedIsPower = "ECrabPerkType::SpeedIsPower";

            // Legendary Perks
            public const string DaggerDash = "ECrabPerkType::DaggerDash";
            public const string IceDash = "ECrabPerkType::IceDash";
            public const string LightningDash = "ECrabPerkType::LightningDash";
            public const string Powerslide = "ECrabPerkType::Powerslide";
            public const string FlammableEnemies = "ECrabPerkType::FlammableEnemies";
            public const string FreezingEnemies = "ECrabPerkType::FreezingEnemies";
            public const string PoisonousEnemies = "ECrabPerkType::PoisonousEnemies";

            // Get all perk type strings for scanning
            public static readonly string[] AllValues = new[]
            {
                Mango, Banana, GlassCannon, Juggernaut, SpeedDemon, Regenerator, Bulletproof,
                Sharpshooter, HeavyHitter, Firestarter, IceCold, HighVoltage, Toxic, PotentMagic,
                Fortitude, Vitality, Endurance, Stamina, MegaCrit, Assassin, Survivor, Collector,
                DoubleVision, ExplodingEnemies, HealthIsPower, MoneyIsPower, SpeedIsPower,
                DaggerDash, IceDash, LightningDash, Powerslide, FlammableEnemies, FreezingEnemies, PoisonousEnemies
            };
        }

        /// <summary>
        /// ECrabRank enum values for weapon/item rarity
        /// </summary>
        public static class ECrabRank
        {
            public const string Common = "ECrabRank::Common";
            public const string Uncommon = "ECrabRank::Uncommon";
            public const string Rare = "ECrabRank::Rare";
            public const string Epic = "ECrabRank::Epic";
            public const string Legendary = "ECrabRank::Legendary";
            public const string Prismatic = "ECrabRank::Prismatic";

            public static readonly string[] AllValues = new[]
            {
                Common, Uncommon, Rare, Epic, Legendary, Prismatic
            };
        }

        /// <summary>
        /// ECrabCosmeticType enum values for cosmetic items
        /// </summary>
        public static class ECrabCosmeticType
        {
            public const string CrabSkin = "ECrabCosmeticType::CrabSkin";
            public const string WeaponSkin = "ECrabCosmeticType::WeaponSkin";
            public const string Emote = "ECrabCosmeticType::Emote";
            public const string Banner = "ECrabCosmeticType::Banner";
            public const string Title = "ECrabCosmeticType::Title";
        }

        /// <summary>
        /// ECrabTurretType enum values for turret types (from .rdata:0x14328DA00)
        /// </summary>
        public static class ECrabTurretType
        {
            public const string None = "ECrabTurretType::None";
            public const string Sentry = "ECrabTurretType::Sentry";
            public const string Sniper = "ECrabTurretType::Sniper";
            public const string Mortar = "ECrabTurretType::Mortar";
            public const string Wave = "ECrabTurretType::Wave";
            public const string Beam = "ECrabTurretType::Beam";

            public static readonly string[] AllValues = new[]
            {
                None, Sentry, Sniper, Mortar, Wave, Beam
            };
        }

        /// <summary>
        /// UE4 UClass names for game objects (from .rdata section)
        /// These are the internal class names used by UE4 reflection
        /// </summary>
        public static class UE4ClassNames
        {
            // Item classes (UTF-16LE in binary)
            public const string CrabPerk = "CrabPerk";               // 0x14328DAC8
            public const string CrabMeleeMod = "CrabMeleeMod";       // 0x14328DAD8 (UTF-16)
            public const string CrabAbilityMod = "CrabAbilityMod";   // 0x14328DB88 (UTF-16)
            public const string CrabWeaponMod = "CrabWeaponMod";     // 0x14328DC38 (UTF-16)
            public const string CrabInventoryCooldown = "CrabInventoryCooldown"; // 0x14328DCE8

            // Data asset classes
            public const string PerkDA = "PerkDA";                   // 0x14328DAC0
            public const string MeleeModDA = "MeleeModDA";           // 0x14328DB68
            public const string AbilityModDA = "AbilityModDA";       // 0x14328DC18
            public const string WeaponModDA = "WeaponModDA";         // 0x14328DCC8
            public const string InventoryDA = "InventoryDA";         // 0x14328DD18
        }

        /// <summary>
        /// UE4 property names used in save/memory (for FName lookups)
        /// </summary>
        public static class UE4PropertyNames
        {
            // Unlock arrays (save file)
            public const string UnlockedPerks = "UnlockedPerks";
            public const string UnlockedWeapons = "UnlockedWeapons";
            public const string UnlockedAbilities = "UnlockedAbilities";
            public const string UnlockedMeleeWeapons = "UnlockedMeleeWeapons";
            public const string UnlockedWeaponMods = "UnlockedWeaponMods";
            public const string UnlockedAbilityMods = "UnlockedAbilityMods";
            public const string UnlockedMeleeMods = "UnlockedMeleeMods";
            public const string UnlockedRelics = "UnlockedRelics";
            public const string RankedWeapons = "RankedWeapons";
            public const string CrabCosmetics = "CrabCosmetics";

            // Item properties (from .rdata)
            public const string InventoryInfo = "InventoryInfo";     // 0x14328DB28 - links items to inventory
            public const string CurrentCooldown = "CurrentCooldown"; // 0x14328DD88
            public const string UnderlyingType = "UnderlyingType";   // 0x14328DDD8
        }

        /// <summary>
        /// Known function addresses from RE (version-specific)
        /// These are example addresses - actual values depend on game version
        /// </summary>
        public static class KnownFunctions
        {
            // Item-related functions (offsets from base)
            public const long MeleeModHandler = 0x140D7A300;     // sub_140D7A300
            public const long AbilityModHandler = 0x140D6F370;   // sub_140D6F370
            public const long WeaponModHandler = 0x140D8A470;    // sub_140D8A470
            public const long InventoryHandler = 0x140D85150;    // sub_140D85150 (shared)
            public const long CooldownHandler = 0x140D789C0;     // sub_140D789C0

            // =============================================
            // SERVER RPC FUNCTIONS (Client→Server)
            // These are the key functions for item injection!
            // =============================================

            // Set Data Asset functions (for giving items)
            public const long ServerSetWeaponDA = 0x140D809C0;      // Set weapon data asset
            public const long ServerSetAbilityDA = 0x140D80960;     // Set ability data asset
            public const long ServerSetMeleeDA = 0x140D80990;       // Set melee data asset

            // Equip functions
            public const long ServerEquipInventory = 0x140D807E0;   // Equip from inventory
            public const long ServerEquipCosmetics = 0x140D807B0;   // Equip cosmetics

            // Remove item functions
            public const long ServerRemoveWeaponMod = 0x140D80930;
            public const long ServerRemoveAbilityMod = 0x140D80870;
            public const long ServerRemoveMeleeMod = 0x140D808A0;
            public const long ServerRemovePerk = 0x140D808D0;
            public const long ServerRemoveRelic = 0x140D80900;

            // Account/progression functions
            public const long ServerRefreshAccount = 0x140D80840;
            public const long ServerIncrementNumInventorySlots = 0x140D80810;

            // =============================================
            // ONREP CALLBACKS (Replication notifications)
            // Called when replicated properties change
            // =============================================
            public const long OnRep_Inventory = 0x140D80690;
            public const long OnRep_Crystals = 0x140D80600;
            public const long OnRep_Keys = 0x140D806F0;
            public const long OnRep_WeaponDA = 0x140D80780;
            public const long OnRep_AbilityDA = 0x140D80540;
            public const long OnRep_MeleeDA = 0x140D80720;
            public const long OnRep_Combo = 0x140D805D0;
            public const long OnRep_Eliminations = 0x140D80660;
            public const long OnRep_AccountLevel = 0x140D80570;
            public const long OnRep_AccountRank = 0x140D805A0;
            public const long OnRep_DamageTakenOnThisIsland = 0x140D80630;
            public const long OnRep_IslandRewardRarity = 0x140D806C0;
            public const long OnRep_ScaleMultiplier = 0x140D80750;
        }

        #endregion

        #region AOB Patterns and Pointer Paths

        /// <summary>
        /// AOB (Array of Bytes) patterns for finding game structures
        /// These are version-specific and may need updating
        /// </summary>
        public static class AOBPatterns
        {
            // Core UE4 pointers
            public const string GEngine = "48 8B 05 ?? ?? ?? ?? 48 8B 88 ?? 00 00 00 48 85 C9";
            public const string UWorld = "48 8B 1D ?? ?? ?? ?? 48 85 DB 74 ?? 41 B0 01";
            public const string GNames = "48 8D 15 ?? ?? ?? ?? EB ?? 48 8D 0D ?? ?? ?? ?? E8";
            public const string PlayerController = "48 8B 05 ?? ?? ?? ?? 48 8B 88 ?? ?? ?? ?? 48 85 C9 74";

            // Game-specific patterns
            public const string Inventory = "48 8B 0D ?? ?? ?? ?? 48 85 C9 74 ?? E8 ?? ?? ?? ?? 48 8B D8";
            public const string PerkManager = "48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B F8 48 85 C0";

            // Health/Armor patterns (float comparison)
            public const string Health = "F3 0F 10 ?? ?? ?? 00 00 0F 2F ?? F3 0F 10";
            public const string Ammo = "89 ?? ?? 00 00 00 83 ?? ?? 00 00 00 00 7E";

            // String patterns for finding enum reflection data
            public const string ECrabPerkTypeString = "45 43 72 61 62 50 65 72 6B 54 79 70 65"; // "ECrabPerkType"
            public const string ECrabRankString = "45 43 72 61 62 52 61 6E 6B"; // "ECrabRank"
        }

        /// <summary>
        /// Pointer path for traversing UE4 object hierarchy
        /// </summary>
        public class PointerPath
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public long[] Offsets { get; set; } = Array.Empty<long>();
            public int ValueSize { get; set; } = 4;
            public bool IsFloat { get; set; }
            public bool IsPointer { get; set; }
        }

        /// <summary>
        /// Known pointer paths - these are version-specific examples
        /// Use FindPointerPath() to discover correct paths for current version
        /// </summary>
        public static readonly Dictionary<string, PointerPath> KnownPointers = new()
        {
            // Player stats (example paths - may need updating)
            ["Health"] = new PointerPath
            {
                Name = "Health",
                Description = "Player current health (float)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x348 },
                IsFloat = true
            },
            ["MaxHealth"] = new PointerPath
            {
                Name = "MaxHealth",
                Description = "Player max health (float)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x34C },
                IsFloat = true
            },
            ["Armor"] = new PointerPath
            {
                Name = "Armor",
                Description = "Player armor value (float)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x350 },
                IsFloat = true
            },

            // Currency
            ["Crystals"] = new PointerPath
            {
                Name = "Crystals",
                Description = "Current crystal count (int32)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x2F0 }
            },
            ["Keys"] = new PointerPath
            {
                Name = "Keys",
                Description = "Current key count (int32)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x2F4 }
            },

            // Weapon
            ["CurrentAmmo"] = new PointerPath
            {
                Name = "CurrentAmmo",
                Description = "Current weapon ammo (int32)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x400 }
            },
            ["MaxAmmo"] = new PointerPath
            {
                Name = "MaxAmmo",
                Description = "Max weapon ammo (int32)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x404 }
            },

            // Level/Progress
            ["CurrentIsland"] = new PointerPath
            {
                Name = "CurrentIsland",
                Description = "Current island number (int32)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x280 }
            },
            ["Wave"] = new PointerPath
            {
                Name = "Wave",
                Description = "Current wave number (int32)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x284 }
            },
        };

        #endregion

        #region Attach/Detach

        /// <summary>
        /// Find and attach to the Crab Champions process
        /// </summary>
        public bool AttachToGame()
        {
            try
            {
                // Try common process names
                string[] processNames = { "CrabChampions", "CrabChampions-Win64-Shipping", "CrabChampions-Win64" };

                foreach (var name in processNames)
                {
                    var processes = Process.GetProcessesByName(name);
                    if (processes.Length > 0)
                    {
                        GameProcess = processes[0];
                        ProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, false, GameProcess.Id);

                        if (ProcessHandle != IntPtr.Zero)
                        {
                            // Try to get game version from file
                            try
                            {
                                var fileInfo = GameProcess.MainModule?.FileVersionInfo;
                                GameVersion = fileInfo?.FileVersion ?? "Unknown";
                            }
                            catch
                            {
                                GameVersion = "Unknown";
                            }

                            StatusChanged?.Invoke(this, $"Attached to {name} (PID: {GameProcess.Id})");
                            GameAttached?.Invoke(this, EventArgs.Empty);
                            return true;
                        }
                    }
                }

                ErrorOccurred?.Invoke(this, "Could not find Crab Champions process. Make sure the game is running.");
                return false;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to attach: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Detach from the game process
        /// </summary>
        public void Detach()
        {
            if (ProcessHandle != IntPtr.Zero)
            {
                CloseHandle(ProcessHandle);
                ProcessHandle = IntPtr.Zero;
            }
            GameProcess = null;
            StatusChanged?.Invoke(this, "Detached from game");
            GameDetached?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Memory Read/Write

        /// <summary>
        /// Read bytes from process memory
        /// </summary>
        public byte[]? ReadMemory(IntPtr address, int size)
        {
            if (!IsAttached) return null;

            byte[] buffer = new byte[size];
            if (ReadProcessMemory(ProcessHandle, address, buffer, size, out int bytesRead))
            {
                return bytesRead == size ? buffer : null;
            }
            return null;
        }

        /// <summary>
        /// Write bytes to process memory
        /// </summary>
        public bool WriteMemory(IntPtr address, byte[] data)
        {
            if (!IsAttached) return false;

            // Change memory protection if needed
            VirtualProtectEx(ProcessHandle, address, (UIntPtr)data.Length, 0x40, out uint oldProtect);

            bool result = WriteProcessMemory(ProcessHandle, address, data, data.Length, out _);

            // Restore protection
            VirtualProtectEx(ProcessHandle, address, (UIntPtr)data.Length, oldProtect, out _);

            return result;
        }

        /// <summary>
        /// Read an integer value
        /// </summary>
        public int ReadInt32(IntPtr address)
        {
            var data = ReadMemory(address, 4);
            return data != null ? BitConverter.ToInt32(data, 0) : 0;
        }

        /// <summary>
        /// Read a float value
        /// </summary>
        public float ReadFloat(IntPtr address)
        {
            var data = ReadMemory(address, 4);
            return data != null ? BitConverter.ToSingle(data, 0) : 0f;
        }

        /// <summary>
        /// Read a 64-bit pointer
        /// </summary>
        public long ReadInt64(IntPtr address)
        {
            var data = ReadMemory(address, 8);
            return data != null ? BitConverter.ToInt64(data, 0) : 0;
        }

        /// <summary>
        /// Write an integer value
        /// </summary>
        public bool WriteInt32(IntPtr address, int value)
        {
            return WriteMemory(address, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Write a float value
        /// </summary>
        public bool WriteFloat(IntPtr address, float value)
        {
            return WriteMemory(address, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Follow a pointer path to get the final address
        /// </summary>
        public IntPtr ResolvePointerPath(PointerPath path)
        {
            if (!IsAttached || path.Offsets.Length == 0) return IntPtr.Zero;

            try
            {
                IntPtr currentAddress = BaseAddress + (int)path.Offsets[0];

                for (int i = 1; i < path.Offsets.Length; i++)
                {
                    long value = ReadInt64(currentAddress);
                    if (value == 0) return IntPtr.Zero;
                    currentAddress = new IntPtr(value + path.Offsets[i]);
                }

                return currentAddress;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Read a value using a pointer path
        /// </summary>
        public object? ReadValue(string pointerName)
        {
            if (!KnownPointers.TryGetValue(pointerName, out var path))
                return null;

            var address = ResolvePointerPath(path);
            if (address == IntPtr.Zero) return null;

            return path.IsFloat ? (object)ReadFloat(address) : (object)ReadInt32(address);
        }

        /// <summary>
        /// Write a value using a pointer path
        /// </summary>
        public bool WriteValue(string pointerName, object value)
        {
            if (!KnownPointers.TryGetValue(pointerName, out var path))
                return false;

            var address = ResolvePointerPath(path);
            if (address == IntPtr.Zero) return false;

            if (path.IsFloat)
                return WriteFloat(address, Convert.ToSingle(value));
            else
                return WriteInt32(address, Convert.ToInt32(value));
        }

        #endregion

        #region AOB Scanning

        /// <summary>
        /// Convert pattern string to byte array with wildcards
        /// </summary>
        private static (byte[] pattern, bool[] mask) ParsePattern(string patternString)
        {
            var parts = patternString.Split(' ');
            var pattern = new byte[parts.Length];
            var mask = new bool[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "??" || parts[i] == "?")
                {
                    pattern[i] = 0;
                    mask[i] = false;
                }
                else
                {
                    pattern[i] = Convert.ToByte(parts[i], 16);
                    mask[i] = true;
                }
            }

            return (pattern, mask);
        }

        /// <summary>
        /// Scan memory for a pattern
        /// </summary>
        public IntPtr AOBScan(string patternString, IntPtr startAddress, int scanSize = 0x10000000)
        {
            if (!IsAttached) return IntPtr.Zero;

            var (pattern, mask) = ParsePattern(patternString);

            // Read memory in chunks
            const int chunkSize = 0x10000;
            byte[] buffer = new byte[chunkSize + pattern.Length];

            for (long offset = 0; offset < scanSize; offset += chunkSize)
            {
                IntPtr currentAddress = IntPtr.Add(startAddress, (int)offset);
                var data = ReadMemory(currentAddress, buffer.Length);
                if (data == null) continue;

                // Search for pattern in chunk
                for (int i = 0; i < chunkSize; i++)
                {
                    bool found = true;
                    for (int j = 0; j < pattern.Length && found; j++)
                    {
                        if (mask[j] && data[i + j] != pattern[j])
                            found = false;
                    }

                    if (found)
                    {
                        return IntPtr.Add(currentAddress, i);
                    }
                }
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Scan for a pattern in the main module
        /// </summary>
        public IntPtr ScanMainModule(string patternString)
        {
            if (!IsAttached || GameProcess?.MainModule == null) return IntPtr.Zero;
            return AOBScan(patternString, BaseAddress, GameProcess.MainModule.ModuleMemorySize);
        }

        #endregion

        #region Cheat Functions

        /// <summary>
        /// Set player health to maximum
        /// </summary>
        public bool SetMaxHealth()
        {
            var maxHealth = ReadValue("MaxHealth");
            if (maxHealth != null)
            {
                return WriteValue("Health", maxHealth);
            }
            return false;
        }

        /// <summary>
        /// Set infinite health (godmode)
        /// </summary>
        public bool SetGodMode(bool enabled)
        {
            if (enabled)
            {
                // Set health to very high value
                return WriteValue("Health", 999999f) && WriteValue("MaxHealth", 999999f);
            }
            else
            {
                // Reset to normal
                return WriteValue("Health", 100f) && WriteValue("MaxHealth", 100f);
            }
        }

        /// <summary>
        /// Set currency values
        /// </summary>
        public bool SetCurrency(int crystals, int keys)
        {
            bool success = true;
            if (crystals >= 0) success &= WriteValue("Crystals", crystals);
            if (keys >= 0) success &= WriteValue("Keys", keys);
            return success;
        }

        /// <summary>
        /// Set infinite ammo
        /// </summary>
        public bool SetInfiniteAmmo(bool enabled)
        {
            if (enabled)
            {
                return WriteValue("CurrentAmmo", 999) && WriteValue("MaxAmmo", 999);
            }
            return true;
        }

        /// <summary>
        /// Get current player stats
        /// </summary>
        public Dictionary<string, object?> GetCurrentStats()
        {
            var stats = new Dictionary<string, object?>();

            foreach (var pointer in KnownPointers)
            {
                stats[pointer.Key] = ReadValue(pointer.Key);
            }

            return stats;
        }

        #endregion

        #region String Scanning (for UE4 Reflection Data)

        /// <summary>
        /// Scan memory for a string pattern (useful for finding enum reflection data)
        /// </summary>
        public List<IntPtr> ScanForString(string searchString, bool unicode = false)
        {
            var results = new List<IntPtr>();
            if (!IsAttached) return results;

            byte[] pattern = unicode
                ? Encoding.Unicode.GetBytes(searchString)
                : Encoding.ASCII.GetBytes(searchString);

            // Scan readable memory regions
            IntPtr address = IntPtr.Zero;
            IntPtr maxAddress = new IntPtr(0x7FFFFFFF0000); // User-space limit

            while (address.ToInt64() < maxAddress.ToInt64())
            {
                if (!VirtualQueryEx(ProcessHandle, address, out var memInfo, (uint)Marshal.SizeOf<MEMORY_BASIC_INFORMATION>()))
                    break;

                // Only scan committed, readable memory
                if (memInfo.State == MEM_COMMIT &&
                    (memInfo.Protect == PAGE_READWRITE || memInfo.Protect == PAGE_EXECUTE_READWRITE))
                {
                    var regionSize = (int)memInfo.RegionSize.ToInt64();
                    if (regionSize > 0 && regionSize < 0x10000000) // Reasonable size limit
                    {
                        var buffer = ReadMemory(memInfo.BaseAddress, regionSize);
                        if (buffer != null)
                        {
                            for (int i = 0; i <= buffer.Length - pattern.Length; i++)
                            {
                                bool match = true;
                                for (int j = 0; j < pattern.Length && match; j++)
                                {
                                    if (buffer[i + j] != pattern[j])
                                        match = false;
                                }
                                if (match)
                                {
                                    results.Add(IntPtr.Add(memInfo.BaseAddress, i));
                                }
                            }
                        }
                    }
                }

                // Move to next region
                address = IntPtr.Add(memInfo.BaseAddress, (int)memInfo.RegionSize.ToInt64());
            }

            return results;
        }

        /// <summary>
        /// Find the address of an ECrabPerkType enum value in memory
        /// </summary>
        public IntPtr FindPerkTypeAddress(string perkType)
        {
            // The enum values are stored as strings in .rdata section
            // e.g., "ECrabPerkType::GlassCannon"
            var addresses = ScanForString(perkType);
            return addresses.FirstOrDefault();
        }

        /// <summary>
        /// Find all perk type string addresses in memory (for debugging/analysis)
        /// </summary>
        public Dictionary<string, IntPtr> FindAllPerkTypeAddresses()
        {
            var results = new Dictionary<string, IntPtr>();

            foreach (var perkType in ECrabPerkType.AllValues)
            {
                var addr = FindPerkTypeAddress(perkType);
                if (addr != IntPtr.Zero)
                {
                    results[perkType] = addr;
                }
            }

            return results;
        }

        /// <summary>
        /// Find UE4 FName for a property by scanning for the string
        /// </summary>
        public IntPtr FindFNameAddress(string propertyName)
        {
            var addresses = ScanForString(propertyName);
            return addresses.FirstOrDefault();
        }

        #endregion

        #region Item/Perk Injection

        /// <summary>
        /// ITEM INJECTION ARCHITECTURE:
        ///
        /// In Crab Champions (UE4), items are stored as:
        /// 1. TArray of UObject* pointers in the player's inventory component
        /// 2. Each item is a UDataAsset (DA_Perk_*, DA_WeaponMod_*, etc.)
        ///
        /// To add items at runtime, you need to either:
        /// A) Find and call the game's native AddPerk/AddItem function (safest)
        /// B) Manually add a UObject pointer to the TArray (risky - may crash)
        ///
        /// Method A requires:
        /// - Finding the function address via AOB or export table
        /// - Setting up proper calling convention (x64 fastcall)
        /// - Creating a remote thread to call the function
        ///
        /// Method B requires:
        /// - Finding the TArray structure (Data pointer, Count, Max)
        /// - Allocating memory for new pointer if array is full
        /// - Writing the UObject* for the item to add
        ///
        /// The ECrabPerkType enum is used by the game to identify perks.
        /// Set a breakpoint on these strings to find the perk granting code.
        /// </summary>

        /// <summary>
        /// Information about an injectable item
        /// </summary>
        public class InjectableItem
        {
            public string Name { get; set; } = "";
            public string AssetPath { get; set; } = "";
            public string EnumValue { get; set; } = "";
            public IntPtr CachedAddress { get; set; }
        }

        // Cache of found item addresses
        private readonly Dictionary<string, InjectableItem> _itemCache = new();

        /// <summary>
        /// Attempts to give a perk to the player.
        /// Requires offsets to be configured for the current game version.
        /// </summary>
        /// <param name="perkId">The perk ID (e.g., "GlassCannon")</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool GivePerk(string perkId)
        {
            if (!IsAttached)
            {
                ErrorOccurred?.Invoke(this, "Not attached to game");
                return false;
            }

            StatusChanged?.Invoke(this, $"Attempting to give perk: {perkId}");

            // Build the enum value string
            string enumValue = $"ECrabPerkType::{perkId}";

            // Step 1: Find where this perk type is referenced
            var perkTypeAddr = FindPerkTypeAddress(enumValue);
            if (perkTypeAddr == IntPtr.Zero)
            {
                StatusChanged?.Invoke(this, $"Could not find {enumValue} in memory. The game may need to be in a run.");
                ErrorOccurred?.Invoke(this, $"Perk type string '{enumValue}' not found in memory");
                return false;
            }

            StatusChanged?.Invoke(this, $"Found {enumValue} at {perkTypeAddr:X}");

            // Step 2: This is where you would:
            // - Find the perk manager component
            // - Find the AddPerk function
            // - Call it with the perk type
            //
            // For now, we just report what we found and recommend using Cheat Engine

            StatusChanged?.Invoke(this, $"Perk injection requires calling game functions. Set breakpoint on {perkTypeAddr:X} to find AddPerk function.");
            ErrorOccurred?.Invoke(this, "Full perk injection not implemented. Use the discovered address with Cheat Engine.");

            return false;
        }

        /// <summary>
        /// Attempts to give an item (weapon mod, ability mod, melee mod, or relic)
        /// </summary>
        public bool GiveItem(string itemType, string itemId)
        {
            if (!IsAttached)
            {
                ErrorOccurred?.Invoke(this, "Not attached to game");
                return false;
            }

            StatusChanged?.Invoke(this, $"Attempting to give {itemType}: {itemId}");

            // Build the asset path based on item type
            string assetPath = itemType.ToLower() switch
            {
                "weaponmod" => $"/Game/Blueprint/Pickup/WeaponMod/DA_WeaponMod_{itemId}",
                "abilitymod" => $"/Game/Blueprint/Pickup/AbilityMod/DA_AbilityMod_{itemId}",
                "meleemod" => $"/Game/Blueprint/Pickup/MeleeMod/DA_MeleeMod_{itemId}",
                "relic" => $"/Game/Blueprint/Pickup/Relic/DA_Relic_{itemId}",
                "perk" => $"/Game/Blueprint/Pickup/Perk/DA_Perk_{itemId}",
                _ => $"/Game/Blueprint/Pickup/{itemType}/DA_{itemType}_{itemId}"
            };

            // Try to find this asset path in memory
            var assetAddresses = ScanForString(assetPath);

            if (assetAddresses.Count == 0)
            {
                StatusChanged?.Invoke(this, $"Asset path not found in memory. Item may not be loaded.");
                ErrorOccurred?.Invoke(this, $"Could not find '{assetPath}' - try picking up the item type first");
                return false;
            }

            StatusChanged?.Invoke(this, $"Found {assetAddresses.Count} references to {itemId}");

            // The actual injection would require finding and modifying the inventory TArray
            ErrorOccurred?.Invoke(this, "Item injection not fully implemented. Use Cheat Engine with discovered addresses.");
            return false;
        }

        /// <summary>
        /// Simplified give item overload
        /// </summary>
        public bool GiveItem(string fullItemPath)
        {
            return GiveItem("generic", fullItemPath);
        }

        /// <summary>
        /// Get information about what's needed to implement full item injection
        /// </summary>
        public string GetInjectionGuide()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== ITEM INJECTION GUIDE ===\n");

            sb.AppendLine("To implement full item injection, you need:\n");

            sb.AppendLine("1. FIND THE PERK/ITEM MANAGER:");
            sb.AppendLine("   - Scan for 'UnlockedPerks' FName");
            sb.AppendLine("   - Set read breakpoint to find the component");
            sb.AppendLine("   - The component contains TArray<UPerk*> for active perks\n");

            sb.AppendLine("2. FIND THE ADD FUNCTION:");
            sb.AppendLine("   - Look for 'ECrabPerkType::' string references");
            sb.AppendLine("   - Set breakpoint when picking up a perk");
            sb.AppendLine("   - Trace back to find AddPerk/GivePerk function\n");

            sb.AppendLine("3. CALL THE FUNCTION:");
            sb.AppendLine("   - Get function address from step 2");
            sb.AppendLine("   - Use CreateRemoteThread to call it");
            sb.AppendLine("   - Pass the ECrabPerkType enum value as parameter\n");

            sb.AppendLine("DISCOVERED ADDRESSES:");
            if (IsAttached)
            {
                // Try to find some useful addresses
                var perkTypeBase = ScanForString("ECrabPerkType");
                if (perkTypeBase.Count > 0)
                    sb.AppendLine($"   ECrabPerkType enum base: {perkTypeBase[0]:X}");

                var unlockedPerks = ScanForString("UnlockedPerks");
                if (unlockedPerks.Count > 0)
                    sb.AppendLine($"   UnlockedPerks FName: {unlockedPerks[0]:X}");
            }
            else
            {
                sb.AppendLine("   (attach to game first)");
            }

            return sb.ToString();
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Read a null-terminated string from memory
        /// </summary>
        public string ReadString(IntPtr address, int maxLength = 256)
        {
            var data = ReadMemory(address, maxLength);
            if (data == null) return "";

            int nullIndex = Array.IndexOf(data, (byte)0);
            if (nullIndex < 0) nullIndex = maxLength;

            return Encoding.UTF8.GetString(data, 0, nullIndex);
        }

        /// <summary>
        /// Read a wide (unicode) string from memory
        /// </summary>
        public string ReadWideString(IntPtr address, int maxLength = 256)
        {
            var data = ReadMemory(address, maxLength * 2);
            if (data == null) return "";

            int nullIndex = -1;
            for (int i = 0; i < data.Length - 1; i += 2)
            {
                if (data[i] == 0 && data[i + 1] == 0)
                {
                    nullIndex = i;
                    break;
                }
            }
            if (nullIndex < 0) nullIndex = maxLength * 2;

            return Encoding.Unicode.GetString(data, 0, nullIndex);
        }

        /// <summary>
        /// Dump memory region for analysis
        /// </summary>
        public string DumpMemory(IntPtr address, int size)
        {
            var data = ReadMemory(address, size);
            if (data == null) return "Failed to read memory";

            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i += 16)
            {
                sb.Append($"{(address.ToInt64() + i):X16}: ");

                // Hex
                for (int j = 0; j < 16 && i + j < data.Length; j++)
                {
                    sb.Append($"{data[i + j]:X2} ");
                }

                // ASCII
                sb.Append(" | ");
                for (int j = 0; j < 16 && i + j < data.Length; j++)
                {
                    char c = (char)data[i + j];
                    sb.Append(char.IsControl(c) ? '.' : c);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        #endregion

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Detach();
            }

            _disposed = true;
        }

        ~MemoryTrainer()
        {
            Dispose(false);
        }

        #endregion
    }

    /// <summary>
    /// Simple trainer settings
    /// </summary>
    public class TrainerSettings
    {
        public bool GodMode { get; set; }
        public bool InfiniteAmmo { get; set; }
        public bool InfiniteCurrency { get; set; }
        public int CrystalAmount { get; set; } = 999999;
        public int KeyAmount { get; set; } = 999;
    }
}
