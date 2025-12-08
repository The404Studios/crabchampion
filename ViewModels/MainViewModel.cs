using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using UnrealSavEditor.Models;

namespace UnrealSavEditor.ViewModels
{
    /// <summary>
    /// Main ViewModel for the Crab Champions SAV Editor
    /// </summary>
    public partial class MainViewModel : ViewModelBase
    {
        private GvasFile? _currentFile;
        private CrabChampionsSave? _crabSave;

        [ObservableProperty]
        private string _windowTitle = "Crab Champions Save Editor";

        [ObservableProperty]
        private string _fileName = "No file loaded";

        [ObservableProperty]
        private string _filePath = string.Empty;

        [ObservableProperty]
        private string _fileSize = string.Empty;

        [ObservableProperty]
        private string _engineVersion = string.Empty;

        [ObservableProperty]
        private string _saveGameClass = string.Empty;

        [ObservableProperty]
        private int _propertyCount;

        [ObservableProperty]
        private bool _hasFile;

        [ObservableProperty]
        private bool _hasUnsavedChanges;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private PropertyTreeItemViewModel? _selectedProperty;

        [ObservableProperty]
        private string _statusMessage = "Ready - Open SaveSlot.sav from Crab Champions";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private int _selectedTabIndex;

        [ObservableProperty]
        private string _hexViewContent = string.Empty;

        [ObservableProperty]
        private string _editValue = string.Empty;

        [ObservableProperty]
        private bool _canEditValue;

        [ObservableProperty]
        private bool _isCrabChampionsSave;

        [ObservableProperty]
        private string _defaultSavePath = string.Empty;

        [ObservableProperty]
        private bool _defaultSaveExists;

        [ObservableProperty]
        private string _compressionType = "None";

        // Unlock summary
        [ObservableProperty]
        private string _weaponsStatus = "0/18";

        [ObservableProperty]
        private string _abilitiesStatus = "0/5";

        [ObservableProperty]
        private string _meleeStatus = "0/3";

        // Quick edit values for Crab Champions
        [ObservableProperty]
        private string _quickCrystals = "0";

        [ObservableProperty]
        private string _quickKeys = "0";

        [ObservableProperty]
        private string _quickHealth = "0";

        [ObservableProperty]
        private string _quickDamage = "0";

        [ObservableProperty]
        private string _quickSpeed = "0";

        [ObservableProperty]
        private string _quickLevel = "0";

        public ObservableCollection<PropertyTreeItemViewModel> Properties { get; } = new();
        public ObservableCollection<PropertyTreeItemViewModel> FilteredProperties { get; } = new();
        public ObservableCollection<RecentFileViewModel> RecentFiles { get; } = new();
        public ObservableCollection<EditableValueViewModel> QuickEditValues { get; } = new();
        public ObservableCollection<CategoryViewModel> Categories { get; } = new();

        public MainViewModel()
        {
            LoadRecentFiles();
            CheckDefaultSavePath();
        }

