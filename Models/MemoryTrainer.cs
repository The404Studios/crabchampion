using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace CrabChampionsSaveEditor.Models
{
    /// <summary>
    /// Memory trainer for live game modification of Crab Champions.
    /// Provides utilities for reading/writing game memory and injecting items.
    ///
    /// ARCHITECTURE NOTES:
    /// - Crab Champions is built on Unreal Engine 4
    /// - Game uses UE4 reflection system for enums (ECrabPerkType, ECrabRank, etc.)
    /// - Items are stored as UObject pointers in TArray structures
    /// - Perks/Mods/Relics use data asset references (DA_Perk_*, DA_WeaponMod_*, etc.)
    /// </summary>
    public class MemoryTrainer : IDisposable
    {
        #region Win32 API Imports

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

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

        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_OPERATION = 0x0008;
        private const uint MEM_COMMIT = 0x1000;
        private const uint PAGE_READWRITE = 0x04;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;
        private const uint PAGE_EXECUTE_READ = 0x20;
        private const uint PAGE_READONLY = 0x02;
        private const uint MEM_RESERVE = 0x2000;
        private const uint MEM_RELEASE = 0x8000;

        // Additional Win32 imports for remote thread execution
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        private static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

        private const uint INFINITE = 0xFFFFFFFF;
        private const uint WAIT_OBJECT_0 = 0x00000000;

        #endregion

        #region Properties

        public Process? GameProcess { get; private set; }
        public IntPtr ProcessHandle { get; private set; }
        public bool IsAttached => GameProcess != null && !GameProcess.HasExited && ProcessHandle != IntPtr.Zero;
        public IntPtr BaseAddress => GameProcess?.MainModule?.BaseAddress ?? IntPtr.Zero;
        public string GameVersion { get; private set; } = "Unknown";

        // Cached addresses (found during initialization)
        public IntPtr GEngineAddress { get; private set; }
        public IntPtr UWorldAddress { get; private set; }
        public IntPtr GNamesAddress { get; private set; }
        public IntPtr PlayerControllerAddress { get; private set; }

        // Events
        public event EventHandler<string>? StatusChanged;
        public event EventHandler<string>? ErrorOccurred;
        public event EventHandler? GameAttached;
        public event EventHandler? GameDetached;

        #endregion

        #region UE4 Enum Definitions (from Reverse Engineering)

        /// <summary>
        /// ECrabPerkType enum values from game binary (.rdata section)
        /// These are used by UE4 reflection system for perk identification
        /// </summary>
        public static class ECrabPerkType
        {
            // Common Perks
            public const string Mango = "ECrabPerkType::Mango";
            public const string Banana = "ECrabPerkType::Banana";
            public const string GlassCannon = "ECrabPerkType::GlassCannon";
            public const string Juggernaut = "ECrabPerkType::Juggernaut";
            public const string SpeedDemon = "ECrabPerkType::SpeedDemon";
            public const string Regenerator = "ECrabPerkType::Regenerator";
            public const string Bulletproof = "ECrabPerkType::Bulletproof";
            public const string Sharpshooter = "ECrabPerkType::Sharpshooter";
            public const string HeavyHitter = "ECrabPerkType::HeavyHitter";
            public const string Firestarter = "ECrabPerkType::Firestarter";
            public const string IceCold = "ECrabPerkType::IceCold";
            public const string HighVoltage = "ECrabPerkType::HighVoltage";
            public const string Toxic = "ECrabPerkType::Toxic";
            public const string PotentMagic = "ECrabPerkType::PotentMagic";
            public const string Fortitude = "ECrabPerkType::Fortitude";
            public const string Vitality = "ECrabPerkType::Vitality";
            public const string Endurance = "ECrabPerkType::Endurance";
            public const string Stamina = "ECrabPerkType::Stamina";

            // Epic Perks
            public const string MegaCrit = "ECrabPerkType::MegaCrit";
            public const string Assassin = "ECrabPerkType::Assassin";
            public const string Survivor = "ECrabPerkType::Survivor";
            public const string Collector = "ECrabPerkType::Collector";
            public const string DoubleVision = "ECrabPerkType::DoubleVision";
            public const string ExplodingEnemies = "ECrabPerkType::ExplodingEnemies";
            public const string HealthIsPower = "ECrabPerkType::HealthIsPower";
            public const string MoneyIsPower = "ECrabPerkType::MoneyIsPower";
            public const string SpeedIsPower = "ECrabPerkType::SpeedIsPower";

            // Legendary Perks
            public const string DaggerDash = "ECrabPerkType::DaggerDash";
            public const string IceDash = "ECrabPerkType::IceDash";
            public const string LightningDash = "ECrabPerkType::LightningDash";
            public const string Powerslide = "ECrabPerkType::Powerslide";
            public const string FlammableEnemies = "ECrabPerkType::FlammableEnemies";
            public const string FreezingEnemies = "ECrabPerkType::FreezingEnemies";
            public const string PoisonousEnemies = "ECrabPerkType::PoisonousEnemies";

            // Get all perk type strings for scanning
            public static readonly string[] AllValues = new[]
            {
                Mango, Banana, GlassCannon, Juggernaut, SpeedDemon, Regenerator, Bulletproof,
                Sharpshooter, HeavyHitter, Firestarter, IceCold, HighVoltage, Toxic, PotentMagic,
                Fortitude, Vitality, Endurance, Stamina, MegaCrit, Assassin, Survivor, Collector,
                DoubleVision, ExplodingEnemies, HealthIsPower, MoneyIsPower, SpeedIsPower,
                DaggerDash, IceDash, LightningDash, Powerslide, FlammableEnemies, FreezingEnemies, PoisonousEnemies
            };
        }

        /// <summary>
        /// ECrabRank enum values for weapon/item rarity
        /// </summary>
        public static class ECrabRank
        {
            public const string Common = "ECrabRank::Common";
            public const string Uncommon = "ECrabRank::Uncommon";
            public const string Rare = "ECrabRank::Rare";
            public const string Epic = "ECrabRank::Epic";
            public const string Legendary = "ECrabRank::Legendary";
            public const string Prismatic = "ECrabRank::Prismatic";

            public static readonly string[] AllValues = new[]
            {
                Common, Uncommon, Rare, Epic, Legendary, Prismatic
            };
        }

        /// <summary>
        /// ECrabCosmeticType enum values for cosmetic items
        /// </summary>
        public static class ECrabCosmeticType
        {
            public const string CrabSkin = "ECrabCosmeticType::CrabSkin";
            public const string WeaponSkin = "ECrabCosmeticType::WeaponSkin";
            public const string Emote = "ECrabCosmeticType::Emote";
            public const string Banner = "ECrabCosmeticType::Banner";
            public const string Title = "ECrabCosmeticType::Title";
        }

        /// <summary>
        /// ECrabTurretType enum values for turret types (from .rdata:0x14328DA00)
        /// </summary>
        public static class ECrabTurretType
        {
            public const string None = "ECrabTurretType::None";
            public const string Sentry = "ECrabTurretType::Sentry";
            public const string Sniper = "ECrabTurretType::Sniper";
            public const string Mortar = "ECrabTurretType::Mortar";
            public const string Wave = "ECrabTurretType::Wave";
            public const string Beam = "ECrabTurretType::Beam";

            public static readonly string[] AllValues = new[]
            {
                None, Sentry, Sniper, Mortar, Wave, Beam
            };
        }

        /// <summary>
        /// ECrabEnhanceableType enum - whether items can be enhanced (from .rdata:0x1432987C0)
        /// </summary>
        public static class ECrabEnhanceableType
        {
            public const string NotEnhanceable = "ECrabEnhanceableType::NotEnhanceable";
            public const string Enhanceable = "ECrabEnhanceableType::Enhanceable";
            public const string EnhanceableNonElemental = "ECrabEnhanceableType::EnhanceableNonElemental";
            public const string EnhanceableNonHoming = "ECrabEnhanceableType::EnhanceableNonHoming";

            public static readonly string[] AllValues = new[]
            {
                NotEnhanceable, Enhanceable, EnhanceableNonElemental, EnhanceableNonHoming
            };
        }

        /// <summary>
        /// ECrabPickupTag enum - tags for categorizing pickups (from .rdata:0x143298890)
        /// Used for filtering/searching items by elemental type or effect
        /// </summary>
        public static class ECrabPickupTag
        {
            public const string None = "ECrabPickupTag::None";               // 0
            public const string Healing = "ECrabPickupTag::Healing";         // 1
            public const string DamageOverTime = "ECrabPickupTag::DamageOverTime"; // 2
            public const string Critical = "ECrabPickupTag::Critical";       // 3
            public const string Speed = "ECrabPickupTag::Speed";             // 4
            public const string Bounce = "ECrabPickupTag::Bounce";           // 5
            public const string Ice = "ECrabPickupTag::Ice";                 // 6
            public const string Fire = "ECrabPickupTag::Fire";               // 7
            public const string Lightning = "ECrabPickupTag::Lightning";     // 8
            public const string Poison = "ECrabPickupTag::Poison";           // 9
            public const string Arcane = "ECrabPickupTag::Arcane";           // 10
            public const string Turret = "ECrabPickupTag::Turret";           // 11
            public const string Combo = "ECrabPickupTag::Combo";             // 12
            public const string GlueShot = "ECrabPickupTag::GlueShot";       // 13

            public static readonly string[] AllValues = new[]
            {
                None, Healing, DamageOverTime, Critical, Speed, Bounce,
                Ice, Fire, Lightning, Poison, Arcane, Turret, Combo, GlueShot
            };
        }

        /// <summary>
        /// ECrabRarity enum - item rarity for loot pools (from .rdata:0x143298AF0)
        /// Note: Different from ECrabRank which is for weapon mastery levels!
        /// </summary>
        public static class ECrabRarity
        {
            public const string None = "ECrabRarity::None";           // 0
            public const string Common = "ECrabRarity::Common";       // 1
            public const string Epic = "ECrabRarity::Epic";           // 2
            public const string Legendary = "ECrabRarity::Legendary"; // 3
            public const string Greed = "ECrabRarity::Greed";         // 4

            public static readonly string[] AllValues = new[]
            {
                None, Common, Epic, Legendary, Greed
            };
        }

        /// <summary>
        /// ECrabLootPool enum - loot pool categories for spawning items (from .rdata:0x143298BD0)
        /// </summary>
        public static class ECrabLootPool
        {
            public const string None = "ECrabLootPool::None";                         // 0
            public const string Damage = "ECrabLootPool::Damage";                     // 1
            public const string Critical = "ECrabLootPool::Critical";                 // 2
            public const string Elemental = "ECrabLootPool::Elemental";               // 3
            public const string Speed = "ECrabLootPool::Speed";                       // 4
            public const string Luck = "ECrabLootPool::Luck";                         // 5
            public const string Health = "ECrabLootPool::Health";                     // 6
            public const string Economy = "ECrabLootPool::Economy";                   // 7
            public const string Skill = "ECrabLootPool::Skill";                       // 8
            public const string Greed = "ECrabLootPool::Greed";                       // 9
            public const string Upgrade = "ECrabLootPool::Upgrade";                   // 10
            public const string Random = "ECrabLootPool::Random";                     // 11
            public const string Anvil = "ECrabLootPool::Anvil";                       // 12
            public const string RelicChest = "ECrabLootPool::RelicChest";             // 13
            public const string SpikedChest = "ECrabLootPool::SpikedChest";           // 14
            public const string EpicChest = "ECrabLootPool::EpicChest";               // 15
            public const string LegendaryChest = "ECrabLootPool::LegendaryChest";     // 16
            public const string RegenerationChest = "ECrabLootPool::RegenerationChest"; // 17
            public const string KeyChest = "ECrabLootPool::KeyChest";                 // 18
            public const string Lesser = "ECrabLootPool::Lesser";                     // 19
            public const string NoRelicsOrConsumables = "ECrabLootPool::NoRelicsOrConsumables"; // 20

            public static readonly string[] AllValues = new[]
            {
                None, Damage, Critical, Elemental, Speed, Luck, Health, Economy, Skill,
                Greed, Upgrade, Random, Anvil, RelicChest, SpikedChest, EpicChest,
                LegendaryChest, RegenerationChest, KeyChest, Lesser, NoRelicsOrConsumables
            };
        }

        /// <summary>
        /// ECrabPickupType enum - the type of pickup item (from .rdata:0x143298F70)
        /// Used to determine which inventory/handler system processes the item
        /// </summary>
        public static class ECrabPickupType
        {
            public const string None = "ECrabPickupType::None";               // 0
            public const string Weapon = "ECrabPickupType::Weapon";           // 1
            public const string Ability = "ECrabPickupType::Ability";         // 2
            public const string Melee = "ECrabPickupType::Melee";             // 3
            public const string WeaponMod = "ECrabPickupType::WeaponMod";     // 4
            public const string AbilityMod = "ECrabPickupType::AbilityMod";   // 5
            public const string MeleeMod = "ECrabPickupType::MeleeMod";       // 6
            public const string Perk = "ECrabPickupType::Perk";               // 7
            public const string Relic = "ECrabPickupType::Relic";             // 8
            public const string Consumable = "ECrabPickupType::Consumable";   // 9
            public const string Random = "ECrabPickupType::Random";           // 10

            public static readonly string[] AllValues = new[]
            {
                None, Weapon, Ability, Melee, WeaponMod, AbilityMod,
                MeleeMod, Perk, Relic, Consumable, Random
            };
        }

        /// <summary>
        /// ECrabWeaponModType enum - all weapon mod types (from .rdata:0x143296D70)
        /// 95 total weapon mods with ordinal values 0-94
        /// </summary>
        public static class ECrabWeaponModType
        {
            public const string None = "ECrabWeaponModType::None";                       // 0
            public const string DoubleShot = "ECrabWeaponModType::DoubleShot";           // 1
            public const string BouncingShot = "ECrabWeaponModType::BouncingShot";       // 2
            public const string AcceleratingShot = "ECrabWeaponModType::AcceleratingShot"; // 3
            public const string ZigZagShot = "ECrabWeaponModType::ZigZagShot";           // 4
            public const string SpiralShot = "ECrabWeaponModType::SpiralShot";           // 5
            public const string SnakeShot = "ECrabWeaponModType::SnakeShot";             // 6
            public const string ChaoticShot = "ECrabWeaponModType::ChaoticShot";         // 7
            public const string BoomerangShot = "ECrabWeaponModType::BoomerangShot";     // 8
            public const string OrbitingShot = "ECrabWeaponModType::OrbitingShot";       // 9
            public const string RecoilShot = "ECrabWeaponModType::RecoilShot";           // 10
            public const string FastShot = "ECrabWeaponModType::FastShot";               // 11
            public const string KnockbackShot = "ECrabWeaponModType::KnockbackShot";     // 12
            public const string BigMag = "ECrabWeaponModType::BigMag";                   // 13
            public const string HighCaliber = "ECrabWeaponModType::HighCaliber";         // 14
            public const string WindUp = "ECrabWeaponModType::WindUp";                   // 15
            public const string SteadyShot = "ECrabWeaponModType::SteadyShot";           // 16
            public const string TrickShot = "ECrabWeaponModType::TrickShot";             // 17
            public const string AerialShot = "ECrabWeaponModType::AerialShot";           // 18
            public const string GripTape = "ECrabWeaponModType::GripTape";               // 19
            public const string BlindFire = "ECrabWeaponModType::BlindFire";             // 20
            public const string TimeShot = "ECrabWeaponModType::TimeShot";               // 21
            public const string TimeBolt = "ECrabWeaponModType::TimeBolt";               // 22
            public const string UltraShot = "ECrabWeaponModType::UltraShot";             // 23
            public const string SharpShot = "ECrabWeaponModType::SharpShot";             // 24
            public const string GlueShot = "ECrabWeaponModType::GlueShot";               // 25
            public const string BigShot = "ECrabWeaponModType::BigShot";                 // 26
            public const string StreakShot = "ECrabWeaponModType::StreakShot";           // 27
            public const string MagShot = "ECrabWeaponModType::MagShot";                 // 28
            public const string Uppercut = "ECrabWeaponModType::Uppercut";               // 29
            public const string HeavyShot = "ECrabWeaponModType::HeavyShot";             // 30
            public const string HeavyHitter = "ECrabWeaponModType::HeavyHitter";         // 31
            public const string RapidFire = "ECrabWeaponModType::RapidFire";             // 32
            public const string EscalatingShot = "ECrabWeaponModType::EscalatingShot";   // 33
            public const string IceShot = "ECrabWeaponModType::IceShot";                 // 34
            public const string FireShot = "ECrabWeaponModType::FireShot";               // 35
            public const string LightningShot = "ECrabWeaponModType::LightningShot";     // 36
            public const string PoisonShot = "ECrabWeaponModType::PoisonShot";           // 37
            public const string ArcaneShot = "ECrabWeaponModType::ArcaneShot";           // 38
            public const string RandomShot = "ECrabWeaponModType::RandomShot";           // 39
            public const string EfficientShot = "ECrabWeaponModType::EfficientShot";     // 40
            public const string ReloadArc = "ECrabWeaponModType::ReloadArc";             // 41
            public const string SonicBoom = "ECrabWeaponModType::SonicBoom";             // 42
            public const string LuckyShot = "ECrabWeaponModType::LuckyShot";             // 43
            public const string TripleShot = "ECrabWeaponModType::TripleShot";           // 44
            public const string ArcShot = "ECrabWeaponModType::ArcShot";                 // 45
            public const string XShot = "ECrabWeaponModType::XShot";                     // 46
            public const string ScatterShot = "ECrabWeaponModType::ScatterShot";         // 47
            public const string TargetingShot = "ECrabWeaponModType::TargetingShot";     // 48
            public const string LinkShot = "ECrabWeaponModType::LinkShot";               // 49
            public const string DrillShot = "ECrabWeaponModType::DrillShot";             // 50
            public const string DoubleTap = "ECrabWeaponModType::DoubleTap";             // 51
            public const string HealthShot = "ECrabWeaponModType::HealthShot";           // 52
            public const string MoneyShot = "ECrabWeaponModType::MoneyShot";             // 53
            public const string DamageShot = "ECrabWeaponModType::DamageShot";           // 54
            public const string Supercharged = "ECrabWeaponModType::Supercharged";       // 55
            public const string Juiced = "ECrabWeaponModType::Juiced";                   // 56
            public const string AuraShot = "ECrabWeaponModType::AuraShot";               // 57
            public const string PiercingShot = "ECrabWeaponModType::PiercingShot";       // 58
            public const string BubbleShot = "ECrabWeaponModType::BubbleShot";           // 59
            public const string PopcornShot = "ECrabWeaponModType::PopcornShot";         // 60
            public const string PumpkinShot = "ECrabWeaponModType::PumpkinShot";         // 61
            public const string DaggerArc = "ECrabWeaponModType::DaggerArc";             // 62
            public const string PiercingWave = "ECrabWeaponModType::PiercingWave";       // 63
            public const string ArcaneBlast = "ECrabWeaponModType::ArcaneBlast";         // 64
            public const string ShotgunBlast = "ECrabWeaponModType::ShotgunBlast";       // 65
            public const string MaceShot = "ECrabWeaponModType::MaceShot";               // 66
            public const string FireworkShot = "ECrabWeaponModType::FireworkShot";       // 67
            public const string ThornShot = "ECrabWeaponModType::ThornShot";             // 68
            public const string Firepower = "ECrabWeaponModType::Firepower";             // 69
            public const string SquareShot = "ECrabWeaponModType::SquareShot";           // 70
            public const string SplitShot = "ECrabWeaponModType::SplitShot";             // 71
            public const string HomingShot = "ECrabWeaponModType::HomingShot";           // 72
            public const string SplashDamage = "ECrabWeaponModType::SplashDamage";       // 73
            public const string SparkShot = "ECrabWeaponModType::SparkShot";             // 74
            public const string ProximityBarrage = "ECrabWeaponModType::ProximityBarrage"; // 75
            public const string HomingBlades = "ECrabWeaponModType::HomingBlades";       // 76
            public const string BombShot = "ECrabWeaponModType::BombShot";               // 77
            public const string LandmineShot = "ECrabWeaponModType::LandmineShot";       // 78
            public const string TorpedoShot = "ECrabWeaponModType::TorpedoShot";         // 79
            public const string FireballShot = "ECrabWeaponModType::FireballShot";       // 80
            public const string SharpenedAxe = "ECrabWeaponModType::SharpenedAxe";       // 81
            public const string TriangleShot = "ECrabWeaponModType::TriangleShot";       // 82
            public const string BeamShot = "ECrabWeaponModType::BeamShot";               // 83
            public const string SporeShot = "ECrabWeaponModType::SporeShot";             // 84
            public const string IceStorm = "ECrabWeaponModType::IceStorm";               // 85
            public const string FireStorm = "ECrabWeaponModType::FireStorm";             // 86
            public const string LightningStorm = "ECrabWeaponModType::LightningStorm";   // 87
            public const string PoisonStorm = "ECrabWeaponModType::PoisonStorm";         // 88
            public const string IceStrike = "ECrabWeaponModType::IceStrike";             // 89
            public const string FireStrike = "ECrabWeaponModType::FireStrike";           // 90
            public const string LightningStrike = "ECrabWeaponModType::LightningStrike"; // 91
            public const string PoisonStrike = "ECrabWeaponModType::PoisonStrike";       // 92
            public const string SpikeStrike = "ECrabWeaponModType::SpikeStrike";         // 93
            public const string DiceShot = "ECrabWeaponModType::DiceShot";               // 94

            public static readonly string[] AllValues = new[]
            {
                None, DoubleShot, BouncingShot, AcceleratingShot, ZigZagShot, SpiralShot, SnakeShot,
                ChaoticShot, BoomerangShot, OrbitingShot, RecoilShot, FastShot, KnockbackShot, BigMag,
                HighCaliber, WindUp, SteadyShot, TrickShot, AerialShot, GripTape, BlindFire, TimeShot,
                TimeBolt, UltraShot, SharpShot, GlueShot, BigShot, StreakShot, MagShot, Uppercut,
                HeavyShot, HeavyHitter, RapidFire, EscalatingShot, IceShot, FireShot, LightningShot,
                PoisonShot, ArcaneShot, RandomShot, EfficientShot, ReloadArc, SonicBoom, LuckyShot,
                TripleShot, ArcShot, XShot, ScatterShot, TargetingShot, LinkShot, DrillShot, DoubleTap,
                HealthShot, MoneyShot, DamageShot, Supercharged, Juiced, AuraShot, PiercingShot,
                BubbleShot, PopcornShot, PumpkinShot, DaggerArc, PiercingWave, ArcaneBlast, ShotgunBlast,
                MaceShot, FireworkShot, ThornShot, Firepower, SquareShot, SplitShot, HomingShot,
                SplashDamage, SparkShot, ProximityBarrage, HomingBlades, BombShot, LandmineShot,
                TorpedoShot, FireballShot, SharpenedAxe, TriangleShot, BeamShot, SporeShot, IceStorm,
                FireStorm, LightningStorm, PoisonStorm, IceStrike, FireStrike, LightningStrike,
                PoisonStrike, SpikeStrike, DiceShot
            };
        }

        /// <summary>
        /// ECrabMiscPickupType enum - special/misc pickup types (from .rdata:0x143298020)
        /// </summary>
        public static class ECrabMiscPickupType
        {
            public const string None = "ECrabMiscPickupType::None";                                   // 0
            public const string StreamerLootUpgradePickup = "ECrabMiscPickupType::StreamerLootUpgradePickup"; // 1
            public const string AutoLootPickup = "ECrabMiscPickupType::AutoLootPickup";               // 2
            public const string InfinitePedestalPickup = "ECrabMiscPickupType::InfinitePedestalPickup"; // 3

            public static readonly string[] AllValues = new[]
            {
                None, StreamerLootUpgradePickup, AutoLootPickup, InfinitePedestalPickup
            };
        }

        /// <summary>
        /// ECrabCurrencyType enum - currency/resource types (from .rdata:0x143298120)
        /// </summary>
        public static class ECrabCurrencyType
        {
            public const string Crystal = "ECrabCurrencyType::Crystal";     // 0
            public const string Key = "ECrabCurrencyType::Key";             // 1
            public const string Health = "ECrabCurrencyType::Health";       // 2
            public const string MaxHealth = "ECrabCurrencyType::MaxHealth"; // 3

            public static readonly string[] AllValues = new[]
            {
                Crystal, Key, Health, MaxHealth
            };
        }

        /// <summary>
        /// ECrabEnhancementType enum - enhancement effects from anvil (from .rdata:0x1432981F0)
        /// 28 enhancement types that can be applied to weapons/abilities
        /// </summary>
        public static class ECrabEnhancementType
        {
            public const string None = "ECrabEnhancementType::None";               // 0
            public const string Bouncing = "ECrabEnhancementType::Bouncing";       // 1
            public const string Accelerating = "ECrabEnhancementType::Accelerating"; // 2
            public const string Zigging = "ECrabEnhancementType::Zigging";         // 3
            public const string Spiraling = "ECrabEnhancementType::Spiraling";     // 4
            public const string Snaking = "ECrabEnhancementType::Snaking";         // 5
            public const string Returning = "ECrabEnhancementType::Returning";     // 6
            public const string Orbiting = "ECrabEnhancementType::Orbiting";       // 7
            public const string Chipping = "ECrabEnhancementType::Chipping";       // 8
            public const string Sticky = "ECrabEnhancementType::Sticky";           // 9
            public const string Growing = "ECrabEnhancementType::Growing";         // 10
            public const string Freezing = "ECrabEnhancementType::Freezing";       // 11
            public const string Flaming = "ECrabEnhancementType::Flaming";         // 12
            public const string Electrifying = "ECrabEnhancementType::Electrifying"; // 13
            public const string Toxifying = "ECrabEnhancementType::Toxifying";     // 14
            public const string Arcanifying = "ECrabEnhancementType::Arcanifying"; // 15
            public const string Persisting = "ECrabEnhancementType::Persisting";   // 16
            public const string Doubling = "ECrabEnhancementType::Doubling";       // 17
            public const string Targeting = "ECrabEnhancementType::Targeting";     // 18
            public const string Damaging = "ECrabEnhancementType::Damaging";       // 19
            public const string Booming = "ECrabEnhancementType::Booming";         // 20
            public const string Tripling = "ECrabEnhancementType::Tripling";       // 21
            public const string Splitting = "ECrabEnhancementType::Splitting";     // 22
            public const string Scattering = "ECrabEnhancementType::Scattering";   // 23
            public const string Expanding = "ECrabEnhancementType::Expanding";     // 24
            public const string Homing = "ECrabEnhancementType::Homing";           // 25
            public const string Endangering = "ECrabEnhancementType::Endangering"; // 26
            public const string Random = "ECrabEnhancementType::Random";           // 27

            public static readonly string[] AllValues = new[]
            {
                None, Bouncing, Accelerating, Zigging, Spiraling, Snaking, Returning, Orbiting,
                Chipping, Sticky, Growing, Freezing, Flaming, Electrifying, Toxifying, Arcanifying,
                Persisting, Doubling, Targeting, Damaging, Booming, Tripling, Splitting, Scattering,
                Expanding, Homing, Endangering, Random
            };
        }

        /// <summary>
        /// UE4 UClass names for game objects (from .rdata section)
        /// These are the internal class names used by UE4 reflection
        /// </summary>
        public static class UE4ClassNames
        {
            // Item classes (UTF-16LE in binary)
            public const string CrabPerk = "CrabPerk";               // 0x14328DAC8
            public const string CrabMeleeMod = "CrabMeleeMod";       // 0x14328DAD8 (UTF-16)
            public const string CrabAbilityMod = "CrabAbilityMod";   // 0x14328DB88 (UTF-16)
            public const string CrabWeaponMod = "CrabWeaponMod";     // 0x14328DC38 (UTF-16)
            public const string CrabInventoryCooldown = "CrabInventoryCooldown"; // 0x14328DCE8

            // Data asset classes
            public const string PerkDA = "PerkDA";                   // 0x14328DAC0
            public const string MeleeModDA = "MeleeModDA";           // 0x14328DB68
            public const string AbilityModDA = "AbilityModDA";       // 0x14328DC18
            public const string WeaponModDA = "WeaponModDA";         // 0x14328DCC8
            public const string InventoryDA = "InventoryDA";         // 0x14328DD18
        }

        /// <summary>
        /// UE4 property names used in save/memory (for FName lookups)
        /// </summary>
        public static class UE4PropertyNames
        {
            // Unlock arrays (save file)
            public const string UnlockedPerks = "UnlockedPerks";
            public const string UnlockedWeapons = "UnlockedWeapons";
            public const string UnlockedAbilities = "UnlockedAbilities";
            public const string UnlockedMeleeWeapons = "UnlockedMeleeWeapons";
            public const string UnlockedWeaponMods = "UnlockedWeaponMods";
            public const string UnlockedAbilityMods = "UnlockedAbilityMods";
            public const string UnlockedMeleeMods = "UnlockedMeleeMods";
            public const string UnlockedRelics = "UnlockedRelics";
            public const string RankedWeapons = "RankedWeapons";
            public const string CrabCosmetics = "CrabCosmetics";

            // Item properties (from .rdata)
            public const string InventoryInfo = "InventoryInfo";     // 0x14328DB28 - links items to inventory
            public const string CurrentCooldown = "CurrentCooldown"; // 0x14328DD88
            public const string UnderlyingType = "UnderlyingType";   // 0x14328DDD8
        }

        /// <summary>
        /// Known function addresses from RE (version-specific)
        /// These are example addresses - actual values depend on game version
        /// </summary>
        public static class KnownFunctions
        {
            // Item-related functions (offsets from base)
            public const long MeleeModHandler = 0x140D7A300;     // sub_140D7A300
            public const long AbilityModHandler = 0x140D6F370;   // sub_140D6F370
            public const long WeaponModHandler = 0x140D8A470;    // sub_140D8A470
            public const long InventoryHandler = 0x140D85150;    // sub_140D85150 (shared)
            public const long CooldownHandler = 0x140D789C0;     // sub_140D789C0

            // =============================================
            // SERVER RPC FUNCTIONS (Client→Server)
            // These are the key functions for item injection!
            // =============================================

            // Set Data Asset functions (for giving items)
            public const long ServerSetWeaponDA = 0x140D809C0;      // Set weapon data asset
            public const long ServerSetAbilityDA = 0x140D80960;     // Set ability data asset
            public const long ServerSetMeleeDA = 0x140D80990;       // Set melee data asset

            // Equip functions
            public const long ServerEquipInventory = 0x140D807E0;   // Equip from inventory
            public const long ServerEquipCosmetics = 0x140D807B0;   // Equip cosmetics

            // Remove item functions
            public const long ServerRemoveWeaponMod = 0x140D80930;
            public const long ServerRemoveAbilityMod = 0x140D80870;
            public const long ServerRemoveMeleeMod = 0x140D808A0;
            public const long ServerRemovePerk = 0x140D808D0;
            public const long ServerRemoveRelic = 0x140D80900;

            // Account/progression functions
            public const long ServerRefreshAccount = 0x140D80840;
            public const long ServerIncrementNumInventorySlots = 0x140D80810;

            // =============================================
            // ONREP CALLBACKS (Replication notifications)
            // Called when replicated properties change
            // =============================================
            public const long OnRep_Inventory = 0x140D80690;
            public const long OnRep_Crystals = 0x140D80600;
            public const long OnRep_Keys = 0x140D806F0;
            public const long OnRep_WeaponDA = 0x140D80780;
            public const long OnRep_AbilityDA = 0x140D80540;
            public const long OnRep_MeleeDA = 0x140D80720;
            public const long OnRep_Combo = 0x140D805D0;
            public const long OnRep_Eliminations = 0x140D80660;
            public const long OnRep_AccountLevel = 0x140D80570;
            public const long OnRep_AccountRank = 0x140D805A0;
            public const long OnRep_DamageTakenOnThisIsland = 0x140D80630;
            public const long OnRep_IslandRewardRarity = 0x140D806C0;
            public const long OnRep_ScaleMultiplier = 0x140D80750;
        }

        #endregion

        #region AOB Patterns and Pointer Paths

        /// <summary>
        /// AOB (Array of Bytes) patterns for finding game structures
        /// These are version-specific and may need updating
        /// </summary>
        public static class AOBPatterns
        {
            // Core UE4 pointers
            public const string GEngine = "48 8B 05 ?? ?? ?? ?? 48 8B 88 ?? 00 00 00 48 85 C9";
            public const string UWorld = "48 8B 1D ?? ?? ?? ?? 48 85 DB 74 ?? 41 B0 01";
            public const string GNames = "48 8D 15 ?? ?? ?? ?? EB ?? 48 8D 0D ?? ?? ?? ?? E8";
            public const string PlayerController = "48 8B 05 ?? ?? ?? ?? 48 8B 88 ?? ?? ?? ?? 48 85 C9 74";

            // Game-specific patterns
            public const string Inventory = "48 8B 0D ?? ?? ?? ?? 48 85 C9 74 ?? E8 ?? ?? ?? ?? 48 8B D8";
            public const string PerkManager = "48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B F8 48 85 C0";

            // Health/Armor patterns (float comparison)
            public const string Health = "F3 0F 10 ?? ?? ?? 00 00 0F 2F ?? F3 0F 10";
            public const string Ammo = "89 ?? ?? 00 00 00 83 ?? ?? 00 00 00 00 7E";

            // String patterns for finding enum reflection data
            public const string ECrabPerkTypeString = "45 43 72 61 62 50 65 72 6B 54 79 70 65"; // "ECrabPerkType"
            public const string ECrabRankString = "45 43 72 61 62 52 61 6E 6B"; // "ECrabRank"
        }

        /// <summary>
        /// Pointer path for traversing UE4 object hierarchy
        /// </summary>
        public class PointerPath
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public long[] Offsets { get; set; } = Array.Empty<long>();
            public int ValueSize { get; set; } = 4;
            public bool IsFloat { get; set; }
            public bool IsPointer { get; set; }
        }

        /// <summary>
        /// Known pointer paths - these are version-specific examples
        /// Use FindPointerPath() to discover correct paths for current version
        /// </summary>
        public static readonly Dictionary<string, PointerPath> KnownPointers = new()
        {
            // Player stats (example paths - may need updating)
            ["Health"] = new PointerPath
            {
                Name = "Health",
                Description = "Player current health (float)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x348 },
                IsFloat = true
            },
            ["MaxHealth"] = new PointerPath
            {
                Name = "MaxHealth",
                Description = "Player max health (float)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x34C },
                IsFloat = true
            },
            ["Armor"] = new PointerPath
            {
                Name = "Armor",
                Description = "Player armor value (float)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x350 },
                IsFloat = true
            },

            // Currency
            ["Crystals"] = new PointerPath
            {
                Name = "Crystals",
                Description = "Current crystal count (int32)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x2F0 }
            },
            ["Keys"] = new PointerPath
            {
                Name = "Keys",
                Description = "Current key count (int32)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x2F4 }
            },

            // Weapon
            ["CurrentAmmo"] = new PointerPath
            {
                Name = "CurrentAmmo",
                Description = "Current weapon ammo (int32)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x400 }
            },
            ["MaxAmmo"] = new PointerPath
            {
                Name = "MaxAmmo",
                Description = "Max weapon ammo (int32)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x404 }
            },

            // Level/Progress
            ["CurrentIsland"] = new PointerPath
            {
                Name = "CurrentIsland",
                Description = "Current island number (int32)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x280 }
            },
            ["Wave"] = new PointerPath
            {
                Name = "Wave",
                Description = "Current wave number (int32)",
                Offsets = new long[] { 0x04B89D68, 0x30, 0x250, 0x284 }
            },
        };

        #endregion

        #region Attach/Detach

        /// <summary>
        /// Find and attach to the Crab Champions process
        /// </summary>
        public bool AttachToGame()
        {
            try
            {
                // Try common process names
                string[] processNames = { "CrabChampions", "CrabChampions-Win64-Shipping", "CrabChampions-Win64" };

                foreach (var name in processNames)
                {
                    var processes = Process.GetProcessesByName(name);
                    if (processes.Length > 0)
                    {
                        GameProcess = processes[0];
                        ProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, false, GameProcess.Id);

                        if (ProcessHandle != IntPtr.Zero)
                        {
                            // Try to get game version from file
                            try
                            {
                                var fileInfo = GameProcess.MainModule?.FileVersionInfo;
                                GameVersion = fileInfo?.FileVersion ?? "Unknown";
                            }
                            catch
                            {
                                GameVersion = "Unknown";
                            }

                            StatusChanged?.Invoke(this, $"Attached to {name} (PID: {GameProcess.Id})");
                            GameAttached?.Invoke(this, EventArgs.Empty);
                            return true;
                        }
                    }
                }

                ErrorOccurred?.Invoke(this, "Could not find Crab Champions process. Make sure the game is running.");
                return false;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to attach: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Detach from the game process
        /// </summary>
        public void Detach()
        {
            if (ProcessHandle != IntPtr.Zero)
            {
                CloseHandle(ProcessHandle);
                ProcessHandle = IntPtr.Zero;
            }
            GameProcess = null;
            StatusChanged?.Invoke(this, "Detached from game");
            GameDetached?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Memory Read/Write

        /// <summary>
        /// Read bytes from process memory
        /// </summary>
        public byte[]? ReadMemory(IntPtr address, int size)
        {
            if (!IsAttached) return null;

            byte[] buffer = new byte[size];
            if (ReadProcessMemory(ProcessHandle, address, buffer, size, out int bytesRead))
            {
                return bytesRead == size ? buffer : null;
            }
            return null;
        }

        /// <summary>
        /// Write bytes to process memory
        /// </summary>
        public bool WriteMemory(IntPtr address, byte[] data)
        {
            if (!IsAttached) return false;

            // Change memory protection if needed
            VirtualProtectEx(ProcessHandle, address, (UIntPtr)data.Length, 0x40, out uint oldProtect);

            bool result = WriteProcessMemory(ProcessHandle, address, data, data.Length, out _);

            // Restore protection
            VirtualProtectEx(ProcessHandle, address, (UIntPtr)data.Length, oldProtect, out _);

            return result;
        }

        /// <summary>
        /// Read an integer value
        /// </summary>
        public int ReadInt32(IntPtr address)
        {
            var data = ReadMemory(address, 4);
            return data != null ? BitConverter.ToInt32(data, 0) : 0;
        }

        /// <summary>
        /// Read a float value
        /// </summary>
        public float ReadFloat(IntPtr address)
        {
            var data = ReadMemory(address, 4);
            return data != null ? BitConverter.ToSingle(data, 0) : 0f;
        }

        /// <summary>
        /// Read a 64-bit pointer
        /// </summary>
        public long ReadInt64(IntPtr address)
        {
            var data = ReadMemory(address, 8);
            return data != null ? BitConverter.ToInt64(data, 0) : 0;
        }

        /// <summary>
        /// Write an integer value
        /// </summary>
        public bool WriteInt32(IntPtr address, int value)
        {
            return WriteMemory(address, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Write a float value
        /// </summary>
        public bool WriteFloat(IntPtr address, float value)
        {
            return WriteMemory(address, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Follow a pointer path to get the final address
        /// </summary>
        public IntPtr ResolvePointerPath(PointerPath path)
        {
            if (!IsAttached || path.Offsets.Length == 0) return IntPtr.Zero;

            try
            {
                IntPtr currentAddress = BaseAddress + (int)path.Offsets[0];

                for (int i = 1; i < path.Offsets.Length; i++)
                {
                    long value = ReadInt64(currentAddress);
                    if (value == 0) return IntPtr.Zero;
                    currentAddress = new IntPtr(value + path.Offsets[i]);
                }

                return currentAddress;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Read a value using a pointer path
        /// </summary>
        public object? ReadValue(string pointerName)
        {
            if (!KnownPointers.TryGetValue(pointerName, out var path))
                return null;

            var address = ResolvePointerPath(path);
            if (address == IntPtr.Zero) return null;

            return path.IsFloat ? (object)ReadFloat(address) : (object)ReadInt32(address);
        }

        /// <summary>
        /// Write a value using a pointer path
        /// </summary>
        public bool WriteValue(string pointerName, object value)
        {
            if (!KnownPointers.TryGetValue(pointerName, out var path))
                return false;

            var address = ResolvePointerPath(path);
            if (address == IntPtr.Zero) return false;

            if (path.IsFloat)
                return WriteFloat(address, Convert.ToSingle(value));
            else
                return WriteInt32(address, Convert.ToInt32(value));
        }

        #endregion

        #region AOB Scanning

        /// <summary>
        /// Convert pattern string to byte array with wildcards
        /// </summary>
        private static (byte[] pattern, bool[] mask) ParsePattern(string patternString)
        {
            var parts = patternString.Split(' ');
            var pattern = new byte[parts.Length];
            var mask = new bool[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "??" || parts[i] == "?")
                {
                    pattern[i] = 0;
                    mask[i] = false;
                }
                else
                {
                    pattern[i] = Convert.ToByte(parts[i], 16);
                    mask[i] = true;
                }
            }

            return (pattern, mask);
        }

        /// <summary>
        /// Scan memory for a pattern
        /// </summary>
        public IntPtr AOBScan(string patternString, IntPtr startAddress, int scanSize = 0x10000000)
        {
            if (!IsAttached) return IntPtr.Zero;

            var (pattern, mask) = ParsePattern(patternString);

            // Read memory in chunks
            const int chunkSize = 0x10000;
            byte[] buffer = new byte[chunkSize + pattern.Length];

            for (long offset = 0; offset < scanSize; offset += chunkSize)
            {
                IntPtr currentAddress = IntPtr.Add(startAddress, (int)offset);
                var data = ReadMemory(currentAddress, buffer.Length);
                if (data == null) continue;

                // Search for pattern in chunk
                for (int i = 0; i < chunkSize; i++)
                {
                    bool found = true;
                    for (int j = 0; j < pattern.Length && found; j++)
                    {
                        if (mask[j] && data[i + j] != pattern[j])
                            found = false;
                    }

                    if (found)
                    {
                        return IntPtr.Add(currentAddress, i);
                    }
                }
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Scan for a pattern in the main module
        /// </summary>
        public IntPtr ScanMainModule(string patternString)
        {
            if (!IsAttached || GameProcess?.MainModule == null) return IntPtr.Zero;
            return AOBScan(patternString, BaseAddress, GameProcess.MainModule.ModuleMemorySize);
        }

        #endregion

        #region Cheat Functions

        /// <summary>
        /// Set player health to maximum
        /// </summary>
        public bool SetMaxHealth()
        {
            var maxHealth = ReadValue("MaxHealth");
            if (maxHealth != null)
            {
                return WriteValue("Health", maxHealth);
            }
            return false;
        }

        /// <summary>
        /// Set infinite health (godmode)
        /// </summary>
        public bool SetGodMode(bool enabled)
        {
            if (enabled)
            {
                // Set health to very high value
                return WriteValue("Health", 999999f) && WriteValue("MaxHealth", 999999f);
            }
            else
            {
                // Reset to normal
                return WriteValue("Health", 100f) && WriteValue("MaxHealth", 100f);
            }
        }

        /// <summary>
        /// Set currency values
        /// </summary>
        public bool SetCurrency(int crystals, int keys)
        {
            bool success = true;
            if (crystals >= 0) success &= WriteValue("Crystals", crystals);
            if (keys >= 0) success &= WriteValue("Keys", keys);
            return success;
        }

        /// <summary>
        /// Set infinite ammo
        /// </summary>
        public bool SetInfiniteAmmo(bool enabled)
        {
            if (enabled)
            {
                return WriteValue("CurrentAmmo", 999) && WriteValue("MaxAmmo", 999);
            }
            return true;
        }

        /// <summary>
        /// Get current player stats
        /// </summary>
        public Dictionary<string, object?> GetCurrentStats()
        {
            var stats = new Dictionary<string, object?>();

            foreach (var pointer in KnownPointers)
            {
                stats[pointer.Key] = ReadValue(pointer.Key);
            }

            return stats;
        }

        #endregion

        #region String Scanning (for UE4 Reflection Data)

        /// <summary>
        /// Scan memory for a string pattern (useful for finding enum reflection data)
        /// </summary>
        public List<IntPtr> ScanForString(string searchString, bool unicode = false)
        {
            var results = new List<IntPtr>();
            if (!IsAttached) return results;

            byte[] pattern = unicode
                ? Encoding.Unicode.GetBytes(searchString)
                : Encoding.ASCII.GetBytes(searchString);

            // Scan readable memory regions
            IntPtr address = IntPtr.Zero;
            IntPtr maxAddress = new IntPtr(0x7FFFFFFF0000); // User-space limit

            while (address.ToInt64() < maxAddress.ToInt64())
            {
                if (!VirtualQueryEx(ProcessHandle, address, out var memInfo, (uint)Marshal.SizeOf<MEMORY_BASIC_INFORMATION>()))
                    break;

                // Only scan committed, readable memory
                if (memInfo.State == MEM_COMMIT &&
                    (memInfo.Protect == PAGE_READWRITE || memInfo.Protect == PAGE_EXECUTE_READWRITE))
                {
                    var regionSize = (int)memInfo.RegionSize.ToInt64();
                    if (regionSize > 0 && regionSize < 0x10000000) // Reasonable size limit
                    {
                        var buffer = ReadMemory(memInfo.BaseAddress, regionSize);
                        if (buffer != null)
                        {
                            for (int i = 0; i <= buffer.Length - pattern.Length; i++)
                            {
                                bool match = true;
                                for (int j = 0; j < pattern.Length && match; j++)
                                {
                                    if (buffer[i + j] != pattern[j])
                                        match = false;
                                }
                                if (match)
                                {
                                    results.Add(IntPtr.Add(memInfo.BaseAddress, i));
                                }
                            }
                        }
                    }
                }

                // Move to next region
                address = IntPtr.Add(memInfo.BaseAddress, (int)memInfo.RegionSize.ToInt64());
            }

            return results;
        }

        /// <summary>
        /// Find the address of an ECrabPerkType enum value in memory
        /// </summary>
        public IntPtr FindPerkTypeAddress(string perkType)
        {
            // The enum values are stored as strings in .rdata section
            // e.g., "ECrabPerkType::GlassCannon"
            var addresses = ScanForString(perkType);
            return addresses.FirstOrDefault();
        }

        /// <summary>
        /// Find all perk type string addresses in memory (for debugging/analysis)
        /// </summary>
        public Dictionary<string, IntPtr> FindAllPerkTypeAddresses()
        {
            var results = new Dictionary<string, IntPtr>();

            foreach (var perkType in ECrabPerkType.AllValues)
            {
                var addr = FindPerkTypeAddress(perkType);
                if (addr != IntPtr.Zero)
                {
                    results[perkType] = addr;
                }
            }

            return results;
        }

        /// <summary>
        /// Find UE4 FName for a property by scanning for the string
        /// </summary>
        public IntPtr FindFNameAddress(string propertyName)
        {
            var addresses = ScanForString(propertyName);
            return addresses.FirstOrDefault();
        }

        #endregion

        #region Item/Perk Injection

        /// <summary>
        /// ITEM INJECTION ARCHITECTURE:
        ///
        /// In Crab Champions (UE4), items are stored as:
        /// 1. TArray of UObject* pointers in the player's inventory component
        /// 2. Each item is a UDataAsset (DA_Perk_*, DA_WeaponMod_*, etc.)
        ///
        /// To add items at runtime, you need to either:
        /// A) Find and call the game's native AddPerk/AddItem function (safest)
        /// B) Manually add a UObject pointer to the TArray (risky - may crash)
        ///
        /// Method A requires:
        /// - Finding the function address via AOB or export table
        /// - Setting up proper calling convention (x64 fastcall)
        /// - Creating a remote thread to call the function
        ///
        /// Method B requires:
        /// - Finding the TArray structure (Data pointer, Count, Max)
        /// - Allocating memory for new pointer if array is full
        /// - Writing the UObject* for the item to add
        ///
        /// The ECrabPerkType enum is used by the game to identify perks.
        /// Set a breakpoint on these strings to find the perk granting code.
        /// </summary>

        /// <summary>
        /// Information about an injectable item
        /// </summary>
        public class InjectableItem
        {
            public string Name { get; set; } = "";
            public string AssetPath { get; set; } = "";
            public string EnumValue { get; set; } = "";
            public IntPtr CachedAddress { get; set; }
        }

        // Cache of found item addresses
        private readonly Dictionary<string, InjectableItem> _itemCache = new();

        /// <summary>
        /// Attempts to give a perk to the player.
        /// Requires offsets to be configured for the current game version.
        /// </summary>
        /// <param name="perkId">The perk ID (e.g., "GlassCannon")</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool GivePerk(string perkId)
        {
            if (!IsAttached)
            {
                ErrorOccurred?.Invoke(this, "Not attached to game");
                return false;
            }

            StatusChanged?.Invoke(this, $"Attempting to give perk: {perkId}");

            // Build the enum value string
            string enumValue = $"ECrabPerkType::{perkId}";

            // Step 1: Find where this perk type is referenced
            var perkTypeAddr = FindPerkTypeAddress(enumValue);
            if (perkTypeAddr == IntPtr.Zero)
            {
                StatusChanged?.Invoke(this, $"Could not find {enumValue} in memory. The game may need to be in a run.");
                ErrorOccurred?.Invoke(this, $"Perk type string '{enumValue}' not found in memory");
                return false;
            }

            StatusChanged?.Invoke(this, $"Found {enumValue} at {perkTypeAddr:X}");

            // Step 2: This is where you would:
            // - Find the perk manager component
            // - Find the AddPerk function
            // - Call it with the perk type
            //
            // For now, we just report what we found and recommend using Cheat Engine

            StatusChanged?.Invoke(this, $"Perk injection requires calling game functions. Set breakpoint on {perkTypeAddr:X} to find AddPerk function.");
            ErrorOccurred?.Invoke(this, "Full perk injection not implemented. Use the discovered address with Cheat Engine.");

            return false;
        }

        /// <summary>
        /// Attempts to give an item (weapon mod, ability mod, melee mod, or relic)
        /// </summary>
        public bool GiveItem(string itemType, string itemId)
        {
            if (!IsAttached)
            {
                ErrorOccurred?.Invoke(this, "Not attached to game");
                return false;
            }

            StatusChanged?.Invoke(this, $"Attempting to give {itemType}: {itemId}");

            // Build the asset path based on item type
            string assetPath = itemType.ToLower() switch
            {
                "weaponmod" => $"/Game/Blueprint/Pickup/WeaponMod/DA_WeaponMod_{itemId}",
                "abilitymod" => $"/Game/Blueprint/Pickup/AbilityMod/DA_AbilityMod_{itemId}",
                "meleemod" => $"/Game/Blueprint/Pickup/MeleeMod/DA_MeleeMod_{itemId}",
                "relic" => $"/Game/Blueprint/Pickup/Relic/DA_Relic_{itemId}",
                "perk" => $"/Game/Blueprint/Pickup/Perk/DA_Perk_{itemId}",
                _ => $"/Game/Blueprint/Pickup/{itemType}/DA_{itemType}_{itemId}"
            };

            // Try to find this asset path in memory
            var assetAddresses = ScanForString(assetPath);

            if (assetAddresses.Count == 0)
            {
                StatusChanged?.Invoke(this, $"Asset path not found in memory. Item may not be loaded.");
                ErrorOccurred?.Invoke(this, $"Could not find '{assetPath}' - try picking up the item type first");
                return false;
            }

            StatusChanged?.Invoke(this, $"Found {assetAddresses.Count} references to {itemId}");

            // The actual injection would require finding and modifying the inventory TArray
            ErrorOccurred?.Invoke(this, "Item injection not fully implemented. Use Cheat Engine with discovered addresses.");
            return false;
        }

        /// <summary>
        /// Simplified give item overload
        /// </summary>
        public bool GiveItem(string fullItemPath)
        {
            return GiveItem("generic", fullItemPath);
        }

        /// <summary>
        /// Get information about what's needed to implement full item injection
        /// </summary>
        public string GetInjectionGuide()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== ITEM INJECTION GUIDE ===\n");

            sb.AppendLine("To implement full item injection, you need:\n");

            sb.AppendLine("1. FIND THE PERK/ITEM MANAGER:");
            sb.AppendLine("   - Scan for 'UnlockedPerks' FName");
            sb.AppendLine("   - Set read breakpoint to find the component");
            sb.AppendLine("   - The component contains TArray<UPerk*> for active perks\n");

            sb.AppendLine("2. FIND THE ADD FUNCTION:");
            sb.AppendLine("   - Look for 'ECrabPerkType::' string references");
            sb.AppendLine("   - Set breakpoint when picking up a perk");
            sb.AppendLine("   - Trace back to find AddPerk/GivePerk function\n");

            sb.AppendLine("3. CALL THE FUNCTION:");
            sb.AppendLine("   - Get function address from step 2");
            sb.AppendLine("   - Use CreateRemoteThread to call it");
            sb.AppendLine("   - Pass the ECrabPerkType enum value as parameter\n");

            sb.AppendLine("DISCOVERED ADDRESSES:");
            if (IsAttached)
            {
                // Try to find some useful addresses
                var perkTypeBase = ScanForString("ECrabPerkType");
                if (perkTypeBase.Count > 0)
                    sb.AppendLine($"   ECrabPerkType enum base: {perkTypeBase[0]:X}");

                var unlockedPerks = ScanForString("UnlockedPerks");
                if (unlockedPerks.Count > 0)
                    sb.AppendLine($"   UnlockedPerks FName: {unlockedPerks[0]:X}");
            }
            else
            {
                sb.AppendLine("   (attach to game first)");
            }

            return sb.ToString();
        }

        #endregion

        #region WeaponDA Scanning and Injection

        /// <summary>
        /// Cached WeaponDA information found in memory
        /// </summary>
        public class WeaponDAInfo
        {
            public string WeaponId { get; set; } = "";
            public string AssetPath { get; set; } = "";
            public IntPtr StringAddress { get; set; }
            public IntPtr DAPointer { get; set; }
            public DateTime FoundAt { get; set; } = DateTime.Now;
        }

        // Cache of found WeaponDA addresses
        private readonly Dictionary<string, WeaponDAInfo> _weaponDACache = new();

        // Captured player controller address (set via breakpoint or scan)
        public IntPtr CapturedPlayerController { get; set; }

        /// <summary>
        /// All known weapon IDs for scanning
        /// </summary>
        public static readonly string[] KnownWeaponIds =
        {
            "AutoRifle", "DualShotguns", "DualPistols", "AutoShotgun", "BurstPistol",
            "Sniper", "Crossbow", "OrbLauncher", "RocketLauncher", "Minigun",
            "BladeLauncher", "ClusterLauncher", "Flamethrower", "ArcaneWand",
            "LaserCannons", "Seagle", "MarksmanRifle", "IceStaff", "LightningScepter", "PoisonCannon"
        };

        /// <summary>
        /// Scan memory for all WeaponDA asset paths
        /// Returns a dictionary of weapon ID -> WeaponDAInfo
        /// </summary>
        public Dictionary<string, WeaponDAInfo> ScanForWeaponDAs()
        {
            if (!IsAttached)
            {
                ErrorOccurred?.Invoke(this, "Not attached to game");
                return new Dictionary<string, WeaponDAInfo>();
            }

            StatusChanged?.Invoke(this, "Scanning for WeaponDA asset paths...");
            _weaponDACache.Clear();

            foreach (var weaponId in KnownWeaponIds)
            {
                string assetPath = $"/Game/Blueprint/Weapon/{weaponId}/DA_Weapon_{weaponId}";
                var addresses = ScanForString(assetPath);

                if (addresses.Count > 0)
                {
                    var info = new WeaponDAInfo
                    {
                        WeaponId = weaponId,
                        AssetPath = assetPath,
                        StringAddress = addresses[0],
                        DAPointer = IntPtr.Zero // Will be resolved by FindDAPointerFromString
                    };

                    // Try to find the actual DA pointer by backtracking from string reference
                    info.DAPointer = FindDAPointerFromString(addresses[0], assetPath);

                    _weaponDACache[weaponId] = info;
                    StatusChanged?.Invoke(this, $"Found {weaponId}: String@{addresses[0]:X}, DA@{info.DAPointer:X}");
                }
            }

            StatusChanged?.Invoke(this, $"Scan complete. Found {_weaponDACache.Count} weapons.");
            return new Dictionary<string, WeaponDAInfo>(_weaponDACache);
        }

        /// <summary>
        /// Scan for a specific weapon's DA in memory
        /// </summary>
        public WeaponDAInfo? ScanForWeaponDA(string weaponId)
        {
            if (!IsAttached)
            {
                ErrorOccurred?.Invoke(this, "Not attached to game");
                return null;
            }

            string assetPath = $"/Game/Blueprint/Weapon/{weaponId}/DA_Weapon_{weaponId}";
            StatusChanged?.Invoke(this, $"Scanning for {weaponId}...");

            var addresses = ScanForString(assetPath);
            if (addresses.Count == 0)
            {
                StatusChanged?.Invoke(this, $"{weaponId} not found in memory. May not be loaded.");
                return null;
            }

            var info = new WeaponDAInfo
            {
                WeaponId = weaponId,
                AssetPath = assetPath,
                StringAddress = addresses[0],
                DAPointer = FindDAPointerFromString(addresses[0], assetPath)
            };

            _weaponDACache[weaponId] = info;
            StatusChanged?.Invoke(this, $"Found {weaponId}: String@{addresses[0]:X}, DA@{info.DAPointer:X}");

            return info;
        }

        /// <summary>
        /// Try to find the UDataAsset pointer from an asset path string address.
        /// In UE4, the FName/path string is typically part of the UObject structure.
        /// The DA pointer is usually found by scanning for references to the string address.
        /// </summary>
        private IntPtr FindDAPointerFromString(IntPtr stringAddress, string assetPath)
        {
            // Strategy 1: Look for pointers referencing near this string address
            // UE4 UObjects have their name/path at a known offset from the object base
            // Typical UObject layout: VTable(8) + Flags(4) + Index(4) + Outer(8) + Name(8) + ...

            // Try common UObject name offsets (typically 0x18 or 0x28 from object base)
            long[] possibleOffsets = { 0x18, 0x28, 0x30, 0x20, 0x38, 0x40 };

            foreach (var offset in possibleOffsets)
            {
                IntPtr possibleBase = IntPtr.Subtract(stringAddress, (int)offset);

                // Verify this looks like a valid UObject (has a vtable pointer in valid range)
                var vtableBytes = ReadMemory(possibleBase, 8);
                if (vtableBytes != null)
                {
                    long vtable = BitConverter.ToInt64(vtableBytes, 0);
                    // VTable should be in the executable's address range
                    if (vtable > 0x140000000 && vtable < 0x150000000)
                    {
                        StatusChanged?.Invoke(this, $"  Potential DA base at offset -{offset:X}: {possibleBase:X}");
                        return possibleBase;
                    }
                }
            }

            // Strategy 2: Scan for pointers TO this string address
            // This finds code/data that references the string
            var stringAddrBytes = BitConverter.GetBytes(stringAddress.ToInt64());
            string pattern = BitConverter.ToString(stringAddrBytes).Replace("-", " ");

            StatusChanged?.Invoke(this, $"  Searching for references to string at {stringAddress:X}...");

            // For now, return the string address as a fallback
            // The actual DA pointer requires more sophisticated analysis
            return stringAddress;
        }

        /// <summary>
        /// Get cached WeaponDA info, or scan if not cached
        /// </summary>
        public WeaponDAInfo? GetWeaponDA(string weaponId)
        {
            if (_weaponDACache.TryGetValue(weaponId, out var cached))
            {
                return cached;
            }
            return ScanForWeaponDA(weaponId);
        }

        /// <summary>
        /// Call ServerSetWeaponDA to give a weapon to the player.
        /// REQUIRES: CapturedPlayerController to be set (from breakpoint on pickup)
        /// </summary>
        /// <param name="weaponId">Weapon ID (e.g., "Minigun", "RocketLauncher")</param>
        /// <returns>True if call was attempted, false on error</returns>
        public bool GiveWeapon(string weaponId)
        {
            if (!IsAttached)
            {
                ErrorOccurred?.Invoke(this, "Not attached to game");
                return false;
            }

            if (CapturedPlayerController == IntPtr.Zero)
            {
                ErrorOccurred?.Invoke(this, "PlayerController not set. Set breakpoint on ServerSetWeaponDA and capture RCX.");
                return false;
            }

            var weaponDA = GetWeaponDA(weaponId);
            if (weaponDA == null || weaponDA.DAPointer == IntPtr.Zero)
            {
                ErrorOccurred?.Invoke(this, $"Could not find WeaponDA for {weaponId}. Try picking up a weapon first to load assets.");
                return false;
            }

            StatusChanged?.Invoke(this, $"Calling ServerSetWeaponDA({CapturedPlayerController:X}, {weaponDA.DAPointer:X})...");

            return CallServerSetWeaponDA(CapturedPlayerController, weaponDA.DAPointer);
        }

        /// <summary>
        /// Call ServerSetWeaponDA with explicit addresses
        /// </summary>
        public bool CallServerSetWeaponDA(IntPtr playerController, IntPtr weaponDAPointer)
        {
            if (!IsAttached)
            {
                ErrorOccurred?.Invoke(this, "Not attached to game");
                return false;
            }

            // ServerSetWeaponDA address (x64 fastcall: RCX=this, RDX=weaponDA)
            IntPtr funcAddress = new IntPtr(KnownFunctions.ServerSetWeaponDA);

            StatusChanged?.Invoke(this, $"Preparing remote call to {funcAddress:X}...");
            StatusChanged?.Invoke(this, $"  RCX (PlayerController): {playerController:X}");
            StatusChanged?.Invoke(this, $"  RDX (WeaponDA): {weaponDAPointer:X}");

            // Build shellcode for x64 fastcall
            // mov rcx, playerController
            // mov rdx, weaponDAPointer
            // mov rax, funcAddress
            // call rax
            // ret
            byte[] shellcode = BuildCallShellcode(playerController, weaponDAPointer, funcAddress);

            return ExecuteRemoteShellcode(shellcode);
        }

        /// <summary>
        /// Build x64 shellcode to call a function with two parameters (fastcall)
        /// </summary>
        private byte[] BuildCallShellcode(IntPtr param1, IntPtr param2, IntPtr funcAddress)
        {
            using var ms = new System.IO.MemoryStream();
            using var bw = new System.IO.BinaryWriter(ms);

            // sub rsp, 0x28 (shadow space + alignment)
            bw.Write(new byte[] { 0x48, 0x83, 0xEC, 0x28 });

            // mov rcx, param1 (48 B9 xx xx xx xx xx xx xx xx)
            bw.Write((byte)0x48);
            bw.Write((byte)0xB9);
            bw.Write(param1.ToInt64());

            // mov rdx, param2 (48 BA xx xx xx xx xx xx xx xx)
            bw.Write((byte)0x48);
            bw.Write((byte)0xBA);
            bw.Write(param2.ToInt64());

            // mov rax, funcAddress (48 B8 xx xx xx xx xx xx xx xx)
            bw.Write((byte)0x48);
            bw.Write((byte)0xB8);
            bw.Write(funcAddress.ToInt64());

            // call rax (FF D0)
            bw.Write(new byte[] { 0xFF, 0xD0 });

            // add rsp, 0x28
            bw.Write(new byte[] { 0x48, 0x83, 0xC4, 0x28 });

            // ret (C3)
            bw.Write((byte)0xC3);

            return ms.ToArray();
        }

        /// <summary>
        /// Execute shellcode in the target process via CreateRemoteThread
        /// </summary>
        private bool ExecuteRemoteShellcode(byte[] shellcode)
        {
            // Allocate executable memory in target process
            IntPtr remoteMem = VirtualAllocEx(ProcessHandle, IntPtr.Zero, (uint)shellcode.Length,
                MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

            if (remoteMem == IntPtr.Zero)
            {
                ErrorOccurred?.Invoke(this, $"VirtualAllocEx failed: {Marshal.GetLastWin32Error()}");
                return false;
            }

            StatusChanged?.Invoke(this, $"Allocated shellcode at {remoteMem:X}");

            try
            {
                // Write shellcode to allocated memory
                if (!WriteProcessMemory(ProcessHandle, remoteMem, shellcode, shellcode.Length, out _))
                {
                    ErrorOccurred?.Invoke(this, $"WriteProcessMemory failed: {Marshal.GetLastWin32Error()}");
                    return false;
                }

                StatusChanged?.Invoke(this, "Shellcode written, creating remote thread...");

                // Create remote thread to execute shellcode
                IntPtr hThread = CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, remoteMem, IntPtr.Zero, 0, out uint threadId);

                if (hThread == IntPtr.Zero)
                {
                    ErrorOccurred?.Invoke(this, $"CreateRemoteThread failed: {Marshal.GetLastWin32Error()}");
                    return false;
                }

                StatusChanged?.Invoke(this, $"Thread {threadId} created, waiting for completion...");

                // Wait for thread to complete (with timeout)
                uint waitResult = WaitForSingleObject(hThread, 5000);

                if (waitResult == WAIT_OBJECT_0)
                {
                    GetExitCodeThread(hThread, out uint exitCode);
                    StatusChanged?.Invoke(this, $"Remote call completed with exit code: {exitCode}");
                    CloseHandle(hThread);
                    return true;
                }
                else
                {
                    ErrorOccurred?.Invoke(this, $"WaitForSingleObject failed or timed out: {waitResult}");
                    CloseHandle(hThread);
                    return false;
                }
            }
            finally
            {
                // Free allocated memory
                VirtualFreeEx(ProcessHandle, remoteMem, 0, MEM_RELEASE);
            }
        }

        /// <summary>
        /// Similar methods for abilities and melee weapons
        /// </summary>
        public bool GiveAbility(string abilityId)
        {
            if (!IsAttached || CapturedPlayerController == IntPtr.Zero)
            {
                ErrorOccurred?.Invoke(this, "Not attached or PlayerController not captured");
                return false;
            }

            string assetPath = $"/Game/Blueprint/Ability/DA_Ability_{abilityId}";
            var addresses = ScanForString(assetPath);

            if (addresses.Count == 0)
            {
                ErrorOccurred?.Invoke(this, $"Ability {abilityId} not found in memory");
                return false;
            }

            IntPtr daPointer = FindDAPointerFromString(addresses[0], assetPath);
            IntPtr funcAddress = new IntPtr(KnownFunctions.ServerSetAbilityDA);

            StatusChanged?.Invoke(this, $"Calling ServerSetAbilityDA for {abilityId}...");

            byte[] shellcode = BuildCallShellcode(CapturedPlayerController, daPointer, funcAddress);
            return ExecuteRemoteShellcode(shellcode);
        }

        public bool GiveMelee(string meleeId)
        {
            if (!IsAttached || CapturedPlayerController == IntPtr.Zero)
            {
                ErrorOccurred?.Invoke(this, "Not attached or PlayerController not captured");
                return false;
            }

            string assetPath = $"/Game/Blueprint/Melee/DA_Melee_{meleeId}";
            var addresses = ScanForString(assetPath);

            if (addresses.Count == 0)
            {
                ErrorOccurred?.Invoke(this, $"Melee {meleeId} not found in memory");
                return false;
            }

            IntPtr daPointer = FindDAPointerFromString(addresses[0], assetPath);
            IntPtr funcAddress = new IntPtr(KnownFunctions.ServerSetMeleeDA);

            StatusChanged?.Invoke(this, $"Calling ServerSetMeleeDA for {meleeId}...");

            byte[] shellcode = BuildCallShellcode(CapturedPlayerController, daPointer, funcAddress);
            return ExecuteRemoteShellcode(shellcode);
        }

        /// <summary>
        /// Generate a debug report of all found WeaponDAs for use with Cheat Engine
        /// </summary>
        public string GenerateWeaponDAReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== WEAPON DA SCAN REPORT ===\n");

            if (!IsAttached)
            {
                sb.AppendLine("Not attached to game. Attach first.");
                return sb.ToString();
            }

            sb.AppendLine($"Game Base: {BaseAddress:X}");
            sb.AppendLine($"Captured PlayerController: {CapturedPlayerController:X}");
            sb.AppendLine($"ServerSetWeaponDA: {KnownFunctions.ServerSetWeaponDA:X}");
            sb.AppendLine();

            var weapons = ScanForWeaponDAs();
            sb.AppendLine($"Found {weapons.Count} weapons:\n");

            foreach (var kvp in weapons.OrderBy(k => k.Key))
            {
                var w = kvp.Value;
                sb.AppendLine($"[{w.WeaponId}]");
                sb.AppendLine($"  Asset: {w.AssetPath}");
                sb.AppendLine($"  String Address: {w.StringAddress:X}");
                sb.AppendLine($"  DA Pointer: {w.DAPointer:X}");
                sb.AppendLine();
            }

            sb.AppendLine("\n=== CHEAT ENGINE USAGE ===");
            sb.AppendLine("1. Set breakpoint on 140D809C0 (ServerSetWeaponDA)");
            sb.AppendLine("2. Pick up any weapon to hit breakpoint");
            sb.AppendLine("3. Note RCX value (PlayerController)");
            sb.AppendLine("4. Use addresses above as RDX to give different weapons");

            return sb.ToString();
        }

        /// <summary>
        /// Set the captured player controller address (from debugging)
        /// </summary>
        public void SetPlayerController(long address)
        {
            CapturedPlayerController = new IntPtr(address);
            StatusChanged?.Invoke(this, $"PlayerController set to {CapturedPlayerController:X}");
        }

        /// <summary>
        /// Set the captured player controller address from hex string
        /// </summary>
        public void SetPlayerController(string hexAddress)
        {
            if (hexAddress.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hexAddress = hexAddress[2..];

            if (long.TryParse(hexAddress, System.Globalization.NumberStyles.HexNumber, null, out long addr))
            {
                SetPlayerController(addr);
            }
            else
            {
                ErrorOccurred?.Invoke(this, $"Invalid hex address: {hexAddress}");
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Read a null-terminated string from memory
        /// </summary>
        public string ReadString(IntPtr address, int maxLength = 256)
        {
            var data = ReadMemory(address, maxLength);
            if (data == null) return "";

            int nullIndex = Array.IndexOf(data, (byte)0);
            if (nullIndex < 0) nullIndex = maxLength;

            return Encoding.UTF8.GetString(data, 0, nullIndex);
        }

        /// <summary>
        /// Read a wide (unicode) string from memory
        /// </summary>
        public string ReadWideString(IntPtr address, int maxLength = 256)
        {
            var data = ReadMemory(address, maxLength * 2);
            if (data == null) return "";

            int nullIndex = -1;
            for (int i = 0; i < data.Length - 1; i += 2)
            {
                if (data[i] == 0 && data[i + 1] == 0)
                {
                    nullIndex = i;
                    break;
                }
            }
            if (nullIndex < 0) nullIndex = maxLength * 2;

            return Encoding.Unicode.GetString(data, 0, nullIndex);
        }

        /// <summary>
        /// Dump memory region for analysis
        /// </summary>
        public string DumpMemory(IntPtr address, int size)
        {
            var data = ReadMemory(address, size);
            if (data == null) return "Failed to read memory";

            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i += 16)
            {
                sb.Append($"{(address.ToInt64() + i):X16}: ");

                // Hex
                for (int j = 0; j < 16 && i + j < data.Length; j++)
                {
                    sb.Append($"{data[i + j]:X2} ");
                }

                // ASCII
                sb.Append(" | ");
                for (int j = 0; j < 16 && i + j < data.Length; j++)
                {
                    char c = (char)data[i + j];
                    sb.Append(char.IsControl(c) ? '.' : c);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        #endregion

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Detach();
            }

            _disposed = true;
        }

        ~MemoryTrainer()
        {
            Dispose(false);
        }

        #endregion
    }

    /// <summary>
    /// Simple trainer settings
    /// </summary>
    public class TrainerSettings
    {
        public bool GodMode { get; set; }
        public bool InfiniteAmmo { get; set; }
        public bool InfiniteCurrency { get; set; }
        public int CrystalAmount { get; set; } = 999999;
        public int KeyAmount { get; set; } = 999;
    }
}
