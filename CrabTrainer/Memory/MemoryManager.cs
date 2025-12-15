using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CrabTrainer.Memory
{
    /// <summary>
    /// Handles reading and writing memory to the target process
    /// </summary>
    public class MemoryManager : IDisposable
    {
        private Process? _process;
        private IntPtr _processHandle;
        private bool _disposed;

        // Windows API imports for memory manipulation
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        // Process access rights
        private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const uint PROCESS_VM_READ = 0x0010;
        private const uint PROCESS_VM_WRITE = 0x0020;
        private const uint PROCESS_VM_OPERATION = 0x0008;
        private const uint PROCESS_QUERY_INFORMATION = 0x0400;

        // Memory protection constants
        private const uint PAGE_READWRITE = 0x04;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        public bool IsAttached => _process != null && !_process.HasExited && _processHandle != IntPtr.Zero;
        public Process? AttachedProcess => _process;
        public string ProcessName => _process?.ProcessName ?? "None";
        public int ProcessId => _process?.Id ?? 0;
        public IntPtr BaseAddress => _process?.MainModule?.BaseAddress ?? IntPtr.Zero;

        /// <summary>
        /// Find and attach to the Crab Champions process
        /// </summary>
        public bool AttachToGame()
        {
            Detach();

            // Try different possible process names
            string[] possibleNames = { "CrabChampions", "Crab Champions", "CrabChampions-Win64-Shipping" };

            foreach (var name in possibleNames)
            {
                var processes = Process.GetProcessesByName(name);
                if (processes.Length > 0)
                {
                    return AttachToProcess(processes[0]);
                }
            }

            // Also try to find by window title
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    if (proc.MainWindowTitle.Contains("Crab Champions", StringComparison.OrdinalIgnoreCase))
                    {
                        return AttachToProcess(proc);
                    }
                }
                catch { }
            }

            return false;
        }

        /// <summary>
        /// Attach to a specific process
        /// </summary>
        public bool AttachToProcess(Process process)
        {
            try
            {
                Detach();

                _process = process;
                _processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);

                if (_processHandle == IntPtr.Zero)
                {
                    // Try with reduced access
                    _processHandle = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);
                }

                return _processHandle != IntPtr.Zero;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Detach from the current process
        /// </summary>
        public void Detach()
        {
            if (_processHandle != IntPtr.Zero)
            {
                CloseHandle(_processHandle);
                _processHandle = IntPtr.Zero;
            }
            _process = null;
        }

        /// <summary>
        /// Read bytes from memory
        /// </summary>
        public byte[]? ReadBytes(IntPtr address, int size)
        {
            if (!IsAttached) return null;

            var buffer = new byte[size];
            if (ReadProcessMemory(_processHandle, address, buffer, size, out int bytesRead) && bytesRead == size)
            {
                return buffer;
            }
            return null;
        }

        /// <summary>
        /// Write bytes to memory
        /// </summary>
        public bool WriteBytes(IntPtr address, byte[] data)
        {
            if (!IsAttached) return false;

            // Try to make memory writable
            VirtualProtectEx(_processHandle, address, (UIntPtr)data.Length, PAGE_EXECUTE_READWRITE, out uint oldProtect);

            bool result = WriteProcessMemory(_processHandle, address, data, data.Length, out int bytesWritten);

            // Restore protection
            VirtualProtectEx(_processHandle, address, (UIntPtr)data.Length, oldProtect, out _);

            return result && bytesWritten == data.Length;
        }

        // Type-specific read methods
        public int? ReadInt32(IntPtr address)
        {
            var bytes = ReadBytes(address, 4);
            return bytes != null ? BitConverter.ToInt32(bytes, 0) : null;
        }

        public float? ReadFloat(IntPtr address)
        {
            var bytes = ReadBytes(address, 4);
            return bytes != null ? BitConverter.ToSingle(bytes, 0) : null;
        }

        public double? ReadDouble(IntPtr address)
        {
            var bytes = ReadBytes(address, 8);
            return bytes != null ? BitConverter.ToDouble(bytes, 0) : null;
        }

        public long? ReadInt64(IntPtr address)
        {
            var bytes = ReadBytes(address, 8);
            return bytes != null ? BitConverter.ToInt64(bytes, 0) : null;
        }

        public IntPtr? ReadPointer(IntPtr address)
        {
            var bytes = ReadBytes(address, IntPtr.Size);
            if (bytes == null) return null;
            return IntPtr.Size == 8
                ? new IntPtr(BitConverter.ToInt64(bytes, 0))
                : new IntPtr(BitConverter.ToInt32(bytes, 0));
        }

        public string? ReadString(IntPtr address, int maxLength = 256)
        {
            var bytes = ReadBytes(address, maxLength);
            if (bytes == null) return null;

            int length = Array.IndexOf(bytes, (byte)0);
            if (length < 0) length = maxLength;
            return Encoding.ASCII.GetString(bytes, 0, length);
        }

        public string? ReadUnicodeString(IntPtr address, int maxLength = 256)
        {
            var bytes = ReadBytes(address, maxLength * 2);
            if (bytes == null) return null;

            int length = 0;
            for (int i = 0; i < bytes.Length - 1; i += 2)
            {
                if (bytes[i] == 0 && bytes[i + 1] == 0)
                {
                    length = i;
                    break;
                }
            }
            if (length == 0) length = bytes.Length;
            return Encoding.Unicode.GetString(bytes, 0, length);
        }

        // Type-specific write methods
        public bool WriteInt32(IntPtr address, int value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public bool WriteFloat(IntPtr address, float value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public bool WriteDouble(IntPtr address, double value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        public bool WriteInt64(IntPtr address, long value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Search for an integer value in memory
        /// </summary>
        public List<IntPtr> ScanForInt32(int value, IntPtr startAddress, long size)
        {
            var results = new List<IntPtr>();
            if (!IsAttached) return results;

            var targetBytes = BitConverter.GetBytes(value);
            var buffer = new byte[4096];

            for (long offset = 0; offset < size; offset += buffer.Length - 3)
            {
                var currentAddress = IntPtr.Add(startAddress, (int)offset);
                if (ReadProcessMemory(_processHandle, currentAddress, buffer, buffer.Length, out int bytesRead))
                {
                    for (int i = 0; i < bytesRead - 3; i++)
                    {
                        if (buffer[i] == targetBytes[0] &&
                            buffer[i + 1] == targetBytes[1] &&
                            buffer[i + 2] == targetBytes[2] &&
                            buffer[i + 3] == targetBytes[3])
                        {
                            results.Add(IntPtr.Add(currentAddress, i));
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Search for a float value in memory
        /// </summary>
        public List<IntPtr> ScanForFloat(float value, IntPtr startAddress, long size, float tolerance = 0.01f)
        {
            var results = new List<IntPtr>();
            if (!IsAttached) return results;

            var buffer = new byte[4096];

            for (long offset = 0; offset < size; offset += buffer.Length - 3)
            {
                var currentAddress = IntPtr.Add(startAddress, (int)offset);
                if (ReadProcessMemory(_processHandle, currentAddress, buffer, buffer.Length, out int bytesRead))
                {
                    for (int i = 0; i < bytesRead - 3; i += 4)
                    {
                        float readValue = BitConverter.ToSingle(buffer, i);
                        if (Math.Abs(readValue - value) <= tolerance)
                        {
                            results.Add(IntPtr.Add(currentAddress, i));
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Follow a pointer chain to get final address
        /// </summary>
        public IntPtr? FollowPointerChain(IntPtr baseAddress, int[] offsets)
        {
            var current = baseAddress;

            for (int i = 0; i < offsets.Length; i++)
            {
                var pointer = ReadPointer(current);
                if (pointer == null || pointer.Value == IntPtr.Zero)
                    return null;

                current = IntPtr.Add(pointer.Value, offsets[i]);
            }

            return current;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Detach();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~MemoryManager()
        {
            Dispose();
        }
    }
}