        private void CheckDefaultSavePath()
        {
            DefaultSavePath = CrabChampionsSave.GetDefaultSavePath();
            DefaultSaveExists = File.Exists(DefaultSavePath);
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        partial void OnSelectedPropertyChanged(PropertyTreeItemViewModel? value)
        {
            if (value?.Property != null)
            {
                var propValue = value.Property.GetValue();
                EditValue = propValue?.ToString() ?? string.Empty;
                CanEditValue = IsEditableProperty(value.Property);
                UpdateHexView(value.Property);
            }
            else
            {
                EditValue = string.Empty;
                CanEditValue = false;
                HexViewContent = string.Empty;
            }
        }

        private bool IsEditableProperty(GvasProperty property)
        {
            return property is IntProperty or UInt32Property or Int64Property or UInt64Property
                or FloatProperty or DoubleProperty or BoolProperty or StrProperty
                or NameProperty or EnumProperty or ByteProperty;
        }

        private void UpdateHexView(GvasProperty property)
        {
            if (property is UnknownProperty up)
            {
                HexViewContent = FormatHexView(up.RawData);
            }
            else if (property is StructProperty sp && sp.RawData != null)
            {
                HexViewContent = FormatHexView(sp.RawData);
            }
            else if (property is MapProperty mp && mp.RawData != null)
            {
                HexViewContent = FormatHexView(mp.RawData);
            }
            else
            {
                HexViewContent = "No raw data available for this property type.";
            }
        }

        private string FormatHexView(byte[] data)
        {
            if (data == null || data.Length == 0) return "No data";

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < data.Length; i += 16)
            {
                sb.Append($"{i:X8}  ");

                for (int j = 0; j < 16; j++)
                {
                    if (i + j < data.Length)
                        sb.Append($"{data[i + j]:X2} ");
                    else
                        sb.Append("   ");

                    if (j == 7) sb.Append(' ');
                }

                sb.Append(" |");

                for (int j = 0; j < 16 && i + j < data.Length; j++)
                {
                    var b = data[i + j];
                    sb.Append(b >= 32 && b < 127 ? (char)b : '.');
                }

                sb.AppendLine("|");
            }

            return sb.ToString();
        }

