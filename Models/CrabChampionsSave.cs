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

        // ============================================
        // ARRAY HELPER METHODS (READ-ONLY)
        // ============================================
        // NOTE: We intentionally do NOT have an AddToArray method.
        // Adding items to arrays without fully understanding the save format
        // can corrupt saves and cause items to be relocked.

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

        // ============================================
        // UNLOCK & MODIFICATION METHODS
        // ============================================
        // IMPORTANT: These methods are READ-SAFE - they only modify existing properties.
        // We NEVER add items to arrays because we don't fully understand the save format.
        // This prevents corrupting saves that already have unlocked items.

        /// <summary>
        /// Unlock all primary weapons by setting boolean flags to true.
        /// Does NOT modify arrays to prevent save corruption.
        /// </summary>
        public int UnlockAllWeapons()
        {
            int unlocked = 0;

            // Only modify boolean unlock flags - NEVER add to arrays
            foreach (var weapon in CrabChampionsData.PrimaryWeapons)
            {
                // Try various property naming patterns
                var prop = FindProperty($"{weapon.Id}Unlocked", $"Unlocked{weapon.Id}",
                    $"Has{weapon.Id}", $"{weapon.Id}_Unlocked", $"b{weapon.Id}Unlocked");
                if (prop is BoolProperty bp && !bp.Value)
                {
                    bp.Value = true;
                    unlocked++;
                }
            }

            return unlocked;
        }

        /// <summary>
        /// Unlock all secondary weapons/abilities by setting boolean flags to true.
        /// Does NOT modify arrays to prevent save corruption.
        /// </summary>
        public int UnlockAllAbilities()
        {
            int unlocked = 0;

            // Only modify boolean unlock flags - NEVER add to arrays
            foreach (var ability in CrabChampionsData.SecondaryWeapons)
            {
                var prop = FindProperty($"{ability.Id}Unlocked", $"Unlocked{ability.Id}",
                    $"Has{ability.Id}", $"{ability.Id}_Unlocked", $"b{ability.Id}Unlocked");
                if (prop is BoolProperty bp && !bp.Value)
                {
                    bp.Value = true;
                    unlocked++;
                }
            }

            return unlocked;
        }

        /// <summary>
        /// Unlock all melee weapons by setting boolean flags to true.
        /// Does NOT modify arrays to prevent save corruption.
        /// </summary>
        public int UnlockAllMelee()
        {
            int unlocked = 0;

            // Only modify boolean unlock flags - NEVER add to arrays
            foreach (var melee in CrabChampionsData.MeleeWeapons)
            {
                var prop = FindProperty($"{melee.Id}Unlocked", $"Unlocked{melee.Id}",
                    $"Has{melee.Id}", $"{melee.Id}_Unlocked", $"b{melee.Id}Unlocked");
                if (prop is BoolProperty bp && !bp.Value)
                {
                    bp.Value = true;
                    unlocked++;
                }
            }

            return unlocked;
        }

        /// <summary>
        /// Unlock all items (weapons, abilities, melee)
        /// </summary>
        public (int weapons, int abilities, int melee) UnlockAll()
        {
            return (UnlockAllWeapons(), UnlockAllAbilities(), UnlockAllMelee());
        }

        /// <summary>
        /// Set all weapons to prismatic rarity (highest tier)
        /// </summary>
        public int SetAllWeaponsToPrismatic()
        {
            int modified = 0;
            int prismaticIndex = CrabChampionsData.GetPrismaticRarityIndex();

            // Look for weapon mastery/rarity maps or arrays
            var masteryProp = FindProperty(CrabChampionsData.PropertyPatterns.WeaponMastery);

            if (masteryProp is MapProperty mp && mp.Entries != null)
            {
                foreach (var kvp in mp.Entries.ToList())
                {
                    if (kvp.Value is IntProperty ip && ip.Value != prismaticIndex)
                    {
                        ip.Value = prismaticIndex;
                        modified++;
                    }
                    else if (kvp.Value is ByteProperty bp)
                    {
                        bp.ByteValue = (byte)prismaticIndex;
                        modified++;
                    }
                    else if (kvp.Value is int intVal && intVal != prismaticIndex)
                    {
                        mp.Entries[kvp.Key] = prismaticIndex;
                        modified++;
                    }
                }
            }

            // Also try individual weapon rarity properties
            foreach (var weapon in CrabChampionsData.PrimaryWeapons)
            {
                var rarityProp = FindProperty($"{weapon.Id}Rarity", $"{weapon.Id}Mastery", $"{weapon.Id}Level", $"{weapon.Id}Tier");
                if (rarityProp != null)
                {
                    if (rarityProp is IntProperty ip && ip.Value != prismaticIndex)
                    {
                        ip.Value = prismaticIndex;
                        modified++;
                    }
                    else if (rarityProp is ByteProperty byp)
                    {
                        byp.ByteValue = (byte)prismaticIndex;
                        modified++;
                    }
                    else if (rarityProp is EnumProperty ep)
                    {
                        ep.Value = "Prismatic";
                        modified++;
                    }
                }
            }

            return modified;
        }

        /// <summary>
        /// Set all abilities to prismatic rarity
        /// </summary>
        public int SetAllAbilitiesToPrismatic()
        {
            int modified = 0;
            int prismaticIndex = CrabChampionsData.GetPrismaticRarityIndex();

            var masteryProp = FindProperty(CrabChampionsData.PropertyPatterns.AbilityMastery);

            if (masteryProp is MapProperty mp && mp.Entries != null)
            {
                foreach (var kvp in mp.Entries.ToList())
                {
                    if (kvp.Value is IntProperty ip)
                    {
                        ip.Value = prismaticIndex;
                        modified++;
                    }
                    else if (kvp.Value is ByteProperty bp)
                    {
                        bp.ByteValue = (byte)prismaticIndex;
                        modified++;
                    }
                    else if (kvp.Value is int intVal && intVal != prismaticIndex)
                    {
                        mp.Entries[kvp.Key] = prismaticIndex;
                        modified++;
                    }
                }
            }

            foreach (var ability in CrabChampionsData.SecondaryWeapons)
            {
                var rarityProp = FindProperty($"{ability.Id}Rarity", $"{ability.Id}Mastery", $"{ability.Id}Level");
                if (rarityProp != null)
                {
                    if (rarityProp is IntProperty ip)
                    {
                        ip.Value = prismaticIndex;
                        modified++;
                    }
                    else if (rarityProp is ByteProperty byp)
                    {
                        byp.ByteValue = (byte)prismaticIndex;
                        modified++;
                    }
                    else if (rarityProp is EnumProperty ep)
                    {
                        ep.Value = "Prismatic";
                        modified++;
                    }
                }
            }

            return modified;
        }

        /// <summary>
        /// Set all items to prismatic rarity
        /// </summary>
        public int SetAllToPrismatic()
        {
            return SetAllWeaponsToPrismatic() + SetAllAbilitiesToPrismatic();
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

            // Try to count unlocked items
            var weaponProp = FindProperty(CrabChampionsData.PropertyPatterns.UnlockedWeapons);
            if (weaponProp is ArrayProperty wap)
                summary.UnlockedWeapons = wap.Items.Count;

            var abilityProp = FindProperty(CrabChampionsData.PropertyPatterns.UnlockedAbilities);
            if (abilityProp is ArrayProperty aap)
                summary.UnlockedAbilities = aap.Items.Count;

            var meleeProp = FindProperty(CrabChampionsData.PropertyPatterns.UnlockedMelee);
            if (meleeProp is ArrayProperty map)
                summary.UnlockedMelee = map.Items.Count;

            return summary;
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

        public string WeaponsStatus => $"{UnlockedWeapons}/{TotalWeapons}";
        public string AbilitiesStatus => $"{UnlockedAbilities}/{TotalAbilities}";
        public string MeleeStatus => $"{UnlockedMelee}/{TotalMelee}";
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

        public int TotalChanges => WeaponsUnlocked + AbilitiesUnlocked + MeleeUnlocked +
                                   DifficultiesUnlocked + ItemsSetPrismatic + MasteryMaxed +
                                   (CurrencyMaxed ? 1 : 0);
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
