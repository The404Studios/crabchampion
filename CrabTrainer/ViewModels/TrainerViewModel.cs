using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrabTrainer.Cheats;

namespace CrabTrainer.ViewModels
{
    public partial class TrainerViewModel : ObservableObject, IDisposable
    {
        private readonly TrainerService _trainer;

        [ObservableProperty]
        private string _statusMessage = "Ready - Start Crab Champions and click Attach";

        [ObservableProperty]
        private bool _isAttached;

        [ObservableProperty]
        private string _processInfo = "Not attached";

        [ObservableProperty]
        private string _searchValue = "100";

        [ObservableProperty]
        private string _searchType = "Float";

        [ObservableProperty]
        private string _newValue = "";

        [ObservableProperty]
        private FoundAddress? _selectedAddress;

        public ObservableCollection<FoundAddress> FoundAddresses { get; } = new();
        public ObservableCollection<PerkDefinition> AvailablePerks { get; } = new();
        public ObservableCollection<string> SearchTypes { get; } = new() { "Int32", "Float" };

        public TrainerViewModel()
        {
            _trainer = new TrainerService();

            // Wire up events
            _trainer.StatusChanged += (s, msg) =>
            {
                Application.Current.Dispatcher.Invoke(() => StatusMessage = msg);
            };

            _trainer.AttachmentChanged += (s, attached) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsAttached = attached;
                    ProcessInfo = _trainer.ProcessInfo;
                });
            };

            _trainer.AddressesUpdated += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(RefreshAddresses);
            };

            // Load perks
            foreach (var perk in CheatDefinitions.Perks)
            {
                AvailablePerks.Add(perk);
            }
        }

        [RelayCommand]
        private void Attach()
        {
            if (IsAttached)
            {
                _trainer.Detach();
            }
            else
            {
                _trainer.Attach();
            }
        }

        [RelayCommand]
        private void Search()
        {
            if (!IsAttached)
            {
                StatusMessage = "Not attached to game!";
                return;
            }

            if (!double.TryParse(SearchValue, out double value))
            {
                StatusMessage = "Invalid search value";
                return;
            }

            _trainer.ClearAddresses();

            if (SearchType == "Int32")
            {
                _trainer.SearchInt32((int)value, "Search Result");
            }
            else
            {
                _trainer.SearchFloat((float)value, "Search Result");
            }
        }

        [RelayCommand]
        private void FilterResults()
        {
            if (!IsAttached || FoundAddresses.Count == 0)
            {
                StatusMessage = "Nothing to filter!";
                return;
            }

            if (!double.TryParse(SearchValue, out double value))
            {
                StatusMessage = "Invalid filter value";
                return;
            }

            if (SearchType == "Int32")
            {
                _trainer.FilterByInt32((int)value);
            }
            else
            {
                _trainer.FilterByFloat((float)value);
            }
        }

        [RelayCommand]
        private void SetValue()
        {
            if (SelectedAddress == null || string.IsNullOrEmpty(NewValue))
            {
                StatusMessage = "Select an address and enter a value";
                return;
            }

            try
            {
                object value = SelectedAddress.Type switch
                {
                    CheatValueType.Int32 => int.Parse(NewValue),
                    CheatValueType.Int64 => long.Parse(NewValue),
                    CheatValueType.Float => float.Parse(NewValue),
                    CheatValueType.Double => double.Parse(NewValue),
                    _ => NewValue
                };

                _trainer.SetValue(SelectedAddress, value);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Invalid value: {ex.Message}";
            }
        }

        [RelayCommand]
        private void FreezeValue()
        {
            if (SelectedAddress == null)
            {
                StatusMessage = "Select an address to freeze";
                return;
            }

            object? freezeVal = null;
            if (!string.IsNullOrEmpty(NewValue))
            {
                try
                {
                    freezeVal = SelectedAddress.Type switch
                    {
                        CheatValueType.Int32 => int.Parse(NewValue),
                        CheatValueType.Float => float.Parse(NewValue),
                        _ => null
                    };
                }
                catch { }
            }

            _trainer.ToggleFreeze(SelectedAddress, freezeVal);
            RefreshAddresses();
        }

        [RelayCommand]
        private void RemoveAddress()
        {
            if (SelectedAddress != null)
            {
                _trainer.RemoveAddress(SelectedAddress);
            }
        }

        [RelayCommand]
        private void ClearAll()
        {
            _trainer.ClearAddresses();
        }

        [RelayCommand]
        private void QuickHealth()
        {
            SearchValue = "100";
            SearchType = "Float";
            StatusMessage = "Search for your current health value, take damage, then filter with new value";
            Search();
        }

        [RelayCommand]
        private void QuickCrystals()
        {
            SearchValue = "0";
            SearchType = "Int32";
            StatusMessage = "Search for your current crystal count, collect some, then filter with new value";
            Search();
        }

        [RelayCommand]
        private void SetHealthMax()
        {
            // Set all float addresses that look like health to a high value
            foreach (var addr in FoundAddresses.Where(a => a.Type == CheatValueType.Float))
            {
                _trainer.SetValue(addr, 99999f);
                _trainer.ToggleFreeze(addr, 99999f);
            }
            StatusMessage = "Set and froze all found float values to 99999";
        }

        [RelayCommand]
        private void SetCrystalsMax()
        {
            // Set all int addresses to max
            foreach (var addr in FoundAddresses.Where(a => a.Type == CheatValueType.Int32))
            {
                _trainer.SetValue(addr, 999999);
            }
            StatusMessage = "Set all found int values to 999999";
        }

        private void RefreshAddresses()
        {
            FoundAddresses.Clear();
            foreach (var addr in _trainer.FoundAddresses)
            {
                FoundAddresses.Add(addr);
            }
        }

        public void Dispose()
        {
            _trainer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
