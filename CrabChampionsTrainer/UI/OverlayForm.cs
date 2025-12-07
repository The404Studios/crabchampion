using System.Runtime.InteropServices;
using CrabChampionsTrainer.Core;

namespace CrabChampionsTrainer.UI;

/// <summary>
/// Transparent overlay window that renders on top of the game
/// Uses a click-through transparent window for the overlay effect
/// </summary>
public class OverlayForm : Form
{
    #region Win32 API

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x80000;
    private const int WS_EX_TRANSPARENT = 0x20;
    private const int WS_EX_TOOLWINDOW = 0x80;
    private const int WS_EX_NOACTIVATE = 0x08000000;

    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;

    #endregion

    private readonly GameManager _gameManager;
    private System.Windows.Forms.Timer _updateTimer = null!;
    private System.Windows.Forms.Timer _positionTimer = null!;

    // Overlay settings
    private bool _showOverlay = true;
    private bool _showStats = true;
    private bool _showMenu = false;
    private float _opacity = 0.85f;

    // Colors
    private readonly Color _backgroundColor = Color.FromArgb(200, 20, 20, 20);
    private readonly Color _accentColor = Color.FromArgb(255, 255, 100, 50);
    private readonly Color _textColor = Color.White;
    private readonly Color _enabledColor = Color.FromArgb(255, 100, 200, 100);
    private readonly Color _disabledColor = Color.FromArgb(255, 150, 150, 150);

    // Fonts
    private Font _titleFont = null!;
    private Font _headerFont = null!;
    private Font _textFont = null!;
    private Font _smallFont = null!;

    // State display
    private readonly Dictionary<string, bool> _toggleStates = new();
    private GameStats _currentStats = new();

    // Target window tracking
    private IntPtr _targetWindow = IntPtr.Zero;
    private int _targetProcessId = 0;

    public OverlayForm(GameManager gameManager)
    {
        _gameManager = gameManager;
        InitializeOverlay();
        SetupTimers();
        SetupEventHandlers();
    }

    private void InitializeOverlay()
    {
        // Form settings for overlay
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        ShowInTaskbar = false;
        BackColor = Color.Black;
        TransparencyKey = Color.Black;
        DoubleBuffered = true;

        // Full screen initially
        var screen = Screen.PrimaryScreen!;
        Location = screen.Bounds.Location;
        Size = screen.Bounds.Size;

        // Initialize fonts
        _titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
        _headerFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        _textFont = new Font("Segoe UI", 10F);
        _smallFont = new Font("Segoe UI", 9F);

        // Initialize toggle states
        _toggleStates["God Mode"] = false;
        _toggleStates["Infinite Health"] = false;
        _toggleStates["Infinite Ammo"] = false;
        _toggleStates["No Clip"] = false;
        _toggleStates["Dual Wield"] = false;
        _toggleStates["Rapid Fire"] = false;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        MakeOverlay();
    }

