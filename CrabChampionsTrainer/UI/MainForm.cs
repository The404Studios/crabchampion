using CrabChampionsTrainer.Core;
using CrabChampionsTrainer.Data;

namespace CrabChampionsTrainer.UI;

public partial class MainForm : Form
{
    private readonly GameManager _gameManager;
    private readonly SettingsManager _settingsManager;
    private readonly Injector _injector;
    private HotkeyManager? _hotkeyManager;
    private OverlayForm? _overlayForm;
    private System.Windows.Forms.Timer? _autoAttachTimer;
    private System.Windows.Forms.Timer? _autoInjectTimer;
    private bool _autoInjectOnStartup = true;
    private bool _overlayEnabled = true;

    // UI Components
    private TabControl _tabControl = null!;
    private RichTextBox _logBox = null!;
    private Label _statusLabel = null!;
    private Panel _statsPanel = null!;

    // Stats labels
    private Label _healthLabel = null!;
    private Label _keysLabel = null!;
    private Label _crystalsLabel = null!;
    private Label _ammoLabel = null!;

    // Toggle states for button colors
    private readonly Dictionary<Button, bool> _toggleStates = new();

    public MainForm()
    {
        _gameManager = new GameManager();
        _settingsManager = new SettingsManager();
        _injector = new Injector();

        InitializeComponent();
        SetupEventHandlers();
        LoadSettings();
        InitializeOverlay();
        StartAutoInject();
    }

    private void InitializeOverlay()
    {
        if (_overlayEnabled)
        {
            _overlayForm = new OverlayForm(_gameManager);
            _overlayForm.Show();
            Log("Overlay initialized");
        }
    }

    private void StartAutoInject()
    {
        if (!_autoInjectOnStartup) return;

        _autoInjectTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        _autoInjectTimer.Tick += async (s, e) =>
        {
            if (!_gameManager.IsAttached)
            {
                // Try to attach
                if (_gameManager.Attach())
                {
                    Log("Auto-attached to game process");

                    // Update overlay with process info
                    _overlayForm?.SetToggleState("Connected", true);
                }
            }
        };
        _autoInjectTimer.Start();
        Log("Auto-inject monitoring started");
    }

    private void InitializeComponent()
    {
        // Form settings
        Text = "Crab Champions Trainer v1.0";
        Size = new Size(800, 650);
        MinimumSize = new Size(700, 550);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(30, 30, 30);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9F);

