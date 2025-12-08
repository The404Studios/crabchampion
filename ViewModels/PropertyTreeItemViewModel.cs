using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using UnrealSavEditor.Models;

namespace UnrealSavEditor.ViewModels
{
    /// <summary>
    /// ViewModel for items in the property tree
    /// </summary>
    public partial class PropertyTreeItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _typeName = string.Empty;

        [ObservableProperty]
        private string _displayValue = string.Empty;

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private string _icon = "📄";

        [ObservableProperty]
        private string _typeColor = "#94A3B8";

        public GvasProperty? Property { get; }
        public ObservableCollection<PropertyTreeItemViewModel> Children { get; } = new();

        public PropertyTreeItemViewModel() { }

        public PropertyTreeItemViewModel(GvasProperty property)
        {
            Property = property;
            Name = property.Name;
            TypeName = property.TypeName;
            UpdateDisplayValue();
            SetIconAndColor();
            LoadChildren();
        }

        public PropertyTreeItemViewModel(string name, string typeName, object? value = null)
        {
            Name = name;
            TypeName = typeName;
            DisplayValue = value?.ToString() ?? string.Empty;
            SetIconAndColor();
        }

        private void UpdateDisplayValue()
        {
            if (Property == null) return;

            var value = Property.GetValue();
            DisplayValue = Property switch
            {
                IntProperty ip => ip.Value.ToString("N0"),
                UInt32Property up => up.Value.ToString("N0"),
                Int64Property lp => lp.Value.ToString("N0"),
                UInt64Property ulp => ulp.Value.ToString("N0"),
                FloatProperty fp => fp.Value.ToString("F4"),
                DoubleProperty dp => dp.Value.ToString("F6"),
                BoolProperty bp => bp.Value ? "True" : "False",
                StrProperty sp => TruncateString(sp.Value, 50),
                NameProperty np => TruncateString(np.Value, 50),
                EnumProperty ep => ep.Value,
                ByteProperty byp => byp.EnumType == "None" ? byp.ByteValue.ToString() : byp.EnumValue,
                ArrayProperty ap => $"[{ap.Items.Count} items]",
                MapProperty mp => $"[{mp.Entries.Count} entries]",
                SetProperty setp => $"[{setp.Items.Count} items]",
                StructProperty stp => GetStructDisplayValue(stp),
                ObjectProperty op => TruncateString(op.Value, 50),
                SoftObjectProperty sop => TruncateString(sop.AssetPath, 50),
                TextProperty tp => TruncateString(tp.DisplayText, 50),
                _ => "[Data]"
            };
        }

        private string GetStructDisplayValue(StructProperty sp)
        {
            if (sp.Vector.HasValue)
                return $"({sp.Vector.Value.X:F2}, {sp.Vector.Value.Y:F2}, {sp.Vector.Value.Z:F2})";
            if (sp.Quat.HasValue)
                return $"({sp.Quat.Value.X:F2}, {sp.Quat.Value.Y:F2}, {sp.Quat.Value.Z:F2}, {sp.Quat.Value.W:F2})";
            if (sp.GuidValue.HasValue)
                return sp.GuidValue.Value.ToString();
            if (sp.DateTime.HasValue)
                return sp.DateTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
            return $"{sp.StructType} [{sp.Properties.Count} props]";
        }

        private void SetIconAndColor()
        {
            (Icon, TypeColor) = TypeName switch
            {
                "IntProperty" or "UInt32Property" or "Int64Property" or "UInt64Property" => ("🔢", "#22D3EE"),
                "FloatProperty" or "DoubleProperty" => ("📊", "#F59E0B"),
                "BoolProperty" => ("✓", "#10B981"),
                "StrProperty" or "NameProperty" or "TextProperty" => ("📝", "#A78BFA"),
                "EnumProperty" or "ByteProperty" => ("🏷️", "#F472B6"),
                "StructProperty" => ("📦", "#6366F1"),
                "ArrayProperty" => ("📚", "#FB923C"),
                "MapProperty" => ("🗺️", "#34D399"),
                "SetProperty" => ("📋", "#60A5FA"),
                "ObjectProperty" or "SoftObjectProperty" => ("🔗", "#FBBF24"),
                _ => ("📄", "#94A3B8")
            };
        }

        private void LoadChildren()
        {
            if (Property is StructProperty sp)
            {
                foreach (var child in sp.Properties)
                {
                    Children.Add(new PropertyTreeItemViewModel(child));
                }
            }
            else if (Property is ArrayProperty ap)
            {
                for (int i = 0; i < ap.Items.Count; i++)
                {
                    var item = ap.Items[i];
                    if (item is StructProperty structItem)
                    {
                        var childVm = new PropertyTreeItemViewModel(structItem)
                        {
                            Name = $"[{i}]"
                        };
                        Children.Add(childVm);
                    }
                    else
                    {
                        Children.Add(new PropertyTreeItemViewModel($"[{i}]", ap.InnerType, item));
                    }
                }
            }
        }

        private static string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value[..maxLength] + "...";
        }

        public bool MatchesFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return true;

            filter = filter.ToLowerInvariant();
            return Name.ToLowerInvariant().Contains(filter) ||
                   TypeName.ToLowerInvariant().Contains(filter) ||
                   DisplayValue.ToLowerInvariant().Contains(filter) ||
                   Children.Any(c => c.MatchesFilter(filter));
        }
    }
}