    private void MakeOverlay()
    {
        // Make window click-through and always on top
        int exStyle = GetWindowLong(Handle, GWL_EXSTYLE);
        SetWindowLong(Handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);

        // Extend frame for transparency
        var margins = new MARGINS { Left = -1, Right = -1, Top = -1, Bottom = -1 };
        DwmExtendFrameIntoClientArea(Handle, ref margins);

        // Set always on top
        SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    private void SetupTimers()
    {
        // Render timer (60 FPS)
        _updateTimer = new System.Windows.Forms.Timer { Interval = 16 };
        _updateTimer.Tick += (s, e) => Invalidate();
        _updateTimer.Start();

        // Position update timer (check target window position)
        _positionTimer = new System.Windows.Forms.Timer { Interval = 100 };
        _positionTimer.Tick += UpdateOverlayPosition;
        _positionTimer.Start();
    }

    private void SetupEventHandlers()
    {
        _gameManager.OnStatsUpdated += (s, stats) => _currentStats = stats;
        _gameManager.OnAttached += (s, e) =>
        {
            if (_gameManager.GameProcess != null)
            {
                _targetProcessId = _gameManager.GameProcess.Id;
            }
        };
        _gameManager.OnDetached += (s, e) =>
        {
            _targetProcessId = 0;
            _targetWindow = IntPtr.Zero;
        };
    }

    private void UpdateOverlayPosition(object? sender, EventArgs e)
    {
        if (_targetProcessId == 0) return;

        try
        {
            // Find target window
            IntPtr foreground = GetForegroundWindow();
            GetWindowThreadProcessId(foreground, out int pid);

            if (pid == _targetProcessId)
            {
                _targetWindow = foreground;
            }

            // Update overlay position to match target window
            if (_targetWindow != IntPtr.Zero && GetWindowRect(_targetWindow, out RECT rect))
            {
                Location = new Point(rect.Left, rect.Top);
                Size = new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);
            }
        }
        catch
        {
            // Ignore errors
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (!_showOverlay) return;

        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // Draw stats panel (top-left)
        if (_showStats)
        {
            DrawStatsPanel(g);
        }

        // Draw menu panel (if enabled)
        if (_showMenu)
        {
            DrawMenuPanel(g);
        }

        // Draw hotkey hints (bottom)
        DrawHotkeyHints(g);
    }

    private void DrawStatsPanel(Graphics g)
    {
        int x = 20;
        int y = 20;
        int width = 220;
        int height = 160;

        // Background
        using var bgBrush = new SolidBrush(_backgroundColor);
        using var borderPen = new Pen(_accentColor, 2);

        g.FillRectangle(bgBrush, x, y, width, height);
        g.DrawRectangle(borderPen, x, y, width, height);

        // Title
        using var titleBrush = new SolidBrush(_accentColor);
        g.DrawString("🦀 CRAB TRAINER", _headerFont, titleBrush, x + 10, y + 8);

        // Connection status
        string status = _gameManager.IsAttached ? "● Connected" : "○ Not Connected";
        Color statusColor = _gameManager.IsAttached ? _enabledColor : _disabledColor;
        using var statusBrush = new SolidBrush(statusColor);
        g.DrawString(status, _smallFont, statusBrush, x + 10, y + 35);

        // Stats
        using var textBrush = new SolidBrush(_textColor);
        int statY = y + 60;
        int lineHeight = 22;

        // Health bar
        DrawStatBar(g, x + 10, statY, width - 20, "HP", _currentStats.Health, _currentStats.MaxHealth, Color.LightGreen);
        statY += lineHeight + 5;

        // Stats text
        g.DrawString($"Keys: {_currentStats.Keys:N0}", _textFont, new SolidBrush(Color.Gold), x + 10, statY);
        statY += lineHeight;

        g.DrawString($"Crystals: {_currentStats.Crystals:N0}", _textFont, new SolidBrush(Color.Cyan), x + 10, statY);
        statY += lineHeight;

        g.DrawString($"Ammo: {_currentStats.CurrentAmmo}/{_currentStats.MaxAmmo}", _textFont, new SolidBrush(Color.Orange), x + 10, statY);
    }

    private void DrawStatBar(Graphics g, int x, int y, int width, string label, float current, float max, Color color)
    {
        int height = 18;
        float percent = max > 0 ? current / max : 0;

        // Background
        using var bgBrush = new SolidBrush(Color.FromArgb(100, 50, 50, 50));
        g.FillRectangle(bgBrush, x, y, width, height);

        // Fill
        using var fillBrush = new SolidBrush(Color.FromArgb(200, color));
        g.FillRectangle(fillBrush, x, y, (int)(width * percent), height);

        // Border
        using var borderPen = new Pen(Color.FromArgb(150, 255, 255, 255));
        g.DrawRectangle(borderPen, x, y, width, height);

        // Text
        string text = $"{label}: {current:F0}/{max:F0}";
        using var textBrush = new SolidBrush(Color.White);
        var textSize = g.MeasureString(text, _smallFont);
        g.DrawString(text, _smallFont, textBrush, x + (width - textSize.Width) / 2, y + 1);
    }

    private void DrawMenuPanel(Graphics g)
    {
        int x = 20;
        int y = 200;
        int width = 280;
        int height = 300;

        // Background
        using var bgBrush = new SolidBrush(_backgroundColor);
        using var borderPen = new Pen(_accentColor, 2);

        g.FillRectangle(bgBrush, x, y, width, height);
        g.DrawRectangle(borderPen, x, y, width, height);

        // Title
        using var titleBrush = new SolidBrush(_accentColor);
        g.DrawString("CHEATS MENU", _headerFont, titleBrush, x + 10, y + 8);

        // Toggle list
        int toggleY = y + 40;
        int lineHeight = 28;

        foreach (var toggle in _toggleStates)
        {
            DrawToggleItem(g, x + 10, toggleY, width - 20, toggle.Key, toggle.Value);
            toggleY += lineHeight;
        }

        // Instructions
        using var hintBrush = new SolidBrush(Color.Gray);
        g.DrawString("Press F1-F6 to toggle options", _smallFont, hintBrush, x + 10, y + height - 30);
    }

    private void DrawToggleItem(Graphics g, int x, int y, int width, string label, bool enabled)
    {
        // Indicator
        Color indicatorColor = enabled ? _enabledColor : _disabledColor;
        using var indicatorBrush = new SolidBrush(indicatorColor);
        g.FillEllipse(indicatorBrush, x, y + 4, 12, 12);

        // Label
        using var textBrush = new SolidBrush(_textColor);
        g.DrawString(label, _textFont, textBrush, x + 20, y);

        // Status
        string status = enabled ? "ON" : "OFF";
        Color statusColor = enabled ? _enabledColor : _disabledColor;
        using var statusBrush = new SolidBrush(statusColor);
        var statusSize = g.MeasureString(status, _textFont);
        g.DrawString(status, _textFont, statusBrush, x + width - statusSize.Width - 10, y);
    }

    private void DrawHotkeyHints(Graphics g)
    {
        if (!_gameManager.IsAttached) return;

        int y = Height - 50;
        string hints = "F1: God | F2: Health | F3: Ammo | F4: NoClip | F5: +Keys | F6: +Crystals | F7: All Prismatics | INSERT: Menu";

        // Background
        using var bgBrush = new SolidBrush(Color.FromArgb(180, 20, 20, 20));
        var textSize = g.MeasureString(hints, _smallFont);
        int x = (Width - (int)textSize.Width) / 2;

        g.FillRectangle(bgBrush, x - 10, y - 5, textSize.Width + 20, textSize.Height + 10);

        // Text
        using var textBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
        g.DrawString(hints, _smallFont, textBrush, x, y);
    }

    #region Public Methods

    public void ToggleOverlay()
    {
        _showOverlay = !_showOverlay;
        Invalidate();
    }

    public void ToggleMenu()
    {
        _showMenu = !_showMenu;
        Invalidate();
    }

    public void ToggleStats()
    {
        _showStats = !_showStats;
        Invalidate();
    }

    public void SetToggleState(string name, bool enabled)
    {
        if (_toggleStates.ContainsKey(name))
        {
            _toggleStates[name] = enabled;
            Invalidate();
        }
    }

    public void SetOpacity(float opacity)
    {
        _opacity = Math.Clamp(opacity, 0.1f, 1.0f);
        Invalidate();
    }

    #endregion

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            return cp;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _updateTimer?.Dispose();
            _positionTimer?.Dispose();
            _titleFont?.Dispose();
            _headerFont?.Dispose();
            _textFont?.Dispose();
            _smallFont?.Dispose();
        }
        base.Dispose(disposing);
    }
}
