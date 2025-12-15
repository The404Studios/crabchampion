using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrabTrainer.Memory;

namespace CrabTrainer.Cheats
{
    /// <summary>
    /// Handles finding and injecting perks using known perk data from save files.
    ///
    /// Strategy:
    /// 1. We know perk IDs/names from save file analysis
    /// 2. Scan memory for these known strings
    /// 3. Find the perk array/manager structure
    /// 4. Add or modify perk entries
    /// </summary>
    public class PerkInjector
    {
        private readonly MemoryManager _memory;

        // Known perk IDs from save file / game data
        // These are the internal names used by the game
        public static readonly PerkData[] KnownPerks = new PerkData[]
        {
            // Damage Perks
            new("DamageUp", "Damage Up", PerkCategory.Damage, 1.1f),
            new("CritChance", "Critical Chance", PerkCategory.Damage, 0.05f),
            new("CritDamage", "Critical Damage", PerkCategory.Damage, 0.25f),
            new("AttackSpeed", "Attack Speed", PerkCategory.Damage, 1.1f),
            new("Multishot", "Multishot", PerkCategory.Damage, 0.15f),
            new("Piercing", "Piercing", PerkCategory.Damage, 1f),
            new("ExplosiveRounds", "Explosive Rounds", PerkCategory.Damage, 1f),
            new("ChainLightning", "Chain Lightning", PerkCategory.Damage, 1f),
            new("BurnDamage", "Burn", PerkCategory.Damage, 1f),
            new("FreezeDamage", "Freeze", PerkCategory.Damage, 1f),
            new("PoisonDamage", "Poison", PerkCategory.Damage, 1f),
            new("BleedDamage", "Bleed", PerkCategory.Damage, 1f),
            new("Ricochet", "Ricochet", PerkCategory.Damage, 1f),
            new("Homing", "Homing", PerkCategory.Damage, 1f),

            // Defense Perks
            new("MaxHealth", "Max Health", PerkCategory.Defense, 25f),
            new("HealthRegen", "Health Regen", PerkCategory.Defense, 1f),
            new("Armor", "Armor", PerkCategory.Defense, 0.1f),
            new("DodgeChance", "Dodge", PerkCategory.Defense, 0.05f),
            new("Shield", "Shield", PerkCategory.Defense, 50f),
            new("Lifesteal", "Lifesteal", PerkCategory.Defense, 0.05f),
            new("DamageReduction", "Damage Reduction", PerkCategory.Defense, 0.1f),
            new("Thorns", "Thorns", PerkCategory.Defense, 0.25f),
            new("Invincibility", "Invincibility", PerkCategory.Defense, 1f),

            // Movement Perks
            new("MoveSpeed", "Move Speed", PerkCategory.Movement, 1.15f),
            new("JumpHeight", "Jump Height", PerkCategory.Movement, 1.2f),
            new("DoubleJump", "Double Jump", PerkCategory.Movement, 1f),
            new("DashDistance", "Dash Distance", PerkCategory.Movement, 1.25f),
            new("DashCooldown", "Dash Cooldown", PerkCategory.Movement, 0.8f),
            new("AirControl", "Air Control", PerkCategory.Movement, 1.5f),
            new("GlidingSlowing", "Gliding", PerkCategory.Movement, 1f),

            // Utility Perks
            new("CooldownReduction", "Cooldown Reduction", PerkCategory.Utility, 0.85f),
            new("Luck", "Luck", PerkCategory.Utility, 1.2f),
            new("XPGain", "XP Boost", PerkCategory.Utility, 1.5f),
            new("GoldFind", "Crystal Find", PerkCategory.Utility, 1.5f),
            new("MagnetRange", "Magnet", PerkCategory.Utility, 2f),
            new("ExtraLife", "Extra Life", PerkCategory.Utility, 1f),
            new("FreeReroll", "Free Reroll", PerkCategory.Utility, 1f),
            new("ExtraChoice", "Extra Choice", PerkCategory.Utility, 1f),
        };

        // Found perk-related addresses
        public List<FoundPerkAddress> FoundPerkAddresses { get; } = new();

        // Potential perk array base
        public IntPtr? PerkArrayBase { get; private set; }

        public event EventHandler<string>? StatusChanged;

        public PerkInjector(MemoryManager memory)
        {
            _memory = memory;
        }

