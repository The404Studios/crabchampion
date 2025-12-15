using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using CrabTrainer.Memory;

namespace CrabTrainer.Cheats
{
    /// <summary>
    /// Main service that handles trainer functionality - searching, freezing, modifying values
    /// </summary>
    public class TrainerService : IDisposable
    {
        private readonly MemoryManager _memory;
        private readonly List<FoundAddress> _foundAddresses;
        private readonly Timer _freezeTimer;
        private readonly Timer _updateTimer;
        private bool _disposed;

        public event EventHandler<string>? StatusChanged;
        public event EventHandler? AddressesUpdated;
        public event EventHandler<bool>? AttachmentChanged;

        public MemoryManager Memory => _memory;
        public IReadOnlyList<FoundAddress> FoundAddresses => _foundAddresses;
        public bool IsAttached => _memory.IsAttached;
        public string ProcessInfo => IsAttached
            ? $"{_memory.ProcessName} (PID: {_memory.ProcessId})"
            : "Not attached";

        public TrainerService()
        {
            _memory = new MemoryManager();
            _foundAddresses = new List<FoundAddress>();

            // Timer to keep frozen values frozen
            _freezeTimer = new Timer(50); // 50ms interval for responsive freezing
            _freezeTimer.Elapsed += OnFreezeTimer;
            _freezeTimer.AutoReset = true;

            // Timer to update displayed values
            _updateTimer = new Timer(500); // Update display every 500ms
            _updateTimer.Elapsed += OnUpdateTimer;
            _updateTimer.AutoReset = true;
        }

        /// <summary>
        /// Attempt to attach to Crab Champions
        /// </summary>
        public bool Attach()
        {
            var result = _memory.AttachToGame();

            if (result)
            {
                _freezeTimer.Start();
                _updateTimer.Start();
                StatusChanged?.Invoke(this, $"Attached to {_memory.ProcessName}");
            }
            else
            {
                StatusChanged?.Invoke(this, "Could not find Crab Champions process");
            }

            AttachmentChanged?.Invoke(this, result);
            return result;
        }

        /// <summary>
        /// Detach from game
        /// </summary>
        public void Detach()
        {
            _freezeTimer.Stop();
            _updateTimer.Stop();
            _memory.Detach();
            _foundAddresses.Clear();
            StatusChanged?.Invoke(this, "Detached");
            AttachmentChanged?.Invoke(this, false);
        }

        /// <summary>
        /// Search for an integer value in memory
        /// </summary>
        public int SearchInt32(int value, string name = "Value")
        {
            if (!IsAttached) return 0;

            StatusChanged?.Invoke(this, $"Searching for int {value}...");

            var baseAddr = _memory.BaseAddress;
            var results = _memory.ScanForInt32(value, baseAddr, 0x10000000); // Search 256MB

            foreach (var addr in results.Take(100)) // Limit results
            {
                _foundAddresses.Add(new FoundAddress
                {
                    Address = addr,
                    Name = name,
                    Type = CheatValueType.Int32,
                    CurrentValue = value,
                    LastUpdated = DateTime.Now
                });
            }

            StatusChanged?.Invoke(this, $"Found {results.Count} addresses for {value}");
            AddressesUpdated?.Invoke(this, EventArgs.Empty);
            return results.Count;
        }

        /// <summary>
        /// Search for a float value in memory
        /// </summary>
        public int SearchFloat(float value, string name = "Value", float tolerance = 0.1f)
        {
            if (!IsAttached) return 0;

            StatusChanged?.Invoke(this, $"Searching for float {value}...");

            var baseAddr = _memory.BaseAddress;
            var results = _memory.ScanForFloat(value, baseAddr, 0x10000000, tolerance);

            foreach (var addr in results.Take(100))
            {
                _foundAddresses.Add(new FoundAddress
                {
                    Address = addr,
                    Name = name,
                    Type = CheatValueType.Float,
                    CurrentValue = value,
                    LastUpdated = DateTime.Now
                });
            }

            StatusChanged?.Invoke(this, $"Found {results.Count} addresses for {value}");
            AddressesUpdated?.Invoke(this, EventArgs.Empty);
            return results.Count;
        }

        /// <summary>
        /// Filter existing results by a new value (for narrowing down)
        /// </summary>
        public int FilterByInt32(int newValue)
        {
            if (!IsAttached) return 0;

            var toRemove = new List<FoundAddress>();

            foreach (var addr in _foundAddresses.Where(a => a.Type == CheatValueType.Int32))
            {
                var currentVal = _memory.ReadInt32(addr.Address);
                if (currentVal != newValue)
                {
                    toRemove.Add(addr);
                }
                else
                {
                    addr.CurrentValue = currentVal;
                    addr.LastUpdated = DateTime.Now;
                }
            }

            foreach (var addr in toRemove)
            {
                _foundAddresses.Remove(addr);
            }

            StatusChanged?.Invoke(this, $"Filtered to {_foundAddresses.Count} addresses");
            AddressesUpdated?.Invoke(this, EventArgs.Empty);
            return _foundAddresses.Count;
        }

