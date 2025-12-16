using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using UnrealSavEditor.Models;

namespace UnrealSavEditor.ViewModels
{
    public class TrainerViewModel : INotifyPropertyChanged
    {
        private readonly MemoryTrainer _trainer;
        private readonly DispatcherTimer _updateTimer;
        private readonly StringBuilder _logBuilder = new();
        private readonly Dispatcher _dispatcher;

        public TrainerViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _trainer = new MemoryTrainer();
            _trainer.StatusChanged += OnTrainerStatus;
            _trainer.ErrorOccurred += OnTrainerError;

            // Update UI every 250ms
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            _updateTimer.Tick += UpdateStats;
            _updateTimer.Start();

            // Initialize commands
            AttachCommand = new RelayCommand(async () => await ToggleAttachAsync(), () => !IsAttaching);
            HealCommand = new RelayCommand(() => _trainer.HealToFull(), () => IsAttached);
            AddArmorCommand = new RelayCommand(() => _trainer.AddArmor(5), () => IsAttached);
            RefillAmmoCommand = new RelayCommand(() => _trainer.RefillAmmo(), () => IsAttached);
            SetHealthAddressCommand = new RelayCommand(SetHealthAddress);
            SetWeaponAddressCommand = new RelayCommand(SetWeaponAddress);
            SetPlayerControllerCommand = new RelayCommand(SetPlayerController);
            VerifySDKCommand = new RelayCommand(VerifySDK, () => IsAttached);
            SpawnPerkCommand = new RelayCommand(SpawnSelectedPerk, () => IsAttached && SelectedPerk != null);

            // Initialize perk list
            AvailablePerks = _trainer.GetAvailablePerks().ToList();
            if (AvailablePerks.Count > 0)
                SelectedPerk = AvailablePerks[0];

            Log("Trainer initialized. Click 'Attach to Game' to begin.");
        }

        #region Properties

        private bool _isAttached;
        public bool IsAttached
        {
            get => _isAttached;
            set { _isAttached = value; OnPropertyChanged(); OnPropertyChanged(nameof(ConnectionStatus)); OnPropertyChanged(nameof(AttachButtonText)); }
        }

        private bool _isAttaching;
        public bool IsAttaching
        {
            get => _isAttaching;
            set { _isAttaching = value; OnPropertyChanged(); OnPropertyChanged(nameof(AttachButtonText)); }
        }

        public string ConnectionStatus => IsAttached ? "Connected to Crab Champions" : "Not Connected";
        public string AttachButtonText => IsAttaching ? "Attaching..." : (IsAttached ? "Detach from Game" : "Attach to Game");

        // Perk spawning
        public List<string> AvailablePerks { get; }

        private string? _selectedPerk;
        public string? SelectedPerk
        {
            get => _selectedPerk;
            set { _selectedPerk = value; OnPropertyChanged(); }
        }

        private bool _godModeEnabled;
        public bool GodModeEnabled
        {
            get => _godModeEnabled;
            set
            {
                if (_godModeEnabled != value)
                {
                    _godModeEnabled = value;
                    _trainer.SetGodMode(value);
                    OnPropertyChanged();
                    Log($"God Mode: {(value ? "ON" : "OFF")}");
                }
            }
        }

        private bool _infiniteAmmoEnabled;
        public bool InfiniteAmmoEnabled
        {
            get => _infiniteAmmoEnabled;
            set
            {
                if (_infiniteAmmoEnabled != value)
                {
                    _infiniteAmmoEnabled = value;
                    _trainer.SetInfiniteAmmo(value);
                    OnPropertyChanged();
                    Log($"Infinite Ammo: {(value ? "ON" : "OFF")}");
                }
            }
        }

        private string _healthDisplay = "N/A";
        public string HealthDisplay
        {
            get => _healthDisplay;
            set { _healthDisplay = value; OnPropertyChanged(); }
        }

        private string _armorDisplay = "N/A";
        public string ArmorDisplay
        {
            get => _armorDisplay;
            set { _armorDisplay = value; OnPropertyChanged(); }
        }

        private string _ammoDisplay = "N/A";
        public string AmmoDisplay
        {
            get => _ammoDisplay;
            set { _ammoDisplay = value; OnPropertyChanged(); }
        }

        private string _cheatLoopStatus = "Stopped";
        public string CheatLoopStatus
        {
            get => _cheatLoopStatus;
            set { _cheatLoopStatus = value; OnPropertyChanged(); }
        }

        private string _healthComponentAddress = "";
        public string HealthComponentAddress
        {
            get => _healthComponentAddress;
            set { _healthComponentAddress = value; OnPropertyChanged(); }
        }

        private string _weaponAddress = "";
        public string WeaponAddress
        {
            get => _weaponAddress;
            set { _weaponAddress = value; OnPropertyChanged(); }
        }

        private string _playerControllerAddress = "";
        public string PlayerControllerAddress
        {
            get => _playerControllerAddress;
            set { _playerControllerAddress = value; OnPropertyChanged(); }
        }

        private string _sdkStatus = "Click 'Verify SDK Offsets' to check";
        public string SDKStatus
        {
            get => _sdkStatus;
            set { _sdkStatus = value; OnPropertyChanged(); }
        }

