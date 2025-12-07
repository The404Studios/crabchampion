using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CrabChampionsTrainer.Core;

/// <summary>
/// Handles DLL injection and process manipulation for the trainer
/// </summary>
public class Injector
{
    #region Win32 API

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetExitCodeThread(IntPtr hThread, out IntPtr lpExitCode);

    private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
    private const uint MEM_COMMIT = 0x1000;
    private const uint MEM_RESERVE = 0x2000;
    private const uint MEM_RELEASE = 0x8000;
    private const uint PAGE_READWRITE = 0x04;
    private const uint PAGE_EXECUTE_READWRITE = 0x40;
    private const uint INFINITE = 0xFFFFFFFF;

    #endregion

    public event EventHandler<string>? OnLog;

    /// <summary>
    /// Inject a DLL into the target process
    /// </summary>
    public bool InjectDll(Process process, string dllPath)
    {
        if (!File.Exists(dllPath))
        {
            Log($"DLL not found: {dllPath}");
            return false;
        }

        IntPtr processHandle = IntPtr.Zero;
        IntPtr allocatedMemory = IntPtr.Zero;
        IntPtr threadHandle = IntPtr.Zero;

        try
        {
            // Open the process
            processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            if (processHandle == IntPtr.Zero)
            {
                Log("Failed to open process for injection");
                return false;
            }

            // Get LoadLibraryA address
            IntPtr kernel32 = GetModuleHandle("kernel32.dll");
            IntPtr loadLibraryAddr = GetProcAddress(kernel32, "LoadLibraryA");

            if (loadLibraryAddr == IntPtr.Zero)
            {
                Log("Failed to get LoadLibraryA address");
                return false;
            }

            // Allocate memory in target process for DLL path
            byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath + "\0");
            allocatedMemory = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)dllPathBytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            if (allocatedMemory == IntPtr.Zero)
            {
                Log("Failed to allocate memory in target process");
                return false;
            }

            // Write DLL path to allocated memory
            if (!WriteProcessMemory(processHandle, allocatedMemory, dllPathBytes, (uint)dllPathBytes.Length, out _))
            {
                Log("Failed to write DLL path to target process");
                return false;
            }

            // Create remote thread to call LoadLibraryA
            threadHandle = CreateRemoteThread(processHandle, IntPtr.Zero, 0, loadLibraryAddr, allocatedMemory, 0, out _);

            if (threadHandle == IntPtr.Zero)
            {
                Log("Failed to create remote thread");
                return false;
            }

            // Wait for the thread to complete
            WaitForSingleObject(threadHandle, INFINITE);

            // Check if injection succeeded
            GetExitCodeThread(threadHandle, out IntPtr exitCode);

            if (exitCode == IntPtr.Zero)
            {
                Log("DLL injection failed - LoadLibrary returned null");
                return false;
            }

            Log($"Successfully injected: {Path.GetFileName(dllPath)}");
            return true;
        }
        catch (Exception ex)
        {
            Log($"Injection error: {ex.Message}");
            return false;
        }
        finally
        {
            // Cleanup
            if (allocatedMemory != IntPtr.Zero && processHandle != IntPtr.Zero)
            {
                VirtualFreeEx(processHandle, allocatedMemory, 0, MEM_RELEASE);
            }

            if (threadHandle != IntPtr.Zero)
            {
                CloseHandle(threadHandle);
            }

            if (processHandle != IntPtr.Zero)
            {
                CloseHandle(processHandle);
            }
        }
    }

    /// <summary>
    /// Inject shellcode into the target process
    /// </summary>
    public bool InjectShellcode(Process process, byte[] shellcode)
    {
        IntPtr processHandle = IntPtr.Zero;
        IntPtr allocatedMemory = IntPtr.Zero;
        IntPtr threadHandle = IntPtr.Zero;

        try
        {
            processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            if (processHandle == IntPtr.Zero)
            {
                Log("Failed to open process for shellcode injection");
                return false;
            }

            // Allocate executable memory
            allocatedMemory = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)shellcode.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

            if (allocatedMemory == IntPtr.Zero)
            {
                Log("Failed to allocate executable memory");
                return false;
            }

            // Write shellcode
            if (!WriteProcessMemory(processHandle, allocatedMemory, shellcode, (uint)shellcode.Length, out _))
            {
                Log("Failed to write shellcode");
                return false;
            }

            // Execute shellcode
            threadHandle = CreateRemoteThread(processHandle, IntPtr.Zero, 0, allocatedMemory, IntPtr.Zero, 0, out _);

            if (threadHandle == IntPtr.Zero)
            {
                Log("Failed to execute shellcode");
                return false;
            }

            Log("Shellcode injected and executed");
            return true;
        }
        catch (Exception ex)
        {
            Log($"Shellcode injection error: {ex.Message}");
            return false;
        }
        finally
        {
            if (threadHandle != IntPtr.Zero)
            {
                CloseHandle(threadHandle);
            }

            if (processHandle != IntPtr.Zero)
            {
                CloseHandle(processHandle);
            }
        }
    }

    /// <summary>
    /// Check if a module is loaded in the target process
    /// </summary>
    public bool IsModuleLoaded(Process process, string moduleName)
    {
        try
        {
            process.Refresh();
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Wait for a process to start
    /// </summary>
    public async Task<Process?> WaitForProcessAsync(string processName, int timeoutMs = 30000, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                Log($"Found process: {processName}");
                return processes[0];
            }

            await Task.Delay(500, cancellationToken);
        }

        Log($"Timeout waiting for process: {processName}");
        return null;
    }

    private void Log(string message)
    {
        OnLog?.Invoke(this, $"[Injector] {message}");
    }
}