        /// <summary>
        /// Filter existing results by a new float value
        /// </summary>
        public int FilterByFloat(float newValue, float tolerance = 0.1f)
        {
            if (!IsAttached) return 0;

            var toRemove = new List<FoundAddress>();

            foreach (var addr in _foundAddresses.Where(a => a.Type == CheatValueType.Float))
            {
                var currentVal = _memory.ReadFloat(addr.Address);
                if (currentVal == null || Math.Abs(currentVal.Value - newValue) > tolerance)
                {
                    toRemove.Add(addr);
                }
                else
                {
                    addr.CurrentValue = currentVal;
                    addr.LastUpdated = DateTime.Now;
                }
            }

            foreach (var addr in toRemove)
            {
                _foundAddresses.Remove(addr);
            }

            StatusChanged?.Invoke(this, $"Filtered to {_foundAddresses.Count} addresses");
            AddressesUpdated?.Invoke(this, EventArgs.Empty);
            return _foundAddresses.Count;
        }

        /// <summary>
        /// Add a known address manually
        /// </summary>
        public void AddAddress(IntPtr address, string name, CheatValueType type)
        {
            var found = new FoundAddress
            {
                Address = address,
                Name = name,
                Type = type,
                LastUpdated = DateTime.Now
            };

            UpdateAddressValue(found);
            _foundAddresses.Add(found);
            AddressesUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Remove an address from the list
        /// </summary>
        public void RemoveAddress(FoundAddress address)
        {
            _foundAddresses.Remove(address);
            AddressesUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Clear all found addresses
        /// </summary>
        public void ClearAddresses()
        {
            _foundAddresses.Clear();
            AddressesUpdated?.Invoke(this, EventArgs.Empty);
            StatusChanged?.Invoke(this, "Cleared all addresses");
        }

        /// <summary>
        /// Set a value at an address
        /// </summary>
        public bool SetValue(FoundAddress address, object value)
        {
            if (!IsAttached) return false;

            bool result = address.Type switch
            {
                CheatValueType.Int32 => _memory.WriteInt32(address.Address, Convert.ToInt32(value)),
                CheatValueType.Int64 => _memory.WriteInt64(address.Address, Convert.ToInt64(value)),
                CheatValueType.Float => _memory.WriteFloat(address.Address, Convert.ToSingle(value)),
                CheatValueType.Double => _memory.WriteDouble(address.Address, Convert.ToDouble(value)),
                _ => false
            };

            if (result)
            {
                address.CurrentValue = value;
                address.LastUpdated = DateTime.Now;
                StatusChanged?.Invoke(this, $"Set {address.Name} to {value}");
            }

            return result;
        }

        /// <summary>
        /// Freeze/unfreeze a value (keeps writing it)
        /// </summary>
        public void ToggleFreeze(FoundAddress address, object? freezeValue = null)
        {
            address.IsFrozen = !address.IsFrozen;

            if (address.IsFrozen)
            {
                address.FrozenValue = freezeValue ?? address.CurrentValue;
                StatusChanged?.Invoke(this, $"Frozen {address.Name} at {address.FrozenValue}");
            }
            else
            {
                address.FrozenValue = null;
                StatusChanged?.Invoke(this, $"Unfrozen {address.Name}");
            }
        }

        /// <summary>
        /// Apply a cheat preset
        /// </summary>
        public void ApplyPreset(CheatPreset preset)
        {
            StatusChanged?.Invoke(this, $"Applying preset: {preset.Name}");
            // Note: Presets would need known addresses to work
            // This is a placeholder for when addresses are discovered
        }

        private void OnFreezeTimer(object? sender, ElapsedEventArgs e)
        {
            if (!IsAttached) return;

            foreach (var addr in _foundAddresses.Where(a => a.IsFrozen && a.FrozenValue != null))
            {
                SetValue(addr, addr.FrozenValue!);
            }
        }

        private void OnUpdateTimer(object? sender, ElapsedEventArgs e)
        {
            if (!IsAttached) return;

            foreach (var addr in _foundAddresses.Where(a => !a.IsFrozen))
            {
                UpdateAddressValue(addr);
            }

            AddressesUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateAddressValue(FoundAddress addr)
        {
            addr.CurrentValue = addr.Type switch
            {
                CheatValueType.Int32 => _memory.ReadInt32(addr.Address),
                CheatValueType.Int64 => _memory.ReadInt64(addr.Address),
                CheatValueType.Float => _memory.ReadFloat(addr.Address),
                CheatValueType.Double => _memory.ReadDouble(addr.Address),
                _ => null
            };
            addr.LastUpdated = DateTime.Now;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _freezeTimer.Stop();
                _freezeTimer.Dispose();
                _updateTimer.Stop();
                _updateTimer.Dispose();
                _memory.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~TrainerService()
        {
            Dispose();
        }
    }
}