        private string _logText = "";
        public string LogText
        {
            get => _logText;
            set { _logText = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand AttachCommand { get; }
        public ICommand HealCommand { get; }
        public ICommand AddArmorCommand { get; }
        public ICommand RefillAmmoCommand { get; }
        public ICommand SetHealthAddressCommand { get; }
        public ICommand SetWeaponAddressCommand { get; }
        public ICommand SetPlayerControllerCommand { get; }
        public ICommand VerifySDKCommand { get; }
        public ICommand SpawnPerkCommand { get; }

        #endregion

        #region Methods

        private async Task ToggleAttachAsync()
        {
            if (IsAttached)
            {
                _trainer.Detach();
                IsAttached = false;
                Log("Detached from game");
            }
            else
            {
                IsAttaching = true;
                Log("Attaching to game...");

                try
                {
                    // Run attach on background thread to prevent UI freeze
                    var result = await Task.Run(() =>
                    {
                        bool success = _trainer.AttachToGame();
                        if (!success) return (success: false, baseAddr: IntPtr.Zero, playerValid: false);

                        // Try to find player (expensive operation)
                        var player = _trainer.FindPlayer();
                        return (success: true, baseAddr: _trainer.BaseAddress, playerValid: player.IsValid);
                    });

                    IsAttached = result.success;

                    if (result.success)
                    {
                        Log($"Attached to game at base {result.baseAddr:X}");
                        if (result.playerValid)
                        {
                            Log("Auto-found player state");
                        }
                        else
                        {
                            Log("Could not auto-find player. Set addresses manually from Cheat Engine.");
                        }
                    }
                    else
                    {
                        Log("Failed to attach - is Crab Champions running?");
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error during attach: {ex.Message}");
                    IsAttached = false;
                }
                finally
                {
                    IsAttaching = false;
                }
            }
        }

        private void SpawnSelectedPerk()
        {
            if (string.IsNullOrEmpty(SelectedPerk))
            {
                Log("No perk selected");
                return;
            }

            Log($"Attempting to spawn perk: {SelectedPerk}");

            try
            {
                bool success = _trainer.TryGivePerk(SelectedPerk);
                if (success)
                {
                    Log($"Successfully spawned {SelectedPerk}!");
                }
                else
                {
                    Log($"Failed to spawn {SelectedPerk}. Make sure PlayerController is set.");
                }
            }
            catch (Exception ex)
            {
                Log($"Error spawning perk: {ex.Message}");
            }
        }

        private void SetHealthAddress()
        {
            if (!string.IsNullOrWhiteSpace(HealthComponentAddress))
            {
                _trainer.SetHealthComponentAddress(HealthComponentAddress);
                Log($"Health component set to {HealthComponentAddress}");
            }
        }

        private void SetWeaponAddress()
        {
            if (!string.IsNullOrWhiteSpace(WeaponAddress))
            {
                _trainer.SetWeaponAddress(WeaponAddress);
                Log($"Weapon address set to {WeaponAddress}");
            }
        }

        private void SetPlayerController()
        {
            if (!string.IsNullOrWhiteSpace(PlayerControllerAddress))
            {
                _trainer.SetPlayerController(PlayerControllerAddress);
                Log($"PlayerController set to {PlayerControllerAddress}");
            }
        }

        private void VerifySDK()
        {
            var results = _trainer.VerifySDKOffsets();
            var sb = new StringBuilder();

            foreach (var kvp in results)
            {
                sb.AppendLine($"{kvp.Key}: {kvp.Value.Address:X} [{(kvp.Value.Valid ? "OK" : "FAIL")}]");
            }

            SDKStatus = sb.ToString();
            Log("SDK verification complete");
        }

        private void UpdateStats(object? sender, EventArgs e)
        {
            if (!IsAttached) return;

            try
            {
                // Update health
                var health = _trainer.GetHealth();
                var maxHealth = _trainer.GetMaxHealth();
                if (health.HasValue && maxHealth.HasValue)
                {
                    HealthDisplay = $"{health.Value:F0} / {maxHealth.Value:F0}";
                }
                else
                {
                    HealthDisplay = "N/A";
                }

                // Update armor
                var armor = _trainer.GetArmorPlates();
                ArmorDisplay = armor.HasValue ? armor.Value.ToString() : "N/A";

                // Update ammo
                var ammo = _trainer.GetAmmo();
                AmmoDisplay = ammo.HasValue ? ammo.Value.ToString() : "N/A";

                // Update cheat loop status
                CheatLoopStatus = (_trainer.State.GodModeEnabled || _trainer.State.InfiniteAmmoEnabled) ? "Running" : "Stopped";

                // Sync toggle states
                _godModeEnabled = _trainer.State.GodModeEnabled;
                _infiniteAmmoEnabled = _trainer.State.InfiniteAmmoEnabled;
                OnPropertyChanged(nameof(GodModeEnabled));
                OnPropertyChanged(nameof(InfiniteAmmoEnabled));
            }
            catch
            {
                // Game may have closed
                if (!_trainer.IsAttached)
                {
                    IsAttached = false;
                    Log("Game disconnected");
                }
            }
        }

        private void OnTrainerStatus(object? sender, string message)
        {
            Log(message);
        }

        private void OnTrainerError(object? sender, string message)
        {
            Log($"ERROR: {message}");
        }

        private void Log(string message)
        {
            _logBuilder.AppendLine($"[{DateTime.Now:HH:mm:ss}] {message}");

            // Keep only last 50 lines
            var lines = _logBuilder.ToString().Split('\n');
            if (lines.Length > 50)
            {
                _logBuilder.Clear();
                for (int i = lines.Length - 50; i < lines.Length; i++)
                {
                    _logBuilder.AppendLine(lines[i]);
                }
            }

            LogText = _logBuilder.ToString();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Simple relay command implementation
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();
    }
}