        /// <summary>
        /// Scan memory for known perk name strings to find perk structures
        /// </summary>
        public int ScanForPerkStrings()
        {
            if (!_memory.IsAttached) return 0;

            FoundPerkAddresses.Clear();
            int totalFound = 0;

            StatusChanged?.Invoke(this, "Scanning for perk strings...");

            var baseAddr = _memory.BaseAddress;

            foreach (var perk in KnownPerks.Take(10)) // Start with first 10 to avoid long scan
            {
                // Search for the perk ID string in memory (ASCII)
                var addresses = ScanForString(perk.Id, baseAddr, 0x10000000);

                foreach (var addr in addresses.Take(5))
                {
                    FoundPerkAddresses.Add(new FoundPerkAddress
                    {
                        Address = addr,
                        PerkId = perk.Id,
                        PerkName = perk.Name,
                        StringAddress = addr
                    });
                    totalFound++;
                }

                // Also search for FName format (common in UE)
                var fnameAddresses = ScanForFName(perk.Id, baseAddr, 0x10000000);
                foreach (var addr in fnameAddresses.Take(3))
                {
                    FoundPerkAddresses.Add(new FoundPerkAddress
                    {
                        Address = addr,
                        PerkId = perk.Id,
                        PerkName = perk.Name + " (FName)",
                        StringAddress = addr,
                        IsFName = true
                    });
                    totalFound++;
                }
            }

            StatusChanged?.Invoke(this, $"Found {totalFound} potential perk references");
            return totalFound;
        }

