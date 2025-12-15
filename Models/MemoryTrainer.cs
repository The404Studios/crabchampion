using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace CrabChampionsSaveEditor.Models
{
    /// <summary>
    /// Memory trainer for live game modification
    /// Allows giving perks, mods, relics, and items during gameplay
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

        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_OPERATION = 0x0008;

        #endregion

        #region Properties

        public Process? GameProcess { get; private set; }
        public IntPtr ProcessHandle { get; private set; }
        public bool IsAttached => GameProcess != null && !GameProcess.HasExited && ProcessHandle != IntPtr.Zero;
        public IntPtr BaseAddress => GameProcess?.MainModule?.BaseAddress ?? IntPtr.Zero;
        public string GameVersion { get; private set; } = "Unknown";

        // Events
        public event EventHandler<string>? StatusChanged;
        public event EventHandler<string>? ErrorOccurred;
        public event EventHandler? GameAttached;
        public event EventHandler? GameDetached;

        #endregion

        #region Known Offsets and Patterns

        // These will need to be updated for each game version
        // AOB (Array of Bytes) patterns for finding pointers
        public static class AOBPatterns
        {
            // Player controller pattern (common UE4 pattern)
            public static readonly string PlayerController = "48 8B 05 ?? ?? ?? ?? 48 8B 88 ?? ?? ?? ?? 48 85 C9 74";

            // Inventory pattern
            public static readonly string Inventory = "48 8B 0D ?? ?? ?? ?? 48 85 C9 74 ?? E8 ?? ?? ?? ?? 48 8B D8";

            // GEngine pattern
            public static readonly string GEngine = "48 8B 05 ?? ?? ?? ?? 48 8B 88 ?? 00 00 00 48 85 C9";

            // UWorld pattern
            public static readonly string UWorld = "48 8B 1D ?? ?? ?? ?? 48 85 DB 74 ?? 41 B0 01";

            // GNames pattern
            public static readonly string GNames = "48 8D 15 ?? ?? ?? ?? EB ?? 48 8D 0D ?? ?? ?? ?? E8";
        }

        // Pointer paths for specific values (offset from base)
        // These need to be found via Cheat Engine for each game version
        public class PointerPath
        {
            public string Name { get; set; } = "";
            public long[] Offsets { get; set; } = Array.Empty<long>();
            public int ValueSize { get; set; } = 4; // bytes
            public bool IsFloat { get; set; } = false;
        }

        // Known pointer paths (to be populated from cheat tables)
        public static readonly Dictionary<string, PointerPath> KnownPointers = new()
        {
            // Player stats
            ["Health"] = new PointerPath { Name = "Health", Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x348 }, IsFloat = true },
            ["MaxHealth"] = new PointerPath { Name = "MaxHealth", Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x34C }, IsFloat = true },
            ["Armor"] = new PointerPath { Name = "Armor", Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x350 }, IsFloat = true },

            // Currency
            ["Crystals"] = new PointerPath { Name = "Crystals", Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x2F0 } },
            ["Keys"] = new PointerPath { Name = "Keys", Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x2F4 } },

            // Weapon
            ["CurrentAmmo"] = new PointerPath { Name = "CurrentAmmo", Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x400 } },
            ["MaxAmmo"] = new PointerPath { Name = "MaxAmmo", Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x404 } },

            // Level
            ["CurrentIsland"] = new PointerPath { Name = "CurrentIsland", Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x280 } },
            ["Wave"] = new PointerPath { Name = "Wave", Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x284 } },
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

        #region Item/Perk Injection

        // This section would require more reverse engineering to implement properly
        // The general approach would be to:
        // 1. Find the inventory/perk array in memory
        // 2. Find how items are stored (likely as UObject pointers)
        // 3. Either:
        //    a) Modify the array to add new items
        //    b) Call the game's native function to add items (code injection)

        /// <summary>
        /// Attempts to give an item by finding and modifying inventory
        /// This is a placeholder - actual implementation requires specific offsets
        /// </summary>
        public bool GiveItem(string itemId)
        {
            StatusChanged?.Invoke(this, $"Attempting to give item: {itemId}");

            // TODO: Implement actual item injection
            // This requires:
            // 1. Finding the inventory component address
            // 2. Understanding the item data structure
            // 3. Adding the item to the inventory array

            ErrorOccurred?.Invoke(this, "Item injection not yet implemented. Use Cheat Engine table for now.");
            return false;
        }

        /// <summary>
        /// Attempts to give a perk
        /// </summary>
        public bool GivePerk(string perkId)
        {
            StatusChanged?.Invoke(this, $"Attempting to give perk: {perkId}");

            // TODO: Implement perk injection
            ErrorOccurred?.Invoke(this, "Perk injection not yet implemented. Use Cheat Engine table for now.");
            return false;
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
