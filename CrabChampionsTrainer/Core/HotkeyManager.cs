using System.Runtime.InteropServices;

namespace CrabChampionsTrainer.Core;

/// <summary>
/// Global hotkey manager for trainer shortcuts
/// </summary>
public class HotkeyManager : IDisposable
{
    #region Win32 API

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    // Modifier keys
    public const uint MOD_NONE = 0x0000;
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;
    public const uint MOD_NOREPEAT = 0x4000;

    // Virtual key codes
    public const int VK_F1 = 0x70;
    public const int VK_F2 = 0x71;
    public const int VK_F3 = 0x72;
    public const int VK_F4 = 0x73;
    public const int VK_F5 = 0x74;
    public const int VK_F6 = 0x75;
    public const int VK_F7 = 0x76;
    public const int VK_F8 = 0x77;
    public const int VK_F9 = 0x78;
    public const int VK_F10 = 0x79;
    public const int VK_F11 = 0x7A;
    public const int VK_F12 = 0x7B;

    public const int VK_NUMPAD0 = 0x60;
    public const int VK_NUMPAD1 = 0x61;
    public const int VK_NUMPAD2 = 0x62;
    public const int VK_NUMPAD3 = 0x63;
    public const int VK_NUMPAD4 = 0x64;
    public const int VK_NUMPAD5 = 0x65;
    public const int VK_NUMPAD6 = 0x66;
    public const int VK_NUMPAD7 = 0x67;
    public const int VK_NUMPAD8 = 0x68;
    public const int VK_NUMPAD9 = 0x69;
    public const int VK_ADD = 0x6B;
    public const int VK_SUBTRACT = 0x6D;
    public const int VK_MULTIPLY = 0x6A;
    public const int VK_DIVIDE = 0x6F;

    public const int VK_INSERT = 0x2D;
    public const int VK_DELETE = 0x2E;
    public const int VK_HOME = 0x24;
    public const int VK_END = 0x23;
    public const int VK_PRIOR = 0x21; // Page Up
    public const int VK_NEXT = 0x22;  // Page Down

    public const int WM_HOTKEY = 0x0312;

    #endregion

    private readonly IntPtr _windowHandle;
    private readonly Dictionary<int, HotkeyInfo> _hotkeys = new();
    private int _nextId = 1;
    private bool _disposed;

    public event EventHandler<string>? OnLog;

    public HotkeyManager(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
    }

    /// <summary>
    /// Register a global hotkey
    /// </summary>
    public int RegisterHotkey(Keys key, Keys modifiers, Action callback, string description)
    {
        int id = _nextId++;
        uint mod = KeysToModifiers(modifiers);
        uint vk = (uint)key;

        if (RegisterHotKey(_windowHandle, id, mod | MOD_NOREPEAT, vk))
        {
            _hotkeys[id] = new HotkeyInfo(id, key, modifiers, callback, description);
            Log($"Registered hotkey: {description} ({GetHotkeyString(key, modifiers)})");
            return id;
        }
        else
        {
            Log($"Failed to register hotkey: {description}");
            return -1;
        }
    }

    /// <summary>
    /// Unregister a hotkey by ID
    /// </summary>
    public void UnregisterHotkey(int id)
    {
        if (_hotkeys.TryGetValue(id, out var info))
        {
            UnregisterHotKey(_windowHandle, id);
            _hotkeys.Remove(id);
            Log($"Unregistered hotkey: {info.Description}");
        }
    }

    /// <summary>
    /// Unregister all hotkeys
    /// </summary>
    public void UnregisterAll()
    {
        foreach (var id in _hotkeys.Keys.ToList())
        {
            UnregisterHotkey(id);
        }
    }

    /// <summary>
    /// Process a hotkey message
    /// </summary>
    public bool ProcessHotkey(int id)
    {
        if (_hotkeys.TryGetValue(id, out var info))
        {
            try
            {
                info.Callback();
                return true;
            }
            catch (Exception ex)
            {
                Log($"Hotkey error: {ex.Message}");
            }
        }
        return false;
    }

    /// <summary>
    /// Check if a key is currently pressed
    /// </summary>
    public static bool IsKeyPressed(int vKey)
    {
        return (GetAsyncKeyState(vKey) & 0x8000) != 0;
    }

    /// <summary>
    /// Get all registered hotkeys
    /// </summary>
    public IEnumerable<HotkeyInfo> GetRegisteredHotkeys()
    {
        return _hotkeys.Values;
    }

    /// <summary>
    /// Get a display string for a hotkey combination
    /// </summary>
    public static string GetHotkeyString(Keys key, Keys modifiers)
    {
        var parts = new List<string>();

        if ((modifiers & Keys.Control) != 0) parts.Add("Ctrl");
        if ((modifiers & Keys.Shift) != 0) parts.Add("Shift");
        if ((modifiers & Keys.Alt) != 0) parts.Add("Alt");

        parts.Add(key.ToString());

        return string.Join("+", parts);
    }

    private static uint KeysToModifiers(Keys modifiers)
    {
        uint mod = MOD_NONE;

        if ((modifiers & Keys.Control) != 0) mod |= MOD_CONTROL;
        if ((modifiers & Keys.Shift) != 0) mod |= MOD_SHIFT;
        if ((modifiers & Keys.Alt) != 0) mod |= MOD_ALT;

        return mod;
    }

    private void Log(string message)
    {
        OnLog?.Invoke(this, $"[Hotkeys] {message}");
    }

    public void Dispose()
    {
        if (_disposed) return;

        UnregisterAll();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    ~HotkeyManager()
    {
        Dispose();
    }
}

public record HotkeyInfo(int Id, Keys Key, Keys Modifiers, Action Callback, string Description);