        /// <summary>
        /// Search for ASCII string in memory
        /// </summary>
        private List<IntPtr> ScanForString(string text, IntPtr startAddress, long size)
        {
            var results = new List<IntPtr>();
            var targetBytes = Encoding.ASCII.GetBytes(text);
            var buffer = new byte[4096];

            for (long offset = 0; offset < size; offset += buffer.Length - targetBytes.Length)
            {
                var currentAddress = IntPtr.Add(startAddress, (int)offset);
                var bytes = _memory.ReadBytes(currentAddress, buffer.Length);

                if (bytes == null) continue;

                for (int i = 0; i < bytes.Length - targetBytes.Length; i++)
                {
                    bool match = true;
                    for (int j = 0; j < targetBytes.Length; j++)
                    {
                        if (bytes[i + j] != targetBytes[j])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        results.Add(IntPtr.Add(currentAddress, i));
                        if (results.Count >= 20) return results; // Limit results
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Search for FName pattern (Unreal Engine name format)
        /// FNames are typically stored as index + number pairs
        /// </summary>
        private List<IntPtr> ScanForFName(string name, IntPtr startAddress, long size)
        {
            // In UE, FNames often have the string nearby in the name pool
            // We look for the string first, then check surrounding memory for FName structure
            var stringAddrs = ScanForString(name, startAddress, size);
            var results = new List<IntPtr>();

            foreach (var stringAddr in stringAddrs.Take(10))
            {
                // Check if this looks like an FName entry (typically has length prefix or is in name pool)
                // Look backwards for a potential FName index pointing here
                for (int backOffset = -0x100; backOffset < 0; backOffset += 8)
                {
                    var checkAddr = IntPtr.Add(stringAddr, backOffset);
                    var potentialIndex = _memory.ReadInt32(checkAddr);

                    // FName indices are typically small positive numbers
                    if (potentialIndex > 0 && potentialIndex < 1000000)
                    {
                        results.Add(checkAddr);
                        break;
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Find perk array by looking for structure patterns around found strings
        /// </summary>
        public bool FindPerkArrayStructure()
        {
            if (FoundPerkAddresses.Count == 0)
            {
                StatusChanged?.Invoke(this, "Run ScanForPerkStrings first");
                return false;
            }

            StatusChanged?.Invoke(this, "Analyzing memory around perk strings...");

            // Look for TArray pattern: Pointer to data, Count, Max
            foreach (var found in FoundPerkAddresses.Take(10))
            {
                // Look backwards from string for array header
                for (int backOffset = -0x200; backOffset < 0; backOffset += 8)
                {
                    var checkAddr = IntPtr.Add(found.Address, backOffset);

                    // Read potential array header
                    var ptr = _memory.ReadPointer(checkAddr);
                    var count = _memory.ReadInt32(IntPtr.Add(checkAddr, 8));
                    var max = _memory.ReadInt32(IntPtr.Add(checkAddr, 12));

                    // Valid TArray: pointer is valid, count > 0, count <= max, max is reasonable
                    if (ptr != null && ptr.Value != IntPtr.Zero &&
                        count > 0 && count <= max && max < 1000 && max > 0)
                    {
                        // Verify pointer points to valid memory
                        var testRead = _memory.ReadBytes(ptr.Value, 8);
                        if (testRead != null)
                        {
                            found.PotentialArrayBase = checkAddr;
                            found.ArrayCount = count;
                            found.ArrayMax = max;

                            StatusChanged?.Invoke(this,
                                $"Found potential perk array at {checkAddr.ToInt64():X} (Count: {count}, Max: {max})");
                        }
                    }
                }
            }

            // Try to find the most likely perk array base
            var arraysFound = FoundPerkAddresses.Where(f => f.PotentialArrayBase != IntPtr.Zero).ToList();
            if (arraysFound.Count > 0)
            {
                // Group by array base to find the most common one
                var grouped = arraysFound.GroupBy(f => f.PotentialArrayBase)
                    .OrderByDescending(g => g.Count())
                    .First();

                PerkArrayBase = grouped.Key;
                StatusChanged?.Invoke(this, $"Most likely perk array at: 0x{PerkArrayBase?.ToInt64():X}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to add a perk by ID (experimental)
        /// </summary>
        public bool TryAddPerk(string perkId, int level = 1)
        {
            if (!_memory.IsAttached || PerkArrayBase == null)
            {
                StatusChanged?.Invoke(this, "Not attached or perk array not found");
                return false;
            }

            var perk = KnownPerks.FirstOrDefault(p => p.Id == perkId);
            if (perk == null)
            {
                StatusChanged?.Invoke(this, $"Unknown perk: {perkId}");
                return false;
            }

            StatusChanged?.Invoke(this, $"Attempting to add perk: {perk.Name}");

            // This is where the actual injection would happen
            // The exact method depends on how the game stores perks
            //
            // Common structures:
            // 1. TArray<FPerkData> where FPerkData = { FName PerkId, int Level, float Value }
            // 2. TMap<FName, int> for perk ID -> level
            // 3. TArray<UPerkObject*> with object references

            // For now, log what we'd need to do
            StatusChanged?.Invoke(this,
                $"Perk injection requires knowing exact structure. " +
                $"Try modifying values at found addresses manually.");

            return false;
        }

        /// <summary>
        /// Dump memory around a found perk address for analysis
        /// </summary>
        public string DumpPerkMemory(FoundPerkAddress found, int beforeBytes = 64, int afterBytes = 64)
        {
            if (!_memory.IsAttached) return "Not attached";

            var sb = new StringBuilder();
            var startAddr = IntPtr.Add(found.Address, -beforeBytes);
            var totalBytes = beforeBytes + afterBytes;

            var bytes = _memory.ReadBytes(startAddr, totalBytes);
            if (bytes == null) return "Failed to read memory";

            sb.AppendLine($"=== Memory dump around {found.PerkId} at 0x{found.Address.ToInt64():X} ===");
            sb.AppendLine();

            for (int i = 0; i < bytes.Length; i += 16)
            {
                var addr = IntPtr.Add(startAddr, i);
                var isTargetLine = (i >= beforeBytes - 8 && i <= beforeBytes + 8);

                sb.Append(isTargetLine ? ">>> " : "    ");
                sb.Append($"{addr.ToInt64():X8}  ");

                // Hex bytes
                for (int j = 0; j < 16 && i + j < bytes.Length; j++)
                {
                    sb.Append($"{bytes[i + j]:X2} ");
                    if (j == 7) sb.Append(" ");
                }

                // ASCII
                sb.Append(" |");
                for (int j = 0; j < 16 && i + j < bytes.Length; j++)
                {
                    var b = bytes[i + j];
                    sb.Append(b >= 32 && b < 127 ? (char)b : '.');
                }
                sb.AppendLine("|");
            }

            // Also try to interpret as common types
            sb.AppendLine();
            sb.AppendLine("=== Interpreted values at target ===");

            var int32Val = _memory.ReadInt32(found.Address);
            var floatVal = _memory.ReadFloat(found.Address);
            var ptrVal = _memory.ReadPointer(found.Address);

            sb.AppendLine($"As Int32: {int32Val}");
            sb.AppendLine($"As Float: {floatVal}");
            sb.AppendLine($"As Pointer: 0x{ptrVal?.ToInt64():X}");

            return sb.ToString();
        }
    }

    /// <summary>
    /// Known perk data from save file analysis
    /// </summary>
    public class PerkData
    {
        public string Id { get; }
        public string Name { get; }
        public PerkCategory Category { get; }
        public float DefaultValue { get; }

        // Asset path format used in save files
        public string AssetPath => $"/Game/Blueprint/Perks/DA_Perk_{Id}.DA_Perk_{Id}";

        public PerkData(string id, string name, PerkCategory category, float defaultValue)
        {
            Id = id;
            Name = name;
            Category = category;
            DefaultValue = defaultValue;
        }
    }

    /// <summary>
    /// A found perk-related memory address
    /// </summary>
    public class FoundPerkAddress
    {
        public IntPtr Address { get; set; }
        public IntPtr StringAddress { get; set; }
        public string PerkId { get; set; } = "";
        public string PerkName { get; set; } = "";
        public bool IsFName { get; set; }

        // If we found an array structure nearby
        public IntPtr PotentialArrayBase { get; set; }
        public int? ArrayCount { get; set; }
        public int? ArrayMax { get; set; }

        public string AddressHex => $"0x{Address.ToInt64():X}";
    }
}
