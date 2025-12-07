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
}
