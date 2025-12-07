using Newtonsoft.Json;

namespace CrabChampionsTrainer.Core;

/// <summary>
/// Manages trainer settings and persistence
/// </summary>
public class SettingsManager
{
    private const string SettingsFileName = "trainer_settings.json";
    private readonly string _settingsPath;

    public TrainerSettings Settings { get; private set; }

    public event EventHandler<string>? OnLog;

    public SettingsManager()
    {
        _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
        Settings = new TrainerSettings();
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                Settings = JsonConvert.DeserializeObject<TrainerSettings>(json) ?? new TrainerSettings();
                Log("Settings loaded successfully");
            }
            else
            {
                Settings = new TrainerSettings();
                Save(); // Create default settings file
                Log("Created default settings");
            }
        }
        catch (Exception ex)
        {
            Log($"Error loading settings: {ex.Message}");
            Settings = new TrainerSettings();
        }
    }

    public void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(_settingsPath, json);
            Log("Settings saved successfully");
        }
        catch (Exception ex)
        {
            Log($"Error saving settings: {ex.Message}");
        }
    }

    public void Reset()
    {
        Settings = new TrainerSettings();
        Save();
        Log("Settings reset to defaults");
    }

    private void Log(string message)
    {
        OnLog?.Invoke(this, $"[Settings] {message}");
    }
}

/// <summary>
/// Trainer settings
/// </summary>
public class TrainerSettings
{
    // Auto-attach settings
    public bool AutoAttach { get; set; } = true;
    public int AutoAttachInterval { get; set; } = 2000; // ms

    // Startup settings
    public bool StartMinimized { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public bool TopMost { get; set; } = false;

    // Default values
    public float DefaultSpeedMultiplier { get; set; } = 1.0f;
    public float DefaultDamageMultiplier { get; set; } = 1.0f;
    public float DefaultFireRateMultiplier { get; set; } = 1.0f;
    public float DefaultJumpMultiplier { get; set; } = 1.0f;

    // Currency settings
    public int KeysPerAdd { get; set; } = 100;
    public int CrystalsPerAdd { get; set; } = 100;

    // Auto-enable on attach
    public bool AutoGodMode { get; set; } = false;
    public bool AutoInfiniteAmmo { get; set; } = false;
    public bool AutoInfiniteHealth { get; set; } = false;

    // Hotkey settings
    public HotkeySettings Hotkeys { get; set; } = new();

    // UI settings
    public bool ShowLogPanel { get; set; } = true;
    public bool ShowStatsPanel { get; set; } = true;
    public int LogMaxLines { get; set; } = 500;

    // Theme
    public bool DarkMode { get; set; } = true;
}

/// <summary>
/// Hotkey configuration
/// </summary>
public class HotkeySettings
{
    public string GodMode { get; set; } = "F1";
    public string InfiniteHealth { get; set; } = "F2";
    public string InfiniteAmmo { get; set; } = "F3";
    public string NoClip { get; set; } = "F4";
    public string AddKeys { get; set; } = "F5";
    public string AddCrystals { get; set; } = "F6";
    public string GiveAllPrismatics { get; set; } = "F7";
    public string GiveAllItems { get; set; } = "F8";
    public string SpeedUp { get; set; } = "NumPadPlus";
    public string SpeedDown { get; set; } = "NumPadMinus";
    public string ResetAll { get; set; } = "F12";
}
