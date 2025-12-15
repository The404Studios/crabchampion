using System;
using System.Collections.Generic;

namespace UnrealSavEditor.Models
{
    /// <summary>
    /// Contains all known Crab Champions game data for the save editor
    /// </summary>
    public static class CrabChampionsData
    {
        // ============================================
        // RARITY/MASTERY LEVELS (from lowest to highest)
        // ============================================
        public static readonly string[] Rarities =
        {
            "Common",
            "Uncommon",
            "Rare",
            "Epic",
            "Legendary",
            "Prismatic"
        };

        public static readonly string[] DifficultyTiers =
        {
            "Bronze",
            "Silver",
            "Gold",
            "Sapphire",
            "Emerald",
            "Ruby",
            "Diamond",
            "Prismatic"
        };

        // ============================================
        // PRIMARY WEAPONS (20 total - from actual save file)
        // Path format: /Game/Blueprint/Weapon/{Id}/DA_Weapon_{Id}.DA_Weapon_{Id}
        // ============================================
        public static readonly WeaponInfo[] PrimaryWeapons =
        {
            // Starting weapons
            new("AutoRifle", "Auto Rifle", "Rifle", true),
            new("DualShotguns", "Dual Shotguns", "Shotgun", true),

            // Unlockable weapons (IDs from actual save file)
            new("DualPistols", "Dual Pistols", "Pistol", false),
            new("AutoShotgun", "Auto Shotgun", "Shotgun", false),
            new("BurstPistol", "Burst Pistol", "Pistol", false),
            new("Sniper", "Sniper", "Sniper", false),
            new("Crossbow", "Crossbow", "Sniper", false),
            new("OrbLauncher", "Orb Launcher", "Launcher", false),
            new("RocketLauncher", "Rocket Launcher", "Launcher", false),
            new("Minigun", "Minigun", "LMG", false),
            new("BladeLauncher", "Blade Launcher", "Launcher", false),
            new("ClusterLauncher", "Cluster Launcher", "Launcher", false),
            new("Flamethrower", "Flamethrower", "Special", false),
            new("ArcaneWand", "Arcane Wand", "Special", false),
            new("LaserCannons", "Laser Cannons", "Special", false),
            new("Seagle", "Seagle", "Rifle", false),
            new("MarksmanRifle", "Marksman Rifle", "Rifle", false),
            new("IceStaff", "Ice Staff", "Special", false),
            new("LightningScepter", "Lightning Scepter", "Special", false),
            new("PoisonCannon", "Poison Cannon", "Special", false),
        };

        // ============================================
        // ABILITIES (7 total - from actual save file)
        // Path format: /Game/Blueprint/Ability/DA_Ability_{Id}.DA_Ability_{Id}
        // ============================================
        public static readonly WeaponInfo[] SecondaryWeapons =
        {
            new("Grenade", "Grenade", "Explosive", true, "Ability"),
            new("GrapplingHook", "Grappling Hook", "Utility", false, "Ability"),
            new("BlackHole", "Black Hole", "Special", false, "Ability"),
            new("LaserBeam", "Laser Beam", "Energy", false, "Ability"),
            new("IceBlast", "Ice Blast", "Ice", false, "Ability"),
            new("ElectroGlobe", "Electro Globe", "Energy", false, "Ability"),
            new("AirStrike", "Air Strike", "Explosive", false, "Ability"),
        };

        // ============================================
        // MELEE WEAPONS (5 total - from actual save file)
        // Path format: /Game/Blueprint/Melee/DA_Melee_{Id}.DA_Melee_{Id}
        // ============================================
        public static readonly WeaponInfo[] MeleeWeapons =
        {
            new("Claw", "Claw", "Melee", true, "Melee"),
            new("Dagger", "Dagger", "Melee", false, "Melee"),
            new("Hammer", "Hammer", "Melee", false, "Melee"),
            new("Pickaxe", "Pickaxe", "Melee", false, "Melee"),
            new("Katana", "Katana", "Melee", false, "Melee"),
        };

