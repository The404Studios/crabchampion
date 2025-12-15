using System;
using System.Collections.Generic;
using System.Linq;
using UnrealSavEditor.Models;

namespace UnrealSavEditor.Models
{
    /// <summary>
    /// Crab Champions specific save file wrapper with easy access to common game values
    /// </summary>
    public class CrabChampionsSave
    {
        private readonly GvasFile _gvasFile;

        // Common property paths for Crab Champions
        public static readonly string[] CrystalPropertyNames = { "Crystals", "TotalCrystals", "Currency", "CrystalCount" };
        public static readonly string[] KeyPropertyNames = { "Keys", "KeyCount", "TotalKeys", "BossKeys" };
        public static readonly string[] HealthPropertyNames = { "Health", "CurrentHealth", "MaxHealth", "PlayerHealth" };
        public static readonly string[] LevelPropertyNames = { "Level", "CurrentLevel", "PlayerLevel", "CharacterLevel" };
        public static readonly string[] WeaponPropertyNames = { "UnlockedWeapons", "Weapons", "WeaponUnlocks", "AvailableWeapons" };
        public static readonly string[] PerkPropertyNames = { "Perks", "UnlockedPerks", "ActivePerks", "PerkList" };
        public static readonly string[] StatsPropertyNames = { "Stats", "Statistics", "PlayerStats", "GameStats" };
        public static readonly string[] RunPropertyNames = { "CurrentRun", "RunData", "ActiveRun", "Run" };

        public GvasFile GvasFile => _gvasFile;

        public CrabChampionsSave(GvasFile gvasFile)
        {
            _gvasFile = gvasFile;
        }

        /// <summary>
        /// Get the save file path for Crab Champions
        /// </summary>
        public static string GetDefaultSavePath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return System.IO.Path.Combine(localAppData, "CrabChampions", "Saved", "SaveGames", "SaveSlot.sav");
        }

