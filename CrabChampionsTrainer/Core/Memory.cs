using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CrabChampionsTrainer.Core;

/// <summary>
/// Low-level memory manipulation for reading/writing game memory
/// </summary>
public class Memory : IDisposable
{
    #region Win32 API Imports

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);

    [DllImport("kernel32.dll")]
    private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
    private const uint PAGE_EXECUTE_READWRITE = 0x40;
    private const uint MEM_COMMIT = 0x1000;

    #endregion

    private Process? _process;
    private IntPtr _processHandle;
    private IntPtr _baseAddress;
    private bool _isAttached;
    private bool _disposed;

    public bool IsAttached => _isAttached && _process != null && !_process.HasExited;
    public IntPtr BaseAddress => _baseAddress;
    public Process? GameProcess => _process;

    public event EventHandler<string>? OnLog;
    public event EventHandler? OnAttached;
    public event EventHandler? OnDetached;

    /// <summary>
    /// Attach to the Crab Champions process
    /// </summary>
    public bool Attach()
    {
        try
        {
            var processes = Process.GetProcessesByName("CrabChampions-Win64-Shipping");

            if (processes.Length == 0)
            {
                processes = Process.GetProcessesByName("CrabChampions");
            }

            if (processes.Length == 0)
            {
                Log("Game process not found. Make sure Crab Champions is running.");
                return false;
            }

            _process = processes[0];
            _processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, _process.Id);

            if (_processHandle == IntPtr.Zero)
            {
                Log("Failed to open process. Try running as Administrator.");
                return false;
            }

            _baseAddress = _process.MainModule?.BaseAddress ?? IntPtr.Zero;
            _isAttached = true;

            Log($"Attached to process: {_process.ProcessName} (PID: {_process.Id})");
            Log($"Base Address: 0x{_baseAddress.ToString("X")}");

            OnAttached?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch (Exception ex)
        {
            Log($"Error attaching to process: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Detach from the game process
    /// </summary>
    public void Detach()
    {
        if (_processHandle != IntPtr.Zero)
        {
            CloseHandle(_processHandle);
            _processHandle = IntPtr.Zero;
        }

        _process = null;
        _isAttached = false;

        Log("Detached from process.");
        OnDetached?.Invoke(this, EventArgs.Empty);
    }

    #region Read Methods

    public byte[] ReadBytes(IntPtr address, int size)
    {
        byte[] buffer = new byte[size];
        ReadProcessMemory(_processHandle, address, buffer, size, out _);
        return buffer;
    }

    public byte ReadByte(IntPtr address)
    {
        return ReadBytes(address, 1)[0];
    }

    public short ReadInt16(IntPtr address)
    {
        return BitConverter.ToInt16(ReadBytes(address, 2), 0);
    }

    public int ReadInt32(IntPtr address)
    {
        return BitConverter.ToInt32(ReadBytes(address, 4), 0);
    }

    public long ReadInt64(IntPtr address)
    {
        return BitConverter.ToInt64(ReadBytes(address, 8), 0);
    }

    public float ReadFloat(IntPtr address)
    {
        return BitConverter.ToSingle(ReadBytes(address, 4), 0);
    }

    public double ReadDouble(IntPtr address)
    {
        return BitConverter.ToDouble(ReadBytes(address, 8), 0);
    }

    public bool ReadBool(IntPtr address)
    {
        return ReadByte(address) != 0;
    }

    public IntPtr ReadPointer(IntPtr address)
    {
        return (IntPtr)ReadInt64(address);
    }

    public string ReadString(IntPtr address, int maxLength = 256)
    {
        byte[] buffer = ReadBytes(address, maxLength);
        int nullIndex = Array.IndexOf(buffer, (byte)0);
        if (nullIndex >= 0)
        {
            return Encoding.UTF8.GetString(buffer, 0, nullIndex);
        }
        return Encoding.UTF8.GetString(buffer);
    }

    public string ReadUnicodeString(IntPtr address, int maxLength = 256)
    {
        byte[] buffer = ReadBytes(address, maxLength * 2);
        int nullIndex = -1;
        for (int i = 0; i < buffer.Length - 1; i += 2)
        {
            if (buffer[i] == 0 && buffer[i + 1] == 0)
            {
                nullIndex = i;
                break;
            }
        }
        if (nullIndex >= 0)
        {
            return Encoding.Unicode.GetString(buffer, 0, nullIndex);
        }
        return Encoding.Unicode.GetString(buffer);
    }

    #endregion

    #region Write Methods

    public bool WriteBytes(IntPtr address, byte[] bytes)
    {
        return WriteProcessMemory(_processHandle, address, bytes, bytes.Length, out _);
    }

    public bool WriteByte(IntPtr address, byte value)
    {
        return WriteBytes(address, new[] { value });
    }

    public bool WriteInt16(IntPtr address, short value)
    {
        return WriteBytes(address, BitConverter.GetBytes(value));
    }

    public bool WriteInt32(IntPtr address, int value)
    {
        return WriteBytes(address, BitConverter.GetBytes(value));
    }

    public bool WriteInt64(IntPtr address, long value)
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

    public bool WriteBool(IntPtr address, bool value)
    {
        return WriteByte(address, (byte)(value ? 1 : 0));
    }

    public bool WritePointer(IntPtr address, IntPtr value)
    {
        return WriteInt64(address, value.ToInt64());
    }

    public bool WriteNop(IntPtr address, int count)
    {
        byte[] nops = new byte[count];
        for (int i = 0; i < count; i++)
        {
            nops[i] = 0x90; // NOP instruction
        }
        return WriteBytes(address, nops);
    }

    #endregion

    #region Pointer Chain Resolution

    /// <summary>
    /// Resolve a pointer chain to get the final address
    /// </summary>
    public IntPtr ResolvePointerChain(IntPtr baseAddress, params int[] offsets)
    {
        IntPtr address = baseAddress;

        for (int i = 0; i < offsets.Length; i++)
        {
            address = ReadPointer(address);
            if (address == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            address = IntPtr.Add(address, offsets[i]);
        }

        return address;
    }

    /// <summary>
    /// Resolve a pointer chain from game base address
    /// </summary>
    public IntPtr ResolvePointerChain(int baseOffset, params int[] offsets)
    {
        IntPtr address = IntPtr.Add(_baseAddress, baseOffset);
        return ResolvePointerChain(address, offsets);
    }

    #endregion

    #region Memory Scanning

    /// <summary>
    /// Scan memory for a pattern (AOB scan)
    /// </summary>
    public IntPtr PatternScan(string pattern, IntPtr startAddress, IntPtr endAddress)
    {
        var patternBytes = ParsePattern(pattern, out var mask);

        IntPtr currentAddress = startAddress;
        long size = endAddress.ToInt64() - startAddress.ToInt64();

        while (currentAddress.ToInt64() < endAddress.ToInt64())
        {
            if (VirtualQueryEx(_processHandle, currentAddress, out var memInfo, Marshal.SizeOf<MEMORY_BASIC_INFORMATION>()) == 0)
            {
                break;
            }

            if (memInfo.State == MEM_COMMIT && (memInfo.Protect & 0xFF) != 0)
            {
                byte[] buffer = ReadBytes(memInfo.BaseAddress, (int)memInfo.RegionSize);

                for (int i = 0; i < buffer.Length - patternBytes.Length; i++)
                {
                    bool found = true;
                    for (int j = 0; j < patternBytes.Length; j++)
                    {
                        if (mask[j] && buffer[i + j] != patternBytes[j])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                    {
                        return IntPtr.Add(memInfo.BaseAddress, i);
                    }
                }
            }

            currentAddress = IntPtr.Add(memInfo.BaseAddress, (int)memInfo.RegionSize);
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Scan game module for a pattern
    /// </summary>
    public IntPtr PatternScan(string pattern)
    {
        if (_process?.MainModule == null)
        {
            return IntPtr.Zero;
        }

        IntPtr start = _baseAddress;
        IntPtr end = IntPtr.Add(start, _process.MainModule.ModuleMemorySize);

        return PatternScan(pattern, start, end);
    }

    private static byte[] ParsePattern(string pattern, out bool[] mask)
    {
        var parts = pattern.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var bytes = new byte[parts.Length];
        mask = new bool[parts.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == "?" || parts[i] == "??")
            {
                bytes[i] = 0;
                mask[i] = false;
            }
            else
            {
                bytes[i] = Convert.ToByte(parts[i], 16);
                mask[i] = true;
            }
        }

        return bytes;
    }

    #endregion

    private void Log(string message)
    {
        OnLog?.Invoke(this, message);
    }

    public void Dispose()
    {
        if (_disposed) return;

        Detach();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    ~Memory()
    {
        Dispose();
    }
}