        // ============================================
        // CHARACTER SKINS (from actual save file)
        // Path format: /Game/Character/Crab/Texture/SkinPrototype/MI_{Id}.MI_{Id}
        // ============================================
        public static readonly SkinInfo[] CharacterSkins =
        {
            // Default and basic skins
            new("Default", "Default", "Default", true, "/Game/Character/Crab/Texture/Default/MI_Crab_Default.MI_Crab_Default"),
            new("BlueIce", "Blue Ice", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_BlueIce.MI_BlueIce"),
            new("Glimmer", "Glimmer", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Glimmer.MI_Glimmer"),
            new("Flow", "Flow", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Flow.MI_Flow"),
            new("Aqua", "Aqua", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Aqua.MI_Aqua"),
            new("Iridescent", "Iridescent", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Iridescent.MI_Iridescent"),
            new("Warped", "Warped", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Warped.MI_Warped"),
            new("Amber", "Amber", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Amber.MI_Amber"),
            new("Watermelon", "Watermelon", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Watermelon.MI_Watermelon"),
            new("Chemical", "Chemical", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Chemical.MI_Chemical"),
            new("Tiger", "Tiger", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Tiger.MI_Tiger"),
            new("Snow", "Snow", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Snow.MI_Snow"),
            new("Cheetah", "Cheetah", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Cheetah.MI_Cheetah"),
            new("Geometric", "Geometric", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Geometric.MI_Geometric"),
            new("Orange", "Orange", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Orange.MI_Orange"),
            new("Pink", "Pink", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Pink.MI_Pink"),
            new("Waves", "Waves", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Waves.MI_Waves"),
            new("Swirl", "Swirl", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Swirl.MI_Swirl"),
            new("Jelly", "Jelly", "Common", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Jelly.MI_Jelly"),
            new("Obsidian", "Obsidian", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Obsidian.MI_Obsidian"),
            new("HotRod", "Hot Rod", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_HotRod.MI_HotRod"),
            new("Toxic", "Toxic", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Toxic.MI_Toxic"),
            new("Grid", "Grid", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Grid.MI_Grid"),
            new("PurpleTiger", "Purple Tiger", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_PurpleTiger.MI_PurpleTiger"),
            new("BlueTiger", "Blue Tiger", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_BlueTiger.MI_BlueTiger"),
            new("RedTiger", "Red Tiger", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_RedTiger.MI_RedTiger"),
            new("JetBlack", "Jet Black", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_JetBlack.MI_JetBlack"),
            new("Ocean", "Ocean", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Ocean.MI_Ocean"),
            new("Focus", "Focus", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Focus.MI_Focus"),
            new("Vibrations", "Vibrations", "Rare", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Vibrations.MI_Vibrations"),
            new("Chrome", "Chrome", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Chrome.MI_Chrome"),
            new("Vampire", "Vampire", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Vampire.MI_Vampire"),
            new("Reptile", "Reptile", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Reptile.MI_Reptile"),
            new("Damascus", "Damascus", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Damascus.MI_Damascus"),
            new("Current", "Current", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Current.MI_Current"),
            new("Regal", "Regal", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Regal.MI_Regal"),
            new("Heat", "Heat", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Heat.MI_Heat"),
            new("Festive", "Festive", "Epic", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Festive.MI_Festive"),

            // Rank skins
            new("Silver", "Silver", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Silver.MI_Silver"),
            new("FakeGold", "Fake Gold", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_FakeGold.MI_FakeGold"),
            new("Gold", "Gold", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Gold.MI_Gold"),
            new("Sapphire", "Sapphire", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Sapphire.MI_Sapphire"),
            new("Emerald", "Emerald", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Emerald.MI_Emerald"),
            new("Ruby", "Ruby", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Ruby.MI_Ruby"),
            new("Kaleidoscopic", "Kaleidoscopic", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Kaleidoscopic.MI_Kaleidoscopic"),
            new("Prismatic", "Prismatic", "Legendary", false, "/Game/Character/Crab/Texture/SkinPrototype/MI_Prismatic.MI_Prismatic"),
        };

        // ============================================
        // WEAPON SKINS
        // Path format: /Game/Blueprint/Cosmetics/WeaponSkins/DA_WeaponSkin_{WeaponId}_{SkinId}.DA_WeaponSkin_{WeaponId}_{SkinId}
        // ============================================
        public static readonly SkinInfo[] WeaponSkins =
        {
            // Universal weapon skins (apply to all weapons)
            new("Default", "Default", "Default", true),
            new("Gold", "Golden", "Rare", false),
            new("Silver", "Silver", "Uncommon", false),
            new("Obsidian", "Obsidian", "Rare", false),
            new("Crystal", "Crystal", "Rare", false),
            new("Neon", "Neon", "Epic", false),
            new("Prismatic", "Prismatic", "Legendary", false),
            new("Flame", "Flame", "Epic", false),
            new("Ice", "Ice", "Epic", false),
            new("Void", "Void", "Legendary", false),
            new("Champion", "Champion", "Legendary", false),
        };

        // ============================================
        // EMOTES
        // Path format: /Game/Blueprint/Cosmetics/Emotes/DA_Emote_{Id}.DA_Emote_{Id}
        // ============================================
        public static readonly CosmeticInfo[] Emotes =
        {
            new("Wave", "Wave", "Default"),
            new("Dance", "Dance", "Rare"),
            new("Flex", "Flex", "Uncommon"),
            new("Clap", "Clap", "Common"),
            new("Bow", "Bow", "Uncommon"),
            new("Victory", "Victory Pose", "Rare"),
            new("Taunt", "Taunt", "Rare"),
            new("Crabwalk", "Crab Walk", "Epic"),
            new("Moonwalk", "Moonwalk", "Epic"),
            new("Spin", "Spin", "Legendary"),
        };

        // ============================================
        // BANNERS / PROFILE COSMETICS
        // Path format: /Game/Blueprint/Cosmetics/Banners/DA_Banner_{Id}.DA_Banner_{Id}
        // ============================================
        public static readonly CosmeticInfo[] Banners =
        {
            new("Default", "Default Banner", "Default"),
            new("Bronze", "Bronze Banner", "Common"),
            new("Silver", "Silver Banner", "Uncommon"),
            new("Gold", "Gold Banner", "Rare"),
            new("Diamond", "Diamond Banner", "Epic"),
            new("Prismatic", "Prismatic Banner", "Legendary"),
            new("Champion", "Champion Banner", "Legendary"),
            new("Slayer", "Slayer Banner", "Rare"),
            new("Speedrunner", "Speedrunner Banner", "Rare"),
            new("Collector", "Collector Banner", "Epic"),
        };

        // ============================================
        // TITLES
        // ============================================
        public static readonly string[] Titles =
        {
            "Crab Champion",
            "Shell Shocker",
            "Wave Rider",
            "Island Hopper",
            "Boss Slayer",
            "Prismatic Master",
            "Speed Demon",
            "Collector",
            "Completionist",
            "Legend",
            "Champion",
            "Immortal",
            "Untouchable",
            "Destroyer",
            "Survivor",
        };

        // ============================================
        // CHALLENGES / ACHIEVEMENTS
        // ============================================
        public static readonly ChallengeInfo[] Challenges =
        {
            new("FirstWin", "First Victory", "Win your first run", 1),
            new("TenWins", "Veteran", "Win 10 runs", 10),
            new("HundredWins", "Champion", "Win 100 runs", 100),
            new("NoDamage", "Untouchable", "Complete a run without taking damage", 1),
            new("SpeedRun", "Speed Demon", "Complete a run in under 30 minutes", 1),
            new("AllWeapons", "Arsenal", "Unlock all weapons", 32),
            new("AllSkins", "Fashionista", "Unlock all character skins", 30),
            new("MaxMastery", "Master", "Max out weapon mastery", 100),
            new("AllDifficulties", "Conqueror", "Beat all difficulty tiers", 8),
            new("BossRush", "Boss Hunter", "Defeat 100 bosses", 100),
            new("Millionaire", "Rich Crab", "Collect 1,000,000 crystals total", 1000000),
            new("Perfectionist", "Perfectionist", "Get 100% accuracy in a run", 100),
            new("Marathon", "Marathon Runner", "Travel 1,000,000 distance", 1000000),
            new("AllPrismatic", "Prismatic Collector", "Get all items to Prismatic rarity", 32),
        };

        // ============================================
        // WEAPON MODS (from actual game data)
        // Path format: /Game/Blueprint/Pickup/WeaponMod/{Rarity}/DA_WeaponMod_{Id}.DA_WeaponMod_{Id}
        // ============================================
        public static readonly ModInfo[] WeaponMods =
        {
            // Common Weapon Mods
            new("AcceleratingShot", "Accelerating Shot", "Common", "Damage increases with consecutive hits"),
            new("ArcaneShot", "Arcane Shot", "Common", "Shots deal arcane damage"),
            new("BigMag", "Big Mag", "Common", "Increased magazine size"),
            new("BigShot", "Big Shot", "Common", "Larger projectiles"),
            new("BlindFire", "Blind Fire", "Common", "Increased fire rate but reduced accuracy"),
            new("BoomerangShot", "Boomerang Shot", "Common", "Shots return to you"),
            new("BouncingShot", "Bouncing Shot", "Common", "Shots bounce off surfaces"),
            new("ChaoticShot", "Chaotic Shot", "Common", "Shots move erratically"),
            new("EfficientShot", "Efficient Shot", "Common", "Reduced ammo consumption"),
            new("EscalatingShot", "Escalating Shot", "Common", "Damage increases over time"),
            new("FastShot", "Fast Shot", "Common", "Faster projectile speed"),
            new("FireShot", "Fire Shot", "Common", "Shots ignite enemies"),
            new("GlueShot", "Glue Shot", "Common", "Shots slow enemies"),
            new("GripTape", "Grip Tape", "Common", "Reduced recoil"),
            new("HeavyHitter", "Heavy Hitter", "Common", "Increased damage"),
            new("HeavyShot", "Heavy Shot", "Common", "More powerful shots"),
            new("HighCaliber", "High Caliber", "Common", "Increased bullet damage"),
            new("IceShot", "Ice Shot", "Common", "Shots freeze enemies"),
            new("KnockbackShot", "Knockback Shot", "Common", "Shots push enemies back"),
            new("LightningShot", "Lightning Shot", "Common", "Shots chain lightning"),
            new("MagShot", "Mag Shot", "Common", "Magazine-based damage bonus"),
            new("OrbitingShot", "Orbiting Shot", "Common", "Shots orbit around target"),
            new("PoisonShot", "Poison Shot", "Common", "Shots poison enemies"),
            new("RandomShot", "Random Shot", "Common", "Random elemental effects"),
            new("RapidFire", "Rapid Fire", "Common", "Increased fire rate"),
            new("RecoilShot", "Recoil Shot", "Common", "Shots have increased knockback"),
            new("ReloadArc", "Reload Arc", "Common", "Arc damage on reload"),
            new("SharpShot", "Sharp Shot", "Common", "Increased critical damage"),
            new("SnakeShot", "Snake Shot", "Common", "Wavy projectile path"),
            new("SonicBoom", "Sonic Boom", "Common", "Shockwave on impact"),
            new("SpiralShot", "Spiral Shot", "Common", "Spiral projectile path"),
            new("SteadyShot", "Steady Shot", "Common", "More accurate shots"),
            new("StreakShot", "Streak Shot", "Common", "Bonus for kill streaks"),
            new("TimeBolt", "Time Bolt", "Common", "Slow time on hit"),
            new("TimeShot", "Time Shot", "Common", "Damage over time"),
            new("TrickShot", "Trick Shot", "Common", "Ricochet damage bonus"),
            new("UltraShot", "Ultra Shot", "Common", "Massive damage boost"),
            new("WindUp", "Wind Up", "Common", "Charge for more damage"),
            new("ZigZagShot", "Zig Zag Shot", "Common", "Zigzag projectile path"),

            // Epic Weapon Mods
            new("ArcShot", "Arc Shot", "Epic", "Arcing projectiles"),
            new("ArcaneBlast", "Arcane Blast", "Epic", "Arcane explosion on hit"),
            new("AuraShot", "Aura Shot", "Epic", "Damage aura on hit"),
            new("DaggerArc", "Dagger Arc", "Epic", "Spawn daggers on reload"),
            new("DamageShot", "Damage Shot", "Epic", "Pure damage increase"),
            new("DoubleTap", "Double Tap", "Epic", "Fire two shots at once"),
            new("DrillShot", "Drill Shot", "Epic", "Piercing drill projectiles"),
            new("HealthShot", "Health Shot", "Epic", "Heal on hit"),
            new("Juiced", "Juiced", "Epic", "Damage boost when full health"),
            new("LinkShot", "Link Shot", "Epic", "Chain damage between enemies"),
            new("MaceShot", "Mace Shot", "Epic", "Heavy impact damage"),
            new("MoneyShot", "Money Shot", "Epic", "Bonus crystals on kill"),
            new("PiercingShot", "Piercing Shot", "Epic", "Shots pierce enemies"),
            new("PiercingWave", "Piercing Wave", "Epic", "Wave that pierces"),
            new("PumpkinShot", "Pumpkin Shot", "Epic", "Explosive pumpkins"),
            new("ScatterShot", "Scatter Shot", "Epic", "Spread shots"),
            new("ShotgunBlast", "Shotgun Blast", "Epic", "Shotgun-style burst"),
            new("Supercharged", "Supercharged", "Epic", "Overcharged shots"),
            new("TargetingShot", "Targeting Shot", "Epic", "Homing projectiles"),

            // Legendary Weapon Mods
            new("BombShot", "Bomb Shot", "Legendary", "Explosive projectiles"),
            new("DiceShot", "Dice Shot", "Legendary", "Random damage multiplier"),
            new("FireStorm", "Fire Storm", "Legendary", "Fire storm on kill"),
            new("FireballShot", "Fireball Shot", "Legendary", "Shoot fireballs"),
            new("Firepower", "Firepower", "Legendary", "Massive fire damage"),
            new("HomingBlades", "Homing Blades", "Legendary", "Spawn homing blades"),
            new("IceStorm", "Ice Storm", "Legendary", "Ice storm on kill"),
            new("IceStrike", "Ice Strike", "Legendary", "Freezing strike damage"),
            new("PoisonStorm", "Poison Storm", "Legendary", "Poison storm on kill"),
            new("ProximityBarrage", "Proximity Barrage", "Legendary", "Auto-fire when enemies near"),
            new("SharpenedAxe", "Sharpened Axe", "Legendary", "Throw axes"),
            new("SparkShot", "Spark Shot", "Legendary", "Electric sparks"),
            new("SplashDamage", "Splash Damage", "Legendary", "Area damage"),
            new("SplitShot", "Split Shot", "Legendary", "Shots split on hit"),
            new("SporeShot", "Spore Shot", "Legendary", "Poison spores on hit"),
        };

        // ============================================
        // ABILITY MODS (from actual game data)
        // Path format: /Game/Blueprint/Pickup/AbilityMod/{Rarity}/DA_AbilityMod_{Id}.DA_AbilityMod_{Id}
        // ============================================
        public static readonly ModInfo[] AbilityMods =
        {
            // Common Ability Mods
            new("BigAbility", "Big Ability", "Common", "Larger ability area"),
            new("BiggerBoom", "Bigger Boom", "Common", "Larger explosions"),
            new("ChaoticExplosion", "Chaotic Explosion", "Common", "Random explosion patterns"),
            new("GlueExplosion", "Glue Explosion", "Common", "Slowing explosions"),
            new("HeatSink", "Heat Sink", "Common", "Reduced cooldown"),
            new("ImplodingExplosion", "Imploding Explosion", "Common", "Pull enemies in"),
            new("IronExplosion", "Iron Explosion", "Common", "Armored explosions"),
            new("TimeExplosion", "Time Explosion", "Common", "Slow-mo explosion"),

            // Epic Ability Mods
            new("AuraExplosion", "Aura Explosion", "Epic", "Damage aura on explosion"),
            new("BouncingExplosion", "Bouncing Explosion", "Epic", "Bouncing explosions"),
            new("BubbleBlast", "Bubble Blast", "Epic", "Bubble projectiles"),
            new("DaggerBlast", "Dagger Blast", "Epic", "Spawn daggers"),
            new("DamageExplosion", "Damage Explosion", "Epic", "High damage explosions"),
            new("GiantDrill", "Giant Drill", "Epic", "Drilling projectile"),
            new("Grenadier", "Grenadier", "Epic", "Extra grenades"),
            new("LayeredExplosion", "Layered Explosion", "Epic", "Multiple explosion layers"),
            new("SentryTurret", "Sentry Turret", "Epic", "Deploy sentry turret"),
            new("SniperTurret", "Sniper Turret", "Epic", "Deploy sniper turret"),
            new("SparkExplosion", "Spark Explosion", "Epic", "Electric explosions"),
            new("ThornExplosion", "Thorn Explosion", "Epic", "Thorn damage on explosion"),

            // Legendary Ability Mods
            new("BeamTurret", "Beam Turret", "Legendary", "Deploy beam turret"),
            new("BombExplosion", "Bomb Explosion", "Legendary", "Cluster bombs"),
            new("CloneExplosion", "Clone Explosion", "Legendary", "Clone explosions"),
            new("CrystalBarrage", "Crystal Barrage", "Legendary", "Crystal projectile barrage"),
            new("FireExplosion", "Fire Explosion", "Legendary", "Fire explosions"),
            new("IceExplosion", "Ice Explosion", "Legendary", "Freezing explosions"),
            new("LightningExplosion", "Lightning Explosion", "Legendary", "Lightning explosions"),
            new("MortarTurret", "Mortar Turret", "Legendary", "Deploy mortar turret"),
            new("PoisonExplosion", "Poison Explosion", "Legendary", "Poison explosions"),
            new("ScytheVortex", "Scythe Vortex", "Legendary", "Spinning scythe vortex"),
            new("SpinningBlade", "Spinning Blade", "Legendary", "Orbiting blade"),
            new("SplitAbility", "Split Ability", "Legendary", "Split into multiple"),
            new("SporeExplosion", "Spore Explosion", "Legendary", "Poison spore cloud"),
            new("TargetingExplosion", "Targeting Explosion", "Legendary", "Homing explosions"),
            new("WaveTurret", "Wave Turret", "Legendary", "Deploy wave turret"),
        };

        // ============================================
        // MELEE MODS (from actual game data)
        // Path format: /Game/Blueprint/Pickup/MeleeMod/{Rarity}/DA_MeleeMod_{Id}.DA_MeleeMod_{Id}
        // ============================================
        public static readonly ModInfo[] MeleeMods =
        {
            // Common Melee Mods
            new("BigClaws", "Big Claws", "Common", "Larger melee range"),
            new("Blender", "Blender", "Common", "Faster attack speed"),
            new("IronClaws", "Iron Claws", "Common", "Increased damage"),
            new("SharpClaws", "Sharp Claws", "Common", "Critical damage boost"),
            new("TimeClaws", "Time Claws", "Common", "Damage over time"),
            new("Vampire", "Vampire", "Common", "Lifesteal on hit"),

            // Epic Melee Mods
            new("ArcaneClaws", "Arcane Claws", "Epic", "Arcane melee damage"),
            new("FireClaws", "Fire Claws", "Epic", "Fire melee damage"),
            new("IceClaws", "Ice Claws", "Epic", "Ice melee damage"),
            new("LightningClaws", "Lightning Claws", "Epic", "Lightning melee damage"),
            new("PoisonClaws", "Poison Claws", "Epic", "Poison melee damage"),

            // Greed Melee Mods
            new("Brawler", "Brawler", "Greed", "High risk high reward melee"),
        };

        // ============================================
        // RELICS (from actual game data)
        // Path format: /Game/Blueprint/Pickup/Relic/{Rarity}/DA_Relic_{Id}.DA_Relic_{Id}
        // ============================================
        public static readonly RelicInfo[] Relics =
        {
            // Common Relics
            new("AdrenalineAmulet", "Adrenaline Amulet", "Common", "Fire rate boost after kills"),
            new("BlacksmithAmulet", "Blacksmith Amulet", "Common", "Extra anvil choices"),
            new("ComboRing", "Combo Ring", "Common", "Combo damage bonus"),
            new("CoralAmulet", "Coral Amulet", "Common", "More chest loot choices"),
            new("Icebreaker", "Icebreaker", "Common", "Bonus damage to frozen enemies"),
            new("PortalRing", "Portal Ring", "Common", "Portal-related bonuses"),
            new("RingOfArmor", "Ring Of Armor", "Common", "Bonus armor"),
            new("RingOfDestruction", "Ring Of Destruction", "Common", "Damage bonus"),
            new("RingOfHealing", "Ring Of Healing", "Common", "Health regeneration"),
            new("RingOfHealthyTurrets", "Ring Of Healthy Turrets", "Common", "Turret health boost"),
            new("RingOfReloading", "Ring Of Reloading", "Common", "Faster reload"),
            new("RingOfVigor", "Ring Of Vigor", "Common", "Max health per island"),
            new("TonysAmulet", "Tony's Amulet", "Common", "More shop items"),

            // Epic Relics
            new("AmmoRing", "Ammo Ring", "Epic", "Ammo bonuses"),
            new("ArcaneRing", "Arcane Ring", "Epic", "Arcane damage boost"),
            new("BlacksmithRing", "Blacksmith Ring", "Epic", "Better anvil options"),
            new("DuplicationRing", "Duplication Ring", "Epic", "Chance to duplicate items"),
            new("EtherealArmor", "Ethereal Armor", "Epic", "Ethereal protection"),
            new("FireRing", "Fire Ring", "Epic", "Fire damage boost"),
            new("FullMetalJacket", "Full Metal Jacket", "Epic", "Armor piercing"),
            new("IceRing", "Ice Ring", "Epic", "Ice damage boost"),
            new("LightningRing", "Lightning Ring", "Epic", "Lightning damage boost"),
            new("PoisonRing", "Poison Ring", "Epic", "Poison damage boost"),
            new("RingOfDefense", "Ring Of Defense", "Epic", "Damage reduction"),
            new("RingOfDeflection", "Ring Of Deflection", "Epic", "Block chance"),
            new("RingOfDividends", "Ring Of Dividends", "Epic", "Crystal multiplier"),
            new("RingOfFury", "Ring Of Fury", "Epic", "Damage when low health"),
            new("RingOfPotential", "Ring Of Potential", "Epic", "Legendary chest chance"),
            new("RingOfPower", "Ring Of Power", "Epic", "Raw damage increase"),
            new("RingOfPrecision", "Ring Of Precision", "Epic", "Critical hit chance"),
            new("RingOfReinforcement", "Ring Of Reinforcement", "Epic", "Extra armor plates"),
            new("RingOfRepulsion", "Ring Of Repulsion", "Epic", "Knockback enemies"),
            new("RingOfValue", "Ring Of Value", "Epic", "Better item values"),
            new("RingOfWisdom", "Ring Of Wisdom", "Epic", "Crit chance per island"),
            new("SkillRing", "Skill Ring", "Epic", "Ability cooldown reduction"),
            new("TimeRing", "Time Ring", "Epic", "DOT damage increase"),
            new("TurboRing", "Turbo Ring", "Epic", "Fire rate per island"),

            // Legendary Relics
            new("AbilityRing", "Ability Ring", "Legendary", "Ability damage multiplier"),
            new("PortalAmulet", "Portal Amulet", "Legendary", "Portal bonuses"),
            new("RingOfGravity", "Ring Of Gravity", "Legendary", "Low gravity"),
            new("RingOfLuck", "Ring Of Luck", "Legendary", "Increased luck"),
            new("RingOfProtection", "Ring Of Protection", "Legendary", "Invulnerability chance"),
            new("RingOfRegeneratingArmor", "Ring Of Regenerating Armor", "Legendary", "Armor per island"),
            new("RingOfSwiftness", "Ring Of Swiftness", "Legendary", "Movement speed"),
            new("TwinRing", "Twin Ring", "Legendary", "Turrets spawn in pairs"),

            // Greed Relics
            new("HighRoller", "High Roller", "Greed", "Risky chest spawns"),
            new("HoarderBackpack", "Hoarder's Backpack", "Greed", "Extra inventory"),
            new("OverspillGoblet", "Overspill Goblet", "Greed", "Crit to crit damage"),
            new("RingOfFavoritism", "Ring Of Favoritism", "Greed", "Boost single mod"),
            new("RingOfTankiness", "Ring Of Tankiness", "Greed", "Health up, speed down"),
            new("TriggerRing", "Trigger Ring", "Greed", "Fire rate up, damage down"),
            new("UpgradeRing", "Upgrade Ring", "Greed", "Guaranteed duplicates"),
        };

        // ============================================
        // PERKS (from actual game data)
        // Path format: /Game/Blueprint/Pickup/Perk/{Rarity}/DA_Perk_{Id}.DA_Perk_{Id}
        // ============================================
        public static readonly PerkInfo[] Perks =
        {
            // Common Perks
            new("AntiCrit", "Anti Crit", "Common", "Reduced enemy crit damage"),
            new("Banana", "Banana", "Common", "Health boost"),
            new("BountyHunter", "Bounty Hunter", "Common", "Bonus crystals from elites"),
            new("Bulletproof", "Bulletproof", "Common", "Projectile damage reduction"),
            new("Bullseye", "Bullseye", "Common", "Accuracy bonus"),
            new("CriticalArrow", "Critical Arrow", "Common", "Critical hit boost"),
            new("CriticalThinking", "Critical Thinking", "Common", "Crit damage increase"),
            new("CrystalCombo", "Crystal Combo", "Common", "Crystals from combos"),
            new("CrystalFertilizer", "Crystal Fertilizer", "Common", "Crystal growth"),
            new("DamageCombo", "Damage Combo", "Common", "Combo damage bonus"),
            new("DangerClose", "Danger Close", "Common", "Close range damage"),
            new("Driller", "Driller", "Common", "Pierce through enemies"),
            new("EagleEye", "Eagle Eye", "Common", "Long range damage"),
            new("ElementalExpert", "Elemental Expert", "Common", "Elemental damage boost"),
            new("ElementalSpecialist", "Elemental Specialist", "Common", "Single element boost"),
            new("Endurance", "Endurance", "Common", "Stamina increase"),
            new("EnhancedTurrets", "Enhanced Turrets", "Common", "Turret upgrades"),
            new("Equalizer", "Equalizer", "Common", "Balanced stats"),
            new("Firestarter", "Firestarter", "Common", "Fire damage boost"),
            new("Fortitude", "Fortitude", "Common", "Max health increase"),
            new("HardTarget", "Hard Target", "Common", "Reduced damage taken"),
            new("HighVoltage", "High Voltage", "Common", "Lightning damage boost"),
            new("HotShot", "Hot Shot", "Common", "Fire rate on kill"),
            new("HotSteam", "Hot Steam", "Common", "Steam damage"),
            new("IceCold", "Ice Cold", "Common", "Ice damage boost"),
            new("Magnify", "Magnify", "Common", "Damage multiplier"),
            new("Mango", "Mango", "Common", "Health and speed"),
            new("Paycheck", "Paycheck", "Common", "Crystals per island"),
            new("PersonalSpace", "Personal Space", "Common", "Melee damage boost"),
            new("PoisonousArmor", "Poisonous Armor", "Common", "Poison on hit"),
            new("PotentMagic", "Potent Magic", "Common", "Arcane damage boost"),
            new("PowerArmor", "Power Armor", "Common", "Armor and damage"),
            new("PowerPunch", "Power Punch", "Common", "Melee damage"),
            new("Regenerator", "Regenerator", "Common", "Health regen"),
            new("Scavenger", "Scavenger", "Common", "Better drops"),
            new("Sharpshooter", "Sharpshooter", "Common", "Accuracy and crit"),
            new("Slugger", "Slugger", "Common", "Heavy weapon damage"),
            new("Snatcher", "Snatcher", "Common", "Item attraction"),
            new("SpecialDelivery", "Special Delivery", "Common", "Shop bonuses"),
            new("SpeedDemon", "Speed Demon", "Common", "Movement speed"),
            new("Stamina", "Stamina", "Common", "Sprint duration"),
            new("StreamerLoot", "Streamer Loot", "Common", "Better loot chance"),
            new("TonysBlackCard", "Tony's Black Card", "Common", "Shop discounts"),
            new("Toxic", "Toxic", "Common", "Poison damage"),
            new("ValuedCustomer", "Valued Customer", "Common", "Shop bonuses"),
            new("Vitality", "Vitality", "Common", "Health boost"),

            // Epic Perks
            new("AllYouCanEat", "All You Can Eat", "Epic", "Food healing boost"),
            new("AmberResin", "Amber Resin", "Epic", "Slow enemies on hit"),
            new("Assassin", "Assassin", "Epic", "Backstab damage"),
            new("BigChests", "Big Chests", "Epic", "Better chest loot"),
            new("BonusCrystals", "Bonus Crystals", "Epic", "Extra crystals"),
            new("Collector", "Collector", "Epic", "Item bonuses"),
            new("CriticalBlast", "Critical Blast", "Epic", "Crit explosions"),
            new("DamageAura", "Damage Aura", "Epic", "Damage aura"),
            new("DoubleVision", "Double Vision", "Epic", "Double projectiles"),
            new("ExplodingEnemies", "Exploding Enemies", "Epic", "Enemies explode on death"),
            new("FlammableArmor", "Flammable Armor", "Epic", "Fire armor"),
            new("Gemstone", "Gemstone", "Epic", "Crystal bonuses"),
            new("GoldCoating", "Gold Coating", "Epic", "Golden weapon bonus"),
            new("GrimReaper", "Grim Reaper", "Epic", "Death bonuses"),
            new("HealthIsPower", "Health Is Power", "Epic", "Damage from health"),
            new("MegaCrit", "Mega Crit", "Epic", "Massive crits"),
            new("MoneyIsPower", "Money Is Power", "Epic", "Damage from crystals"),
            new("OrbitingScythes", "Orbiting Scythes", "Epic", "Orbital damage"),
            new("PerformanceBonus", "Performance Bonus", "Epic", "Combo rewards"),
            new("PoisonAura", "Poison Aura", "Epic", "Poison aura"),
            new("SilverLining", "Silver Lining", "Epic", "Bonus on low health"),
            new("SpeedIsPower", "Speed Is Power", "Epic", "Damage from speed"),
            new("SturdyTotems", "Sturdy Totems", "Epic", "Totem bonuses"),
            new("Survivor", "Survivor", "Epic", "Survival bonuses"),
            new("TotemEnthusiast", "Totem Enthusiast", "Epic", "Totem bonuses"),

            // Legendary Perks
            new("DaggerDash", "Dagger Dash", "Legendary", "Daggers on dash"),
            new("FaultyChests", "Faulty Chests", "Legendary", "Explosive chests"),
            new("FlammableEnemies", "Flammable Enemies", "Legendary", "Fire spread"),
            new("FreezingEnemies", "Freezing Enemies", "Legendary", "Ice spread"),
            new("IceDash", "Ice Dash", "Legendary", "Ice trail on dash"),
            new("LevelUp", "Level Up", "Legendary", "Level bonuses"),
            new("LightningDash", "Lightning Dash", "Legendary", "Lightning on dash"),
            new("PoisonousEnemies", "Poisonous Enemies", "Legendary", "Poison spread"),
            new("Powerslide", "Powerslide", "Legendary", "Slide damage"),
            new("RareTreasure", "Rare Treasure", "Legendary", "Rare loot chance"),

            // Greed Perks
            new("BigBones", "Big Bones", "Greed", "Size and health increase"),
            new("Bribe", "Bribe", "Greed", "Pay for bonuses"),
            new("BruteForce", "Brute Force", "Greed", "Damage up, defense down"),
            new("DamageSeeker", "Damage Seeker", "Greed", "Risk for damage"),
            new("DoubleTrouble", "Double Trouble", "Greed", "Double effects"),
            new("GlassCannon", "Glass Cannon", "Greed", "High damage, low health"),
            new("Juggernaut", "Juggernaut", "Greed", "Unstoppable force"),
            new("LeapOfFaith", "Leap Of Faith", "Greed", "Random health change"),
            new("LimitedLoot", "Limited Loot", "Greed", "Less but better loot"),
            new("RisingStar", "Rising Star", "Greed", "Growing power"),
            new("SlipperySlope", "Slippery Slope", "Greed", "Speed and risk"),
            new("UpTheAnte", "Up The Ante", "Greed", "Increasing stakes"),
            new("Workaholic", "Workaholic", "Greed", "Work harder bonuses"),
        };

        // ============================================
        // GAME MODES
        // ============================================
        public static readonly string[] GameModes =
        {
            "Classic",
            "Endless",
            "Challenge",
            "Daily",
            "Weekly"
        };

        // ============================================
        // ISLAND BIOMES
        // ============================================
        public static readonly string[] Biomes =
        {
            "Beach",
            "Jungle",
            "Volcano",
            "Crystal",
            "Ice",
            "Void"
        };

        // ============================================
        // BOSS TYPES
        // ============================================
        public static readonly string[] Bosses =
        {
            "KingCrab",
            "GiantSquid",
            "MegaShark",
            "LavaGolem",
            "IceDragon",
            "VoidKraken"
        };

        // ============================================
        // STAT TRACKING CATEGORIES
        // ============================================
        public static readonly StatInfo[] TrackedStats =
        {
            new("TotalKills", "Total Kills", "Combat", "Total enemies killed"),
            new("TotalDeaths", "Total Deaths", "Combat", "Total times died"),
            new("BossesKilled", "Bosses Killed", "Combat", "Total bosses defeated"),
            new("DamageDealt", "Damage Dealt", "Combat", "Total damage inflicted"),
            new("DamageTaken", "Damage Taken", "Combat", "Total damage received"),
            new("CriticalHits", "Critical Hits", "Combat", "Total critical hits landed"),

            new("TotalRuns", "Total Runs", "Progress", "Total runs started"),
            new("RunsCompleted", "Runs Completed", "Progress", "Successful run completions"),
            new("HighestWave", "Highest Wave", "Progress", "Best wave reached"),
            new("HighestDifficulty", "Highest Difficulty", "Progress", "Hardest difficulty beaten"),

            new("TotalPlayTime", "Play Time", "General", "Total time played (seconds)"),
            new("CrystalsCollected", "Crystals Collected", "General", "Total crystals earned"),
            new("KeysCollected", "Keys Collected", "General", "Total keys found"),
            new("ItemsCollected", "Items Collected", "General", "Total items picked up"),
            new("PerksObtained", "Perks Obtained", "General", "Total perks collected"),

            new("ShotsFired", "Shots Fired", "Misc", "Total projectiles fired"),
            new("Accuracy", "Accuracy", "Misc", "Hit percentage"),
            new("DistanceTraveled", "Distance Traveled", "Misc", "Total distance moved"),
            new("JumpsPerformed", "Jumps", "Misc", "Total jumps"),
        };

        // ============================================
        // PRESET PROFILES
        // ============================================
        public static readonly PresetProfile[] Presets =
        {
            new("GodMode", "God Mode", "Unlock everything, max stats, prismatic rarity, all skins",
                unlockAll: true, prismatic: true, maxCurrency: true, maxMastery: true,
                unlockSkins: true, unlockCosmetics: true),
            new("AllUnlocks", "All Unlocks", "Unlock all weapons, skins and difficulties",
                unlockAll: true, prismatic: false, maxCurrency: false, maxMastery: false,
                unlockSkins: true),
            new("Prismatic", "Prismatic Collection", "Set all items to prismatic rarity",
                unlockAll: false, prismatic: true, maxCurrency: false, maxMastery: false),
            new("RichCrab", "Rich Crab", "Max out currency only",
                unlockAll: false, prismatic: false, maxCurrency: true, maxMastery: false),
            new("Fashionista", "Fashionista", "Unlock all skins and cosmetics only",
                unlockAll: false, prismatic: false, maxCurrency: false, maxMastery: false,
                unlockSkins: true, unlockCosmetics: true),
            new("FreshStart", "Fresh Start", "Reset to default (starter weapons only)",
                unlockAll: false, prismatic: false, maxCurrency: false, maxMastery: false, isReset: true),
        };

        // ============================================
        // PROPERTY NAME PATTERNS FOR SAVE FILE
        // Based on actual Crab Champions save structure
        // ============================================
        public static class PropertyPatterns
        {
            // Unlock patterns - lowercase variants common in UE4/UE5
            public static readonly string[] UnlockedWeapons = {
                "UnlockedWeapons", "unlockedweapons", "WeaponUnlocks", "weaponunlocks",
                "Weapons", "weapons", "UnlockedPrimaryWeapons", "unlockedprimaryweapons",
                "PrimaryWeapons", "primaryweapons", "ownedweapons", "OwnedWeapons"
            };
            public static readonly string[] UnlockedAbilities = {
                "UnlockedAbilities", "unlockedabilities", "AbilityUnlocks", "abilityunlocks",
                "Abilities", "abilities", "UnlockedSecondaryWeapons", "unlockedsecondaryweapons",
                "SecondaryWeapons", "secondaryweapons", "ownedabilities", "OwnedAbilities",
                "grenades", "Grenades"
            };
            public static readonly string[] UnlockedMelee = {
                "UnlockedMelee", "unlockedmelee", "MeleeUnlocks", "meleeunlocks",
                "UnlockedMeleeWeapons", "unlockedmeleeweapons", "MeleeWeapons", "meleeweapons",
                "ownedmelee", "OwnedMelee"
            };
            public static readonly string[] UnlockedPerks = {
                "UnlockedPerks", "unlockedperks", "PerkUnlocks", "perkunlocks",
                "Perks", "perks", "AvailablePerks", "availableperks"
            };

            // Mastery/Rarity patterns
            public static readonly string[] WeaponMastery = {
                "WeaponMastery", "weaponmastery", "Mastery", "mastery",
                "WeaponLevels", "weaponlevels", "WeaponRarity", "weaponrarity",
                "weapondata", "WeaponData", "weaponstats", "WeaponStats"
            };
            public static readonly string[] AbilityMastery = {
                "AbilityMastery", "abilitymastery", "AbilityLevels", "abilitylevels",
                "AbilityRarity", "abilityrarity"
            };

            // Difficulty/Wins patterns - actual Crab Champions naming
            public static readonly string[] DifficultyWins = {
                "bronzewins", "BronzeWins", "silverwins", "SilverWins",
                "goldwins", "GoldWins", "sapphirewins", "SapphireWins",
                "emeraldwins", "EmeraldWins", "rubywins", "RubyWins",
                "diamondwins", "DiamondWins", "prismaticwins", "PrismaticWins"
            };

            // Stats patterns
            public static readonly string[] TotalKills = { "TotalKills", "totalkills", "Kills", "kills", "EnemiesKilled", "enemieskilled" };
            public static readonly string[] TotalDeaths = { "TotalDeaths", "totaldeaths", "Deaths", "deaths", "DeathCount", "deathcount" };
            public static readonly string[] TotalRuns = { "TotalRuns", "totalruns", "RunsCompleted", "runscompleted", "GamesPlayed", "gamesplayed" };
            public static readonly string[] HighestWave = { "HighestWave", "highestwave", "MaxWave", "maxwave", "BestWave", "bestwave" };
            public static readonly string[] TotalPlayTime = { "TotalPlayTime", "totalplaytime", "PlayTime", "playtime", "TimePlayed", "timeplayed" };
            public static readonly string[] BossesKilled = { "BossesKilled", "bosseskilled", "BossKills", "bosskills", "BossesDefeated", "bossesdefeated" };

            // Currency patterns
            public static readonly string[] Crystals = { "Crystals", "crystals", "TotalCrystals", "totalcrystals", "Currency", "currency", "gold", "Gold" };
            public static readonly string[] Keys = { "Keys", "keys", "TotalKeys", "totalkeys", "KeyCount", "keycount" };

            // Challenges patterns
            public static readonly string[] CompletedChallenges = { "CompletedChallenges", "completedchallenges", "Challenges", "challenges", "Achievements", "achievements" };
            public static readonly string[] UnlockedDifficulties = { "UnlockedDifficulties", "unlockeddifficulties", "Difficulties", "difficulties", "DifficultyUnlocks", "difficultyunlocks" };
        }

        // ============================================
        // SAVE MODIFICATION HELPERS
        // ============================================

        /// <summary>
        /// Get the maximum mastery level value
        /// </summary>
        public static int GetMaxMasteryLevel() => 100;

        /// <summary>
        /// Get prismatic rarity index
        /// </summary>
        public static int GetPrismaticRarityIndex() => Rarities.Length - 1; // 5 = Prismatic

        /// <summary>
        /// Get all weapon IDs for unlocking
        /// </summary>
        public static List<string> GetAllWeaponIds()
        {
            var ids = new List<string>();
            foreach (var w in PrimaryWeapons) ids.Add(w.Id);
            return ids;
        }

        /// <summary>
        /// Get all ability IDs for unlocking
        /// </summary>
        public static List<string> GetAllAbilityIds()
        {
            var ids = new List<string>();
            foreach (var a in SecondaryWeapons) ids.Add(a.Id);
            return ids;
        }

        /// <summary>
        /// Get all melee IDs for unlocking
        /// </summary>
        public static List<string> GetAllMeleeIds()
        {
            var ids = new List<string>();
            foreach (var m in MeleeWeapons) ids.Add(m.Id);
            return ids;
        }

        /// <summary>
        /// Get all perk IDs for unlocking
        /// </summary>
        public static List<string> GetAllPerkIds()
        {
            var ids = new List<string>();
            foreach (var p in Perks) ids.Add(p.Id);
            return ids;
        }

        /// <summary>
        /// Get all weapon asset paths for unlocking
        /// </summary>
        public static List<string> GetAllWeaponAssetPaths()
        {
            var paths = new List<string>();
            foreach (var w in PrimaryWeapons) paths.Add(w.AssetPath);
            return paths;
        }

        /// <summary>
        /// Get all ability asset paths for unlocking
        /// </summary>
        public static List<string> GetAllAbilityAssetPaths()
        {
            var paths = new List<string>();
            foreach (var a in SecondaryWeapons) paths.Add(a.AssetPath);
            return paths;
        }

        /// <summary>
        /// Get all melee asset paths for unlocking
        /// </summary>
        public static List<string> GetAllMeleeAssetPaths()
        {
            var paths = new List<string>();
            foreach (var m in MeleeWeapons) paths.Add(m.AssetPath);
            return paths;
        }
    }

    /// <summary>
    /// Represents a weapon/ability in the game
    /// </summary>
    public class WeaponInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public bool IsStarterWeapon { get; }
        public string AssetPath { get; }

        // For weapons: /Game/Blueprint/Weapon/{Id}/DA_Weapon_{Id}.DA_Weapon_{Id}
        public WeaponInfo(string id, string displayName, string category, bool isStarter, string assetType = "Weapon")
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            IsStarterWeapon = isStarter;
            // Weapons have a subfolder, abilities/melee don't
            if (assetType == "Weapon")
                AssetPath = $"/Game/Blueprint/Weapon/{id}/DA_Weapon_{id}.DA_Weapon_{id}";
            else if (assetType == "Ability")
                AssetPath = $"/Game/Blueprint/Ability/DA_Ability_{id}.DA_Ability_{id}";
            else if (assetType == "Melee")
                AssetPath = $"/Game/Blueprint/Melee/DA_Melee_{id}.DA_Melee_{id}";
            else
                AssetPath = $"/Game/Blueprint/{assetType}/{id}/DA_{assetType}_{id}.DA_{assetType}_{id}";
        }

        public WeaponInfo(string id, string displayName, string category, bool isStarter, string assetPath, bool customPath)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            IsStarterWeapon = isStarter;
            AssetPath = assetPath;
        }
    }

    /// <summary>
    /// Represents a perk/upgrade in the game
    /// </summary>
    public class PerkInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public string Description { get; }

        public PerkInfo(string id, string displayName, string category, string description)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            Description = description;
        }
    }

    /// <summary>
    /// Represents a tracked statistic
    /// </summary>
    public class StatInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public string Description { get; }

        public StatInfo(string id, string displayName, string category, string description)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            Description = description;
        }
    }

    /// <summary>
    /// Represents a preset profile for quick modifications
    /// </summary>
    public class PresetProfile
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public bool UnlockAll { get; }
        public bool SetPrismatic { get; }
        public bool MaxCurrency { get; }
        public bool MaxMastery { get; }
        public bool IsReset { get; }
        public bool UnlockSkins { get; }
        public bool UnlockCosmetics { get; }

        public PresetProfile(string id, string displayName, string description,
            bool unlockAll = false, bool prismatic = false, bool maxCurrency = false,
            bool maxMastery = false, bool isReset = false, bool unlockSkins = false,
            bool unlockCosmetics = false)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            UnlockAll = unlockAll;
            SetPrismatic = prismatic;
            MaxCurrency = maxCurrency;
            MaxMastery = maxMastery;
            IsReset = isReset;
            UnlockSkins = unlockSkins;
            UnlockCosmetics = unlockCosmetics;
        }
    }

    /// <summary>
    /// Represents a character or weapon skin
    /// </summary>
    public class SkinInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Rarity { get; }
        public bool IsDefault { get; }
        public string AssetPath { get; }

        public SkinInfo(string id, string displayName, string rarity, bool isDefault)
        {
            Id = id;
            DisplayName = displayName;
            Rarity = rarity;
            IsDefault = isDefault;
            AssetPath = $"/Game/Blueprint/Cosmetics/Skins/DA_Skin_{id}.DA_Skin_{id}";
        }

        public SkinInfo(string id, string displayName, string rarity, bool isDefault, string customPath)
        {
            Id = id;
            DisplayName = displayName;
            Rarity = rarity;
            IsDefault = isDefault;
            AssetPath = customPath;
        }
    }

    /// <summary>
    /// Represents an emote, banner, or other cosmetic item
    /// </summary>
    public class CosmeticInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Rarity { get; }
        public string AssetPath { get; }

        public CosmeticInfo(string id, string displayName, string rarity)
        {
            Id = id;
            DisplayName = displayName;
            Rarity = rarity;
            AssetPath = $"/Game/Blueprint/Cosmetics/{id}/DA_{id}.DA_{id}";
        }
    }

    /// <summary>
    /// Represents a challenge/achievement
    /// </summary>
    public class ChallengeInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int RequiredValue { get; }
        public string AssetPath { get; }

        public ChallengeInfo(string id, string displayName, string description, int requiredValue)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            RequiredValue = requiredValue;
            AssetPath = $"/Game/Blueprint/Challenges/DA_Challenge_{id}.DA_Challenge_{id}";
        }
    }

    /// <summary>
    /// Represents a mod (weapon mod, ability mod, or melee mod)
    /// </summary>
    public class ModInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Rarity { get; }
        public string Description { get; }

        public ModInfo(string id, string displayName, string rarity, string description)
        {
            Id = id;
            DisplayName = displayName;
            Rarity = rarity;
            Description = description;
        }

        /// <summary>
        /// Get the asset path for this mod
        /// </summary>
        public string GetAssetPath(string modType)
        {
            // modType is "WeaponMod", "AbilityMod", or "MeleeMod"
            return $"/Game/Blueprint/Pickup/{modType}/{Rarity}/DA_{modType}_{Id}.DA_{modType}_{Id}";
        }
    }

    /// <summary>
    /// Represents a relic
    /// </summary>
    public class RelicInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Rarity { get; }
        public string Description { get; }
        public string AssetPath { get; }

        public RelicInfo(string id, string displayName, string rarity, string description)
        {
            Id = id;
            DisplayName = displayName;
            Rarity = rarity;
            Description = description;
            AssetPath = $"/Game/Blueprint/Pickup/Relic/{rarity}/DA_Relic_{id}.DA_Relic_{id}";
        }
    }
}