        /// <summary>
        /// Check if a file appears to be a Crab Champions save
        /// </summary>
        public static bool IsCrabChampionsSave(GvasFile file)
        {
            // Check if the save game class contains "CrabChampions" or common CC identifiers
            return file.SaveGameClassName.Contains("CrabChampions", StringComparison.OrdinalIgnoreCase) ||
                   file.SaveGameClassName.Contains("CC_", StringComparison.OrdinalIgnoreCase) ||
                   file.Properties.Any(p => p.Name.Contains("Crab", StringComparison.OrdinalIgnoreCase)) ||
                   file.Properties.Any(p => p.Name.Contains("Island", StringComparison.OrdinalIgnoreCase)) ||
                   file.Properties.Any(p => p.Name.Contains("Weapon", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Find a property by checking multiple possible names
        /// </summary>
        public GvasProperty? FindProperty(params string[] possibleNames)
        {
            return FindPropertyRecursive(_gvasFile.Properties, possibleNames);
        }

        private GvasProperty? FindPropertyRecursive(List<GvasProperty> properties, string[] possibleNames)
        {
            foreach (var prop in properties)
            {
                if (possibleNames.Any(name => prop.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                                               prop.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return prop;
                }

                // Search in nested structs
                if (prop is StructProperty sp)
                {
                    var found = FindPropertyRecursive(sp.Properties, possibleNames);
                    if (found != null) return found;
                }

                // Search in arrays of structs
                if (prop is ArrayProperty ap)
                {
                    foreach (var item in ap.Items.OfType<StructProperty>())
                    {
                        var found = FindPropertyRecursive(item.Properties, possibleNames);
                        if (found != null) return found;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get all properties matching a category
        /// </summary>
        public List<GvasProperty> FindPropertiesByCategory(string[] categoryNames)
        {
            var results = new List<GvasProperty>();
            FindPropertiesByCategoryRecursive(_gvasFile.Properties, categoryNames, results);
            return results;
        }

        private void FindPropertiesByCategoryRecursive(List<GvasProperty> properties, string[] categoryNames, List<GvasProperty> results)
        {
            foreach (var prop in properties)
            {
                if (categoryNames.Any(name => prop.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    results.Add(prop);
                }

                if (prop is StructProperty sp)
                {
                    FindPropertiesByCategoryRecursive(sp.Properties, categoryNames, results);
                }

                if (prop is ArrayProperty ap)
                {
                    foreach (var item in ap.Items.OfType<StructProperty>())
                    {
                        FindPropertiesByCategoryRecursive(item.Properties, categoryNames, results);
                    }
                }
            }
        }

        /// <summary>
        /// Categorize all properties for the UI
        /// </summary>
        public Dictionary<string, List<GvasProperty>> CategorizeProperties()
        {
            var categories = new Dictionary<string, List<GvasProperty>>
            {
                ["Currency & Resources"] = new(),
                ["Player Stats"] = new(),
                ["Weapons & Equipment"] = new(),
                ["Perks & Abilities"] = new(),
                ["Run Progress"] = new(),
                ["Unlocks & Achievements"] = new(),
                ["Settings"] = new(),
                ["Other"] = new()
            };

            foreach (var prop in _gvasFile.Properties)
            {
                var category = CategorizeProperty(prop);
                categories[category].Add(prop);
            }

            return categories;
        }

        private string CategorizeProperty(GvasProperty prop)
        {
            var name = prop.Name.ToLowerInvariant();

            if (name.Contains("crystal") || name.Contains("key") || name.Contains("currency") ||
                name.Contains("coin") || name.Contains("gold") || name.Contains("resource"))
                return "Currency & Resources";

            if (name.Contains("health") || name.Contains("damage") || name.Contains("speed") ||
                name.Contains("stat") || name.Contains("level") || name.Contains("xp") ||
                name.Contains("experience"))
                return "Player Stats";

            if (name.Contains("weapon") || name.Contains("gun") || name.Contains("equipment") ||
                name.Contains("loadout") || name.Contains("melee"))
                return "Weapons & Equipment";

            if (name.Contains("perk") || name.Contains("ability") || name.Contains("skill") ||
                name.Contains("mod") || name.Contains("relic") || name.Contains("buff"))
                return "Perks & Abilities";

            if (name.Contains("run") || name.Contains("island") || name.Contains("wave") ||
                name.Contains("stage") || name.Contains("floor") || name.Contains("progress"))
                return "Run Progress";

            if (name.Contains("unlock") || name.Contains("achieve") || name.Contains("complete") ||
                name.Contains("discovered"))
                return "Unlocks & Achievements";

            if (name.Contains("setting") || name.Contains("option") || name.Contains("config") ||
                name.Contains("audio") || name.Contains("video") || name.Contains("control"))
                return "Settings";

            return "Other";
        }

        /// <summary>
        /// Get or set crystal count (tries multiple property names)
        /// </summary>
        public int? Crystals
        {
            get
            {
                var prop = FindProperty(CrystalPropertyNames);
                return prop switch
                {
                    IntProperty ip => ip.Value,
                    UInt32Property up => (int)up.Value,
                    Int64Property lp => (int)lp.Value,
                    FloatProperty fp => (int)fp.Value,
                    _ => null
                };
            }
            set
            {
                if (value == null) return;
                var prop = FindProperty(CrystalPropertyNames);
                if (prop != null) prop.SetValue(value.Value);
            }
        }

        /// <summary>
        /// Get or set key count
        /// </summary>
        public int? Keys
        {
            get
            {
                var prop = FindProperty(KeyPropertyNames);
                return prop switch
                {
                    IntProperty ip => ip.Value,
                    UInt32Property up => (int)up.Value,
                    Int64Property lp => (int)lp.Value,
                    _ => null
                };
            }
            set
            {
                if (value == null) return;
                var prop = FindProperty(KeyPropertyNames);
                if (prop != null) prop.SetValue(value.Value);
            }
        }

        /// <summary>
        /// Get or set health
        /// </summary>
        public float? Health
        {
            get
            {
                var prop = FindProperty(HealthPropertyNames);
                return prop switch
                {
                    FloatProperty fp => fp.Value,
                    IntProperty ip => ip.Value,
                    DoubleProperty dp => (float)dp.Value,
                    _ => null
                };
            }
            set
            {
                if (value == null) return;
                var prop = FindProperty(HealthPropertyNames);
                if (prop != null) prop.SetValue(value.Value);
            }
        }

        /// <summary>
        /// Get editable numeric properties for quick access
        /// </summary>
        public List<EditableValue> GetEditableValues()
        {
            var values = new List<EditableValue>();

            void AddIfFound(string[] names, string displayName, string icon, string category)
            {
                var prop = FindProperty(names);
                if (prop != null && IsNumericProperty(prop))
                {
                    values.Add(new EditableValue
                    {
                        Property = prop,
                        DisplayName = displayName,
                        Icon = icon,
                        Category = category,
                        CurrentValue = GetNumericValue(prop)
                    });
                }
            }

            // Try to find common game values
            AddIfFound(CrystalPropertyNames, "Crystals", "💎", "Currency");
            AddIfFound(KeyPropertyNames, "Keys", "🔑", "Currency");
            AddIfFound(HealthPropertyNames, "Health", "❤️", "Stats");
            AddIfFound(new[] { "MaxHealth" }, "Max Health", "💖", "Stats");
            AddIfFound(LevelPropertyNames, "Level", "⭐", "Stats");
            AddIfFound(new[] { "Damage", "BaseDamage", "DamageMultiplier" }, "Damage", "⚔️", "Stats");
            AddIfFound(new[] { "Speed", "MoveSpeed", "MovementSpeed" }, "Speed", "👟", "Stats");
            AddIfFound(new[] { "CritChance", "CriticalChance" }, "Crit Chance", "🎯", "Stats");
            AddIfFound(new[] { "CritDamage", "CriticalDamage" }, "Crit Damage", "💥", "Stats");

            return values;
        }

        private bool IsNumericProperty(GvasProperty prop)
        {
            return prop is IntProperty or UInt32Property or Int64Property or UInt64Property
                or FloatProperty or DoubleProperty;
        }

        private double GetNumericValue(GvasProperty prop)
        {
            return prop switch
            {
                IntProperty ip => ip.Value,
                UInt32Property up => up.Value,
                Int64Property lp => lp.Value,
                UInt64Property ulp => ulp.Value,
                FloatProperty fp => fp.Value,
                DoubleProperty dp => dp.Value,
                _ => 0
            };
        }

        // ============================================
        // DEBUG / DISCOVERY METHODS
        // ============================================

        /// <summary>
        /// Get all property names in the save file (for debugging)
        /// </summary>
        public List<string> GetAllPropertyNames()
        {
            var names = new List<string>();
            CollectPropertyNames(_gvasFile.Properties, names, "");
            return names;
        }

        private void CollectPropertyNames(IEnumerable<GvasProperty> props, List<string> names, string prefix)
        {
            foreach (var prop in props)
            {
                var fullName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                names.Add($"{fullName} ({prop.GetType().Name})");

                if (prop is StructProperty sp)
                {
                    CollectPropertyNames(sp.Properties, names, fullName);
                }
                else if (prop is ArrayProperty ap)
                {
                    foreach (var item in ap.Items)
                    {
                        if (item is StructProperty structItem)
                        {
                            CollectPropertyNames(structItem.Properties, names, $"{fullName}[]");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find all properties containing a specific substring (case-insensitive)
        /// </summary>
        public List<(string Path, GvasProperty Property)> FindPropertiesContaining(string substring)
        {
            var results = new List<(string, GvasProperty)>();
            SearchPropertiesContaining(_gvasFile.Properties, substring.ToLowerInvariant(), "", results);
            return results;
        }

        private void SearchPropertiesContaining(IEnumerable<GvasProperty> props, string substring, string prefix, List<(string, GvasProperty)> results)
        {
            foreach (var prop in props)
            {
                var fullName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";

                if (prop.Name.ToLowerInvariant().Contains(substring))
                {
                    results.Add((fullName, prop));
                }

                if (prop is StructProperty sp)
                {
                    SearchPropertiesContaining(sp.Properties, substring, fullName, results);
                }
                else if (prop is ArrayProperty ap)
                {
                    foreach (var item in ap.Items)
                    {
                        if (item is StructProperty structItem)
                        {
                            SearchPropertiesContaining(structItem.Properties, substring, $"{fullName}[]", results);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all integer properties that might be win counts or difficulty-related
        /// </summary>
        public Dictionary<string, int> GetAllWinCounts()
        {
            var wins = new Dictionary<string, int>();

            foreach (var prop in _gvasFile.Properties)
            {
                CollectWinCounts(prop, wins);
            }

            return wins;
        }

        private void CollectWinCounts(GvasProperty prop, Dictionary<string, int> wins)
        {
            var name = prop.Name.ToLowerInvariant();

            // Check if this is a win count or difficulty-related
            if (name.Contains("win") || name.Contains("bronze") || name.Contains("silver") ||
                name.Contains("gold") || name.Contains("sapphire") || name.Contains("emerald") ||
                name.Contains("ruby") || name.Contains("diamond") || name.Contains("prismatic") ||
                name.Contains("difficulty") || name.Contains("tier"))
            {
                if (prop is IntProperty ip)
                    wins[prop.Name] = ip.Value;
                else if (prop is UInt32Property up)
                    wins[prop.Name] = (int)up.Value;
            }

            if (prop is StructProperty sp)
            {
                foreach (var child in sp.Properties)
                    CollectWinCounts(child, wins);
            }
        }

        /// <summary>
        /// Get all MapProperty entries (used for weapon mastery/rarity usually)
        /// </summary>
        public List<(string Path, string KeyType, string ValueType, int Count, string Sample)> GetAllMapProperties()
        {
            var maps = new List<(string, string, string, int, string)>();
            CollectMapProperties(_gvasFile.Properties, "", maps);
            return maps;
        }

        private void CollectMapProperties(IEnumerable<GvasProperty> props, string prefix, List<(string, string, string, int, string)> maps)
        {
            foreach (var prop in props)
            {
                var fullName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";

                if (prop is MapProperty mp && mp.Entries != null)
                {
                    var sample = "";
                    if (mp.Entries.Count > 0)
                    {
                        var first = mp.Entries.First();
                        sample = $"{first.Key} -> {first.Value}";
                    }
                    maps.Add((fullName, mp.KeyType ?? "?", mp.ValueType ?? "?", mp.Entries.Count, sample));
                }

                if (prop is StructProperty sp)
                {
                    CollectMapProperties(sp.Properties, fullName, maps);
                }

                if (prop is ArrayProperty ap)
                {
                    foreach (var item in ap.Items.OfType<StructProperty>())
                    {
                        CollectMapProperties(item.Properties, $"{fullName}[]", maps);
                    }
                }
            }
        }

        /// <summary>
        /// Get all ArrayProperty entries with their contents
        /// </summary>
        public List<(string Path, string InnerType, int Count, List<string> Samples)> GetAllArrayProperties()
        {
            var arrays = new List<(string, string, int, List<string>)>();
            CollectArrayProperties(_gvasFile.Properties, "", arrays);
            return arrays;
        }

        private void CollectArrayProperties(IEnumerable<GvasProperty> props, string prefix, List<(string, string, int, List<string>)> arrays)
        {
            foreach (var prop in props)
            {
                var fullName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";

                if (prop is ArrayProperty ap)
                {
                    var samples = new List<string>();
                    foreach (var item in ap.Items.Take(5))
                    {
                        if (item is string s) samples.Add(s);
                        else if (item is StrProperty sp) samples.Add(sp.Value);
                        else if (item is NameProperty np) samples.Add(np.Value);
                        else if (item is StructProperty) samples.Add("[Struct]");
                        else samples.Add(item?.ToString() ?? "null");
                    }
                    arrays.Add((fullName, ap.InnerType ?? "?", ap.Items.Count, samples));

                    // Also check inside struct arrays
                    foreach (var item in ap.Items.OfType<StructProperty>())
                    {
                        CollectArrayProperties(item.Properties, $"{fullName}[]", arrays);
                    }
                }

                if (prop is StructProperty structProp)
                {
                    CollectArrayProperties(structProp.Properties, fullName, arrays);
                }
            }
        }

        /// <summary>
        /// Get all small integer values (0-10) that could be rarity/mastery levels
        /// </summary>
        public List<(string Path, int Value)> GetPotentialRarityValues()
        {
            var values = new List<(string, int)>();
            CollectSmallInts(_gvasFile.Properties, "", values);
            return values;
        }

        private void CollectSmallInts(IEnumerable<GvasProperty> props, string prefix, List<(string, int)> values)
        {
            foreach (var prop in props)
            {
                var fullName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";

                // Look for small integers that could be rarity indices (0-10 range)
                if (prop is IntProperty ip && ip.Value >= 0 && ip.Value <= 10)
                {
                    values.Add((fullName, ip.Value));
                }
                else if (prop is ByteProperty bp && bp.ByteValue <= 10)
                {
                    values.Add((fullName, bp.ByteValue));
                }

                if (prop is StructProperty sp)
                {
                    CollectSmallInts(sp.Properties, fullName, values);
                }

                if (prop is ArrayProperty ap)
                {
                    foreach (var item in ap.Items.OfType<StructProperty>())
                    {
                        CollectSmallInts(item.Properties, $"{fullName}[]", values);
                    }
                }
            }
        }

        /// <summary>
        /// Dump complete save structure in a readable format
        /// </summary>
        public string DumpSaveStructure()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== CRAB CHAMPIONS SAVE STRUCTURE ===\n");
            sb.AppendLine($"Save Class: {_gvasFile.SaveGameClassName}");
            sb.AppendLine($"Total Top-Level Properties: {_gvasFile.Properties.Count}\n");

            // Maps (likely weapon/ability rarity storage)
            var maps = GetAllMapProperties();
            sb.AppendLine($"--- MAP PROPERTIES ({maps.Count}) ---");
            sb.AppendLine("(These often store weapon rarity/mastery)");
            foreach (var (path, keyType, valueType, count, sample) in maps)
            {
                sb.AppendLine($"  {path}");
                sb.AppendLine($"    Type: Map<{keyType}, {valueType}>, Count: {count}");
                if (!string.IsNullOrEmpty(sample))
                    sb.AppendLine($"    Sample: {sample}");
            }

            // Arrays (likely unlock lists)
            var arrays = GetAllArrayProperties();
            sb.AppendLine($"\n--- ARRAY PROPERTIES ({arrays.Count}) ---");
            sb.AppendLine("(These often store unlocked items)");
            foreach (var (path, innerType, count, samples) in arrays)
            {
                sb.AppendLine($"  {path}");
                sb.AppendLine($"    Type: Array<{innerType}>, Count: {count}");
                if (samples.Count > 0)
                    sb.AppendLine($"    Items: {string.Join(", ", samples)}");
            }

            // Small integers (potential rarity values)
            var rarities = GetPotentialRarityValues();
            var relevantRarities = rarities.Where(r =>
                r.Path.ToLowerInvariant().Contains("rarity") ||
                r.Path.ToLowerInvariant().Contains("tier") ||
                r.Path.ToLowerInvariant().Contains("level") ||
                r.Path.ToLowerInvariant().Contains("mastery") ||
                r.Path.ToLowerInvariant().Contains("rank")).ToList();

            sb.AppendLine($"\n--- POTENTIAL RARITY/MASTERY VALUES ---");
            sb.AppendLine("(Small integers 0-10 in rarity/tier/level/mastery properties)");
            foreach (var (path, value) in relevantRarities.Take(30))
            {
                sb.AppendLine($"  {path} = {value}");
            }

            return sb.ToString();
        }

        // ============================================
        // ARRAY HELPER METHODS
        // ============================================

        /// <summary>
        /// Check if an array contains a specific string value (for reading only)
        /// </summary>
        private bool ArrayContainsValue(ArrayProperty ap, string value)
        {
            foreach (var item in ap.Items)
            {
                // Check raw string values (for StrProperty/NameProperty arrays)
                if (item is string str && str == value)
                    return true;

                // Check property objects
                if (item is StrProperty sp && sp.Value == value)
                    return true;
                if (item is NameProperty np && np.Value == value)
                    return true;

                // Check enum properties
                if (item is EnumProperty ep && ep.Value.Contains(value))
                    return true;

                // Check string representation
                if (item?.ToString() == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Find or create an ArrayProperty for object references
        /// </summary>
        private ArrayProperty? FindOrCreateObjectArray(string propertyName)
        {
            // Try to find existing property
            var prop = FindProperty(propertyName, propertyName.ToLowerInvariant());

            if (prop is ArrayProperty ap && ap.InnerType == "ObjectProperty")
                return ap;

            // If not found, create a new one
            if (prop == null)
            {
                var newArray = new ArrayProperty
                {
                    Name = propertyName,
                    TypeName = "ArrayProperty",
                    InnerType = "ObjectProperty",
                    Items = new List<object>()
                };
                _gvasFile.Properties.Add(newArray);
                return newArray;
            }

            return null;
        }

        /// <summary>
        /// Find or create a StrProperty array
        /// </summary>
        private ArrayProperty? FindOrCreateStringArray(string propertyName)
        {
            var prop = FindProperty(propertyName, propertyName.ToLowerInvariant());

            if (prop is ArrayProperty ap && (ap.InnerType == "StrProperty" || ap.InnerType == "NameProperty"))
                return ap;

            if (prop == null)
            {
                var newArray = new ArrayProperty
                {
                    Name = propertyName,
                    TypeName = "ArrayProperty",
                    InnerType = "StrProperty",
                    Items = new List<object>()
                };
                _gvasFile.Properties.Add(newArray);
                return newArray;
            }

            return null;
        }

        // ============================================
        // UNLOCK & MODIFICATION METHODS
        // ============================================
        // These methods add items to unlock arrays using correct asset paths
        // verified from actual save file analysis.

        /// <summary>
        /// Check if array contains a path (case-insensitive)
        /// </summary>
        private bool ArrayContainsPath(ArrayProperty ap, string path)
        {
            var pathLower = path.ToLowerInvariant();
            foreach (var item in ap.Items)
            {
                if (item?.ToString()?.ToLowerInvariant() == pathLower)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Unlock all primary weapons
        /// </summary>
        public int UnlockAllWeapons()
        {
            int unlocked = 0;

            // Try multiple possible property names
            string[] possibleNames = { "UnlockedWeapons", "unlockedweapons", "UnlockedPrimaryWeapons",
                                       "OwnedWeapons", "Weapons", "PrimaryWeapons" };

            ArrayProperty? ap = null;
            foreach (var name in possibleNames)
            {
                var prop = FindPropertyExact(name);
                if (prop is ArrayProperty arr && arr.InnerType == "ObjectProperty")
                {
                    ap = arr;
                    break;
                }
            }

            // If not found, create the array
            if (ap == null)
            {
                ap = FindOrCreateObjectArray("UnlockedWeapons");
            }

            if (ap != null)
            {
                foreach (var weapon in CrabChampionsData.PrimaryWeapons)
                {
                    if (!ArrayContainsPath(ap, weapon.AssetPath))
                    {
                        ap.Items.Add(weapon.AssetPath);
                        unlocked++;
                    }
                }
            }
            return unlocked;
        }

        /// <summary>
        /// Unlock all abilities
        /// </summary>
        public int UnlockAllAbilities()
        {
            int unlocked = 0;

            // Try multiple possible property names
            string[] possibleNames = { "UnlockedAbilities", "unlockedabilities", "UnlockedSecondaryWeapons",
                                       "OwnedAbilities", "Abilities", "SecondaryWeapons" };

            ArrayProperty? ap = null;
            foreach (var name in possibleNames)
            {
                var prop = FindPropertyExact(name);
                if (prop is ArrayProperty arr && arr.InnerType == "ObjectProperty")
                {
                    ap = arr;
                    break;
                }
            }

            // If not found, create the array
            if (ap == null)
            {
                ap = FindOrCreateObjectArray("UnlockedAbilities");
            }

            if (ap != null)
            {
                foreach (var ability in CrabChampionsData.SecondaryWeapons)
                {
                    if (!ArrayContainsPath(ap, ability.AssetPath))
                    {
                        ap.Items.Add(ability.AssetPath);
                        unlocked++;
                    }
                }
            }
            return unlocked;
        }

        /// <summary>
        /// Unlock all melee weapons
        /// </summary>
        public int UnlockAllMelee()
        {
            int unlocked = 0;

            // Try multiple possible property names
            string[] possibleNames = { "UnlockedMeleeWeapons", "unlockedmeleeweapons", "UnlockedMelee",
                                       "OwnedMelee", "MeleeWeapons", "Melee" };

            ArrayProperty? ap = null;
            foreach (var name in possibleNames)
            {
                var prop = FindPropertyExact(name);
                if (prop is ArrayProperty arr && arr.InnerType == "ObjectProperty")
                {
                    ap = arr;
                    break;
                }
            }

            // If not found, create the array
            if (ap == null)
            {
                ap = FindOrCreateObjectArray("UnlockedMeleeWeapons");
            }

            if (ap != null)
            {
                foreach (var melee in CrabChampionsData.MeleeWeapons)
                {
                    if (!ArrayContainsPath(ap, melee.AssetPath))
                    {
                        ap.Items.Add(melee.AssetPath);
                        unlocked++;
                    }
                }
            }
            return unlocked;
        }

        /// <summary>
        /// Find a property by exact name match
        /// </summary>
        private GvasProperty? FindPropertyExact(string name)
        {
            return _gvasFile.Properties.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Unlock all items (weapons, abilities, melee)
        /// </summary>
        public (int weapons, int abilities, int melee) UnlockAll()
        {
            return (UnlockAllWeapons(), UnlockAllAbilities(), UnlockAllMelee());
        }

        /// <summary>
        /// Unlock all character skins
        /// </summary>
        public int UnlockAllCharacterSkins()
        {
            int unlocked = 0;

            // Try multiple possible property names
            string[] possibleNames = { "UnlockedSkins", "unlockedskins", "CharacterSkins", "characterskins",
                                       "OwnedSkins", "Skins", "PlayerSkins", "CosmeticSkins" };

            ArrayProperty? ap = null;
            foreach (var name in possibleNames)
            {
                var prop = FindPropertyExact(name);
                if (prop is ArrayProperty arr && (arr.InnerType == "ObjectProperty" || arr.InnerType == "StrProperty"))
                {
                    ap = arr;
                    break;
                }
            }

            // If not found, create the array
            if (ap == null)
            {
                ap = FindOrCreateObjectArray("UnlockedSkins");
            }

            if (ap != null)
            {
                foreach (var skin in CrabChampionsData.CharacterSkins)
                {
                    if (!ArrayContainsPath(ap, skin.AssetPath))
                    {
                        ap.Items.Add(skin.AssetPath);
                        unlocked++;
                    }
                }
            }
            return unlocked;
        }

        /// <summary>
        /// Unlock all weapon skins
        /// </summary>
        public int UnlockAllWeaponSkins()
        {
            int unlocked = 0;

            // Try multiple possible property names
            string[] possibleNames = { "UnlockedWeaponSkins", "unlockedweaponskins", "WeaponSkins",
                                       "weaponskins", "OwnedWeaponSkins", "WeaponCosmetics" };

            ArrayProperty? ap = null;
            foreach (var name in possibleNames)
            {
                var prop = FindPropertyExact(name);
                if (prop is ArrayProperty arr && (arr.InnerType == "ObjectProperty" || arr.InnerType == "StrProperty"))
                {
                    ap = arr;
                    break;
                }
            }

            // If not found, create the array
            if (ap == null)
            {
                ap = FindOrCreateObjectArray("UnlockedWeaponSkins");
            }

            if (ap != null)
            {
                foreach (var skin in CrabChampionsData.WeaponSkins)
                {
                    // For weapon skins, we add the skin for each weapon type
                    foreach (var weapon in CrabChampionsData.PrimaryWeapons)
                    {
                        var skinPath = $"/Game/Blueprint/Cosmetics/WeaponSkins/DA_WeaponSkin_{weapon.Id}_{skin.Id}.DA_WeaponSkin_{weapon.Id}_{skin.Id}";
                        if (!ArrayContainsPath(ap, skinPath) && !ArrayContainsPath(ap, skin.AssetPath))
                        {
                            ap.Items.Add(skinPath);
                            unlocked++;
                        }
                    }
                }
            }
            return unlocked;
        }

        /// <summary>
        /// Unlock all emotes
        /// </summary>
        public int UnlockAllEmotes()
        {
            int unlocked = 0;

            string[] possibleNames = { "UnlockedEmotes", "unlockedemotes", "Emotes", "emotes",
                                       "OwnedEmotes", "PlayerEmotes" };

            ArrayProperty? ap = null;
            foreach (var name in possibleNames)
            {
                var prop = FindPropertyExact(name);
                if (prop is ArrayProperty arr && (arr.InnerType == "ObjectProperty" || arr.InnerType == "StrProperty"))
                {
                    ap = arr;
                    break;
                }
            }

            if (ap == null)
            {
                ap = FindOrCreateObjectArray("UnlockedEmotes");
            }

            if (ap != null)
            {
                foreach (var emote in CrabChampionsData.Emotes)
                {
                    var emotePath = $"/Game/Blueprint/Cosmetics/Emotes/DA_Emote_{emote.Id}.DA_Emote_{emote.Id}";
                    if (!ArrayContainsPath(ap, emotePath))
                    {
                        ap.Items.Add(emotePath);
                        unlocked++;
                    }
                }
            }
            return unlocked;
        }

        /// <summary>
        /// Unlock all banners
        /// </summary>
        public int UnlockAllBanners()
        {
            int unlocked = 0;

            string[] possibleNames = { "UnlockedBanners", "unlockedbanners", "Banners", "banners",
                                       "OwnedBanners", "PlayerBanners", "ProfileBanners" };

            ArrayProperty? ap = null;
            foreach (var name in possibleNames)
            {
                var prop = FindPropertyExact(name);
                if (prop is ArrayProperty arr && (arr.InnerType == "ObjectProperty" || arr.InnerType == "StrProperty"))
                {
                    ap = arr;
                    break;
                }
            }

            if (ap == null)
            {
                ap = FindOrCreateObjectArray("UnlockedBanners");
            }

            if (ap != null)
            {
                foreach (var banner in CrabChampionsData.Banners)
                {
                    var bannerPath = $"/Game/Blueprint/Cosmetics/Banners/DA_Banner_{banner.Id}.DA_Banner_{banner.Id}";
                    if (!ArrayContainsPath(ap, bannerPath))
                    {
                        ap.Items.Add(bannerPath);
                        unlocked++;
                    }
                }
            }
            return unlocked;
        }

        /// <summary>
        /// Unlock all titles
        /// </summary>
        public int UnlockAllTitles()
        {
            int unlocked = 0;

            string[] possibleNames = { "UnlockedTitles", "unlockedtitles", "Titles", "titles",
                                       "OwnedTitles", "PlayerTitles" };

            ArrayProperty? ap = null;
            foreach (var name in possibleNames)
            {
                var prop = FindPropertyExact(name);
                if (prop is ArrayProperty arr && (arr.InnerType == "StrProperty" || arr.InnerType == "NameProperty"))
                {
                    ap = arr;
                    break;
                }
            }

            if (ap == null)
            {
                ap = FindOrCreateStringArray("UnlockedTitles");
            }

            if (ap != null)
            {
                foreach (var title in CrabChampionsData.Titles)
                {
                    if (!ArrayContainsValue(ap, title))
                    {
                        ap.Items.Add(title);
                        unlocked++;
                    }
                }
            }
            return unlocked;
        }

        /// <summary>
        /// Complete all challenges/achievements
        /// </summary>
        public int CompleteAllChallenges()
        {
            int completed = 0;

            // Try to find challenge completion tracking
            string[] possibleNames = { "CompletedChallenges", "completedchallenges", "Challenges",
                                       "Achievements", "achievements", "UnlockedAchievements" };

            ArrayProperty? ap = null;
            foreach (var name in possibleNames)
            {
                var prop = FindPropertyExact(name);
                if (prop is ArrayProperty arr)
                {
                    ap = arr;
                    break;
                }
            }

            if (ap == null)
            {
                ap = FindOrCreateStringArray("CompletedChallenges");
            }

            if (ap != null)
            {
                foreach (var challenge in CrabChampionsData.Challenges)
                {
                    if (!ArrayContainsValue(ap, challenge.Id) && !ArrayContainsPath(ap, challenge.AssetPath))
                    {
                        ap.Items.Add(challenge.Id);
                        completed++;
                    }
                }
            }

            // Also try to set boolean challenge flags
            foreach (var challenge in CrabChampionsData.Challenges)
            {
                var boolProp = FindProperty($"{challenge.Id}Completed", $"{challenge.Id}_Complete",
                    $"Has{challenge.Id}", $"b{challenge.Id}");
                if (boolProp is BoolProperty bp && !bp.Value)
                {
                    bp.Value = true;
                    completed++;
                }

                // Also set progress to max
                var progressProp = FindProperty($"{challenge.Id}Progress", $"{challenge.Id}_Progress",
                    $"{challenge.Id}Count");
                if (progressProp is IntProperty ip && ip.Value < challenge.RequiredValue)
                {
                    ip.Value = challenge.RequiredValue;
                    completed++;
                }
            }

            return completed;
        }

        /// <summary>
        /// Unlock all skins (character + weapon)
        /// </summary>
        public (int characterSkins, int weaponSkins) UnlockAllSkins()
        {
            return (UnlockAllCharacterSkins(), UnlockAllWeaponSkins());
        }

        /// <summary>
        /// Unlock all cosmetics (skins, emotes, banners, titles)
        /// </summary>
        public (int skins, int emotes, int banners, int titles) UnlockAllCosmetics()
        {
            var (charSkins, weapSkins) = UnlockAllSkins();
            return (charSkins + weapSkins, UnlockAllEmotes(), UnlockAllBanners(), UnlockAllTitles());
        }

        /// <summary>
        /// Get count of currently unlocked weapons
        /// </summary>
        public int GetUnlockedWeaponsCount()
        {
            var unlockedProp = FindProperty("UnlockedWeapons", "unlockedweapons");
            if (unlockedProp is ArrayProperty ap)
                return ap.Items.Count;
            return 0;
        }

        /// <summary>
        /// Get count of currently unlocked abilities
        /// </summary>
        public int GetUnlockedAbilitiesCount()
        {
            var unlockedProp = FindProperty("UnlockedAbilities", "unlockedabilities");
            if (unlockedProp is ArrayProperty ap)
                return ap.Items.Count;
            return 0;
        }

        /// <summary>
        /// Get count of currently unlocked melee weapons
        /// </summary>
        public int GetUnlockedMeleeCount()
        {
            var unlockedProp = FindProperty("UnlockedMeleeWeapons", "unlockedmeleeweapons");
            if (unlockedProp is ArrayProperty ap)
                return ap.Items.Count;
            return 0;
        }

        /// <summary>
        /// Get count of items in RankedWeapons array
        /// </summary>
        public int GetRankedItemsCount()
        {
            var rankedProp = FindProperty("RankedWeapons", "rankedweapons");
            if (rankedProp is ArrayProperty ap)
                return ap.Items.Count;
            return 0;
        }

        /// <summary>
        /// Set all weapons/abilities/melee to prismatic rarity.
        /// Uses the RankedWeapons array which contains all ranked items.
        /// </summary>
        public int SetAllWeaponsToPrismatic()
        {
            return SetAllRankedItemsToPrismatic();
        }

        /// <summary>
        /// Set all ranked items to Prismatic rank.
        /// The game stores all weapon/ability/melee ranks in the RankedWeapons array.
        /// Each entry has: Weapon (ObjectProperty) and Rank (EnumProperty ECrabRank)
        /// </summary>
        public int SetAllRankedItemsToPrismatic()
        {
            int modified = 0;
            const string prismaticRank = "ECrabRank::Prismatic";

            // Find the RankedWeapons array - this contains ALL ranked items (weapons, abilities, melee)
            var rankedProp = FindProperty("RankedWeapons", "rankedweapons");

            if (rankedProp is ArrayProperty ap)
            {
                foreach (var item in ap.Items)
                {
                    // Each item is a StructProperty containing Weapon and Rank
                    if (item is StructProperty sp)
                    {
                        // Find the Rank property within this struct
                        var rankProp = sp.Properties.FirstOrDefault(p =>
                            p.Name.Equals("Rank", StringComparison.OrdinalIgnoreCase));

                        if (rankProp is EnumProperty ep)
                        {
                            if (ep.Value != prismaticRank)
                            {
                                ep.Value = prismaticRank;
                                modified++;
                            }
                        }
                    }
                }
            }

            return modified;
        }

        /// <summary>
        /// Set all abilities to prismatic rarity (uses same RankedWeapons array)
        /// </summary>
        public int SetAllAbilitiesToPrismatic()
        {
            // Abilities are also stored in RankedWeapons, so this is handled by SetAllRankedItemsToPrismatic
            // Return 0 to avoid double-counting
            return 0;
        }

        /// <summary>
        /// Set all items to prismatic rarity
        /// </summary>
        public int SetAllToPrismatic()
        {
            // All ranked items (weapons, abilities, melee) are in RankedWeapons array
            return SetAllRankedItemsToPrismatic();
        }

        /// <summary>
        /// Max out all mastery levels
        /// </summary>
        public int MaxAllMastery()
        {
            int modified = 0;
            int maxLevel = CrabChampionsData.GetMaxMasteryLevel();

            // Find any mastery-related integer properties
            foreach (var prop in _gvasFile.Properties)
            {
                modified += MaxMasteryRecursive(prop, maxLevel);
            }

            return modified;
        }

        private int MaxMasteryRecursive(GvasProperty prop, int maxLevel)
        {
            int modified = 0;
            var name = prop.Name.ToLowerInvariant();

            if (name.Contains("mastery") || name.Contains("level") && !name.Contains("difficultylevel"))
            {
                if (prop is IntProperty ip && ip.Value < maxLevel)
                {
                    ip.Value = maxLevel;
                    modified++;
                }
            }

            if (prop is StructProperty sp)
            {
                foreach (var child in sp.Properties)
                {
                    modified += MaxMasteryRecursive(child, maxLevel);
                }
            }

            if (prop is ArrayProperty ap)
            {
                foreach (var item in ap.Items.OfType<StructProperty>())
                {
                    foreach (var child in item.Properties)
                    {
                        modified += MaxMasteryRecursive(child, maxLevel);
                    }
                }
            }

            return modified;
        }

        /// <summary>
        /// Unlock all difficulty tiers by setting boolean flags to true.
        /// Does NOT modify arrays to prevent save corruption.
        /// </summary>
        public int UnlockAllDifficulties()
        {
            int unlocked = 0;

            // Only modify boolean unlock flags - NEVER add to arrays
            foreach (var tier in CrabChampionsData.DifficultyTiers)
            {
                var prop = FindProperty($"{tier}Unlocked", $"Unlocked{tier}",
                    $"Has{tier}Difficulty", $"{tier}_Unlocked", $"b{tier}Unlocked");
                if (prop is BoolProperty bp && !bp.Value)
                {
                    bp.Value = true;
                    unlocked++;
                }
            }

            return unlocked;
        }

        /// <summary>
        /// Set currency values to max
        /// </summary>
        public void MaxCurrency(int maxValue = 999999)
        {
            var crystalProp = FindProperty(CrystalPropertyNames);
            if (crystalProp != null) crystalProp.SetValue(maxValue);

            var keyProp = FindProperty(KeyPropertyNames);
            if (keyProp != null) keyProp.SetValue(maxValue);
        }

        /// <summary>
        /// Get summary of what can be unlocked
        /// </summary>
        public UnlockSummary GetUnlockSummary()
        {
            var summary = new UnlockSummary();

            // Count current unlocks vs total
            summary.TotalWeapons = CrabChampionsData.PrimaryWeapons.Length;
            summary.TotalAbilities = CrabChampionsData.SecondaryWeapons.Length;
            summary.TotalMelee = CrabChampionsData.MeleeWeapons.Length;
            summary.TotalDifficulties = CrabChampionsData.DifficultyTiers.Length;

            // Get actual unlock counts from arrays
            summary.UnlockedWeapons = GetUnlockedWeaponsCount();
            summary.UnlockedAbilities = GetUnlockedAbilitiesCount();
            summary.UnlockedMelee = GetUnlockedMeleeCount();

            // Also get ranked items count for additional info
            summary.RankedItems = GetRankedItemsCount();

            // Cosmetics counts
            summary.TotalSkins = CrabChampionsData.CharacterSkins.Length + CrabChampionsData.WeaponSkins.Length;
            summary.TotalEmotes = CrabChampionsData.Emotes.Length;
            summary.TotalBanners = CrabChampionsData.Banners.Length;
            summary.TotalTitles = CrabChampionsData.Titles.Length;

            // Get unlocked cosmetics counts
            summary.UnlockedSkins = GetUnlockedSkinsCount();
            summary.UnlockedEmotes = GetUnlockedEmotesCount();
            summary.UnlockedBanners = GetUnlockedBannersCount();
            summary.UnlockedTitles = GetUnlockedTitlesCount();

            return summary;
        }

        /// <summary>
        /// Get count of unlocked skins
        /// </summary>
        public int GetUnlockedSkinsCount()
        {
            int count = 0;
            var charSkinsProp = FindPropertyExact("UnlockedSkins");
            if (charSkinsProp is ArrayProperty ap1)
                count += ap1.Items.Count;

            var weapSkinsProp = FindPropertyExact("UnlockedWeaponSkins");
            if (weapSkinsProp is ArrayProperty ap2)
                count += ap2.Items.Count;

            return count;
        }

        /// <summary>
        /// Get count of unlocked emotes
        /// </summary>
        public int GetUnlockedEmotesCount()
        {
            var prop = FindPropertyExact("UnlockedEmotes");
            if (prop is ArrayProperty ap)
                return ap.Items.Count;
            return 0;
        }

        /// <summary>
        /// Get count of unlocked banners
        /// </summary>
        public int GetUnlockedBannersCount()
        {
            var prop = FindPropertyExact("UnlockedBanners");
            if (prop is ArrayProperty ap)
                return ap.Items.Count;
            return 0;
        }

        /// <summary>
        /// Get count of unlocked titles
        /// </summary>
        public int GetUnlockedTitlesCount()
        {
            var prop = FindPropertyExact("UnlockedTitles");
            if (prop is ArrayProperty ap)
                return ap.Items.Count;
            return 0;
        }

        // ============================================
        // PRESET PROFILE METHODS
        // ============================================

        /// <summary>
        /// Apply a preset profile to the save
        /// </summary>
        public PresetResult ApplyPreset(PresetProfile preset)
        {
            var result = new PresetResult { PresetName = preset.DisplayName };

            if (preset.IsReset)
            {
                result.Message = "Reset functionality requires manual confirmation";
                return result;
            }

            if (preset.UnlockAll)
            {
                result.WeaponsUnlocked = UnlockAllWeapons();
                result.AbilitiesUnlocked = UnlockAllAbilities();
                result.MeleeUnlocked = UnlockAllMelee();
                result.DifficultiesUnlocked = UnlockAllDifficulties();
            }

            if (preset.SetPrismatic)
            {
                result.ItemsSetPrismatic = SetAllToPrismatic();
            }

            if (preset.MaxMastery)
            {
                result.MasteryMaxed = MaxAllMastery();
            }

            if (preset.MaxCurrency)
            {
                MaxCurrency();
                result.CurrencyMaxed = true;
            }

            if (preset.UnlockSkins)
            {
                var (charSkins, weapSkins) = UnlockAllSkins();
                result.SkinsUnlocked = charSkins + weapSkins;
            }

            if (preset.UnlockCosmetics)
            {
                result.EmotesUnlocked = UnlockAllEmotes();
                result.BannersUnlocked = UnlockAllBanners();
                result.TitlesUnlocked = UnlockAllTitles();
            }

            return result;
        }

        // ============================================
        // STATS EDITING METHODS
        // ============================================

        /// <summary>
        /// Get all tracked stats from the save
        /// </summary>
        public Dictionary<string, StatValue> GetAllStats()
        {
            var stats = new Dictionary<string, StatValue>();

            foreach (var statInfo in CrabChampionsData.TrackedStats)
            {
                var prop = FindPropertyByPatterns(statInfo.Id);
                if (prop != null)
                {
                    stats[statInfo.Id] = new StatValue
                    {
                        Info = statInfo,
                        Property = prop,
                        CurrentValue = GetNumericValue(prop)
                    };
                }
            }

            return stats;
        }

        /// <summary>
        /// Set a specific stat value
        /// </summary>
        public bool SetStat(string statId, double value)
        {
            var prop = FindPropertyByPatterns(statId);
            if (prop == null) return false;

            switch (prop)
            {
                case IntProperty ip:
                    ip.Value = (int)value;
                    return true;
                case UInt32Property up:
                    up.Value = (uint)value;
                    return true;
                case Int64Property lp:
                    lp.Value = (long)value;
                    return true;
                case FloatProperty fp:
                    fp.Value = (float)value;
                    return true;
                case DoubleProperty dp:
                    dp.Value = value;
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Max out all stats
        /// </summary>
        public int MaxAllStats()
        {
            int modified = 0;

            foreach (var statInfo in CrabChampionsData.TrackedStats)
            {
                var prop = FindPropertyByPatterns(statInfo.Id);
                if (prop != null)
                {
                    int maxValue = statInfo.Category == "Misc" && statInfo.Id == "Accuracy" ? 100 : 999999;
                    if (SetStat(statInfo.Id, maxValue))
                        modified++;
                }
            }

            return modified;
        }

        /// <summary>
        /// Reset all stats to zero
        /// </summary>
        public int ResetAllStats()
        {
            int modified = 0;

            foreach (var statInfo in CrabChampionsData.TrackedStats)
            {
                if (SetStat(statInfo.Id, 0))
                    modified++;
            }

            return modified;
        }

        /// <summary>
        /// Set specific stats for "impressive" profile
        /// </summary>
        public void SetImpressiveStats()
        {
            SetStat("TotalKills", 50000);
            SetStat("BossesKilled", 500);
            SetStat("TotalRuns", 100);
            SetStat("RunsCompleted", 75);
            SetStat("HighestWave", 50);
            SetStat("CrystalsCollected", 100000);
        }

        private GvasProperty? FindPropertyByPatterns(string primaryName)
        {
            // Try exact match first
            var prop = FindProperty(primaryName);
            if (prop != null) return prop;

            // Try common variations
            string[] patterns = { primaryName, $"Total{primaryName}", $"{primaryName}Count", $"Player{primaryName}" };
            return FindProperty(patterns);
        }

        // ============================================
        // UNLOCK ALL PERKS
        // ============================================

        /// <summary>
        /// Unlock all perks/upgrades by setting boolean flags to true.
        /// Does NOT modify arrays to prevent save corruption.
        /// </summary>
        public int UnlockAllPerks()
        {
            int unlocked = 0;

            // Only modify boolean unlock flags - NEVER add to arrays
            foreach (var perk in CrabChampionsData.Perks)
            {
                var prop = FindProperty($"{perk.Id}Unlocked", $"Has{perk.Id}",
                    $"Unlocked{perk.Id}", $"{perk.Id}_Unlocked", $"b{perk.Id}Unlocked");
                if (prop is BoolProperty bp && !bp.Value)
                {
                    bp.Value = true;
                    unlocked++;
                }
            }

            return unlocked;
        }
    }

    /// <summary>
    /// Represents an editable value for quick access UI
    /// </summary>
    public class EditableValue
    {
        public GvasProperty Property { get; set; } = null!;
        public string DisplayName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
    }

    /// <summary>
    /// Summary of unlock status
    /// </summary>
    public class UnlockSummary
    {
        public int UnlockedWeapons { get; set; }
        public int TotalWeapons { get; set; }
        public int UnlockedAbilities { get; set; }
        public int TotalAbilities { get; set; }
        public int UnlockedMelee { get; set; }
        public int TotalMelee { get; set; }
        public int TotalDifficulties { get; set; }
        public int RankedItems { get; set; }

        // Cosmetics
        public int UnlockedSkins { get; set; }
        public int TotalSkins { get; set; }
        public int UnlockedEmotes { get; set; }
        public int TotalEmotes { get; set; }
        public int UnlockedBanners { get; set; }
        public int TotalBanners { get; set; }
        public int UnlockedTitles { get; set; }
        public int TotalTitles { get; set; }

        public string WeaponsStatus => $"{UnlockedWeapons}/{TotalWeapons}";
        public string AbilitiesStatus => $"{UnlockedAbilities}/{TotalAbilities}";
        public string MeleeStatus => $"{UnlockedMelee}/{TotalMelee}";
        public string SkinsStatus => $"{UnlockedSkins}/{TotalSkins}";
        public string EmotesStatus => $"{UnlockedEmotes}/{TotalEmotes}";
        public string BannersStatus => $"{UnlockedBanners}/{TotalBanners}";
        public string TitlesStatus => $"{UnlockedTitles}/{TotalTitles}";
        public int TotalUnlocked => UnlockedWeapons + UnlockedAbilities + UnlockedMelee;
        public int TotalItems => TotalWeapons + TotalAbilities + TotalMelee;
        public int TotalCosmetics => TotalSkins + TotalEmotes + TotalBanners + TotalTitles;
        public int UnlockedCosmetics => UnlockedSkins + UnlockedEmotes + UnlockedBanners + UnlockedTitles;
    }

    /// <summary>
    /// Result of applying a preset
    /// </summary>
    public class PresetResult
    {
        public string PresetName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int WeaponsUnlocked { get; set; }
        public int AbilitiesUnlocked { get; set; }
        public int MeleeUnlocked { get; set; }
        public int DifficultiesUnlocked { get; set; }
        public int ItemsSetPrismatic { get; set; }
        public int MasteryMaxed { get; set; }
        public bool CurrencyMaxed { get; set; }
        public int SkinsUnlocked { get; set; }
        public int EmotesUnlocked { get; set; }
        public int BannersUnlocked { get; set; }
        public int TitlesUnlocked { get; set; }
        public int ChallengesCompleted { get; set; }

        public int TotalChanges => WeaponsUnlocked + AbilitiesUnlocked + MeleeUnlocked +
                                   DifficultiesUnlocked + ItemsSetPrismatic + MasteryMaxed +
                                   (CurrencyMaxed ? 1 : 0) + SkinsUnlocked + EmotesUnlocked +
                                   BannersUnlocked + TitlesUnlocked + ChallengesCompleted;
    }

    /// <summary>
    /// Represents a stat value for editing
    /// </summary>
    public class StatValue
    {
        public StatInfo Info { get; set; } = null!;
        public GvasProperty Property { get; set; } = null!;
        public double CurrentValue { get; set; }
    }
}