        [RelayCommand]
        private async Task OpenDefaultSaveAsync()
        {
            if (DefaultSaveExists)
            {
                await LoadFileAsync(DefaultSavePath);
            }
            else
            {
                MessageBox.Show(
                    $"Crab Champions save file not found at:\n{DefaultSavePath}\n\nMake sure you have played the game at least once.",
                    "Save File Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        [RelayCommand]
        private async Task OpenFileAsync()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Crab Champions Save File",
                Filter = "Save Files (*.sav)|*.sav|All Files (*.*)|*.*",
                DefaultExt = ".sav",
                InitialDirectory = Path.GetDirectoryName(DefaultSavePath)
            };

            if (dialog.ShowDialog() == true)
            {
                await LoadFileAsync(dialog.FileName);
            }
        }

        [RelayCommand]
        private async Task SaveFileAsync()
        {
            if (_currentFile == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Saving file...";

                // Create automatic backup before saving
                var backupPath = $"{_currentFile.FilePath}.backup";
                if (File.Exists(_currentFile.FilePath))
                {
                    File.Copy(_currentFile.FilePath, backupPath, true);
                }

                await Task.Run(() => _currentFile.Save());

                HasUnsavedChanges = false;
                var compressionInfo = _currentFile.OriginalCompression != SaveCompressionType.None
                    ? $" (re-compressed as {_currentFile.OriginalCompression})"
                    : "";
                StatusMessage = $"File saved successfully!{compressionInfo} Backup created.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Error saving file";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveFileAsAsync()
        {
            if (_currentFile == null) return;

            var dialog = new SaveFileDialog
            {
                Title = "Save File As",
                Filter = "Save Files (*.sav)|*.sav|All Files (*.*)|*.*",
                DefaultExt = ".sav",
                FileName = Path.GetFileName(_currentFile.FilePath)
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "Saving file...";

                    await Task.Run(() => _currentFile.Save(dialog.FileName));

                    _currentFile.FilePath = dialog.FileName;
                    FilePath = dialog.FileName;
                    FileName = Path.GetFileName(dialog.FileName);
                    WindowTitle = $"Crab Champions Save Editor - {FileName}";
                    HasUnsavedChanges = false;
                    StatusMessage = "File saved successfully!";
                    AddToRecentFiles(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusMessage = "Error saving file";
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        [RelayCommand]
        private void CreateBackup()
        {
            if (_currentFile == null || string.IsNullOrEmpty(_currentFile.FilePath)) return;

            try
            {
                var backupPath = $"{_currentFile.FilePath}.backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                File.Copy(_currentFile.FilePath, backupPath);
                StatusMessage = $"Backup created: {Path.GetFileName(backupPath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ApplyEdit()
        {
            if (SelectedProperty?.Property == null || string.IsNullOrEmpty(EditValue)) return;

            try
            {
                var property = SelectedProperty.Property;

                switch (property)
                {
                    case IntProperty ip:
                        ip.Value = int.Parse(EditValue);
                        break;
                    case UInt32Property up:
                        up.Value = uint.Parse(EditValue);
                        break;
                    case Int64Property lp:
                        lp.Value = long.Parse(EditValue);
                        break;
                    case UInt64Property ulp:
                        ulp.Value = ulong.Parse(EditValue);
                        break;
                    case FloatProperty fp:
                        fp.Value = float.Parse(EditValue);
                        break;
                    case DoubleProperty dp:
                        dp.Value = double.Parse(EditValue);
                        break;
                    case BoolProperty bp:
                        bp.Value = bool.Parse(EditValue);
                        break;
                    case StrProperty sp:
                        sp.Value = EditValue;
                        break;
                    case NameProperty np:
                        np.Value = EditValue;
                        break;
                    case EnumProperty ep:
                        ep.Value = EditValue;
                        break;
                    case ByteProperty byp:
                        if (byp.EnumType == "None")
                            byp.ByteValue = byte.Parse(EditValue);
                        else
                            byp.EnumValue = EditValue;
                        break;
                }

                HasUnsavedChanges = true;
                StatusMessage = $"Updated {property.Name}";
                RefreshTree();
                UpdateQuickEditValues();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid value: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        [RelayCommand]
        private void ApplyQuickEdit(EditableValueViewModel? value)
        {
            if (value?.Property == null) return;

            try
            {
                switch (value.Property)
                {
                    case IntProperty ip:
                        ip.Value = (int)value.CurrentValue;
                        break;
                    case UInt32Property up:
                        up.Value = (uint)value.CurrentValue;
                        break;
                    case Int64Property lp:
                        lp.Value = (long)value.CurrentValue;
                        break;
                    case FloatProperty fp:
                        fp.Value = (float)value.CurrentValue;
                        break;
                    case DoubleProperty dp:
                        dp.Value = value.CurrentValue;
                        break;
                }

                HasUnsavedChanges = true;
                StatusMessage = $"Updated {value.DisplayName} to {value.CurrentValue:N0}";
                RefreshTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating value: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        [RelayCommand]
        private void MaxOutValue(EditableValueViewModel? value)
        {
            if (value == null) return;
            value.CurrentValue = 999999;
            ApplyQuickEdit(value);
        }

        // ============================================
        // QUICK ACTION COMMANDS
        // ============================================

        [RelayCommand]
        private void UnlockAllWeapons()
        {
            if (_crabSave == null) return;

            try
            {
                int count = _crabSave.UnlockAllWeapons();
                HasUnsavedChanges = true;
                UpdateUnlockStatus();
                RefreshTree();
                StatusMessage = count > 0 ? $"Unlocked {count} weapons!" : "All weapons already unlocked!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error unlocking weapons: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void UnlockAllAbilities()
        {
            if (_crabSave == null) return;

            try
            {
                int count = _crabSave.UnlockAllAbilities();
                HasUnsavedChanges = true;
                UpdateUnlockStatus();
                RefreshTree();
                StatusMessage = count > 0 ? $"Unlocked {count} abilities!" : "All abilities already unlocked!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error unlocking abilities: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void UnlockAllMelee()
        {
            if (_crabSave == null) return;

            try
            {
                int count = _crabSave.UnlockAllMelee();
                HasUnsavedChanges = true;
                UpdateUnlockStatus();
                RefreshTree();
                StatusMessage = count > 0 ? $"Unlocked {count} melee weapons!" : "All melee weapons already unlocked!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error unlocking melee: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void UnlockEverything()
        {
            if (_crabSave == null) return;

            try
            {
                var (weapons, abilities, melee) = _crabSave.UnlockAll();
                int difficulties = _crabSave.UnlockAllDifficulties();
                HasUnsavedChanges = true;
                UpdateUnlockStatus();
                RefreshTree();

                int total = weapons + abilities + melee + difficulties;
                StatusMessage = total > 0
                    ? $"Unlocked everything! ({weapons} weapons, {abilities} abilities, {melee} melee, {difficulties} difficulties)"
                    : "Everything already unlocked!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error unlocking everything: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void SetAllPrismatic()
        {
            if (_crabSave == null) return;

            try
            {
                int count = _crabSave.SetAllToPrismatic();
                HasUnsavedChanges = true;
                RefreshTree();
                StatusMessage = count > 0 ? $"Set {count} items to Prismatic rarity!" : "All items already Prismatic!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting prismatic: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void MaxAllMastery()
        {
            if (_crabSave == null) return;

            try
            {
                int count = _crabSave.MaxAllMastery();
                HasUnsavedChanges = true;
                RefreshTree();
                StatusMessage = count > 0 ? $"Maxed {count} mastery levels!" : "All mastery levels already maxed!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error maxing mastery: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void MaxCurrency()
        {
            if (_crabSave == null) return;

            try
            {
                _crabSave.MaxCurrency();
                HasUnsavedChanges = true;
                UpdateQuickEditValues();
                RefreshTree();
                StatusMessage = "Currency maxed to 999,999!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error maxing currency: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void UnlockAllDifficulties()
        {
            if (_crabSave == null) return;

            try
            {
                int count = _crabSave.UnlockAllDifficulties();
                HasUnsavedChanges = true;
                RefreshTree();
                StatusMessage = count > 0 ? $"Unlocked {count} difficulty tiers!" : "All difficulties already unlocked!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error unlocking difficulties: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void GodMode()
        {
            if (_crabSave == null) return;

            try
            {
                // Apply all cheats at once
                var (weapons, abilities, melee) = _crabSave.UnlockAll();
                int difficulties = _crabSave.UnlockAllDifficulties();
                int prismatic = _crabSave.SetAllToPrismatic();
                int mastery = _crabSave.MaxAllMastery();
                _crabSave.MaxCurrency();

                HasUnsavedChanges = true;
                UpdateUnlockStatus();
                UpdateQuickEditValues();
                RefreshTree();

                StatusMessage = $"GOD MODE ACTIVATED! Unlocked {weapons + abilities + melee} items, {difficulties} difficulties, {prismatic} set to Prismatic, {mastery} mastery maxed!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error activating god mode: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUnlockStatus()
        {
            if (_crabSave == null) return;

            var summary = _crabSave.GetUnlockSummary();
            WeaponsStatus = summary.WeaponsStatus;
            AbilitiesStatus = summary.AbilitiesStatus;
            MeleeStatus = summary.MeleeStatus;
        }

        // ============================================
        // PRESET PROFILE COMMANDS
        // ============================================

        [RelayCommand]
        private void ApplyPreset(string presetId)
        {
            if (_crabSave == null) return;

            var preset = CrabChampionsData.Presets.FirstOrDefault(p => p.Id == presetId);
            if (preset == null) return;

            if (preset.IsReset)
            {
                var result = MessageBox.Show(
                    "This will reset your save to default. Are you sure?",
                    "Confirm Reset",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;
            }

            try
            {
                var presetResult = _crabSave.ApplyPreset(preset);
                HasUnsavedChanges = true;
                UpdateUnlockStatus();
                UpdateQuickEditValues();
                RefreshTree();

                StatusMessage = presetResult.TotalChanges > 0
                    ? $"{preset.DisplayName} applied! {presetResult.TotalChanges} changes made."
                    : $"{preset.DisplayName} - No changes needed (already applied).";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying preset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================
        // STATS COMMANDS
        // ============================================

        [RelayCommand]
        private void MaxAllStats()
        {
            if (_crabSave == null) return;

            try
            {
                int count = _crabSave.MaxAllStats();
                HasUnsavedChanges = true;
                RefreshTree();
                StatusMessage = count > 0 ? $"Maxed {count} stats!" : "No stats found to modify.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error maxing stats: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ResetAllStats()
        {
            if (_crabSave == null) return;

            var result = MessageBox.Show(
                "This will reset all your stats to zero. Are you sure?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                int count = _crabSave.ResetAllStats();
                HasUnsavedChanges = true;
                RefreshTree();
                StatusMessage = count > 0 ? $"Reset {count} stats to zero." : "No stats found to reset.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting stats: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void SetImpressiveStats()
        {
            if (_crabSave == null) return;

            try
            {
                _crabSave.SetImpressiveStats();
                HasUnsavedChanges = true;
                RefreshTree();
                StatusMessage = "Stats set to impressive values!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting stats: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void UnlockAllPerks()
        {
            if (_crabSave == null) return;

            try
            {
                int count = _crabSave.UnlockAllPerks();
                HasUnsavedChanges = true;
                RefreshTree();
                StatusMessage = count > 0 ? $"Unlocked {count} perks!" : "All perks already unlocked!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error unlocking perks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ShowSaveStructure()
        {
            if (_crabSave == null) return;

            try
            {
                var propNames = _crabSave.GetAllPropertyNames();
                var winCounts = _crabSave.GetAllWinCounts();

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("=== SAVE FILE STRUCTURE ===\n");
                sb.AppendLine($"Total Properties: {propNames.Count}\n");

                sb.AppendLine("--- Win/Difficulty Properties ---");
                foreach (var win in winCounts.OrderBy(k => k.Key))
                {
                    sb.AppendLine($"  {win.Key} = {win.Value}");
                }

                sb.AppendLine("\n--- All Properties ---");
                foreach (var name in propNames.Take(100)) // Limit to first 100
                {
                    sb.AppendLine($"  {name}");
                }

                if (propNames.Count > 100)
                    sb.AppendLine($"  ... and {propNames.Count - 100} more");

                MessageBox.Show(sb.ToString(), "Save File Structure", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading structure: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void SearchProperties()
        {
            if (_crabSave == null) return;

            // Simple search for weapon-related properties
            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("=== WEAPON-RELATED PROPERTIES ===\n");

                var weaponProps = _crabSave.FindPropertiesContaining("weapon");
                sb.AppendLine($"Properties containing 'weapon': {weaponProps.Count}");
                foreach (var (path, prop) in weaponProps.Take(20))
                {
                    sb.AppendLine($"  {path} ({prop.GetType().Name})");
                }

                var unlockProps = _crabSave.FindPropertiesContaining("unlock");
                sb.AppendLine($"\nProperties containing 'unlock': {unlockProps.Count}");
                foreach (var (path, prop) in unlockProps.Take(20))
                {
                    sb.AppendLine($"  {path} ({prop.GetType().Name})");
                }

                var masteryProps = _crabSave.FindPropertiesContaining("mastery");
                sb.AppendLine($"\nProperties containing 'mastery': {masteryProps.Count}");
                foreach (var (path, prop) in masteryProps.Take(20))
                {
                    sb.AppendLine($"  {path} ({prop.GetType().Name})");
                }

                var rarityProps = _crabSave.FindPropertiesContaining("rarity");
                sb.AppendLine($"\nProperties containing 'rarity': {rarityProps.Count}");
                foreach (var (path, prop) in rarityProps.Take(20))
                {
                    sb.AppendLine($"  {path} ({prop.GetType().Name})");
                }

                MessageBox.Show(sb.ToString(), "Property Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ExpandAll()
        {
            SetExpanded(Properties, true);
        }

        [RelayCommand]
        private void CollapseAll()
        {
            SetExpanded(Properties, false);
        }

        private void SetExpanded(ObservableCollection<PropertyTreeItemViewModel> items, bool expanded)
        {
            foreach (var item in items)
            {
                item.IsExpanded = expanded;
                SetExpanded(item.Children, expanded);
            }
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        [RelayCommand]
        private async Task OpenRecentFileAsync(string path)
        {
            if (File.Exists(path))
            {
                await LoadFileAsync(path);
            }
            else
            {
                MessageBox.Show("File not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                var toRemove = RecentFiles.FirstOrDefault(f => f.FilePath == path);
                if (toRemove != null) RecentFiles.Remove(toRemove);
            }
        }

        [RelayCommand]
        private void OpenSaveFolder()
        {
            var folder = Path.GetDirectoryName(DefaultSavePath);
            if (Directory.Exists(folder))
            {
                System.Diagnostics.Process.Start("explorer.exe", folder);
            }
        }

        private async Task LoadFileAsync(string path)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading save file...";
                Properties.Clear();
                QuickEditValues.Clear();
                Categories.Clear();

                await Task.Run(() =>
                {
                    _currentFile = GvasFile.Load(path);
                });

                if (_currentFile != null)
                {
                    FilePath = path;
                    FileName = Path.GetFileName(path);
                    WindowTitle = $"Crab Champions Save Editor - {FileName}";
                    FileSize = FormatFileSize(new FileInfo(path).Length);
                    EngineVersion = _currentFile.EngineVersion.ToString();
                    SaveGameClass = _currentFile.SaveGameClassName;
                    PropertyCount = CountAllProperties(_currentFile.Properties);
                    HasFile = true;
                    HasUnsavedChanges = false;

                    // Create Crab Champions wrapper
                    _crabSave = new CrabChampionsSave(_currentFile);
                    IsCrabChampionsSave = CrabChampionsSave.IsCrabChampionsSave(_currentFile);
                    CompressionType = _currentFile.OriginalCompression.ToString();

                    // Populate tree
                    foreach (var prop in _currentFile.Properties)
                    {
                        Properties.Add(new PropertyTreeItemViewModel(prop));
                    }

                    // Populate categories
                    var categories = _crabSave.CategorizeProperties();
                    foreach (var category in categories.Where(c => c.Value.Count > 0))
                    {
                        Categories.Add(new CategoryViewModel
                        {
                            Name = category.Key,
                            Properties = new ObservableCollection<PropertyTreeItemViewModel>(
                                category.Value.Select(p => new PropertyTreeItemViewModel(p)))
                        });
                    }

                    // Populate quick edit values
                    UpdateQuickEditValues();

                    // Update unlock status
                    UpdateUnlockStatus();

                    ApplyFilter();
                    AddToRecentFiles(path);
                    StatusMessage = $"Loaded {PropertyCount} properties" + (IsCrabChampionsSave ? " (Crab Champions detected)" : "");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Error loading file";
                HasFile = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private int CountAllProperties(List<GvasProperty> properties)
        {
            int count = properties.Count;
            foreach (var prop in properties)
            {
                if (prop is StructProperty sp)
                    count += CountAllProperties(sp.Properties);
                if (prop is ArrayProperty ap)
                {
                    foreach (var item in ap.Items.OfType<StructProperty>())
                        count += CountAllProperties(item.Properties);
                }
            }
            return count;
        }

        private void UpdateQuickEditValues()
        {
            QuickEditValues.Clear();

            if (_crabSave == null) return;

            var editableValues = _crabSave.GetEditableValues();
            foreach (var val in editableValues)
            {
                QuickEditValues.Add(new EditableValueViewModel
                {
                    Property = val.Property,
                    DisplayName = val.DisplayName,
                    Icon = val.Icon,
                    Category = val.Category,
                    CurrentValue = val.CurrentValue
                });
            }

            // Add all numeric properties that weren't found by name
            AddAllNumericProperties(_currentFile?.Properties ?? new List<GvasProperty>());
        }

        private void AddAllNumericProperties(List<GvasProperty> properties, string prefix = "")
        {
            foreach (var prop in properties)
            {
                var fullName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";

                if (IsNumericProperty(prop) && !QuickEditValues.Any(v => v.Property == prop))
                {
                    QuickEditValues.Add(new EditableValueViewModel
                    {
                        Property = prop,
                        DisplayName = prop.Name,
                        Icon = GetIconForProperty(prop),
                        Category = "Other Values",
                        CurrentValue = GetNumericValue(prop)
                    });
                }

                if (prop is StructProperty sp)
                {
                    AddAllNumericProperties(sp.Properties, fullName);
                }
            }
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

        private string GetIconForProperty(GvasProperty prop)
        {
            var name = prop.Name.ToLowerInvariant();
            if (name.Contains("crystal") || name.Contains("coin") || name.Contains("gold"))
                return "💎";
            if (name.Contains("key"))
                return "🔑";
            if (name.Contains("health") || name.Contains("hp"))
                return "❤️";
            if (name.Contains("damage") || name.Contains("attack"))
                return "⚔️";
            if (name.Contains("speed"))
                return "👟";
            if (name.Contains("level") || name.Contains("xp"))
                return "⭐";
            if (name.Contains("crit"))
                return "🎯";
            return "📊";
        }

        private void RefreshTree()
        {
            if (_currentFile == null) return;

            Properties.Clear();
            foreach (var prop in _currentFile.Properties)
            {
                Properties.Add(new PropertyTreeItemViewModel(prop));
            }
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredProperties.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var prop in Properties)
                {
                    FilteredProperties.Add(prop);
                }
            }
            else
            {
                foreach (var prop in Properties)
                {
                    if (prop.MatchesFilter(SearchText))
                    {
                        FilteredProperties.Add(prop);
                    }
                }
            }
        }

        private void AddToRecentFiles(string path)
        {
            var existing = RecentFiles.FirstOrDefault(f => f.FilePath == path);
            if (existing != null)
            {
                RecentFiles.Remove(existing);
            }

            RecentFiles.Insert(0, new RecentFileViewModel
            {
                FilePath = path,
                FileName = Path.GetFileName(path),
                LastOpened = DateTime.Now
            });

            while (RecentFiles.Count > 10)
            {
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            }

            SaveRecentFiles();
        }

        private void LoadRecentFiles()
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var settingsPath = Path.Combine(appData, "CrabChampionsSaveEditor", "recent.txt");

                if (File.Exists(settingsPath))
                {
                    var lines = File.ReadAllLines(settingsPath);
                    foreach (var line in lines.Take(10))
                    {
                        if (File.Exists(line))
                        {
                            RecentFiles.Add(new RecentFileViewModel
                            {
                                FilePath = line,
                                FileName = Path.GetFileName(line),
                                LastOpened = File.GetLastAccessTime(line)
                            });
                        }
                    }
                }
            }
            catch { }
        }

        private void SaveRecentFiles()
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var settingsDir = Path.Combine(appData, "CrabChampionsSaveEditor");
                Directory.CreateDirectory(settingsDir);
                var settingsPath = Path.Combine(settingsDir, "recent.txt");
                File.WriteAllLines(settingsPath, RecentFiles.Select(f => f.FilePath));
            }
            catch { }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
    }

    public class RecentFileViewModel
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime LastOpened { get; set; }
    }

    public partial class EditableValueViewModel : ObservableObject
    {
        public GvasProperty Property { get; set; } = null!;
        public string DisplayName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        [ObservableProperty]
        private double _currentValue;
    }

    public class CategoryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public ObservableCollection<PropertyTreeItemViewModel> Properties { get; set; } = new();
    }
}