        // Main layout
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(10),
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); // Header
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Content
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // Log
        Controls.Add(mainLayout);

        // Header panel
        var headerPanel = CreateHeaderPanel();
        mainLayout.Controls.Add(headerPanel, 0, 0);

        // Tab control
        _tabControl = CreateTabControl();
        mainLayout.Controls.Add(_tabControl, 0, 1);

        // Log panel
        var logPanel = CreateLogPanel();
        mainLayout.Controls.Add(logPanel, 0, 2);
    }

    private Panel CreateHeaderPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 45),
            Padding = new Padding(10),
        };

        // Title
        var titleLabel = new Label
        {
            Text = "🦀 CRAB CHAMPIONS TRAINER",
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = Color.FromArgb(255, 100, 50),
            AutoSize = true,
            Location = new Point(10, 10),
        };
        panel.Controls.Add(titleLabel);

        // Status label
        _statusLabel = new Label
        {
            Text = "⚫ Not Attached",
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(10, 45),
        };
        panel.Controls.Add(_statusLabel);

        // Attach button
        var attachButton = CreateStyledButton("Attach to Game", 150);
        attachButton.Location = new Point(panel.Width - 170, 20);
        attachButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        attachButton.Click += (s, e) =>
        {
            if (_gameManager.IsAttached)
            {
                _gameManager.Detach();
                attachButton.Text = "Attach to Game";
            }
            else
            {
                if (_gameManager.Attach())
                {
                    attachButton.Text = "Detach";
                }
            }
        };
        panel.Controls.Add(attachButton);

        // Stats panel
        _statsPanel = new Panel
        {
            Location = new Point(250, 10),
            Size = new Size(300, 60),
            BackColor = Color.FromArgb(35, 35, 35),
        };

        _healthLabel = new Label { Text = "HP: ---", Location = new Point(10, 5), AutoSize = true, ForeColor = Color.LightGreen };
        _keysLabel = new Label { Text = "Keys: ---", Location = new Point(10, 25), AutoSize = true, ForeColor = Color.Gold };
        _crystalsLabel = new Label { Text = "Crystals: ---", Location = new Point(100, 25), AutoSize = true, ForeColor = Color.Cyan };
        _ammoLabel = new Label { Text = "Ammo: ---", Location = new Point(100, 5), AutoSize = true, ForeColor = Color.Orange };

        _statsPanel.Controls.AddRange(new Control[] { _healthLabel, _keysLabel, _crystalsLabel, _ammoLabel });
        panel.Controls.Add(_statsPanel);

        return panel;
    }

    private TabControl CreateTabControl()
    {
        var tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
        };

        // Player tab
        tabControl.TabPages.Add(CreatePlayerTab());

        // Items & Prismatics tab
        tabControl.TabPages.Add(CreateItemsTab());

        // Currency tab
        tabControl.TabPages.Add(CreateCurrencyTab());

        // Weapons tab
        tabControl.TabPages.Add(CreateWeaponsTab());

        // Unlocks tab
        tabControl.TabPages.Add(CreateUnlocksTab());

        // Settings tab
        tabControl.TabPages.Add(CreateSettingsTab());

        return tabControl;
    }

    private TabPage CreatePlayerTab()
    {
        var tab = new TabPage("Player")
        {
            BackColor = Color.FromArgb(40, 40, 40),
            Padding = new Padding(15),
        };

        var flowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
        };

        // Section: Toggles
        flowPanel.Controls.Add(CreateSectionLabel("Player Toggles"));

        var togglePanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            Margin = new Padding(0, 5, 0, 15),
        };

        var godModeBtn = CreateToggleButton("God Mode [F1]", 140);
        godModeBtn.Click += (s, e) => ToggleButton(godModeBtn, () => _gameManager.SetGodMode(GetToggleState(godModeBtn)));
        togglePanel.Controls.Add(godModeBtn);

        var infiniteHealthBtn = CreateToggleButton("Infinite Health [F2]", 140);
        infiniteHealthBtn.Click += (s, e) => ToggleButton(infiniteHealthBtn, () => _gameManager.SetInfiniteHealth(GetToggleState(infiniteHealthBtn)));
        togglePanel.Controls.Add(infiniteHealthBtn);

        var infiniteAmmoBtn = CreateToggleButton("Infinite Ammo [F3]", 140);
        infiniteAmmoBtn.Click += (s, e) => ToggleButton(infiniteAmmoBtn, () => _gameManager.SetInfiniteAmmo(GetToggleState(infiniteAmmoBtn)));
        togglePanel.Controls.Add(infiniteAmmoBtn);

        var noClipBtn = CreateToggleButton("No Clip [F4]", 140);
        noClipBtn.Click += (s, e) => ToggleButton(noClipBtn, () => _gameManager.SetNoClip(GetToggleState(noClipBtn)));
        togglePanel.Controls.Add(noClipBtn);

        var oneHitBtn = CreateToggleButton("One Hit Kill", 140);
        oneHitBtn.Click += (s, e) => ToggleButton(oneHitBtn, () => _gameManager.SetOneHitKill(GetToggleState(oneHitBtn)));
        togglePanel.Controls.Add(oneHitBtn);

        flowPanel.Controls.Add(togglePanel);

        // Section: Multipliers
        flowPanel.Controls.Add(CreateSectionLabel("Multipliers"));

        var multipliersPanel = new TableLayoutPanel
        {
            ColumnCount = 3,
            RowCount = 4,
            AutoSize = true,
            Margin = new Padding(0, 5, 0, 15),
        };

        // Speed
        multipliersPanel.Controls.Add(new Label { Text = "Speed:", AutoSize = true, ForeColor = Color.White, Margin = new Padding(0, 8, 10, 0) }, 0, 0);
        var speedTrack = CreateTrackBar(1, 100, 10);
        speedTrack.ValueChanged += (s, e) => _gameManager.SetSpeedMultiplier(speedTrack.Value / 10f);
        multipliersPanel.Controls.Add(speedTrack, 1, 0);
        var speedLabel = new Label { Text = "1.0x", AutoSize = true, ForeColor = Color.Cyan, Margin = new Padding(5, 8, 0, 0) };
        speedTrack.ValueChanged += (s, e) => speedLabel.Text = $"{speedTrack.Value / 10f:F1}x";
        multipliersPanel.Controls.Add(speedLabel, 2, 0);

        // Damage
        multipliersPanel.Controls.Add(new Label { Text = "Damage:", AutoSize = true, ForeColor = Color.White, Margin = new Padding(0, 8, 10, 0) }, 0, 1);
        var damageTrack = CreateTrackBar(1, 1000, 10);
        damageTrack.ValueChanged += (s, e) => _gameManager.SetDamageMultiplier(damageTrack.Value / 10f);
        multipliersPanel.Controls.Add(damageTrack, 1, 1);
        var damageLabel = new Label { Text = "1.0x", AutoSize = true, ForeColor = Color.Red, Margin = new Padding(5, 8, 0, 0) };
        damageTrack.ValueChanged += (s, e) => damageLabel.Text = $"{damageTrack.Value / 10f:F1}x";
        multipliersPanel.Controls.Add(damageLabel, 2, 1);

        // Fire Rate
        multipliersPanel.Controls.Add(new Label { Text = "Fire Rate:", AutoSize = true, ForeColor = Color.White, Margin = new Padding(0, 8, 10, 0) }, 0, 2);
        var fireRateTrack = CreateTrackBar(1, 100, 10);
        fireRateTrack.ValueChanged += (s, e) => _gameManager.SetFireRateMultiplier(fireRateTrack.Value / 10f);
        multipliersPanel.Controls.Add(fireRateTrack, 1, 2);
        var fireRateLabel = new Label { Text = "1.0x", AutoSize = true, ForeColor = Color.Orange, Margin = new Padding(5, 8, 0, 0) };
        fireRateTrack.ValueChanged += (s, e) => fireRateLabel.Text = $"{fireRateTrack.Value / 10f:F1}x";
        multipliersPanel.Controls.Add(fireRateLabel, 2, 2);

        // Jump
        multipliersPanel.Controls.Add(new Label { Text = "Jump:", AutoSize = true, ForeColor = Color.White, Margin = new Padding(0, 8, 10, 0) }, 0, 3);
        var jumpTrack = CreateTrackBar(1, 100, 10);
        jumpTrack.ValueChanged += (s, e) => _gameManager.SetJumpMultiplier(jumpTrack.Value / 10f);
        multipliersPanel.Controls.Add(jumpTrack, 1, 3);
        var jumpLabel = new Label { Text = "1.0x", AutoSize = true, ForeColor = Color.LightGreen, Margin = new Padding(5, 8, 0, 0) };
        jumpTrack.ValueChanged += (s, e) => jumpLabel.Text = $"{jumpTrack.Value / 10f:F1}x";
        multipliersPanel.Controls.Add(jumpLabel, 2, 3);

        flowPanel.Controls.Add(multipliersPanel);

        // Section: Quick Actions
        flowPanel.Controls.Add(CreateSectionLabel("Quick Actions"));

        var actionsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
        };

        var healBtn = CreateStyledButton("Full Heal", 100);
        healBtn.Click += (s, e) => _gameManager.SetHealth(_gameManager.ReadMaxHealth());
        actionsPanel.Controls.Add(healBtn);

        var maxHealthBtn = CreateStyledButton("Max Health +100", 120);
        maxHealthBtn.Click += (s, e) => _gameManager.SetMaxHealth(_gameManager.ReadMaxHealth() + 100);
        actionsPanel.Controls.Add(maxHealthBtn);

        var resetBtn = CreateStyledButton("Reset All", 100);
        resetBtn.BackColor = Color.FromArgb(150, 50, 50);
        resetBtn.Click += (s, e) => ResetAllToggles();
        actionsPanel.Controls.Add(resetBtn);

        flowPanel.Controls.Add(actionsPanel);

        tab.Controls.Add(flowPanel);
        return tab;
    }

    private TabPage CreateItemsTab()
    {
        var tab = new TabPage("Items & Prismatics")
        {
            BackColor = Color.FromArgb(40, 40, 40),
            Padding = new Padding(15),
        };

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        // Left panel - Prismatics
        var prismaticPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5),
        };

        var prismaticFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
        };

        prismaticFlow.Controls.Add(CreateSectionLabel("✨ PRISMATICS"));

        // GIVE ALL PRISMATICS BUTTON (Main feature requested)
        var giveAllPrismaticsBtn = CreateStyledButton("⭐ GIVE ALL PRISMATICS [F7]", 250);
        giveAllPrismaticsBtn.BackColor = Color.FromArgb(180, 100, 255);
        giveAllPrismaticsBtn.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        giveAllPrismaticsBtn.Height = 45;
        giveAllPrismaticsBtn.Click += (s, e) => _gameManager.GiveAllPrismatics();
        prismaticFlow.Controls.Add(giveAllPrismaticsBtn);

        prismaticFlow.Controls.Add(new Label { Text = "By Category:", ForeColor = Color.Gray, AutoSize = true, Margin = new Padding(0, 10, 0, 5) });

        var combatBtn = CreateStyledButton("Combat Prismatics", 180);
        combatBtn.BackColor = Color.FromArgb(200, 50, 50);
        combatBtn.Click += (s, e) => _gameManager.GivePrismaticsByCategory(PrismaticCategory.Combat);
        prismaticFlow.Controls.Add(combatBtn);

        var defenseBtn = CreateStyledButton("Defense Prismatics", 180);
        defenseBtn.BackColor = Color.FromArgb(50, 150, 50);
        defenseBtn.Click += (s, e) => _gameManager.GivePrismaticsByCategory(PrismaticCategory.Defense);
        prismaticFlow.Controls.Add(defenseBtn);

        var movementBtn = CreateStyledButton("Movement Prismatics", 180);
        movementBtn.BackColor = Color.FromArgb(50, 150, 200);
        movementBtn.Click += (s, e) => _gameManager.GivePrismaticsByCategory(PrismaticCategory.Movement);
        prismaticFlow.Controls.Add(movementBtn);

        var utilityBtn = CreateStyledButton("Utility Prismatics", 180);
        utilityBtn.BackColor = Color.FromArgb(200, 150, 50);
        utilityBtn.Click += (s, e) => _gameManager.GivePrismaticsByCategory(PrismaticCategory.Utility);
        prismaticFlow.Controls.Add(utilityBtn);

        // Individual prismatic dropdown
        prismaticFlow.Controls.Add(new Label { Text = "Individual:", ForeColor = Color.Gray, AutoSize = true, Margin = new Padding(0, 10, 0, 5) });

        var prismaticCombo = new ComboBox
        {
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.White,
        };
        prismaticCombo.Items.AddRange(GameData.Prismatics.Keys.ToArray());
        if (prismaticCombo.Items.Count > 0) prismaticCombo.SelectedIndex = 0;
        prismaticFlow.Controls.Add(prismaticCombo);

        var givePrismaticBtn = CreateStyledButton("Give Selected", 120);
        givePrismaticBtn.Click += (s, e) =>
        {
            if (prismaticCombo.SelectedItem is string name)
                _gameManager.GivePrismatic(name);
        };
        prismaticFlow.Controls.Add(givePrismaticBtn);

        prismaticPanel.Controls.Add(prismaticFlow);
        mainLayout.Controls.Add(prismaticPanel, 0, 0);

        // Right panel - Items
        var itemsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5),
        };

        var itemsFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
        };

        itemsFlow.Controls.Add(CreateSectionLabel("📦 ITEMS"));

        var giveAllItemsBtn = CreateStyledButton("⭐ GIVE ALL ITEMS [F8]", 200);
        giveAllItemsBtn.BackColor = Color.FromArgb(100, 180, 100);
        giveAllItemsBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        giveAllItemsBtn.Height = 40;
        giveAllItemsBtn.Click += (s, e) => _gameManager.GiveAllItems();
        itemsFlow.Controls.Add(giveAllItemsBtn);

        itemsFlow.Controls.Add(new Label { Text = "By Category:", ForeColor = Color.Gray, AutoSize = true, Margin = new Padding(0, 10, 0, 5) });

        foreach (ItemCategory category in Enum.GetValues<ItemCategory>())
        {
            var btn = CreateStyledButton($"{category} Items", 150);
            btn.Click += (s, e) => _gameManager.GiveItemsByCategory(category);
            itemsFlow.Controls.Add(btn);
        }

        // Random loadout
        itemsFlow.Controls.Add(new Label { Text = "Special:", ForeColor = Color.Gray, AutoSize = true, Margin = new Padding(0, 10, 0, 5) });

        var randomBtn = CreateStyledButton("🎲 Random Loadout", 150);
        randomBtn.Click += (s, e) => _gameManager.RandomizeLoadout();
        itemsFlow.Controls.Add(randomBtn);

        var clearBtn = CreateStyledButton("🗑 Clear Inventory", 150);
        clearBtn.BackColor = Color.FromArgb(150, 50, 50);
        clearBtn.Click += (s, e) => _gameManager.ClearInventory();
        itemsFlow.Controls.Add(clearBtn);

        itemsPanel.Controls.Add(itemsFlow);
        mainLayout.Controls.Add(itemsPanel, 1, 0);

        tab.Controls.Add(mainLayout);
        return tab;
    }

    private TabPage CreateCurrencyTab()
    {
        var tab = new TabPage("Currency")
        {
            BackColor = Color.FromArgb(40, 40, 40),
            Padding = new Padding(15),
        };

        var flowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
        };

        // Keys section
        flowPanel.Controls.Add(CreateSectionLabel("🔑 Keys"));

        var keysPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Margin = new Padding(0, 5, 0, 20),
        };

        var keysInput = new NumericUpDown
        {
            Width = 120,
            Maximum = 999999,
            Value = 1000,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 11F),
        };
        keysPanel.Controls.Add(keysInput);

        var setKeysBtn = CreateStyledButton("Set Keys", 100);
        setKeysBtn.Click += (s, e) => _gameManager.SetKeys((int)keysInput.Value);
        keysPanel.Controls.Add(setKeysBtn);

        var add100KeysBtn = CreateStyledButton("+100 [F5]", 80);
        add100KeysBtn.Click += (s, e) => _gameManager.AddKeys(100);
        keysPanel.Controls.Add(add100KeysBtn);

        var add1000KeysBtn = CreateStyledButton("+1000", 80);
        add1000KeysBtn.Click += (s, e) => _gameManager.AddKeys(1000);
        keysPanel.Controls.Add(add1000KeysBtn);

        var maxKeysBtn = CreateStyledButton("MAX", 60);
        maxKeysBtn.BackColor = Color.FromArgb(200, 150, 50);
        maxKeysBtn.Click += (s, e) => _gameManager.SetKeys(999999);
        keysPanel.Controls.Add(maxKeysBtn);

        flowPanel.Controls.Add(keysPanel);

        // Crystals section
        flowPanel.Controls.Add(CreateSectionLabel("💎 Crystals"));

        var crystalsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Margin = new Padding(0, 5, 0, 20),
        };

        var crystalsInput = new NumericUpDown
        {
            Width = 120,
            Maximum = 999999,
            Value = 1000,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.Cyan,
            Font = new Font("Segoe UI", 11F),
        };
        crystalsPanel.Controls.Add(crystalsInput);

        var setCrystalsBtn = CreateStyledButton("Set Crystals", 100);
        setCrystalsBtn.Click += (s, e) => _gameManager.SetCrystals((int)crystalsInput.Value);
        crystalsPanel.Controls.Add(setCrystalsBtn);

        var add100CrystalsBtn = CreateStyledButton("+100 [F6]", 80);
        add100CrystalsBtn.Click += (s, e) => _gameManager.AddCrystals(100);
        crystalsPanel.Controls.Add(add100CrystalsBtn);

        var add1000CrystalsBtn = CreateStyledButton("+1000", 80);
        add1000CrystalsBtn.Click += (s, e) => _gameManager.AddCrystals(1000);
        crystalsPanel.Controls.Add(add1000CrystalsBtn);

        var maxCrystalsBtn = CreateStyledButton("MAX", 60);
        maxCrystalsBtn.BackColor = Color.FromArgb(50, 150, 200);
        maxCrystalsBtn.Click += (s, e) => _gameManager.SetCrystals(999999);
        crystalsPanel.Controls.Add(maxCrystalsBtn);

        flowPanel.Controls.Add(crystalsPanel);

        // Quick Actions
        flowPanel.Controls.Add(CreateSectionLabel("Quick Actions"));

        var quickPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
        };

        var maxAllBtn = CreateStyledButton("💰 MAX ALL CURRENCY", 180);
        maxAllBtn.BackColor = Color.FromArgb(180, 150, 50);
        maxAllBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        maxAllBtn.Click += (s, e) =>
        {
            _gameManager.SetKeys(999999);
            _gameManager.SetCrystals(999999);
        };
        quickPanel.Controls.Add(maxAllBtn);

        var resetCurrencyBtn = CreateStyledButton("Reset to 0", 100);
        resetCurrencyBtn.BackColor = Color.FromArgb(150, 50, 50);
        resetCurrencyBtn.Click += (s, e) =>
        {
            _gameManager.SetKeys(0);
            _gameManager.SetCrystals(0);
        };
        quickPanel.Controls.Add(resetCurrencyBtn);

        flowPanel.Controls.Add(quickPanel);

        tab.Controls.Add(flowPanel);
        return tab;
    }

    private TabPage CreateWeaponsTab()
    {
        var tab = new TabPage("Weapons")
        {
            BackColor = Color.FromArgb(40, 40, 40),
            Padding = new Padding(15),
        };

        var flowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
        };

        // Weapon Toggles
        flowPanel.Controls.Add(CreateSectionLabel("⚔️ Weapon Modifications"));

        var togglePanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            Margin = new Padding(0, 5, 0, 15),
        };

        var dualWieldBtn = CreateToggleButton("Dual Wield", 120);
        dualWieldBtn.Click += (s, e) => ToggleButton(dualWieldBtn, () => _gameManager.SetDualWield(GetToggleState(dualWieldBtn)));
        togglePanel.Controls.Add(dualWieldBtn);

        var infiniteMagBtn = CreateToggleButton("Infinite Mag", 120);
        infiniteMagBtn.Click += (s, e) => ToggleButton(infiniteMagBtn, () => _gameManager.SetInfiniteMagazine(GetToggleState(infiniteMagBtn)));
        togglePanel.Controls.Add(infiniteMagBtn);

        var noReloadBtn = CreateToggleButton("No Reload", 120);
        noReloadBtn.Click += (s, e) => ToggleButton(noReloadBtn, () => _gameManager.SetNoReload(GetToggleState(noReloadBtn)));
        togglePanel.Controls.Add(noReloadBtn);

        var noRecoilBtn = CreateToggleButton("No Recoil", 120);
        noRecoilBtn.Click += (s, e) => ToggleButton(noRecoilBtn, () => _gameManager.SetNoRecoil(GetToggleState(noRecoilBtn)));
        togglePanel.Controls.Add(noRecoilBtn);

        var noSpreadBtn = CreateToggleButton("No Spread", 120);
        noSpreadBtn.Click += (s, e) => ToggleButton(noSpreadBtn, () => _gameManager.SetNoSpread(GetToggleState(noSpreadBtn)));
        togglePanel.Controls.Add(noSpreadBtn);

        var rapidFireBtn = CreateToggleButton("Rapid Fire", 120);
        rapidFireBtn.Click += (s, e) => ToggleButton(rapidFireBtn, () => _gameManager.SetRapidFire(GetToggleState(rapidFireBtn)));
        togglePanel.Controls.Add(rapidFireBtn);

        flowPanel.Controls.Add(togglePanel);

        // Give Weapons
        flowPanel.Controls.Add(CreateSectionLabel("🔫 Give Weapons"));

        var giveAllWeaponsBtn = CreateStyledButton("⭐ GIVE ALL WEAPONS", 180);
        giveAllWeaponsBtn.BackColor = Color.FromArgb(150, 100, 50);
        giveAllWeaponsBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        giveAllWeaponsBtn.Click += (s, e) => _gameManager.GiveAllWeapons();
        flowPanel.Controls.Add(giveAllWeaponsBtn);

        // Weapon category buttons
        var weaponCatPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 15),
        };

        foreach (WeaponCategory category in Enum.GetValues<WeaponCategory>())
        {
            var btn = CreateStyledButton(category.ToString(), 100);
            btn.Click += (s, e) =>
            {
                var weapons = GameData.Weapons.Where(w => w.Value.Category == category).Select(w => w.Key);
                foreach (var weapon in weapons)
                {
                    _gameManager.GiveWeapon(weapon);
                }
            };
            weaponCatPanel.Controls.Add(btn);
        }

        flowPanel.Controls.Add(weaponCatPanel);

        // Individual weapon selection
        flowPanel.Controls.Add(new Label { Text = "Select Weapon:", ForeColor = Color.Gray, AutoSize = true });

        var weaponCombo = new ComboBox
        {
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.White,
        };
        weaponCombo.Items.AddRange(GameData.Weapons.Keys.ToArray());
        if (weaponCombo.Items.Count > 0) weaponCombo.SelectedIndex = 0;
        flowPanel.Controls.Add(weaponCombo);

        var giveWeaponBtn = CreateStyledButton("Give Weapon", 120);
        giveWeaponBtn.Click += (s, e) =>
        {
            if (weaponCombo.SelectedItem is string name)
                _gameManager.GiveWeapon(name);
        };
        flowPanel.Controls.Add(giveWeaponBtn);

        tab.Controls.Add(flowPanel);
        return tab;
    }

    private TabPage CreateUnlocksTab()
    {
        var tab = new TabPage("Unlocks")
        {
            BackColor = Color.FromArgb(40, 40, 40),
            Padding = new Padding(15),
        };

        var flowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
        };

        flowPanel.Controls.Add(CreateSectionLabel("🔓 Unlock Everything"));

        var unlockAllBtn = CreateStyledButton("⭐ UNLOCK EVERYTHING", 220);
        unlockAllBtn.BackColor = Color.FromArgb(200, 150, 50);
        unlockAllBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        unlockAllBtn.Height = 50;
        unlockAllBtn.Click += (s, e) => _gameManager.UnlockEverything();
        flowPanel.Controls.Add(unlockAllBtn);

        flowPanel.Controls.Add(CreateSectionLabel("Individual Unlocks"));

        var skinsBtn = CreateStyledButton("Unlock All Skins", 160);
        skinsBtn.Click += (s, e) => _gameManager.UnlockAllSkins();
        flowPanel.Controls.Add(skinsBtn);

        var cosmeticsBtn = CreateStyledButton("Unlock All Cosmetics", 160);
        cosmeticsBtn.Click += (s, e) => _gameManager.UnlockAllCosmetics();
        flowPanel.Controls.Add(cosmeticsBtn);

        flowPanel.Controls.Add(CreateSectionLabel("Reset"));

        var resetUnlocksBtn = CreateStyledButton("Reset All Unlocks", 160);
        resetUnlocksBtn.BackColor = Color.FromArgb(150, 50, 50);
        resetUnlocksBtn.Click += (s, e) => _gameManager.ResetUnlocks();
        flowPanel.Controls.Add(resetUnlocksBtn);

        // Skin list
        flowPanel.Controls.Add(CreateSectionLabel("Available Skins"));

        var skinList = new ListBox
        {
            Width = 200,
            Height = 150,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.White,
        };
        skinList.Items.AddRange(GameData.Skins);
        flowPanel.Controls.Add(skinList);

        tab.Controls.Add(flowPanel);
        return tab;
    }

    private TabPage CreateSettingsTab()
    {
        var tab = new TabPage("Settings")
        {
            BackColor = Color.FromArgb(40, 40, 40),
            Padding = new Padding(15),
        };

        var flowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
        };

        flowPanel.Controls.Add(CreateSectionLabel("⚙️ General Settings"));

        var autoAttachCheck = new CheckBox
        {
            Text = "Auto-attach to game",
            Checked = _settingsManager.Settings.AutoAttach,
            ForeColor = Color.White,
            AutoSize = true,
        };
        autoAttachCheck.CheckedChanged += (s, e) =>
        {
            _settingsManager.Settings.AutoAttach = autoAttachCheck.Checked;
            _settingsManager.Save();
        };
        flowPanel.Controls.Add(autoAttachCheck);

        var topMostCheck = new CheckBox
        {
            Text = "Always on top",
            Checked = _settingsManager.Settings.TopMost,
            ForeColor = Color.White,
            AutoSize = true,
        };
        topMostCheck.CheckedChanged += (s, e) =>
        {
            _settingsManager.Settings.TopMost = topMostCheck.Checked;
            TopMost = topMostCheck.Checked;
            _settingsManager.Save();
        };
        flowPanel.Controls.Add(topMostCheck);

        var minimizeToTrayCheck = new CheckBox
        {
            Text = "Minimize to system tray",
            Checked = _settingsManager.Settings.MinimizeToTray,
            ForeColor = Color.White,
            AutoSize = true,
        };
        minimizeToTrayCheck.CheckedChanged += (s, e) =>
        {
            _settingsManager.Settings.MinimizeToTray = minimizeToTrayCheck.Checked;
            _settingsManager.Save();
        };
        flowPanel.Controls.Add(minimizeToTrayCheck);

        flowPanel.Controls.Add(CreateSectionLabel("⌨️ Hotkeys"));

        var hotkeyInfo = new Label
        {
            Text = "F1: God Mode | F2: Infinite Health | F3: Infinite Ammo | F4: No Clip\n" +
                   "F5: Add Keys | F6: Add Crystals | F7: All Prismatics | F8: All Items\n" +
                   "NumPad+/-: Speed | F12: Reset All",
            ForeColor = Color.LightGray,
            AutoSize = true,
        };
        flowPanel.Controls.Add(hotkeyInfo);

        flowPanel.Controls.Add(CreateSectionLabel("About"));

        var aboutLabel = new Label
        {
            Text = "Crab Champions Trainer v1.0\n" +
                   "Built with UE4SS research\n\n" +
                   "For single-player use only.\n" +
                   "Use responsibly!",
            ForeColor = Color.Gray,
            AutoSize = true,
        };
        flowPanel.Controls.Add(aboutLabel);

        var resetSettingsBtn = CreateStyledButton("Reset Settings", 120);
        resetSettingsBtn.BackColor = Color.FromArgb(150, 50, 50);
        resetSettingsBtn.Click += (s, e) =>
        {
            _settingsManager.Reset();
            MessageBox.Show("Settings reset to defaults.", "Settings", MessageBoxButtons.OK);
        };
        flowPanel.Controls.Add(resetSettingsBtn);

        tab.Controls.Add(flowPanel);
        return tab;
    }

    private Panel CreateLogPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(25, 25, 25),
            Padding = new Padding(5),
        };

        var label = new Label
        {
            Text = "Log",
            ForeColor = Color.Gray,
            Dock = DockStyle.Top,
            Height = 20,
        };
        panel.Controls.Add(label);

        _logBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 20, 20),
            ForeColor = Color.LightGray,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
        };
        panel.Controls.Add(_logBox);

        return panel;
    }

    #region Helper Methods

    private static Label CreateSectionLabel(string text)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(255, 150, 100),
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 5),
        };
    }

    private static Button CreateStyledButton(string text, int width)
    {
        return new Button
        {
            Text = text,
            Width = width,
            Height = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 60, 60),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Margin = new Padding(3),
        };
    }

    private Button CreateToggleButton(string text, int width)
    {
        var btn = CreateStyledButton(text, width);
        _toggleStates[btn] = false;
        return btn;
    }

    private void ToggleButton(Button btn, Action action)
    {
        _toggleStates[btn] = !_toggleStates[btn];
        btn.BackColor = _toggleStates[btn]
            ? Color.FromArgb(50, 150, 50)
            : Color.FromArgb(60, 60, 60);
        action();
    }

    private bool GetToggleState(Button btn)
    {
        return _toggleStates.TryGetValue(btn, out var state) && state;
    }

    private static TrackBar CreateTrackBar(int min, int max, int value)
    {
        return new TrackBar
        {
            Minimum = min,
            Maximum = max,
            Value = value,
            Width = 200,
            TickFrequency = 10,
            BackColor = Color.FromArgb(40, 40, 40),
        };
    }

    private void ResetAllToggles()
    {
        foreach (var btn in _toggleStates.Keys.ToList())
        {
            _toggleStates[btn] = false;
            btn.BackColor = Color.FromArgb(60, 60, 60);
        }

        _gameManager.SetGodMode(false);
        _gameManager.SetInfiniteHealth(false);
        _gameManager.SetInfiniteAmmo(false);
        _gameManager.SetNoClip(false);
        _gameManager.SetOneHitKill(false);
        _gameManager.SetSpeedMultiplier(1.0f);
        _gameManager.SetDamageMultiplier(1.0f);
        _gameManager.SetFireRateMultiplier(1.0f);

        Log("All modifications reset");
    }

    #endregion

    #region Event Handlers

    private void SetupEventHandlers()
    {
        _gameManager.OnLog += (s, msg) => Log(msg);
        _gameManager.OnStatsUpdated += (s, stats) => UpdateStatsDisplay(stats);
        _gameManager.OnAttached += (s, e) => UpdateConnectionStatus(true);
        _gameManager.OnDetached += (s, e) => UpdateConnectionStatus(false);

        _settingsManager.OnLog += (s, msg) => Log(msg);

        FormClosing += (s, e) =>
        {
            _autoInjectTimer?.Stop();
            _autoInjectTimer?.Dispose();
            _overlayForm?.Close();
            _overlayForm?.Dispose();
            _hotkeyManager?.Dispose();
            _gameManager.Dispose();
        };

        Load += (s, e) =>
        {
            SetupHotkeys();
            if (_settingsManager.Settings.AutoAttach)
            {
                StartAutoAttach();
            }
        };
    }

    private void LoadSettings()
    {
        _settingsManager.Load();
        TopMost = _settingsManager.Settings.TopMost;
    }

    private void SetupHotkeys()
    {
        _hotkeyManager = new HotkeyManager(Handle);
        _hotkeyManager.OnLog += (s, msg) => Log(msg);

        _hotkeyManager.RegisterHotkey(Keys.F1, Keys.None, () => _gameManager.SetGodMode(!_gameManager.IsAttached), "God Mode");
        _hotkeyManager.RegisterHotkey(Keys.F2, Keys.None, () => _gameManager.SetInfiniteHealth(true), "Infinite Health");
        _hotkeyManager.RegisterHotkey(Keys.F3, Keys.None, () => _gameManager.SetInfiniteAmmo(true), "Infinite Ammo");
        _hotkeyManager.RegisterHotkey(Keys.F4, Keys.None, () => _gameManager.SetNoClip(true), "No Clip");
        _hotkeyManager.RegisterHotkey(Keys.F5, Keys.None, () => _gameManager.AddKeys(100), "Add Keys");
        _hotkeyManager.RegisterHotkey(Keys.F6, Keys.None, () => _gameManager.AddCrystals(100), "Add Crystals");
        _hotkeyManager.RegisterHotkey(Keys.F7, Keys.None, () => _gameManager.GiveAllPrismatics(), "Give All Prismatics");
        _hotkeyManager.RegisterHotkey(Keys.F8, Keys.None, () => _gameManager.GiveAllItems(), "Give All Items");
        _hotkeyManager.RegisterHotkey(Keys.F12, Keys.None, ResetAllToggles, "Reset All");
        _hotkeyManager.RegisterHotkey(Keys.Insert, Keys.None, () => _overlayForm?.ToggleMenu(), "Toggle Overlay Menu");
        _hotkeyManager.RegisterHotkey(Keys.Home, Keys.None, () => _overlayForm?.ToggleOverlay(), "Toggle Overlay");
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == HotkeyManager.WM_HOTKEY)
        {
            _hotkeyManager?.ProcessHotkey((int)m.WParam);
        }
        base.WndProc(ref m);
    }

    private void StartAutoAttach()
    {
        _autoAttachTimer = new System.Windows.Forms.Timer
        {
            Interval = _settingsManager.Settings.AutoAttachInterval,
        };
        _autoAttachTimer.Tick += (s, e) =>
        {
            if (!_gameManager.IsAttached)
            {
                _gameManager.Attach();
            }
        };
        _autoAttachTimer.Start();
    }

    private void UpdateConnectionStatus(bool connected)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateConnectionStatus(connected));
            return;
        }

        _statusLabel.Text = connected ? "🟢 Attached" : "⚫ Not Attached";
        _statusLabel.ForeColor = connected ? Color.LightGreen : Color.Gray;
    }

    private void UpdateStatsDisplay(GameStats stats)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateStatsDisplay(stats));
            return;
        }

        _healthLabel.Text = $"HP: {stats.Health:F0}/{stats.MaxHealth:F0}";
        _keysLabel.Text = $"Keys: {stats.Keys}";
        _crystalsLabel.Text = $"Crystals: {stats.Crystals}";
        _ammoLabel.Text = $"Ammo: {stats.CurrentAmmo}/{stats.MaxAmmo}";
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => Log(message));
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _logBox.AppendText($"[{timestamp}] {message}\n");
        _logBox.ScrollToCaret();

        // Limit log size
        if (_logBox.Lines.Length > _settingsManager.Settings.LogMaxLines)
        {
            _logBox.Lines = _logBox.Lines.Skip(100).ToArray();
        }
    }

    #endregion
}
