using System;
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
    /// Main ViewModel for the SAV Editor
    /// </summary>
    public partial class MainViewModel : ViewModelBase
    {
        private GvasFile? _currentFile;

        [ObservableProperty]
        private string _windowTitle = "Unreal SAV Editor";

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
        private string _statusMessage = "Ready";

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

        public ObservableCollection<PropertyTreeItemViewModel> Properties { get; } = new();
        public ObservableCollection<PropertyTreeItemViewModel> FilteredProperties { get; } = new();
        public ObservableCollection<RecentFileViewModel> RecentFiles { get; } = new();

        public MainViewModel()
        {
            LoadRecentFiles();
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

                // Hex bytes
                for (int j = 0; j < 16; j++)
                {
                    if (i + j < data.Length)
                        sb.Append($"{data[i + j]:X2} ");
                    else
                        sb.Append("   ");

                    if (j == 7) sb.Append(' ');
                }

                sb.Append(" |");

                // ASCII representation
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
        private async Task OpenFileAsync()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Save File",
                Filter = "Save Files (*.sav)|*.sav|All Files (*.*)|*.*",
                DefaultExt = ".sav"
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

                await Task.Run(() => _currentFile.Save());

                HasUnsavedChanges = false;
                StatusMessage = "File saved successfully!";
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
                    WindowTitle = $"Unreal SAV Editor - {FileName}";
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

                // Update the tree item display
                SelectedProperty = new PropertyTreeItemViewModel(property);
                HasUnsavedChanges = true;
                StatusMessage = "Property value updated";

                // Refresh the tree
                RefreshTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid value: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private async Task LoadFileAsync(string path)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading file...";
                Properties.Clear();

                await Task.Run(() =>
                {
                    _currentFile = GvasFile.Load(path);
                });

                if (_currentFile != null)
                {
                    FilePath = path;
                    FileName = Path.GetFileName(path);
                    WindowTitle = $"Unreal SAV Editor - {FileName}";
                    FileSize = FormatFileSize(new FileInfo(path).Length);
                    EngineVersion = _currentFile.EngineVersion.ToString();
                    SaveGameClass = _currentFile.SaveGameClassName;
                    PropertyCount = _currentFile.Properties.Count;
                    HasFile = true;
                    HasUnsavedChanges = false;

                    // Populate tree
                    foreach (var prop in _currentFile.Properties)
                    {
                        Properties.Add(new PropertyTreeItemViewModel(prop));
                    }

                    ApplyFilter();
                    AddToRecentFiles(path);
                    StatusMessage = $"Loaded {PropertyCount} properties";
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
                var settingsPath = Path.Combine(appData, "UnrealSavEditor", "recent.txt");

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
                var settingsDir = Path.Combine(appData, "UnrealSavEditor");
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
}
