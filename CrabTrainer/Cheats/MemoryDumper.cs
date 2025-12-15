using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CrabTrainer.Memory;

namespace CrabTrainer.Cheats
{
    /// <summary>
    /// Dumps memory around known game strings to discover offsets and structures.
    /// This helps find pointer chains for cheats.
    /// </summary>
    public class MemoryDumper
    {
        private readonly MemoryManager _memory;

        public event EventHandler<string>? StatusChanged;
        public event EventHandler<int>? ProgressChanged;

        // All known strings to search for (from save file data)
        public static readonly KnownString[] KnownStrings = new KnownString[]
        {
            // Weapons
            new("AutoRifle", StringType.Weapon, "/Game/Blueprint/Weapon/AutoRifle/DA_Weapon_AutoRifle"),
            new("DualShotguns", StringType.Weapon, "/Game/Blueprint/Weapon/DualShotguns/DA_Weapon_DualShotguns"),
            new("DualPistols", StringType.Weapon, "/Game/Blueprint/Weapon/DualPistols/DA_Weapon_DualPistols"),
            new("Sniper", StringType.Weapon, "/Game/Blueprint/Weapon/Sniper/DA_Weapon_Sniper"),
            new("Minigun", StringType.Weapon, "/Game/Blueprint/Weapon/Minigun/DA_Weapon_Minigun"),
            new("RocketLauncher", StringType.Weapon, "/Game/Blueprint/Weapon/RocketLauncher/DA_Weapon_RocketLauncher"),
            new("Flamethrower", StringType.Weapon, "/Game/Blueprint/Weapon/Flamethrower/DA_Weapon_Flamethrower"),

            // Abilities
            new("Grenade", StringType.Ability, "/Game/Blueprint/Ability/DA_Ability_Grenade"),
            new("GrapplingHook", StringType.Ability, "/Game/Blueprint/Ability/DA_Ability_GrapplingHook"),
            new("BlackHole", StringType.Ability, "/Game/Blueprint/Ability/DA_Ability_BlackHole"),
            new("LaserBeam", StringType.Ability, "/Game/Blueprint/Ability/DA_Ability_LaserBeam"),

            // Melee
            new("Claw", StringType.Melee, "/Game/Blueprint/Melee/DA_Melee_Claw"),
            new("Hammer", StringType.Melee, "/Game/Blueprint/Melee/DA_Melee_Hammer"),
            new("Katana", StringType.Melee, "/Game/Blueprint/Melee/DA_Melee_Katana"),

            // Perks (in-run upgrades)
            new("DamageUp", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_DamageUp"),
            new("CritChance", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_CritChance"),
            new("CritDamage", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_CritDamage"),
            new("AttackSpeed", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_AttackSpeed"),
            new("Multishot", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_Multishot"),
            new("Piercing", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_Piercing"),
            new("MaxHealth", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_MaxHealth"),
            new("HealthRegen", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_HealthRegen"),
            new("Armor", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_Armor"),
            new("Lifesteal", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_Lifesteal"),
            new("MoveSpeed", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_MoveSpeed"),
            new("CooldownReduction", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_CooldownReduction"),
            new("Luck", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_Luck"),
            new("GoldFind", StringType.Perk, "/Game/Blueprint/Perks/DA_Perk_GoldFind"),

            // Skins
            new("MI_BlueIce", StringType.Skin, "/Game/Character/Crab/Texture/SkinPrototype/MI_BlueIce"),
            new("MI_Kaleidoscopic", StringType.Skin, "/Game/Character/Crab/Texture/SkinPrototype/MI_Kaleidoscopic"),
            new("MI_Prismatic", StringType.Skin, "/Game/Character/Crab/Texture/SkinPrototype/MI_Prismatic"),
            new("MI_Gold", StringType.Skin, "/Game/Character/Crab/Texture/SkinPrototype/MI_Gold"),

            // Common UE strings
            new("PlayerController", StringType.Engine, ""),
            new("CharacterMovement", StringType.Engine, ""),
            new("HealthComponent", StringType.Engine, ""),
            new("InventoryComponent", StringType.Engine, ""),
            new("PerkManager", StringType.Engine, ""),
            new("AbilitySystem", StringType.Engine, ""),

            // Crab Champions specific
            new("CrabCharacter", StringType.Engine, ""),
            new("CrabPlayerState", StringType.Engine, ""),
            new("CrabGameInstance", StringType.Engine, ""),
            new("CrabSaveGame", StringType.Engine, ""),
        };

        public List<FoundString> FoundStrings { get; } = new();
        public List<StructureAnalysis> AnalyzedStructures { get; } = new();

        public MemoryDumper(MemoryManager memory)
        {
            _memory = memory;
        }

        /// <summary>
        /// Scan memory for all known strings and dump results
        /// </summary>
        public string ScanAndDump(string outputPath = null)
        {
            if (!_memory.IsAttached)
                return "Not attached to game!";

            FoundStrings.Clear();
            AnalyzedStructures.Clear();

            var sb = new StringBuilder();
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine("CRAB CHAMPIONS MEMORY DUMP");
            sb.AppendLine($"Generated: {DateTime.Now}");
            sb.AppendLine($"Process: {_memory.ProcessName} (PID: {_memory.ProcessId})");
            sb.AppendLine($"Base Address: 0x{_memory.BaseAddress.ToInt64():X}");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine();

            var baseAddr = _memory.BaseAddress;
            int totalFound = 0;
            int progress = 0;

            foreach (var known in KnownStrings)
            {
                progress++;
                ProgressChanged?.Invoke(this, (progress * 100) / KnownStrings.Length);
                StatusChanged?.Invoke(this, $"Scanning for: {known.Name}");

                // Search for the name string
                var addresses = ScanForString(known.Name, baseAddr, 0x08000000);

                if (addresses.Count > 0)
                {
                    sb.AppendLine($"--- {known.Type}: {known.Name} ---");
                    sb.AppendLine($"Found {addresses.Count} occurrences");
                    sb.AppendLine();

                    foreach (var addr in addresses.Take(5)) // Limit to first 5
                    {
                        var found = new FoundString
                        {
                            Name = known.Name,
                            Type = known.Type,
                            Address = addr,
                            RelativeOffset = addr.ToInt64() - baseAddr.ToInt64()
                        };
                        FoundStrings.Add(found);
                        totalFound++;

                        sb.AppendLine($"  Address: 0x{addr.ToInt64():X}");
                        sb.AppendLine($"  Offset from base: 0x{found.RelativeOffset:X}");

                        // Dump surrounding memory
                        var analysis = AnalyzeStructure(addr, known.Name);
                        if (analysis != null)
                        {
                            AnalyzedStructures.Add(analysis);
                            sb.AppendLine($"  Potential structure found!");
                            sb.AppendLine($"    Pointers pointing here: {analysis.PointersToThis.Count}");
                            if (analysis.NearbyArrays.Count > 0)
                            {
                                sb.AppendLine($"    Nearby arrays: {analysis.NearbyArrays.Count}");
                                foreach (var arr in analysis.NearbyArrays.Take(3))
                                {
                                    sb.AppendLine($"      - Offset {arr.Offset:+0;-0}: Count={arr.Count}, Max={arr.Max}");
                                }
                            }
                            if (analysis.NearbyFloats.Count > 0)
                            {
                                sb.AppendLine($"    Nearby floats: {string.Join(", ", analysis.NearbyFloats.Take(5).Select(f => $"{f.Value:F2}@{f.Offset:+0;-0}"))}");
                            }
                            if (analysis.NearbyInts.Count > 0)
                            {
                                sb.AppendLine($"    Nearby ints: {string.Join(", ", analysis.NearbyInts.Take(5).Select(i => $"{i.Value}@{i.Offset:+0;-0}"))}");
                            }
                        }

                        // Hex dump
                        sb.AppendLine();
                        sb.AppendLine("  Hex dump (-32 to +64 bytes):");
                        var hexDump = DumpHex(addr, -32, 96);
                        foreach (var line in hexDump.Split('\n'))
                        {
                            sb.AppendLine($"    {line}");
                        }
                        sb.AppendLine();
                    }

                    sb.AppendLine();
                }
            }

            // Summary
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine("SUMMARY");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine($"Total strings found: {totalFound}");
            sb.AppendLine($"Structures analyzed: {AnalyzedStructures.Count}");
            sb.AppendLine();

            // Group by type
            var byType = FoundStrings.GroupBy(f => f.Type);
            foreach (var group in byType)
            {
                sb.AppendLine($"{group.Key}: {group.Count()} found");
                foreach (var item in group.Take(10))
                {
                    sb.AppendLine($"  {item.Name}: 0x{item.Address.ToInt64():X} (base+0x{item.RelativeOffset:X})");
                }
            }

            // Pointer chains discovered
            sb.AppendLine();
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine("POTENTIAL POINTER CHAINS");
            sb.AppendLine("=".PadRight(80, '='));

            foreach (var analysis in AnalyzedStructures.Where(a => a.PointersToThis.Count > 0).Take(20))
            {
                sb.AppendLine($"\n{analysis.Name}:");
                foreach (var ptr in analysis.PointersToThis.Take(5))
                {
                    sb.AppendLine($"  Base+0x{ptr.Offset:X} -> 0x{analysis.Address.ToInt64():X}");
                }
            }

            var result = sb.ToString();

            // Save to file if path provided
            if (!string.IsNullOrEmpty(outputPath))
            {
                File.WriteAllText(outputPath, result);
                StatusChanged?.Invoke(this, $"Dump saved to: {outputPath}");
            }
            else
            {
                // Default path
                var defaultPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"CrabChampions_MemoryDump_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(defaultPath, result);
                StatusChanged?.Invoke(this, $"Dump saved to: {defaultPath}");
            }

            return result;
        }

        /// <summary>
        /// Analyze memory structure around an address
        /// </summary>
        private StructureAnalysis AnalyzeStructure(IntPtr addr, string name)
        {
            var analysis = new StructureAnalysis
            {
                Name = name,
                Address = addr,
                RelativeOffset = addr.ToInt64() - _memory.BaseAddress.ToInt64()
            };

            // Look for pointers pointing to this address (or nearby)
            // This is expensive, so we do a limited search
            var baseAddr = _memory.BaseAddress;
            var targetRange = new[] { addr.ToInt64() - 0x100, addr.ToInt64() + 0x100 };

            // Check nearby memory for interesting patterns
            for (int offset = -256; offset < 256; offset += 4)
            {
                var checkAddr = IntPtr.Add(addr, offset);

                // Check for array pattern (ptr, count, max)
                var ptr = _memory.ReadPointer(checkAddr);
                var count = _memory.ReadInt32(IntPtr.Add(checkAddr, 8));
                var max = _memory.ReadInt32(IntPtr.Add(checkAddr, 12));

                if (ptr != null && ptr.Value != IntPtr.Zero &&
                    count >= 0 && count <= 1000 && max >= count && max <= 1000 && max > 0)
                {
                    // Verify pointer is valid
                    var test = _memory.ReadBytes(ptr.Value, 4);
                    if (test != null)
                    {
                        analysis.NearbyArrays.Add(new ArrayInfo
                        {
                            Offset = offset,
                            DataPointer = ptr.Value,
                            Count = count.Value,
                            Max = max.Value
                        });
                    }
                }

                // Check for reasonable float values
                var floatVal = _memory.ReadFloat(checkAddr);
                if (floatVal != null && !float.IsNaN(floatVal.Value) && !float.IsInfinity(floatVal.Value))
                {
                    // Common game values
                    if ((floatVal.Value > 0 && floatVal.Value < 10000) ||
                        (floatVal.Value >= 0.01f && floatVal.Value <= 10f))
                    {
                        analysis.NearbyFloats.Add(new ValueInfo<float>
                        {
                            Offset = offset,
                            Value = floatVal.Value
                        });
                    }
                }

                // Check for reasonable int values
                var intVal = _memory.ReadInt32(checkAddr);
                if (intVal != null && intVal.Value >= 0 && intVal.Value < 1000000)
                {
                    analysis.NearbyInts.Add(new ValueInfo<int>
                    {
                        Offset = offset,
                        Value = intVal.Value
                    });
                }
            }

            // Search for pointers to this address in a range
            // This helps build pointer chains
            for (long searchOffset = 0; searchOffset < 0x1000000; searchOffset += 0x10000)
            {
                var searchAddr = IntPtr.Add(baseAddr, (int)searchOffset);
                var bytes = _memory.ReadBytes(searchAddr, 0x10000);
                if (bytes == null) continue;

                for (int i = 0; i < bytes.Length - 8; i += 8)
                {
                    var ptrValue = BitConverter.ToInt64(bytes, i);
                    if (ptrValue >= targetRange[0] && ptrValue <= targetRange[1])
                    {
                        analysis.PointersToThis.Add(new PointerInfo
                        {
                            Address = IntPtr.Add(searchAddr, i),
                            Offset = searchOffset + i,
                            PointsTo = new IntPtr(ptrValue)
                        });

                        if (analysis.PointersToThis.Count >= 20) break;
                    }
                }

                if (analysis.PointersToThis.Count >= 20) break;
            }

            return analysis;
        }

        /// <summary>
        /// Scan for ASCII string
        /// </summary>
        private List<IntPtr> ScanForString(string text, IntPtr startAddress, long size)
        {
            var results = new List<IntPtr>();
            var targetBytes = Encoding.ASCII.GetBytes(text);
            var buffer = new byte[0x10000]; // 64KB chunks

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
                        if (results.Count >= 10) return results;
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Create hex dump of memory region
        /// </summary>
        private string DumpHex(IntPtr addr, int beforeBytes, int totalBytes)
        {
            var startAddr = IntPtr.Add(addr, beforeBytes);
            var bytes = _memory.ReadBytes(startAddr, totalBytes);

            if (bytes == null) return "Failed to read memory";

            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i += 16)
            {
                var lineAddr = IntPtr.Add(startAddr, i);
                var isTarget = (i >= -beforeBytes - 8 && i <= -beforeBytes + 8);

                sb.Append($"{lineAddr.ToInt64():X8}  ");

                for (int j = 0; j < 16 && i + j < bytes.Length; j++)
                {
                    sb.Append($"{bytes[i + j]:X2} ");
                    if (j == 7) sb.Append(" ");
                }

                sb.Append(" |");
                for (int j = 0; j < 16 && i + j < bytes.Length; j++)
                {
                    var b = bytes[i + j];
                    sb.Append(b >= 32 && b < 127 ? (char)b : '.');
                }
                sb.AppendLine("|");
            }

            return sb.ToString();
        }
    }

    public enum StringType
    {
        Weapon,
        Ability,
        Melee,
        Perk,
        Skin,
        Engine
    }

    public class KnownString
    {
        public string Name { get; }
        public StringType Type { get; }
        public string AssetPath { get; }

        public KnownString(string name, StringType type, string assetPath)
        {
            Name = name;
            Type = type;
            AssetPath = assetPath;
        }
    }

    public class FoundString
    {
        public string Name { get; set; } = "";
        public StringType Type { get; set; }
        public IntPtr Address { get; set; }
        public long RelativeOffset { get; set; }
    }

    public class StructureAnalysis
    {
        public string Name { get; set; } = "";
        public IntPtr Address { get; set; }
        public long RelativeOffset { get; set; }
        public List<PointerInfo> PointersToThis { get; } = new();
        public List<ArrayInfo> NearbyArrays { get; } = new();
        public List<ValueInfo<float>> NearbyFloats { get; } = new();
        public List<ValueInfo<int>> NearbyInts { get; } = new();
    }

    public class PointerInfo
    {
        public IntPtr Address { get; set; }
        public long Offset { get; set; }
        public IntPtr PointsTo { get; set; }
    }

    public class ArrayInfo
    {
        public int Offset { get; set; }
        public IntPtr DataPointer { get; set; }
        public int Count { get; set; }
        public int Max { get; set; }
    }

    public class ValueInfo<T>
    {
        public int Offset { get; set; }
        public T Value { get; set; } = default!;
    }
}
