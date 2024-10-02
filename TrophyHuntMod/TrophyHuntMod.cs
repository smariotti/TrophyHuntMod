using BepInEx;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using static Terminal;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Threading.Tasks;
using static CharacterDrop;
using UnityEngine.EventSystems;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static TrophyHuntMod.TrophyHuntMod;
using static UnityEngine.EventSystems.EventTrigger;
using System.Configuration;
using System.Reflection;
using System.Xml;
using Valheim.UI;
using UnityEngine.SocialPlatforms.Impl;
using System.CodeDom;
using UnityEngine.UIElements;
using static UnityEngine.UI.GridLayoutGroup;

namespace TrophyHuntMod
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    internal class TrophyHuntMod : BaseUnityPlugin
    {
        public const string PluginGUID = "com.oathorse.TrophyHuntMod";
        public const string PluginName = "TrophyHuntMod";
        public const string PluginVersion = "0.5.8";
        private readonly Harmony harmony = new Harmony(PluginGUID);

        // Configuration variables
        private const Boolean DUMP_TROPHY_DATA = false;
        private const Boolean UPDATE_LEADERBOARD = true;
        private const Boolean COLLECT_PLAYER_PATH = true;
        private const Boolean COLLECT_DROP_RATES = true;

        static TrophyHuntMod __m_trophyHuntMod;

        public enum Biome
        {
            Meadows = 0,
            Forest = 1,
            Swamp = 2,
            Mountains = 3,
            Plains = 4,
            Mistlands = 5,
            Ashlands = 6,
            Ocean = 7,
            Hildir = 8
        };

        public struct TrophyHuntData
        {
            public TrophyHuntData(string name, string prettyName, Biome biome, int value, float dropPercent, List<string> enemies)
            {
                m_name = name;
                m_prettyName = prettyName;
                m_biome = biome;
                m_value = value;
                m_dropPercent = dropPercent;
                m_enemies = enemies;
            }

            public string m_name;
            public string m_prettyName;
            public Biome m_biome;
            public int m_value;
            public float m_dropPercent;
            public List<string> m_enemies;
        }

        const int TROPHY_HUNT_DEATH_PENALTY = -20;
        const int TROPHY_HUNT_LOGOUT_PENALTY = -10;
        
        const int TROPHY_RUSH_DEATH_PENALTY = -10;
        const int TROPHY_RUSH_SLASHDIE_PENALTY = -10;
        const int TROPHY_RUSH_LOGOUT_PENALTY = -5;

        const int TROPHY_SAGA_DEATH_PENALTY = -30;
        const int TROPHY_SAGA_LOGOUT_PENALTY = -15;
        const float TROPHY_SAGA_SAILING_SPEED_MULTIPLIER = 2.0f;
        const float TROPHY_SAGA_DROP_MULTIPLIER = 1.5f;

        const string LEADERBOARD_URL = "https://valheim.help/api/trackhunt";

        const float LOGOUT_PENALTY_GRACE_DISTANCE = 50.0f;  // total distance you're allowed to walk/run from initial spawn and get a free logout to clear wet debuff

        const float DEFAULT_SCORE_FONT_SIZE = 30;

        //
        // Trophy Scores updated from Discord chat 08/18/24
        // Archy:
        //  *eik/elder/bonemass/moder/yag     -   40/60/80/100/120 pts 
        //  *hildir bosses trophies respectively -   25/45/65 pts
        //

        //            new TrophyHuntData("TrophyDraugrFem", Biome.Swamp, 20, new List<string> { "" }),
        //            new TrophyHuntData("TrophyForestTroll", Biome.Forest, 20, new List<string> { "" }),

        // Drop Percentages are from the Valheim Fandom Wiki: https://valheim.fandom.com/wiki/Trophies
        //

        static public TrophyHuntData[] __m_trophyHuntData = new TrophyHuntData[]
        {//                     Trophy Name                     Pretty Name         Biome               Score   Drop%   Dropping Enemy Name(s)
            new TrophyHuntData("TrophyAbomination",             "Abomination",      Biome.Swamp,        20,     50,     new List<string> { "$enemy_abomination" }),
            new TrophyHuntData("TrophyAsksvin",                 "Asksvin",          Biome.Ashlands,     50,     50,     new List<string> { "$enemy_asksvin" }),
            new TrophyHuntData("TrophyBlob",                    "Blob",             Biome.Swamp,        20,     10,     new List<string> { "$enemy_blob",       "$enemy_blobelite" }),
            new TrophyHuntData("TrophyBoar",                    "Boar",             Biome.Meadows,      10,     15,     new List<string> { "$enemy_boar" }),
            new TrophyHuntData("TrophyBonemass",                "Bonemass",         Biome.Swamp,        80,     100,    new List<string> { "$enemy_bonemass" }),
            new TrophyHuntData("TrophyBonemawSerpent",          "Bonemaw",          Biome.Ashlands,     50,     33,     new List<string> { "$enemy_bonemawserpent" }),
            new TrophyHuntData("TrophyCharredArcher",           "Charred Archer",   Biome.Ashlands,     50,     5,      new List<string> { "$enemy_charred_archer" }),
            new TrophyHuntData("TrophyCharredMage",             "Charred Warlock",  Biome.Ashlands,     50,     5,      new List<string> { "$enemy_charred_mage" }),
            new TrophyHuntData("TrophyCharredMelee",            "Charred Warrior",  Biome.Ashlands,     50,     5,      new List<string> { "$enemy_charred_melee" }),
            new TrophyHuntData("TrophyCultist",                 "Cultist",          Biome.Mountains,    30,     10,     new List<string> { "$enemy_fenringcultist" }),
            new TrophyHuntData("TrophyCultist_Hildir",          "Geirrhafa",        Biome.Hildir,    45,     100,    new List<string> { "$enemy_fenringcultist_hildir" }),
            new TrophyHuntData("TrophyDeathsquito",             "Deathsquito",      Biome.Plains,       30,     5,      new List<string> { "$enemy_deathsquito" }),
            new TrophyHuntData("TrophyDeer",                    "Deer",             Biome.Meadows,      10,     50,     new List<string> { "$enemy_deer" }),
            new TrophyHuntData("TrophyDragonQueen",             "Moder",            Biome.Mountains,    100,    100,    new List<string> { "$enemy_dragon" }),
            new TrophyHuntData("TrophyDraugr",                  "Draugr",           Biome.Swamp,        20,     10,     new List<string> { "$enemy_draugr" }),
            new TrophyHuntData("TrophyDraugrElite",             "Draugr Elite",     Biome.Swamp,        20,     10,     new List<string> { "$enemy_draugrelite" }),
            new TrophyHuntData("TrophyDvergr",                  "Dvergr",           Biome.Mistlands,    40,     5,      new List<string> { "$enemy_dvergr",     "$enemy_dvergr_mage" }),
            new TrophyHuntData("TrophyEikthyr",                 "Eikthyr",          Biome.Meadows,      40,     100,    new List<string> { "$enemy_eikthyr" }),
            new TrophyHuntData("TrophyFader",                   "Fader",            Biome.Ashlands,     1000,   100,    new List<string> { "$enemy_fader" }),
            new TrophyHuntData("TrophyFallenValkyrie",          "Fallen Valkyrie",  Biome.Ashlands,     50,     5,      new List<string> { "$enemy_fallenvalkyrie" }),
            new TrophyHuntData("TrophyFenring",                 "Fenring",          Biome.Mountains,    30,     10,     new List<string> { "$enemy_fenring" }),
            new TrophyHuntData("TrophyFrostTroll",              "Troll",            Biome.Forest,       20,     50,     new List<string> { "$enemy_troll" }),
            new TrophyHuntData("TrophyGjall",                   "Gjall",            Biome.Mistlands,    40,     30,     new List<string> { "$enemy_gjall" }),
            new TrophyHuntData("TrophyGoblin",                  "Fuling",           Biome.Plains,       30,     10,     new List<string> { "$enemy_goblin" }),
            new TrophyHuntData("TrophyGoblinBrute",             "Fuling Berserker", Biome.Plains,       30,     5,      new List<string> { "$enemy_goblinbrute" }),
            new TrophyHuntData("TrophyGoblinBruteBrosBrute",    "Thungr",           Biome.Hildir,       65,     100,    new List<string> { "$enemy_goblinbrute_hildircombined" }),
            new TrophyHuntData("TrophyGoblinBruteBrosShaman",   "Zil",              Biome.Hildir,       65,     100,    new List<string> { "$enemy_goblin_hildir" }),
            new TrophyHuntData("TrophyGoblinKing",              "Yagluth",          Biome.Plains,       120,    100,    new List<string> { "$enemy_goblinking" }),
            new TrophyHuntData("TrophyGoblinShaman",            "Fuling Shaman",    Biome.Plains,       30,     10,     new List<string> { "$enemy_goblinshaman" }),
            new TrophyHuntData("TrophyGreydwarf",               "Greydwarf",        Biome.Forest,       20,     5,      new List<string> { "$enemy_greydwarf" }),
            new TrophyHuntData("TrophyGreydwarfBrute",          "Greydwarf Brute",  Biome.Forest,       20,     10,     new List<string> { "$enemy_greydwarfbrute" }),
            new TrophyHuntData("TrophyGreydwarfShaman",         "Greydwarf Shaman", Biome.Forest,       20,     10,     new List<string> { "$enemy_greydwarfshaman" }),
            new TrophyHuntData("TrophyGrowth",                  "Growth",           Biome.Plains,       30,     10,     new List<string> { "$enemy_blobtar" }),
            new TrophyHuntData("TrophyHare",                    "Misthare",         Biome.Mistlands,    40,     5,      new List<string> { "$enemy_hare" }),
            new TrophyHuntData("TrophyHatchling",               "Drake",            Biome.Mountains,    30,     10,     new List<string> { "$enemy_thehive",    "$enemy_drake" }),
            new TrophyHuntData("TrophyLeech",                   "Leech",            Biome.Swamp,        20,     10,     new List<string> { "$enemy_leech" }),
            new TrophyHuntData("TrophyLox",                     "Lox",              Biome.Plains,       30,     10,     new List<string> { "$enemy_lox" }),
            new TrophyHuntData("TrophyMorgen",                  "Morgen",           Biome.Ashlands,     50,     5,      new List<string> { "$enemy_morgen" }),
            new TrophyHuntData("TrophyNeck",                    "Neck",             Biome.Meadows,      10,     5,      new List<string> { "$enemy_neck" }),
            new TrophyHuntData("TrophySeeker",                  "Seeker",           Biome.Mistlands,    40,     10,     new List<string> { "$enemy_seeker" }),
            new TrophyHuntData("TrophySeekerBrute",             "Seeker Soldier",   Biome.Mistlands,    40,     5,      new List<string> { "$enemy_seekerbrute" }),
            new TrophyHuntData("TrophySeekerQueen",             "The Queen",        Biome.Mistlands,    1000,   100,    new List<string> { "$enemy_seekerqueen" }),
            new TrophyHuntData("TrophySerpent",                 "Serpent",          Biome.Ocean,        25,     33,     new List<string> { "$enemy_serpent" }),
            new TrophyHuntData("TrophySGolem",                  "Stone Golem",      Biome.Mountains,    30,     5,      new List<string> { "$enemy_stonegolem" }),
            new TrophyHuntData("TrophySkeleton",                "Skeleton",         Biome.Forest,       20,     10,     new List<string> { "$enemy_skeleton" }),
            new TrophyHuntData("TrophySkeletonHildir",          "Brenna",           Biome.Hildir,       25,     100,    new List<string> { "$enemy_skeletonfire" }),
            new TrophyHuntData("TrophySkeletonPoison",          "Rancid Remains",   Biome.Forest,       20,     10,     new List<string> { "$enemy_skeletonpoison" }),
            new TrophyHuntData("TrophySurtling",                "Surtling",         Biome.Swamp,        20,     5,      new List<string> { "$enemy_surtling" }),
            new TrophyHuntData("TrophyTheElder",                "The Elder",        Biome.Forest,       60,     100,    new List<string> { "$enemy_gdking" }),
            new TrophyHuntData("TrophyTick",                    "Tick",             Biome.Mistlands,    40,     5,      new List<string> { "$enemy_tick" }),
            new TrophyHuntData("TrophyUlv",                     "Ulv",              Biome.Mountains,    30,     5,      new List<string> { "$enemy_ulv" }),
            new TrophyHuntData("TrophyVolture",                 "Volture",          Biome.Ashlands,     50,     50,     new List<string> { "$enemy_volture" }),
            new TrophyHuntData("TrophyWolf",                    "Wolf",             Biome.Mountains,    30,     10,     new List<string> { "$enemy_wolf" }),
            new TrophyHuntData("TrophyWraith",                  "Wraith",           Biome.Swamp,        20,     5,      new List<string> { "$enemy_wraith" })
        };

        static public Color[] __m_biomeColors = new Color[]
        {
            new Color(0.2f, 0.2f, 0.1f, 0.3f),  // Biome.Meadows
            new Color(0.0f, 0.2f, 0.0f, 0.3f),  // Biome.Forest   
            new Color(0.2f, 0.1f, 0.0f, 0.3f),  // Biome.Swamp
            new Color(0.2f, 0.2f, 0.2f, 0.3f),  // Biome.Mountains
            new Color(0.2f, 0.2f, 0.0f, 0.3f),  // Biome.Plains 
            new Color(0.2f, 0.1f, 0.2f, 0.3f),  // Biome.Mistlands
            new Color(0.2f, 0.0f, 0.0f, 0.3f),  // Biome.Ashlands 
            new Color(0.1f, 0.1f, 0.2f, 0.3f),  // Biome.Ocean    
            new Color(0.2f, 0.1f, 0.0f, 0.3f),  // Biome.Hildir
        };

        public struct BiomeBonus
        {
            public BiomeBonus(Biome biome, string biomeName, int bonus, List<string> trophies)
            {
                m_biome = biome;
                m_biomeName = biomeName;
                m_bonus = bonus;
                m_trophies = trophies;
            }

            public Biome m_biome;
            public string m_biomeName;
            public int m_bonus;
            public List<string> m_trophies;
        }

        static public BiomeBonus[] __m_biomeBonuses = new BiomeBonus[]
        {
            new BiomeBonus(Biome.Meadows,   "Meadows",        20,      new List<string> { "TrophyBoar", "TrophyDeer", "TrophyNeck" }),
            new BiomeBonus(Biome.Forest,    "Black Forest",   40,      new List<string> { "TrophyFrostTroll", "TrophyGreydwarf", "TrophyGreydwarfBrute", "TrophyGreydwarfShaman", "TrophySkeleton", "TrophySkeletonPoison" }),
            new BiomeBonus(Biome.Swamp,     "Swamp",          40,      new List<string> { "TrophyAbomination", "TrophyBlob", "TrophyDraugr", "TrophyDraugrElite", "TrophyLeech", "TrophySurtling", "TrophyWraith" }),
            new BiomeBonus(Biome.Mountains, "Mountains",      60,      new List<string> { "TrophyCultist", "TrophyFenring", "TrophyHatchling", "TrophySGolem", "TrophyUlv", "TrophyWolf" }),
            new BiomeBonus(Biome.Plains,    "Plains",         60,      new List<string> { "TrophyDeathsquito", "TrophyGoblin", "TrophyGoblinBrute", "TrophyGoblinShaman", "TrophyGrowth", "TrophyLox" }),
            new BiomeBonus(Biome.Mistlands, "Mistlands",      80,      new List<string> { "TrophyDvergr", "TrophyGjall", "TrophyHare", "TrophySeeker", "TrophySeekerBrute", "TrophyTick" }),
            new BiomeBonus(Biome.Ashlands,  "Ashlands",       100,     new List<string> { "TrophyAsksvin", "TrophyBonemawSerpent", "TrophyCharredArcher", "TrophyCharredMage", "TrophyCharredMelee", "TrophyFallenValkyrie", "TrophyMorgen", "TrophyVolture" }),
        };

        // UI Elements
        static GameObject __m_scoreTextElement = null;
        static GameObject __m_deathsTextElement = null;
        static GameObject __m_relogsTextElement = null;
        static List<GameObject> __m_iconList = null;
        static List<GameObject> __m_biomeIconList = null;

        static float __m_baseTrophyScale = 1.4f;
        static float __m_userTrophyScale = 1.0f;
        static float __m_userScoreScale = 1.0f;
        static float __m_userTrophySpacing = 0.0f;

        // TrophyHuntData list
        //        static List<string> __m_trophiesInObjectDB = new List<string>();

        // Cache for detecting newly arrived trophies and flashing the new ones
        static List<string> __m_trophyCache = new List<string>();

        // Death counter
        static int __m_deaths = 0;
        static int __m_slashDieCount = 0;
        static int __m_logoutCount = 0;

        // Player Path
        static bool __m_pathAddedToMinimap = false;                                // are we showing the path on the minimap? 
        static List<Minimap.PinData> __m_pathPins = new List<Minimap.PinData>();   // keep track of the special pins we add to the minimap so we can remove them
        static List<Vector3> __m_playerPathData = new List<Vector3>();   // list of player positions during the session
        static bool __m_collectingPlayerPath = false;                           // are we actively asynchronously collecting the player position?
        static float __m_playerPathCollectionInterval = 8.0f;                   // seconds between checks to see if we can store the current player position
        static float __m_minPathPlayerMoveDistance = 50.0f;                     // the min distance the player has to have moved to consider storing the new path position
        static Vector3 __m_previousPlayerPos;                                   // last player position stored

        // Only mod running flag
        static bool __m_onlyModRunning = false;

        // Trophy rush flag
        public enum TrophyGameMode
        {
            TrophyHunt,
            TrophyRush,
            TrophySaga,
            TrophyFiesta,
            
            Max
        }

//        static bool __m_trophyRushEnabled = false;

        // TrophyHuntMod current Game Mode
        static TrophyGameMode __m_trophyGameMode = TrophyGameMode.TrophyHunt;
        static public TrophyGameMode GetGameMode() { return __m_trophyGameMode; }

        static bool __m_fiestaFlashing = false;
        static Color[] __m_fiestaColors = new Color[]
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.cyan,
            Color.magenta,
            Color.yellow,
        };

        // Track all enemy deaths and trophies flag
        static bool __m_showAllTrophyStats = false;
        static bool __m_invalidForTournamentPlay = false;
        static bool __m_ignoreLogouts = false;

        // For tracking the unique ID for this user/player combo (unique to a given player character)
        static long __m_storedPlayerID = 0;
        static TrophyGameMode __m_storedGameMode = TrophyGameMode.Max;
        static string __m_storedWorldSeed = "";

        public struct DropInfo
        {
            public DropInfo()
            {
                m_numKilled = 0;
                m_trophies = 0;
            }

            public int m_numKilled = 0;
            public int m_trophies = 0;
        }

        // ALL the killed enemies and trophy drops that happen in the game
        static Dictionary<string, DropInfo> __m_allTrophyDropInfo = new Dictionary<string, DropInfo>();

        // Just the killed enemies and trophies dropped and picked up by the player
        static Dictionary<string, DropInfo> __m_playerTrophyDropInfo = new Dictionary<string, DropInfo>();

        // Biomes we've completed 
        static List<Biome> __m_completedBiomeBonuses = new List<Biome>();

        private void Awake()
        {
            // Get the list of loaded plugins
            var loadedPlugins = BepInEx.Bootstrap.Chainloader.PluginInfos;

            // Check if the count of loaded plugins is 1 and if it's this mod
            if (loadedPlugins.Count == 1 && loadedPlugins.ContainsKey(Info.Metadata.GUID))
            {
                Debug.LogWarning($"[TrophyHuntMod] v{PluginVersion} is loaded and is the ONLY mod running! Let's Hunt!");

                __m_onlyModRunning = true;
            }
            else
            {
                Debug.LogWarning($"[TrophyHuntMod] v{PluginVersion} detected other mods running. For official events, it must be the ONLY mod running.");

                __m_onlyModRunning = false;
            }

            __m_trophyHuntMod = this;

            // Patch with Harmony
            harmony.PatchAll();

            AddConsoleCommands();

            // Create the drop data for collecting info about trophy drops vs. kills
            //
            if (COLLECT_DROP_RATES)
            {
                InitTrophyDropInfo();
            }
        }

        public static void InitTrophyDropInfo()
        {
            __m_allTrophyDropInfo.Clear();
            __m_playerTrophyDropInfo.Clear();
            foreach (TrophyHuntData td in __m_trophyHuntData)
            {
                __m_allTrophyDropInfo.Add(td.m_name, new DropInfo());
                __m_playerTrophyDropInfo.Add(td.m_name, new DropInfo());
            }
            __m_completedBiomeBonuses.Clear();
        }

        // New Console Commands for TrophyHuntMod
        #region Console Commands
        public static void PrintToConsole(string message)
        {
            if (Console.m_instance) Console.m_instance.AddString(message);
            if (Chat.m_instance) Chat.m_instance.AddString(message);
            Debug.Log(message);
        }

        void AddConsoleCommands()
        {
            ConsoleCommand trophyHuntCommand = new ConsoleCommand("trophyhunt", "Prints trophy hunt data", delegate (ConsoleEventArgs args)
            {
                if (!Game.instance)
                {
                    PrintToConsole("'trophyhunt' console command can only be used in-game.");
                    return true;
                }

                PrintToConsole($"[Trophy Hunt Scoring]");

                PrintToConsole($"Trophies:");
                int score = Player_OnSpawned_Patch.CalculateTrophyPoints(true);
                PrintToConsole($"Trophy Score Total: {score}");
                int deathScore = Player_OnSpawned_Patch.CalculateDeathPenalty();
                int logoutScore = Player_OnSpawned_Patch.CalculateLogoutPenalty();
                PrintToConsole($"Penalties:");
                PrintToConsole($"  Deaths: {__m_deaths} Score: {deathScore}");
                PrintToConsole($"  Logouts: {__m_logoutCount} Score: {logoutScore}");

                int biomeBonus = 0;
                if (GetGameMode() == TrophyGameMode.TrophyRush)
                {
                    Player_OnSpawned_Patch.CalculateBiomeBonusScore(Player.m_localPlayer);
                    PrintToConsole($"Biome Bonus Total: {biomeBonus}");
                }
                score += deathScore;
                score += logoutScore;
                score += biomeBonus;
                PrintToConsole($"Total Score: {score}");

                return true;
            });

            ConsoleCommand showPathCommand = new ConsoleCommand("showpath", "Show the path the player took", delegate (ConsoleEventArgs args)
            {
                if (!Game.instance)
                {
                    PrintToConsole("'showpath' console command can only be used in-game.");
                }

                ShowPlayerPath(!__m_pathAddedToMinimap);
            });

/*
            ConsoleCommand dumpEnemies = new ConsoleCommand("dumpenemies", "Dump the names of enemies that can drop trophies and what trophies they drop", delegate (ConsoleEventArgs args)
            {
                Debug.Log("Dumping enemy names:");

                // Get the ZNetScene instance
                ZNetScene zNetScene = ZNetScene.instance;

                // Loop through all prefabs in the ZNetScene
                foreach (GameObject prefab in zNetScene.m_prefabs)
                {
                    if (prefab == null) continue;

                    // Check if the prefab has a Character component
                    Character character = prefab.GetComponent<Character>();
                    if (character != null)
                    {
                        CharacterDrop charDrop = character.GetComponent<CharacterDrop>();
                        if (charDrop != null)
                        {
                            foreach (Drop drop in charDrop.m_drops)
                            {
                                if (drop == null) continue;
                                if (drop.m_prefab == null) continue;
                                string dropName = drop.m_prefab.name;

                                if (dropName.Contains("Trophy"))
                                {
                                    Debug.Log($"{dropName},{character.m_name}");
                                }
                            }

                        }
                    }
                }
            });
*/
            //ConsoleCommand dumpDropRates = new ConsoleCommand("dumpdroprates", "Dump the drop counts and rates to a logfile", delegate (ConsoleEventArgs args)
            //{
            //    PrintToConsole($"{"TrophyName",-18} {"Killed",-7} {"Dropped",-8} {"Rate",-6}%");
            //    foreach (KeyValuePair<string, DropInfo> entry in __m_trophyDropInfo)
            //    {
            //        TrophyHuntData trophyHuntData = Array.Find(__m_trophyHuntData, element => element.m_name == entry.Key);
            //        string enemyName = trophyHuntData.m_prettyName;
            //        string percentStr = "n/a";
            //        if (entry.Value.m_numKilled > 0)
            //            percentStr = (100.0f * (float)entry.Value.m_trophiesDropped / (float)entry.Value.m_numKilled).ToString("0.0");
            //        PrintToConsole($"{enemyName,-18} {entry.Value.m_numKilled,-7} {entry.Value.m_trophiesDropped,-8} {percentStr,-6}%");
            //    }
            //});

            ConsoleCommand trophyRushCommand = new ConsoleCommand("trophyrush", "Toggle Trophy Rush Mode on and off", delegate (ConsoleEventArgs args)
            {
                if (Game.instance)
                {
                    PrintToConsole("'/trophyrush' console command can only be used at the main menu via the F5 console.");
                    return;
                }

                ToggleTrophyRush();
            });

            ConsoleCommand ignoreLogoutsCommand = new ConsoleCommand("ignorelogouts", "Don't subtract points for logouts", delegate (ConsoleEventArgs args)
            {
                if (!Game.instance)
                {
                    PrintToConsole("'/ignorelogouts' can only be used in gameplay.");
                    return;
                }

                __m_ignoreLogouts = !__m_ignoreLogouts;

                __m_invalidForTournamentPlay = true;

                if (__m_scoreTextElement != null)
                {
                    if (__m_ignoreLogouts)
                    {
                        TMPro.TextMeshProUGUI tmText = __m_scoreTextElement.GetComponent<TMPro.TextMeshProUGUI>();

                        tmText.color = Color.green;
                    }
                }

                if (__m_relogsTextElement != null)
                {
                    if (__m_ignoreLogouts)
                    {
                        TMPro.TextMeshProUGUI tmText = __m_relogsTextElement.GetComponent<TMPro.TextMeshProUGUI>();

                        tmText.color = Color.gray;
                    }
                }
            });


            ConsoleCommand showAllTrophyStats = new ConsoleCommand("showalltrophystats", "Toggle tracking ALL enemy deaths and trophies with JUST tracking player kills and trophies", delegate (ConsoleEventArgs args)
            {
                if (!Game.instance)
                {
                    PrintToConsole("'/showalltrophystats' can only be used in gameplay.");
                    return;
                }

                ToggleShowAllTrophyStats();

                __m_invalidForTournamentPlay = true;

                if (__m_scoreTextElement != null)
                {
                    if (__m_showAllTrophyStats)
                    {
                        TMPro.TextMeshProUGUI tmText = __m_scoreTextElement.GetComponent<TMPro.TextMeshProUGUI>();

                        tmText.color = Color.green;
                    }
                }
            });


            ConsoleCommand scoreScaleCommand = new ConsoleCommand("scorescale", "Scale the score text sizes (1.0 is default)", delegate (ConsoleEventArgs args)
            {
                if (!Game.instance)
                {
                    PrintToConsole("'scorescale' console command can only be used in-game.");
                }

                // First argument is user trophy scale
                if (args.Length > 1)
                {
                    float userScale = float.Parse(args[1]);
                    if (userScale == 0) userScale = 1;
                    __m_userScoreScale = userScale;

                }
                else
                {
                    // no arguments means reset
                    __m_userScoreScale = 1.0f;
                }

                // Readjust the UI elements' trophy sizes
                Player player = Player.m_localPlayer;
                if (player != null)
                {
                    TextMeshProUGUI textElement = __m_scoreTextElement.GetComponent<TextMeshProUGUI>();
                    if (textElement != null)
                    {
                        textElement.fontSize = DEFAULT_SCORE_FONT_SIZE * __m_userScoreScale;
                    }
                }
            });

            ConsoleCommand trophyScaleCommand = new ConsoleCommand("trophyscale", "Scale the trophy sizes (1.0 is default)", delegate (ConsoleEventArgs args)
            {
                if (!Game.instance)
                {
                    PrintToConsole("'trophyscale' console command can only be used in-game.");
                }

                // First argument is user trophy scale
                if (args.Length > 1)
                {
                    float userScale = float.Parse(args[1]);
                    if (userScale == 0) userScale = 1;
                    __m_userTrophyScale = userScale;

                    // second argument is base trophy scale (for debugging)
                    if (args.Length > 2)
                    {
                        float baseScale = float.Parse(args[2]);
                        if (baseScale == 0) baseScale = 1;
                        __m_baseTrophyScale = baseScale;
                    }
                }
                else
                {
                    // no arguments means reset
                    __m_userTrophyScale = 1.0f;
                    __m_baseTrophyScale = 1.0f;
                }

                // Readjust the UI elements' trophy sizes
                Player player = Player.m_localPlayer;
                if (player != null)
                {
                    List<string> discoveredTrophies = player.GetTrophies();
                    foreach (TrophyHuntData td in __m_trophyHuntData)
                    {
                        string trophyName = td.m_name;

                        GameObject iconGameObject = __m_iconList.Find(gameObject => gameObject.name == trophyName);

                        if (iconGameObject != null)
                        {
                            UnityEngine.UI.Image image = iconGameObject.GetComponent<UnityEngine.UI.Image>();
                            if (image != null)
                            {
                                RectTransform imageRect = iconGameObject.GetComponent<RectTransform>();

                                if (imageRect != null)
                                {
                                    imageRect.localScale = new Vector3(__m_baseTrophyScale, __m_baseTrophyScale, __m_baseTrophyScale) * __m_userTrophyScale;
                                }
                            }
                        }
                    }
                }
            });

            ConsoleCommand trophySpacingCommand = new ConsoleCommand("trophyspacing", "Space the trophies out (negative and positive numbers work)", delegate (ConsoleEventArgs args)
            {
                if (!Game.instance)
                {
                    PrintToConsole("'trophyspacing' console command can only be used in-game.");
                    return;
                }

                if (Player.m_localPlayer == null)
                {
                    return;
                }

                Player player = Player.m_localPlayer;

                // First argument is user trophy scale
                if (args.Length > 1)
                {
                    float userSpacing = float.Parse(args[1]);
                    if (userSpacing == 0) userSpacing = 1;
                    __m_userTrophySpacing = userSpacing;
                }
                else
                {
                    // no arguments means reset
                    __m_userTrophySpacing = 0.0f;
                }

                Transform healthPanelTransform = Hud.instance.transform.Find("hudroot/healthpanel");
                if (healthPanelTransform == null)
                {
                    Debug.LogError("Health panel transform not found.");

                    return;
                }

                Player_OnSpawned_Patch.DeleteTrophyIconElements(__m_iconList);
                Player_OnSpawned_Patch.CreateTrophyIconElements(healthPanelTransform, __m_trophyHuntData, __m_iconList);
                Player_OnSpawned_Patch.EnableTrophyHuntIcons(player);
            });

            ConsoleCommand showTrophies = new ConsoleCommand("showtrophies", "Toggle Trophy Rush Mode on and off", delegate (ConsoleEventArgs args)
            {
                if (!Game.instance)
                {
                    PrintToConsole("'/showtrophies' console command can only be used during gameplay.");
                    return;
                }

                __m_showingTrophies = !__m_showingTrophies;

                ShowTrophies(__m_showingTrophies);
            });
        }

        public static bool __m_showingTrophies = true;

        public static void ShowTrophies(bool show)
        {
            foreach (GameObject trophyIcon in __m_iconList)
            {
                trophyIcon.SetActive(show);
            }
        }

        public static void ToggleTrophyRush()
        {
            __m_trophyGameMode += 1;
//            if (__m_trophyGameMode >= TrophyGameMode.Max)
            if (__m_trophyGameMode >= TrophyGameMode.TrophyFiesta) // Temporarily disable selecting Fiesta
            {
                __m_trophyGameMode = TrophyGameMode.TrophyHunt;
            }

            //__m_trophyRushEnabled = !__m_trophyRushEnabled;

            if (__m_trophyHuntMainMenuText != null)
            {
                __m_trophyHuntMainMenuText.text = GetTrophyHuntMainMenuText();
            }
        }

        public static void ToggleShowAllTrophyStats()
        {
            __m_showAllTrophyStats = !__m_showAllTrophyStats;

            if (__m_showAllTrophyStats)
            {
                PrintToConsole($"Displaying ALL enemy deaths for kills and trophies!");
                PrintToConsole($"WARNING: Not legal for Tournament Play!");
            }
            else
            {
                PrintToConsole($"Displaying ONLY Player enemy kills and picked up trophies!");
            }

            // If the game's running, fix the tooltip UI
            if (Game.instance)
            {
                Player_OnSpawned_Patch.DeleteTrophyTooltip();
                Player_OnSpawned_Patch.CreateTrophyTooltip();
            }

            if (__m_trophyHuntMainMenuText != null)
            {
                __m_trophyHuntMainMenuText.text = GetTrophyHuntMainMenuText();
            }
        }

        public static TextMeshProUGUI __m_trophyHuntMainMenuText = null;

        public static string GetGameModeText()
        {
            string text = "";

            float resourceMultiplier = 1f;
            string combatDifficulty = "Normal";
            string dropRate = "Normal";
            bool hasBiomeBonuses = false;
            bool hasAdditionalSlashDiePenalty = false;

            switch (GetGameMode())
            {
                case TrophyGameMode.TrophyHunt:
                    // Trophy Hunt game mode
                    text += "\n<align=\"left\"><size=18>Game Mode: <color=yellow>Trophy Hunt</color></size>\n";
                    break;
                case TrophyGameMode.TrophyRush:
                    // Trophy Rush game mode
                    text += "\n<align=\"left\"><size=18>Game Mode: <color=orange>Trophy Rush</color></size>";
                    text += "\n<align=\"center\"><size=12> <color=yellow>NOTE:</color> To use existing world, change World Modifiers manually!</size>\n";
                    resourceMultiplier = 2f;
                    combatDifficulty = "Very Hard";
                    dropRate = "100%";
                    hasBiomeBonuses = true;
                    hasAdditionalSlashDiePenalty = true;
                    break;
                case TrophyGameMode.TrophySaga:
                    text += "\n<align=\"left\"><size=18>Game Mode: <color=yellow>Trophy Saga</color></size>";
                    text += "\n<align=\"center\"><size=12> <color=yellow>NOTE:</color> To use existing world, change World Modifiers manually!</size>\n";
                    text += "<align=\"center\"><size=14><color=red>EXPERIMENTAL!</color>\n</size>";
                    resourceMultiplier = 1.5f;
                    combatDifficulty = "Hard";
                    dropRate = "Increased";
                    hasBiomeBonuses = false;
                    break;
                case TrophyGameMode.TrophyFiesta:
                    text += "\n<align=\"left\"><size=18>Game Mode: <color=yellow>Trophy</color> <color=green>F</color><color=purple>i</color><color=red>e</color><color=yellow>s</color><color=orange>t</color><color=blue>a</color></size>\n";
                    text += "\n<align=\"left\"><size=14><color=yellow>    Nothing to see here.</color></size>\n";
                    break;
            }

            if (GetGameMode() != TrophyGameMode.TrophyFiesta)
            {
                text += "<align=\"left\"><size=14>    Rules:\n";
                text += $"<align=\"left\">      * Resources: <color=orange>{resourceMultiplier.ToString("0.0")}x</color>\n";
                text += $"<align=\"left\">      * Combat Difficulty: <color=orange>{combatDifficulty}</color>\n";
                text += $"<align=\"left\">      * Trophy Drop Rate: <color=orange>{dropRate}</color>\n";
                if (GetGameMode() == TrophyGameMode.TrophySaga)
                {
                    text += $"<align=\"left\">      * Boat Speed is Doubled.\n";
                    text += $"<align=\"left\">      * Ores insta-smelt on pickup\n";
                }
                if (hasBiomeBonuses)
                {
                    text += $"<align=\"left\">      * <color=orange>Biome Bonuses</color> for trophy sets.\n";
                }
                text += $"<align=\"left\">      * Logout Penalty: <color=red>{Player_OnSpawned_Patch.GetLogoutPointCost()}</color>\n";
                text += $"<align=\"left\">      * Death Penalty: <color=red>{Player_OnSpawned_Patch.GetDeathPointCost()}</color>\n";
                if (hasAdditionalSlashDiePenalty)
                {
                    text += $"<align=\"left\">      * '/die' Penalty: <color=red>{Player_OnSpawned_Patch.GetDeathPointCost() + Player_OnSpawned_Patch.GetSlashDiePointCost()}</color>\n";
                }
                text += "</size>";
            }

            return text;
        }

        public static string GetTrophyHuntMainMenuText()
        {
            string textStr = $"<b><size=34><color=#FFB75B>TrophyHuntMod</color></size></b>\n<size=18>           (Version: {PluginVersion})</size>";

            textStr += GetGameModeText();

            //if (__m_showAllTrophyStats)
            //{
            //    textStr += ("\n<size=18><color=orange>Tracking ALL enemy deaths and trophies!</color>" +
            //                "\n<color=red>NOT LEGAL FOR TOURNAMENT PLAY!</color></size>");
            //}

            return textStr;
        }

        public static void ShowPlayerPath(bool showPlayerPath)
        {
            if (!showPlayerPath)
            {
                foreach (Minimap.PinData pinData in __m_pathPins)
                {
                    Minimap.instance.RemovePin(pinData);
                }

                __m_pathPins.Clear();

                __m_pathAddedToMinimap = false;
            }
            else
            {
                __m_pathPins.Clear();

                foreach (Vector3 pathPos in __m_playerPathData)
                {
                    Minimap.PinType pinType = Minimap.PinType.Icon3;
                    Minimap.PinData newPin = Minimap.instance.AddPin(pathPos, pinType, "", save: false, isChecked: false);

                    __m_pathPins.Add(newPin);
                }

                __m_pathAddedToMinimap = true;
            }

        }
        #endregion

        // OnSpawned() is required instead of Awake
        //   this is because at Awake() time, Player.m_trophyList and Player.m_localPlayer haven't been initialized yet
        //
        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        public class Player_OnSpawned_Patch
        {
            static void Postfix(Player __instance)
            {
                if (__instance != Player.m_localPlayer)
                {
                    return;
                }

                Debug.LogWarning("Local Player is Spawned!");

                // Sort the trophies by biome, score and name
                Array.Sort<TrophyHuntData>(__m_trophyHuntData, (x, y) => x.m_biome.CompareTo(y.m_biome) * 100000 + x.m_value.CompareTo(y.m_value) * 10000 + x.m_name.CompareTo(y.m_name));

                // Dump loaded trophy data
                if (DUMP_TROPHY_DATA)
                {
                    foreach (var t in __m_trophyHuntData)
                    {
                        Debug.LogWarning($"{t.m_biome.ToString()}, {t.m_name}, {t.m_value}");
                    }
                }

                // Cache already discovered trophies
                __m_trophyCache = Player.m_localPlayer.GetTrophies();

                if (__m_showAllTrophyStats || __m_ignoreLogouts || GetGameMode() == TrophyGameMode.TrophySaga || GetGameMode() == TrophyGameMode.TrophyFiesta)
                {
                    __m_invalidForTournamentPlay = true;
                }

                // Create all the UI elements we need for this mod
                BuildUIElements();

                // Until the player has moved 10 meters, ignore logouts. This is a hack
                // to get around switching players and accounting for logouts in case the 
                // user was playing another character before starting the trophy hunt run
                //
                if (GetTotalOnFootDistance(Game.instance) < 10.0f)
                {
                    __m_logoutCount = 0;
                }

                //                Debug.LogWarning($"Stored PlayerID: {__m_currentPlayerID}, m_localPlayer PlayerID: {Player.m_localPlayer.GetPlayerID()}");

                // If this is a different character, gamemode, or world seed, clear all in-memory stats
                if (__m_storedPlayerID != Player.m_localPlayer.GetPlayerID() ||
                    __m_storedGameMode != __m_trophyGameMode ||
                    __m_storedWorldSeed != WorldGenerator.instance.m_world.m_seedName)
                {
                    InitializeTrackedDataForNewPlayer();
                }

                Debug.LogWarning($"Total Logouts: {__m_logoutCount}");

                string workingDirectory = Directory.GetCurrentDirectory();
                Debug.Log($"Working Directory for Trophy Hunt Mod: {workingDirectory}");
                Debug.Log($"Steam username: {SteamFriends.GetPersonaName()}");

                // Do initial update of all UI elements to the current state of the game
                UpdateTrophyHuntUI(Player.m_localPlayer);


                // Start collecting player position map pin data
                if (COLLECT_PLAYER_PATH)
                {
                    ShowPlayerPath(false);
                    StopCollectingPlayerPath();
                    StartCollectingPlayerPath();
                }

                // Store the current session data to help determine the player changing these
                // things at the main menu
                __m_storedPlayerID = Player.m_localPlayer.GetPlayerID();
                __m_storedGameMode = __m_trophyGameMode;
                __m_storedWorldSeed = WorldGenerator.instance.m_world.m_seedName;
            }

            public static void InitializeTrackedDataForNewPlayer()
            {
                // Reset logout count
                __m_logoutCount = 0;

                // Reset logout ignoring for new character
                __m_ignoreLogouts = false;

                // Track how many times player has done "/die" command
                __m_slashDieCount = 0;

                // New players never start with show-all-stats
                __m_showAllTrophyStats = false;

                // Reset whether we've shown enemy deaths
                __m_invalidForTournamentPlay = false;

                // Clear the map screen pin player location data
                __m_playerPathData.Clear();

                // Clear the dropped trophies tracking data
                InitTrophyDropInfo();
            }

            public static int CalculateTrophyPoints(bool displayToLog = false)
            {
                int score = 0;
                foreach (TrophyHuntData thData in __m_trophyHuntData)
                {
                    if (__m_trophyCache.Contains(thData.m_name))
                    {
                        if (displayToLog)
                        {
                            PrintToConsole($"  {thData.m_name}: Score: {thData.m_value} Biome: {thData.m_biome.ToString()}");
                        }
                        score += thData.m_value;
                    }
                }

                return score;
            }

            public static int GetDeathPointCost()
            {
                int deathCost = TROPHY_HUNT_DEATH_PENALTY;

                if (GetGameMode() == TrophyGameMode.TrophyRush)
                    deathCost = TROPHY_RUSH_DEATH_PENALTY;
                else if (GetGameMode() == TrophyGameMode.TrophySaga)
                    deathCost = TROPHY_SAGA_DEATH_PENALTY;

                return deathCost;
            }

            public static int GetSlashDiePointCost()
            {
                int additionalCost = 0;

                if (GetGameMode() == TrophyGameMode.TrophyRush)
                    additionalCost = TROPHY_RUSH_SLASHDIE_PENALTY;

                return additionalCost;
            }

            public static int CalculateDeathPenalty()
            {
                int deathScore = __m_deaths * GetDeathPointCost();

                deathScore += __m_slashDieCount * GetSlashDiePointCost();

                return deathScore;
            }

            public static int GetLogoutPointCost()
            {
                int logoutCost = TROPHY_HUNT_LOGOUT_PENALTY;

                if (GetGameMode() == TrophyGameMode.TrophyRush)
                    logoutCost = TROPHY_RUSH_LOGOUT_PENALTY;
                else if (GetGameMode() == TrophyGameMode.TrophySaga)
                    logoutCost = TROPHY_SAGA_LOGOUT_PENALTY;

                return logoutCost;
            }

            public static int CalculateLogoutPenalty()
            {
                int logoutScore = __m_logoutCount * GetLogoutPointCost();

                return logoutScore;
            }

            static void BuildUIElements()
            {
                if (Hud.instance == null || Hud.instance.m_rootObject == null)
                {
                    Debug.LogError("TrophyHuntMod: Hud.instance.m_rootObject is NOT valid");

                    return;
                }

                if (__m_deathsTextElement == null && __m_scoreTextElement == null)
                {
                    Transform healthPanelTransform = Hud.instance.transform.Find("hudroot/healthpanel");
                    if (healthPanelTransform == null)
                    {
                        Debug.LogError("Health panel transform not found.");

                        return;
                    }

                    if (__m_scoreTextElement == null)
                    {
                        __m_scoreTextElement = CreateScoreTextElement(healthPanelTransform);
                    }

                    if (__m_deathsTextElement == null)
                    {
                        __m_deathsTextElement = CreateDeathsElement(healthPanelTransform);
                    }

                    if (__m_relogsTextElement == null)
                    {
                        __m_relogsTextElement = CreateRelogsElements(healthPanelTransform);
                    }

                    __m_iconList = new List<GameObject>();
                    CreateTrophyIconElements(healthPanelTransform, __m_trophyHuntData, __m_iconList);

//                    __m_biomeIconList = new List<GameObject>();
//                    CreateBiomeElements(healthPanelTransform, __m_biomeIconList);

                    // Create the hover text object
                    if (COLLECT_DROP_RATES)
                    {
                        CreateTrophyTooltip();
                        CreateLuckTooltip();
                    }

                    CreateLuckOMeterElements(healthPanelTransform);

                    CreateScoreTooltip();
                }
            }

            static GameObject CreateRelogsElements(Transform parentTransform)
            {
                Sprite logSprite = GetTrophySprite("RoundLog");

                GameObject logElement = new GameObject("RelogsIcon");
                logElement.transform.SetParent(parentTransform);

                RectTransform rectTransform = logElement.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(40, 40);
                rectTransform.anchoredPosition = new Vector2(-70, -105);

                UnityEngine.UI.Image image = logElement.AddComponent<UnityEngine.UI.Image>();
                image.sprite = logSprite;
                image.color = Color.white;

                // Text Element
                GameObject relogsElement = new GameObject("RelogsElement");
                relogsElement.transform.SetParent(parentTransform);

                // Add RectTransform component for positioning
                rectTransform = relogsElement.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(60, 20); // Set size
                rectTransform.anchoredPosition = new Vector2(-70, -105); // Set position

                TMPro.TextMeshProUGUI tmText = relogsElement.AddComponent<TMPro.TextMeshProUGUI>();

                tmText.text = $"{__m_logoutCount}";
                tmText.fontSize = 24;
                tmText.color = Color.yellow;
                tmText.alignment = TextAlignmentOptions.Center;
                tmText.raycastTarget = false;
                tmText.fontMaterial.EnableKeyword("OUTLINE_ON");
                tmText.outlineColor = Color.black;
                tmText.outlineWidth = 0.06f; // Adjust the thickness

                if (__m_ignoreLogouts)
                {
                    tmText.color = Color.gray;
                }

                return relogsElement;
            }

            static GameObject CreateLuckOMeterElements(Transform parentTransform)
            {
                Sprite luckSprite = GetTrophySprite("HelmetMidsummerCrown");

                GameObject luckElement = new GameObject("LuckImage");
                luckElement.transform.SetParent(parentTransform);

                RectTransform rectTransform = luckElement.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(40, 40);
                rectTransform.anchoredPosition = new Vector2(-70, -20);

                UnityEngine.UI.Image image = luckElement.AddComponent<UnityEngine.UI.Image>();
                image.sprite = luckSprite;
                image.color = Color.white;
                image.raycastTarget = true;

                //GameObject luckTextElement = new GameObject("LuckText");
                //luckTextElement.transform.SetParent(parentTransform);%
                //RectTransform luckTextTransform = luckTextElement.AddComponent<RectTransform>();
                //luckTextTransform.sizeDelta = new Vector2(50, 20);
                //luckTextTransform.anchoredPosition = new Vector3(-75, -50);
                //TMPro.TextMeshProUGUI tmText = luckTextElement.AddComponent<TMPro.TextMeshProUGUI>();
                //tmText.text = "Luck";
                //tmText.fontSize = 14;
                //tmText.color = Color.yellow;
                //tmText.alignment = TextAlignmentOptions.Center;
                //tmText.raycastTarget = false;
                //                AddTooltipTriggersToLuckObject(luckTextElement);

                AddTooltipTriggersToLuckObject(luckElement);

                return luckElement;
            }

            static GameObject CreateDeathsElement(Transform parentTransform)
            {
                // use the charred skull sprite for our Death count indicator in the UI
                Sprite skullSprite = GetTrophySprite("Charredskull");

                // Create the skullElement for deaths
                GameObject skullElement = new GameObject("DeathsIcon");
                skullElement.transform.SetParent(parentTransform);

                // Add RectTransform component for positioning
                RectTransform rectTransform = skullElement.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(50, 50);
                rectTransform.anchoredPosition = new Vector2(-70, -65); // Set position

                // Add an Image component
                UnityEngine.UI.Image image = skullElement.AddComponent<UnityEngine.UI.Image>();
                image.sprite = skullSprite;
                image.color = Color.white;
                image.raycastTarget = false;

                GameObject deathsTextElement = new GameObject("DeathsText");
                deathsTextElement.transform.SetParent(parentTransform);

                RectTransform deathsTextTransform = deathsTextElement.AddComponent<RectTransform>();
                deathsTextTransform.sizeDelta = new Vector2(40, 40);
                deathsTextTransform.anchoredPosition = rectTransform.anchoredPosition;

                TMPro.TextMeshProUGUI tmText = deathsTextElement.AddComponent<TMPro.TextMeshProUGUI>();
                tmText.text = $"{__m_deaths}";
                tmText.fontSize = 22;
                tmText.color = Color.yellow;
                tmText.alignment = TextAlignmentOptions.Center;
                tmText.raycastTarget = false;

                return deathsTextElement;
            }

            static GameObject CreateScoreTextElement(Transform parentTransform)
            {
                GameObject scoreBGElement = new GameObject("ScoreBG");
                scoreBGElement.transform.SetParent(parentTransform);

                Vector2 scorePos = new Vector2(-65, -140);
                Vector2 scoreSize = new Vector2(70, 42);

                //RectTransform bgTransform = scoreBGElement.AddComponent<RectTransform>();
                //bgTransform.sizeDelta = scoreSize;
                //bgTransform.anchoredPosition = scorePos;

                //// Add an Image component for the background
                //UnityEngine.UI.Image backgroundImage = scoreBGElement.AddComponent<UnityEngine.UI.Image>();
                //backgroundImage.color = new Color(0, 0, 0, 0.85f); // Semi-transparent black background

                // Create a new GameObject for the text
                GameObject scoreTextElement = new GameObject("ScoreText");

                // Set the parent to the HUD canvas
                scoreTextElement.transform.SetParent(parentTransform);


                // Add RectTransform component for positioning
                RectTransform rectTransform = scoreTextElement.AddComponent<RectTransform>();
                rectTransform.sizeDelta = scoreSize;
                rectTransform.anchoredPosition = scorePos;

                int scoreValue = 9999;

                TMPro.TextMeshProUGUI tmText = scoreTextElement.AddComponent<TMPro.TextMeshProUGUI>();

                tmText.text = $"{scoreValue}";
                tmText.fontSize = DEFAULT_SCORE_FONT_SIZE;
                tmText.fontStyle = FontStyles.Bold;
                tmText.color = Color.yellow;
                tmText.alignment = TextAlignmentOptions.Center;
                tmText.raycastTarget = true;
                tmText.fontMaterial.EnableKeyword("OUTLINE_ON");
                tmText.outlineColor = Color.black;
                tmText.outlineWidth = 0.07f; // Adjust the thickness
                                             //                tmText.enableAutoSizing = true;

                AddTooltipTriggersToScoreObject(scoreTextElement);

                if (!__m_onlyModRunning)
                {
                    tmText.color = Color.cyan;
                }

                if (__m_showAllTrophyStats || __m_invalidForTournamentPlay)
                {
                    tmText.color = Color.green;
                }

                return scoreTextElement;
            }
            static GameObject CreateTrophyIconElement(Transform parentTransform, Sprite iconSprite, string iconName, Biome iconBiome, int index)
            {

                int iconSize = 33;
                int iconBorderSize = -1;
                int xOffset = -20;
                int yOffset = -140;

                int biomeIndex = (int)iconBiome;
                Color backgroundColor = __m_biomeColors[biomeIndex];

                // Create a new GameObject for the icon
                GameObject iconElement = new GameObject(iconName);
                iconElement.transform.SetParent(parentTransform);

                // Add RectTransform component for positioning Sprite
                RectTransform iconRectTransform = iconElement.AddComponent<RectTransform>();
                iconRectTransform.sizeDelta = new Vector2(iconSize, iconSize); // Set size
                iconRectTransform.anchoredPosition = new Vector2(xOffset + index * (iconSize + iconBorderSize + __m_userTrophySpacing), yOffset); // Set position
                iconRectTransform.localScale = new Vector3(__m_baseTrophyScale, __m_baseTrophyScale, __m_baseTrophyScale) * __m_userTrophyScale;

                // Add an Image component for Sprite
                UnityEngine.UI.Image iconImage = iconElement.AddComponent<UnityEngine.UI.Image>();
                iconImage.sprite = iconSprite;
                iconImage.color = new Color(0.0f, 0.2f, 0.1f, 0.95f);
                iconImage.raycastTarget = true;

//                if (__m_trophyRushEnabled)
                if (GetGameMode() == TrophyGameMode.TrophyRush)
                {
                    iconImage.color = new Color(0.5f, 0.0f, 0.0f);
                }
                else if (GetGameMode() == TrophyGameMode.TrophySaga)
                {
                    iconImage.color = new Color(0f, 0f, 0.5f);
                }

                AddTooltipTriggersToTrophyIcon(iconElement);

                return iconElement;
            }

            public static void DeleteTrophyIconElements(List<GameObject> iconList)
            {
                foreach (GameObject trophyIconObject in iconList)
                {
                    GameObject.Destroy(trophyIconObject);
                }

                iconList.Clear();
            }

            public static void CreateTrophyIconElements(Transform parentTransform, TrophyHuntData[] trophies, List<GameObject> iconList)
            {
                foreach (TrophyHuntData trophy in trophies)
                {
                    Sprite trophySprite = GetTrophySprite(trophy.m_name);
                    if (trophySprite == null)
                    {
                        //ACK
                        Debug.LogError($"Unable to find trophy sprite for {trophy.m_name}");
                        continue;
                    }

                    GameObject iconElement = CreateTrophyIconElement(parentTransform, trophySprite, trophy.m_name, trophy.m_biome, iconList.Count);
                    iconElement.name = trophy.m_name;

                    iconList.Add(iconElement);
                }

                if (GetGameMode() == TrophyGameMode.TrophyFiesta)
                {
                    __m_fiestaFlashing = true;
                    __m_trophyHuntMod.StartCoroutine(FlashTrophyFiesta());
                }
            }

            //public static void DeleteBiomeElements(List<GameObject> biomeIconList)
            //{
            //    foreach (GameObject biomeIconObject in biomeIconList)
            //    {
            //        GameObject.Destroy(biomeIconObject);
            //    }

            //    biomeIconList.Clear();
            //}

            //public static void CreateBiomeElements(Transform parentTransform, List<GameObject> iconList)
            //{
            //    foreach (BiomeBonus biomeBonus in __m_biomeBonuses)
            //    {
            //        // Create a new GameObject for the icon
            //        GameObject biomeIcon = new GameObject(biomeBonus.m_biomeName);
            //        biomeIcon.transform.SetParent(parentTransform);

            //        // Add RectTransform component for positioning Sprite
            //        RectTransform iconRectTransform = biomeIcon.AddComponent<RectTransform>();

            //        // Add an Image component for Sprite
            //        UnityEngine.UI.Image iconImage = biomeIcon.AddComponent<UnityEngine.UI.Image>();
            //        iconImage.color = new Color(1f, 1f, 1f, 0.5f);
            //        iconImage.raycastTarget = false;

            //        // Build the list of Biome elements from the trophy icon list
            //        float biomeIconXPos = int.MaxValue;
            //        float biomeIconYPos = int.MaxValue;

            //        float biomeIconHeight = 0f;
            //        float biomeIconWidth = 0f;

            //        foreach (GameObject icon in iconList)
            //        {
            //            RectTransform iconRect = icon.GetComponent<RectTransform>();

            //            // What biome is this trophy in?
            //            TrophyHuntData trophyHuntData = Array.Find(__m_trophyHuntData, element => element.m_name == icon.name);

            //            // If it's not the biome we're looking for, ignore it
            //            if (trophyHuntData.m_biome != biomeBonus.m_biome)
            //            {
            //                continue;
            //            }

            //            // Only build the icon rectangle for the biome we're looking at
            //            if (iconRect.anchoredPosition.x < biomeIconXPos)
            //            {
            //                biomeIconXPos = iconRect.anchoredPosition.x;
            //            }

            //            // Lazy grab, they're all at the same Y level
            //            biomeIconYPos = iconRect.anchoredPosition.y;
                        
            //            biomeIconWidth += iconRect.sizeDelta.x;

            //            // Lazy grab, assume they're all the same height
            //            biomeIconHeight = iconRect.sizeDelta.y;
            //        }

            //        iconRectTransform.sizeDelta = new Vector2(biomeIconWidth, biomeIconHeight);
            //        iconRectTransform.anchoredPosition = new Vector2(biomeIconXPos, biomeIconYPos);

            //        __m_biomeIconList.Add(biomeIcon);
            //    }
            //}

            static Sprite GetTrophySprite(string trophyPrefabName)
            {
                // Ensure the ObjectDB is loaded
                if (ObjectDB.instance == null)
                {
                    Debug.LogError("ObjectDB is not loaded.");
                    return null;
                }

                // Find the prefab for the specified trophy
                GameObject trophyPrefab = ObjectDB.instance.GetItemPrefab(trophyPrefabName);
                if (trophyPrefab == null)
                {
                    Debug.LogError($"Trophy prefab '{trophyPrefabName}' not found.");
                    return null;
                }

                // Extract the ItemDrop component and get the item's icon
                ItemDrop itemDrop = trophyPrefab.GetComponent<ItemDrop>();
                if (itemDrop == null)
                {
                    Debug.LogError($"ItemDrop component not found on prefab '{trophyPrefabName}'.");
                    return null;
                }

                return itemDrop.m_itemData.m_shared.m_icons[0];
            }

            static void EnableTrophyHuntIcon(string trophyName)
            {
                // Find the UI element and bold it
                if (__m_iconList == null)
                {
                    Debug.LogError("__m_iconList is null in EnableTrophyHuntIcon()");

                    return;
                }

                GameObject iconGameObject = __m_iconList.Find(gameObject => gameObject.name == trophyName);

                if (iconGameObject != null)
                {
                    UnityEngine.UI.Image image = iconGameObject.GetComponent<UnityEngine.UI.Image>();
                    if (image != null)
                    {
                        image.color = Color.white;
                    }
                }
                else
                {
                    Debug.LogError($"Unable to find {trophyName} in __m_iconList");
                }
            }

            static int CalculateTrophyScore(Player player)
            {
                int score = 0;
                foreach (string trophyName in player.GetTrophies())
                {
                    TrophyHuntData trophyHuntData = Array.Find(__m_trophyHuntData, element => element.m_name == trophyName);

                    if (trophyHuntData.m_name == trophyName)
                    {
                        // Add the value to our score
                        score += trophyHuntData.m_value;
                    }
                }

                return score;
            }

            static void CalculateBiomeBonusStats(Biome biome, out int numCollected, out int numTotal, out int biomeScore)
            {
                BiomeBonus biomeBonus = Array.Find(__m_biomeBonuses, element => element.m_biome == biome);

                numCollected = 0;
                numTotal = biomeBonus.m_trophies.Count;
                biomeScore = biomeBonus.m_bonus;

                foreach (string trophyName in biomeBonus.m_trophies)
                {
                    if (__m_trophyCache.Contains(trophyName))
                    {
                        numCollected++;
                    }
                }
            }

            public static int CalculateBiomeBonusScore(Player player)
            {
                int bonusScore = 0;

                foreach (BiomeBonus biomeBonus in __m_biomeBonuses)
                {
                    int numCollected = 0;
                    int numTotal = 0;
                    int biomeScore = 0;

                    CalculateBiomeBonusStats(biomeBonus.m_biome, out numCollected, out numTotal, out biomeScore);

                    if (numCollected == numTotal)
                    {
                        bonusScore += biomeScore;
                    }
                }

                return bonusScore;
            }

            // Returns TRUE if the trophy completes the set for a biome and adds that biome to the list of completed ones
            public static bool UpdateBiomeBonusTrophies(string trophyName)
            {
                TrophyHuntData trophyHuntData = Array.Find(__m_trophyHuntData, element => element.m_name == trophyName);

                int numCollected = 0;
                int numTotal = 0;
                int biomeScore = 0;

                CalculateBiomeBonusStats(trophyHuntData.m_biome, out numCollected, out numTotal, out biomeScore);

                if (numCollected == numTotal && !__m_completedBiomeBonuses.Contains(trophyHuntData.m_biome))
                {
                    PrintToConsole($"Biome Completed! {trophyHuntData.m_biome.ToString()}");

                    __m_completedBiomeBonuses.Add(trophyHuntData.m_biome);

                    return true;
                }

                return false;
            }
            public static void EnableTrophyHuntIcons(Player player)
            {
                // Enable found trophies
                foreach (string trophyName in player.GetTrophies())
                {
                    EnableTrophyHuntIcon(trophyName);
                }
            }

            public static void EnableBiomes(Player player)
            {
                // Enable found trophies
                foreach (string trophyName in player.GetTrophies())
                {
                    EnableTrophyHuntIcon(trophyName);
                }
            }


            static void UpdateTrophyHuntUI(Player player)
            {
                // If there's no Hud yet, don't do anything here
                if (Hud.instance == null)
                {
                    Debug.LogError("Hud.instance is null");
                    return;
                }

                if (Hud.instance.m_rootObject == null)
                {
                    Debug.LogError("Hud.instance.m_rootObject is null");
                    return;
                }

                // If there's no player yet, or no trophy list, don't do anything here
                if (player == null)
                {
                    Debug.LogError("Player.m_localPlayer is null");
                    return;
                }

                if (player.m_trophies == null)
                {
                    Debug.LogError("Player.m_localPlayer.m_trophies is null");
                    return;
                }

                EnableTrophyHuntIcons(player);
                EnableBiomes(player);

                int score = CalculateTrophyScore(player);

                // Update the deaths text and subtract deaths from score
                //
                PlayerProfile profile = Game.instance.GetPlayerProfile();
                if (profile != null)
                {
                    PlayerProfile.PlayerStats stats = profile.m_playerStats;
                    if (stats != null)
                    {
                        __m_deaths = (int)stats[PlayerStatType.Deaths];

                        Debug.LogWarning($"Subtracting score for {__m_deaths} deaths.");

                        score += CalculateDeathPenalty();

                        // Update the UI element
                        TMPro.TextMeshProUGUI deathsText = __m_deathsTextElement.GetComponent<TMPro.TextMeshProUGUI>();
                        if (deathsText != null)
                        {
                            deathsText.SetText(__m_deaths.ToString());
                        }
                    }
                }

                // Subtract points for logouts
                //                Debug.LogWarning($"Subtracting score for {__m_logoutCount} logouts.");
                if (!__m_ignoreLogouts)
                {
                    score += CalculateLogoutPenalty();
                }

                if (GetGameMode() == TrophyGameMode.TrophyRush)
                {
                    score += CalculateBiomeBonusScore(player);
                }

                // Update the Score string
                __m_scoreTextElement.GetComponent<TMPro.TextMeshProUGUI>().text = score.ToString();

                // Update the Logouts string
                __m_relogsTextElement.GetComponent<TMPro.TextMeshProUGUI>().text = __m_logoutCount.ToString();

                if (UPDATE_LEADERBOARD)
                {
                    // Send the score to the web page
                    SendScoreToLeaderboard(score);
                }
            }

            static IEnumerator FlashImage(UnityEngine.UI.Image targetImage, RectTransform imageRect)
            {
                float flashDuration = 0.809f;
                int numFlashes = 6;

                Vector2 originalAnchoredPosition = imageRect.anchoredPosition;
                Vector3 originalScale = imageRect.localScale;

                for (int i = 0; i < numFlashes; i++)
                {
                    for (float t = 0.0f; t < flashDuration; t += Time.deltaTime)
                    {
                        float interpValue = Math.Min(1.0f, t / flashDuration);

                        int flash = (int)(interpValue * 5.0f);
                        if (flash % 2 == 0)
                        {
                            targetImage.color = Color.white;
                        }
                        else
                        {
                            targetImage.color = Color.green;
                        }

                        float flashScale = 1 + (1.5f * interpValue);
                        imageRect.localScale = new Vector3(__m_baseTrophyScale, __m_baseTrophyScale, __m_baseTrophyScale) * flashScale * __m_userTrophyScale;
                        imageRect.anchoredPosition = originalAnchoredPosition + (new Vector2(0, 150.0f) * (float)Math.Sin((float)interpValue/2f));

                        yield return null;
                    }

                    imageRect.anchoredPosition = originalAnchoredPosition;
                }

                targetImage.color = Color.white;
                imageRect.localScale = originalScale;
                imageRect.anchoredPosition = originalAnchoredPosition;
            }

            static IEnumerator FlashImage2(UnityEngine.UI.Image targetImage, RectTransform imageRect)
            {
                float flashDuration = 0.5f;
                int numFlashes = 4;

                Vector2 originalAnchoredPosition = imageRect.anchoredPosition;
                Vector3 originalScale = imageRect.localScale;

                float curAccel = 0.0f;
                float curVelocity = 0.0f;
                float curPosition = 0.0f;
                float timeElapsed = 0.0f;

                for (int i = 0; i < numFlashes; i++)
                {
                    // Apply impulse
                    curAccel = 10.0f; // m/sec
                    curVelocity = 0.0f;
                    curPosition = 0.0f;
                    timeElapsed = 0.0f;

                    while (curVelocity > 0.1f)
                    {
                        float dt = Time.deltaTime;

                        // Do integration
                        curAccel += -10.0f * dt;
                        curVelocity = curVelocity + curAccel * dt;
                        curPosition = curPosition + curVelocity * dt;

                        float flashScale = 1 + (timeElapsed / flashDuration);

                        imageRect.localScale = new Vector3(__m_baseTrophyScale, __m_baseTrophyScale, __m_baseTrophyScale) * flashScale * __m_userTrophyScale;
                        imageRect.anchoredPosition = originalAnchoredPosition + (new Vector2(0, 200.0f) * curPosition);

                        yield return null;
                    }
                }

                targetImage.color = Color.white;
                imageRect.localScale = originalScale;
                imageRect.anchoredPosition = originalAnchoredPosition;
            }

            static IEnumerator FlashBiomeImage(UnityEngine.UI.Image targetImage, RectTransform imageRect)
            {
                float flashDuration = 6f;

                Quaternion originalRotation = imageRect.rotation;

                for (float t = 0.0f; t < flashDuration; t += Time.deltaTime)
                {
                    imageRect.localEulerAngles += new Vector3(0f, 0f, t);

                    yield return null;
                }

                imageRect.rotation = originalRotation;
            }


            static IEnumerator FlashTrophyFiesta()
            {
                int startingColorIndex = 0;
                float elapsedTime = 0f;
                float flashInterval = 0.6f;

                while (__m_fiestaFlashing)
                {
                    elapsedTime += Time.deltaTime;
                    if (elapsedTime > flashInterval)
                    {
                        elapsedTime = 0f;

                        int iconIndex = 0;
                        foreach (GameObject go in __m_iconList)
                        {
                            if (go != null)
                            {
                                UnityEngine.UI.Image image = go.GetComponent<UnityEngine.UI.Image>();

                                if (image != null)
                                {
                                    int colorIndex = (startingColorIndex + iconIndex) % __m_fiestaColors.Length;

                                    if (image.color != Color.white)
                                    {
                                        Color color = __m_fiestaColors[colorIndex];
                                        color.a = 0.5f;
                                        image.color = color;
                                    }
                                }
                            }

                            iconIndex++;
                        }

                        if (++startingColorIndex >= __m_fiestaColors.Length)
                        {
                            startingColorIndex = 0;
                        }
                    }

                    yield return null;
                }

                foreach (GameObject go in __m_iconList)
                {
                    if (go != null)
                    {
                        UnityEngine.UI.Image image = go.GetComponent<UnityEngine.UI.Image>();

                        if (image != null)
                        {
                            image.color = Color.white;
                        }
                    }
                }
            }

            static void FlashTrophy(string trophyName)
            {
                GameObject iconGameObject = __m_iconList.Find(gameObject => gameObject.name == trophyName);

                if (iconGameObject != null)
                {
                    UnityEngine.UI.Image image = iconGameObject.GetComponent<UnityEngine.UI.Image>();
                    if (image != null)
                    {
                        RectTransform imageRect = iconGameObject.GetComponent<RectTransform>();

                        if (imageRect != null)
                        {
                            // Flash it with a CoRoutine
                            __m_trophyHuntMod.StartCoroutine(FlashImage(image, imageRect));
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Unable to find {trophyName} in __m_iconList");
                }
            }

            static void FlashBiomeTrophies(string trophyName)
            {
                TrophyHuntData trophyHuntData = Array.Find(__m_trophyHuntData, element => element.m_name == trophyName);

                BiomeBonus biomeBonus = Array.Find(__m_biomeBonuses, element => element.m_biome == trophyHuntData.m_biome);

                foreach(string biomeTrophyName in biomeBonus.m_trophies)
                {
                    GameObject iconGameObject = __m_iconList.Find(gameObject => gameObject.name == biomeTrophyName);
                    if (iconGameObject != null)
                    {
                        UnityEngine.UI.Image image = iconGameObject.GetComponent<UnityEngine.UI.Image>();
                        if (image != null)
                        {
                            RectTransform imageRect = iconGameObject.GetComponent<RectTransform>();

                            if (imageRect != null)
                            {
                                // Flash it with a CoRoutine
                                __m_trophyHuntMod.StartCoroutine(FlashBiomeImage(image, imageRect));
                            }
                        }
                    }
                }
            }


            [HarmonyPatch(typeof(Player), nameof(Player.AddTrophy), new[] { typeof(ItemDrop.ItemData) })]
            public static class Player_AddTrophy_Patch
            {
                public static void Postfix(Player __instance, ItemDrop.ItemData item)
                {
                    var player = __instance;

                    if (player != null && item != null)
                    {
                        var name = item.m_dropPrefab.name;

                        // Check to see if this one's in the cache, if not, it's new to us
                        if (__m_trophyCache.Find(trophyName => trophyName == name) != name)
                        {
                            // Haven't collected this one before, flash the UI for it
                            FlashTrophy(name);

                            // Update Trophy cache
                            __m_trophyCache = player.GetTrophies();

                            if (GetGameMode() == TrophyGameMode.TrophyRush)
                            {
                                // Did we complete a biome bonus with this trophy?
                                if (UpdateBiomeBonusTrophies(name))
                                {
                                    FlashBiomeTrophies(name);
                                }
                            }
                            UpdateTrophyHuntUI(player);
                        }
                    }
                }
            }

            // Player Path Collection
            #region Player Path Collection

            public static void StartCollectingPlayerPath()
            {
                if (!__m_collectingPlayerPath)
                {
                    Debug.Log("Starting Player Path collection");

                    //                   AddPlayerPathUI();

                    __m_previousPlayerPos = Player.m_localPlayer.transform.position;

                    __m_collectingPlayerPath = true;

                    __m_trophyHuntMod.StartCoroutine(CollectPlayerPath());
                }
            }

            public static void StopCollectingPlayerPath()
            {
                Debug.Log("Stopping Player Path collection");

                if (__m_collectingPlayerPath)
                {
                    __m_trophyHuntMod.StopCoroutine(CollectPlayerPath());

                    __m_collectingPlayerPath = false;
                }
            }

            public static IEnumerator CollectPlayerPath()
            {
                if (Player.m_localPlayer != null)
                {
                    while (__m_collectingPlayerPath && Player.m_localPlayer != null)
                    {
                        Vector3 curPlayerPos = Player.m_localPlayer.transform.position;
                        if (Vector3.Distance(curPlayerPos, __m_previousPlayerPos) > __m_minPathPlayerMoveDistance)
                        {
                            __m_playerPathData.Add(curPlayerPos);
                            __m_previousPlayerPos = curPlayerPos;

                            Debug.Log($"Collected player position at {curPlayerPos.ToString()}");
                        }

                        yield return new WaitForSeconds(__m_playerPathCollectionInterval);
                    }
                }
            }
            #endregion

            // Leaderboard
            #region Leaderboard

            [System.Serializable]
            public class LeaderboardData
            {
                public string player_name;
                public int current_score;
                public string session_id;
                public string player_location;
                public string trophies;
                public int deaths;
                public int logouts;
                public string gamemode;
            }

            private static void SendScoreToLeaderboard(int score)
            {
                string steamName = SteamFriends.GetPersonaName();
                string seed = WorldGenerator.instance.m_world.m_seedName;
                string sessionId = seed.ToString();
                string playerPos = Player.m_localPlayer.transform.position.ToString();
                string trophyList = string.Join(", ", __m_trophyCache);

                // Example data to send to the leaderboard
                var leaderboardData = new LeaderboardData
                {
                    player_name = steamName,
                    current_score = score,
                    session_id = sessionId,
                    player_location = playerPos,
                    trophies = trophyList,
                    deaths = __m_deaths,
                    logouts = __m_logoutCount,
                    gamemode = (GetGameMode() == TrophyGameMode.TrophyHunt) 
                                    ? "TrophyHunt" : (GetGameMode() == TrophyGameMode.TrophyRush) 
                                    ? "TrophyRush" : "TrophyFiesta" //__m_trophyRushEnabled ? "TrophyRush" : "TrophyHunt"
                };

                // Start the coroutine to post the data
                __m_trophyHuntMod.StartCoroutine(PostLeaderboardDataCoroutine(LEADERBOARD_URL, leaderboardData));
            }

            // HACK: use API key to communicate with server instead of viciously bypassing security.
            // Valheim configured Unity to require secure connections
            //
            //private class BypassCertificateHandler : CertificateHandler
            //{
            //    protected override bool ValidateCertificate(byte[] certificateData)
            //    {
            //        return true; // Always return true to bypass validation
            //    }
            //}

            private static IEnumerator PostLeaderboardDataCoroutine(string url, LeaderboardData data)
            {
                // Convert the data to JSON
                string jsonData = JsonUtility.ToJson(data);

                Debug.Log(jsonData);

                // Create a UnityWebRequest for the POST operation
                var request = new UnityWebRequest(url, "POST");
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                //request.certificateHandler = new BypassCertificateHandler();

                // Send the request and wait for a response
                yield return request.SendWebRequest();

                // Handle the response
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Leaderboard POST successful! Response: " + request.downloadHandler.text);
                }
                else
                {
                    Debug.LogError("Leaderboard POST failed: " + request.error);
                }

                Debug.Log("Leaderboard Response: " + request.error);
                Debug.Log(request.downloadHandler.text);
            }

            #endregion Leaderboard

            // Logout Tracking
            #region Logout Handling

            static float GetTotalOnFootDistance(Game game)
            {
                if (game == null)
                {
                    Debug.LogError($"No Game object found in GetTotalOnFootDistance");

                    return 0.0f;
                }

                PlayerProfile profile = game.GetPlayerProfile();
                if (profile != null)
                {
                    PlayerProfile.PlayerStats stats = profile.m_playerStats;
                    if (stats != null)
                    {
                        float onFootDistance = stats[PlayerStatType.DistanceWalk] + stats[PlayerStatType.DistanceRun];

                        return onFootDistance;
                    }
                }

                return 0.0f;
            }

            // public void Logout(bool save = true, bool changeToStartScene = true)
            [HarmonyPatch(typeof(Game), nameof(Game.Logout), new[] { typeof(bool), typeof(bool) })]
            public static class Game_Logout_Patch
            {
                public static void Postfix(Game __instance, bool save, bool changeToStartScene)
                {
                    float onFootDistance = GetTotalOnFootDistance(__instance);
                    Debug.LogError($"Total on-foot distance moved: {onFootDistance}");

                    // If you've never logged out, and your total run/walk distance is less than the max grace distance, no penalty
                    if (__m_logoutCount < 1 && onFootDistance < LOGOUT_PENALTY_GRACE_DISTANCE)
                    {
                        // ignore this logout
                        return;
                    }

                    if (!__m_ignoreLogouts)
                    {
                        __m_logoutCount++;
                    }
                }
            }

            #endregion

            #region Tooltips

            // Score Tooltip
            static GameObject __m_scoreTooltipObject = null;
            static GameObject __m_scoreTooltipBackground = null;
            static TextMeshProUGUI __m_scoreTooltipText;
            static Vector2 __m_trophyHuntScoreTooltipWindowSize = new Vector2(240, 155);
            static Vector2 __m_trophyRushScoreTooltipWindowSize = new Vector2(290, 320);
            static Vector2 __m_scoreTooltipTextOffset = new Vector2(5, 2);

            public static void CreateScoreTooltip()
            {
                // Tooltip Background
                __m_scoreTooltipBackground = new GameObject("Score Tooltip Background");

                Vector2 tooltipWindowSize = __m_trophyHuntScoreTooltipWindowSize;
                if (GetGameMode() == TrophyGameMode.TrophyRush)
                {
                    tooltipWindowSize = __m_trophyRushScoreTooltipWindowSize;
                }

                // Set %the parent to the HUD
                Transform hudrootTransform = Hud.instance.transform;
                __m_scoreTooltipBackground.transform.SetParent(hudrootTransform, false);

                RectTransform bgTransform = __m_scoreTooltipBackground.AddComponent<RectTransform>();
                bgTransform.sizeDelta = tooltipWindowSize;

                // Add an Image component for the background
                UnityEngine.UI.Image backgroundImage = __m_scoreTooltipBackground.AddComponent<UnityEngine.UI.Image>();
                backgroundImage.color = new Color(0, 0, 0, 0.95f); // Semi-transparent black background

                __m_scoreTooltipBackground.SetActive(false);

                // Create a new GameObject for the tooltip
                __m_scoreTooltipObject = new GameObject("Score Tooltip Text");
                __m_scoreTooltipObject.transform.SetParent(__m_scoreTooltipBackground.transform, false);

                // Add a RectTransform component for positioning
                RectTransform rectTransform = __m_scoreTooltipObject.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(tooltipWindowSize.x - __m_scoreTooltipTextOffset.x, tooltipWindowSize.y - __m_scoreTooltipTextOffset.y);

                // Add a TextMeshProUGUI component for displaying the tooltip text
                __m_scoreTooltipText = __m_scoreTooltipObject.AddComponent<TextMeshProUGUI>();
                __m_scoreTooltipText.fontSize = 14;
                __m_scoreTooltipText.alignment = TextAlignmentOptions.TopLeft;
                __m_scoreTooltipText.color = Color.yellow;

                // Initially hide the tooltip
                __m_scoreTooltipObject.SetActive(false);
            }

            public static void AddTooltipTriggersToScoreObject(GameObject uiObject)
            {
                if (!COLLECT_DROP_RATES)
                {
                    return;
                }

                // Add EventTrigger component if not already present
                EventTrigger trigger = uiObject.GetComponent<EventTrigger>();
                if (trigger != null)
                {
                    return;
                }

                trigger = uiObject.AddComponent<EventTrigger>();

                // Mouse Enter event (pointer enters the icon area)
                EventTrigger.Entry entryEnter = new EventTrigger.Entry();
                entryEnter.eventID = EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((eventData) => ShowScoreTooltip(uiObject));
                trigger.triggers.Add(entryEnter);

                // Mouse Exit event (pointer exits the icon area)
                EventTrigger.Entry entryExit = new EventTrigger.Entry();
                entryExit.eventID = EventTriggerType.PointerExit;
                entryExit.callback.AddListener((eventData) => HideScoreTooltip());
                trigger.triggers.Add(entryExit);
            }

            public static string BuildScoreTooltipText(GameObject uiObject)
            {
                string text = "<n/a>";

                string gameModeText = "";

                switch (GetGameMode())
                {
                    case TrophyGameMode.TrophyHunt:
                        gameModeText = "Trophy Hunt";
                        break;
                    case TrophyGameMode.TrophyRush:
                        gameModeText = "Trophy Rush";
                        break;
                    case TrophyGameMode.TrophySaga:
                        gameModeText = "Trophy Saga";
                        break;
                    case TrophyGameMode.TrophyFiesta:
                        gameModeText = "Trophy Fiesta";
                        break;
                }
//                if (__m_trophyRushEnabled)
//                    gameModeText = "Trophy Rush";

                int trophyCount = __m_trophyCache.Count;

                text = $"<size=20><b><color=#FFB75B>{gameModeText}</color><b></size>\n";
                text += $"<size=14><color=white>\n";
                text += $"  Trophies:\n    Num: <color=orange>{trophyCount}</color> <color=yellow>({CalculateTrophyPoints().ToString()} Points)</color>\n";
                text += $"  Logouts: (Penalty: <color=red>{GetLogoutPointCost()}</color>)\n    Num: <color=orange>{__m_logoutCount}</color> <color=yellow>({CalculateLogoutPenalty().ToString()} Points)</color>\n";
                text += $"  Deaths: (Penalty: <color=red>{GetDeathPointCost()}</color>)\n    Num: <color=orange>{__m_deaths}</color> <color=yellow>({CalculateDeathPenalty().ToString()} Points)</color>\n";
                if (GetGameMode() == TrophyGameMode.TrophyRush)
                {
                    text += $"  /die's: (Penalty: <color=red>{TROPHY_RUSH_SLASHDIE_PENALTY}</color>)\n    Num: <color=orange>{__m_slashDieCount}</color> <color=yellow>({__m_slashDieCount * TROPHY_RUSH_SLASHDIE_PENALTY} Points)</color>\n";
                }

                if (GetGameMode() == TrophyGameMode.TrophyRush)
                {
                    text += $"  Biome Bonuses:\n";
                    foreach (BiomeBonus biomeBonus in __m_biomeBonuses)
                    {
                        int numCollected, numTotal, biomeScore;

                        CalculateBiomeBonusStats(biomeBonus.m_biome, out numCollected, out numTotal, out biomeScore);

                        int bonusScore = 0;
                        if (numCollected == numTotal)
                        {
                            bonusScore = biomeScore;
                        }
                        text += $"    {biomeBonus.m_biomeName} (+{biomeBonus.m_bonus}): <color=orange>{numCollected}/{numTotal}</color> <color=yellow>({bonusScore} Points)</color>\n";
                    }
                }

                text += $"</color></size>";
                return text;
            }


            public static void ShowScoreTooltip(GameObject uiObject)
            {
                if (uiObject == null)
                    return;

                string text = BuildScoreTooltipText(uiObject);

                __m_scoreTooltipText.text = text;

                __m_scoreTooltipBackground.SetActive(true);
                __m_scoreTooltipObject.SetActive(true);

                Vector2 tooltipWindowSize = __m_trophyHuntScoreTooltipWindowSize;
                if (GetGameMode() == TrophyGameMode.TrophyRush)
                {
                    tooltipWindowSize = __m_trophyRushScoreTooltipWindowSize;
                }

                Vector3 tooltipOffset = new Vector3(tooltipWindowSize.x / 2, tooltipWindowSize.y, 0);
                Vector3 mousePosition = Input.mousePosition;
                Vector3 desiredPosition = mousePosition + tooltipOffset;

                // Clamp the tooltip window onscreen
                if (desiredPosition.x < 200) desiredPosition.x = 200;
                if (desiredPosition.y < 200) desiredPosition.y = 200;
                if (desiredPosition.x > Screen.width - tooltipWindowSize.x)
                    desiredPosition.x = Screen.width - tooltipWindowSize.x;
                if (desiredPosition.y > Screen.height - tooltipWindowSize.y)
                    desiredPosition.y = Screen.height - tooltipWindowSize.y;

                //                Debug.LogWarning($"Luck Tooltip x={desiredPosition.x} y={desiredPosition.y}");

                __m_scoreTooltipBackground.transform.position = desiredPosition;
                __m_scoreTooltipObject.transform.position = new Vector3(desiredPosition.x + __m_scoreTooltipTextOffset.x, desiredPosition.y - __m_scoreTooltipTextOffset.y, 0f);
            }

            public static void HideScoreTooltip()
            {
                __m_scoreTooltipBackground.SetActive(false);
                __m_scoreTooltipObject.SetActive(false);
            }


            // Luck Tooltips

            static GameObject __m_luckTooltipObject = null;
            static GameObject __m_luckTooltipBackground = null;
            static TextMeshProUGUI __m_luckTooltip;
            static Vector2 __m_luckTooltipWindowSize = new Vector2(220, 135);
            static Vector2 __m_luckTooltipTextOffset = new Vector2(5, 2);

            public static void CreateLuckTooltip()
            {
                // Tooltip Background
                __m_luckTooltipBackground = new GameObject("Luck Tooltip Background");

                // Set %the parent to the HUD
                Transform hudrootTransform = Hud.instance.transform;
                __m_luckTooltipBackground.transform.SetParent(hudrootTransform, false);

                RectTransform bgTransform = __m_luckTooltipBackground.AddComponent<RectTransform>();
                bgTransform.sizeDelta = __m_luckTooltipWindowSize;

                // Add an Image component for the background
                UnityEngine.UI.Image backgroundImage = __m_luckTooltipBackground.AddComponent<UnityEngine.UI.Image>();
                backgroundImage.color = new Color(0, 0, 0, 0.85f); // Semi-transparent black background

                __m_luckTooltipBackground.SetActive(false);

                // Create a new GameObject for the tooltip
                __m_luckTooltipObject = new GameObject("Luck Tooltip Text");
                __m_luckTooltipObject.transform.SetParent(__m_luckTooltipBackground.transform, false);

                // Add a RectTransform component for positioning
                RectTransform rectTransform = __m_luckTooltipObject.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(__m_luckTooltipWindowSize.x - __m_luckTooltipTextOffset.x, __m_luckTooltipWindowSize.y - __m_luckTooltipTextOffset.y);

                // Add a TextMeshProUGUI component for displaying the tooltip text
                __m_luckTooltip = __m_luckTooltipObject.AddComponent<TextMeshProUGUI>();
                __m_luckTooltip.fontSize = 14;
                __m_luckTooltip.alignment = TextAlignmentOptions.TopLeft;
                __m_luckTooltip.color = Color.yellow;

                // Initially hide the tooltip
                __m_luckTooltipObject.SetActive(false);
            }

            public static void AddTooltipTriggersToLuckObject(GameObject uiObject)
            {
                if (!COLLECT_DROP_RATES)
                {
                    return;
                }

                // Add EventTrigger component if not already present
                EventTrigger trigger = uiObject.GetComponent<EventTrigger>();
                if (trigger != null)
                {
                    return;
                }

                trigger = uiObject.AddComponent<EventTrigger>();

                // Mouse Enter event (pointer enters the icon area)
                EventTrigger.Entry entryEnter = new EventTrigger.Entry();
                entryEnter.eventID = EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((eventData) => ShowLuckTooltip(uiObject));
                trigger.triggers.Add(entryEnter);

                // Mouse Exit event (pointer exits the icon area)
                EventTrigger.Entry entryExit = new EventTrigger.Entry();
                entryExit.eventID = EventTriggerType.PointerExit;
                entryExit.callback.AddListener((eventData) => HideLuckTooltip());
                trigger.triggers.Add(entryExit);
            }

            public struct LuckRating
            {
                public LuckRating(float percent, string luckString, string colorStr)
                {
                    m_percent = percent;
                    m_luckString = luckString;
                    m_colorString = colorStr;
                }
                public float m_percent = 0;
                public string m_luckString = "<n/a>";
                public string m_colorString = "white";
            }

            public static LuckRating[] __m_luckRatingTable = new LuckRating[]
                {
                    new LuckRating (70.0f,      "Bad",          "#BF6000"),
                    new LuckRating (100.0f,     "Average",      "#BFBF00"),
                    new LuckRating (140.0f,     "Good",         "#00BF00"),
                    new LuckRating (9999.0f,    "Bonkers",      "#6000BF"),
                };

            public static int GetLuckRatingIndex(float luckPercentage)
            {
                int index = 0;
                foreach (LuckRating rating in __m_luckRatingTable)
                {
                    if (luckPercentage <= rating.m_percent)
                    {
                        return index;
                    }

                    index++;
                }

                return 0;
            }

            public static string GetLuckRatingUIString(float luckPercentage)
            {
                int ratingIndex = GetLuckRatingIndex(luckPercentage);

                LuckRating luckRating = __m_luckRatingTable[ratingIndex];

                return $"<color={luckRating.m_colorString}>{luckRating.m_luckString}</color>";
            }

            public static string BuildLuckTooltipText(GameObject uiObject)
            {
                if (uiObject == null)
                {
                    return "Invalid";
                }

                int numTrophyTypesKilled = 0;
                float cumulativeDropRatio = 0f;

                float luckiestScore = float.MinValue;
                string luckiestTrophy = "<n/a>";
                float luckiestActualPercent = 0f;
                float luckiestExpectedPercent = 0f;
                float luckiestRatio = 0f;
                float unluckiestScore = float.MaxValue;
                string unluckiestTrophy = "<n/a>";
                float unluckiestActualPercent = 0f;
                float unluckiestExpectedPercent = 0f;
                float unluckiestRatio = 0f;

                // Compute Luck
                foreach (KeyValuePair<string, DropInfo> entry in __m_playerTrophyDropInfo)
                {
                    DropInfo di = entry.Value;
                    if (di.m_numKilled == 0)
                    {
                        continue;
                    }

                    string trophyName = entry.Key;
                    TrophyHuntData data = Array.Find(__m_trophyHuntData, element => element.m_name == trophyName);

                    // Ignore 100% drop trophies
                    if (data.m_dropPercent >= 100)
                    {
                        continue;
                    }

                    // Ignore if you haven't killed enough to get a drop
                    if (di.m_trophies == 0 ||
                        di.m_numKilled < (100 / data.m_dropPercent))
                    {
                        continue;
                    }

                    float actualDropPercent = 100.0f * (float)di.m_trophies / (float)di.m_numKilled;
                    float wikiDropPercent = data.m_dropPercent;

                    float dropRatio = actualDropPercent / wikiDropPercent;

                    if (dropRatio > luckiestScore)
                    {
                        luckiestScore = dropRatio;
                        luckiestTrophy = data.m_prettyName;
                        luckiestActualPercent = actualDropPercent;
                        luckiestExpectedPercent = data.m_dropPercent;
                        luckiestRatio = luckiestActualPercent / luckiestExpectedPercent * 100.0f;
                    }
                    if (dropRatio < unluckiestScore)
                    {
                        unluckiestScore = dropRatio;
                        unluckiestTrophy = data.m_prettyName;
                        unluckiestActualPercent = actualDropPercent;
                        unluckiestExpectedPercent = data.m_dropPercent;
                        unluckiestRatio = unluckiestActualPercent / unluckiestExpectedPercent * 100.0f;
                    }
                    //                    Debug.LogWarning($"Drop: {trophyName}: {dropRatio}");

                    cumulativeDropRatio += dropRatio;

                    numTrophyTypesKilled++;
                }


                string luckPercentStr = "<n/a>";
                string luckRatingStr = "<n/a>";
                float luckPercentage = 0.0f;
                //                int luckRatingIndex = -1;

                if (numTrophyTypesKilled > 0)
                {
                    luckPercentage = (100.0f * (cumulativeDropRatio / (float)numTrophyTypesKilled));
                    luckPercentStr = luckPercentage.ToString("0.0");
                    luckRatingStr = GetLuckRatingUIString(luckPercentage);
                    //                    luckRatingIndex = GetLuckRatingIndex(luckPercentage);

                }

                string text =
                    $"<size=16><b><color=#FFB75B>Luck-O-Meter</color><b></size>\n" +
                    $"<color=white>  Player Luck Score: </color><color=orange>{luckPercentStr}</color>\n" +
                    $"<color=white>  Player Luck Rating: </color>{luckRatingStr}\n";

                //int index = 0;
                //foreach (LuckRating luckRating in __m_luckRatingTable)
                //{
                //    string colorStr = "#606060";
                //    if (index == luckRatingIndex)
                //    {
                //        colorStr = luckRating.m_colorString;
                //    }
                //    text += $"      <color={colorStr}>{luckRating.m_luckString}</color>\n";

                //    index++;
                //}

                string luckiestColor = __m_luckRatingTable[GetLuckRatingIndex(luckiestRatio)].m_colorString;
                string unluckiestColor = __m_luckRatingTable[GetLuckRatingIndex(unluckiestRatio)].m_colorString;

                // Luckiest and Unluckiest
                text += $"<color=white>  Luckiest:</color>\n";
                text += $"    <color={luckiestColor}>{luckiestTrophy}</color> <color=orange>{luckiestActualPercent.ToString("0.0")}%</color> (<color=yellow>{luckiestExpectedPercent}%)</color>\n";
                text += $"<color=white>  Unluckiest:</color>\n";
                text += $"    <color={unluckiestColor}>{unluckiestTrophy}</color> <color=orange>{unluckiestActualPercent.ToString("0.0")}%</color> (<color=yellow>{unluckiestExpectedPercent}%)</color>\n";

                return text;
            }

            public static void ShowLuckTooltip(GameObject uiObject)
            {
                if (uiObject == null)
                    return;

                string text = BuildLuckTooltipText(uiObject);

                __m_luckTooltip.text = text;

                __m_luckTooltipBackground.SetActive(true);
                __m_luckTooltipObject.SetActive(true);

                Vector3 tooltipOffset = new Vector3(__m_luckTooltipWindowSize.x / 2, __m_luckTooltipWindowSize.y, 0);
                Vector3 mousePosition = Input.mousePosition;
                Vector3 desiredPosition = mousePosition + tooltipOffset;

                // Clamp the tooltip window onscreen
                if (desiredPosition.x < 150) desiredPosition.x = 150;
                if (desiredPosition.y < 150) desiredPosition.y = 150;
                if (desiredPosition.x > Screen.width - __m_luckTooltipWindowSize.x)
                    desiredPosition.x = Screen.width - __m_luckTooltipWindowSize.x;
                if (desiredPosition.y > Screen.height - __m_luckTooltipWindowSize.y)
                    desiredPosition.y = Screen.height - __m_luckTooltipWindowSize.y;

                //                Debug.LogWarning($"Luck Tooltip x={desiredPosition.x} y={desiredPosition.y}");

                __m_luckTooltipBackground.transform.position = desiredPosition;
                __m_luckTooltipObject.transform.position = new Vector3(desiredPosition.x + __m_luckTooltipTextOffset.x, desiredPosition.y - __m_luckTooltipTextOffset.y, 0f);
            }

            public static void HideLuckTooltip()
            {
                __m_luckTooltipBackground.SetActive(false);
                __m_luckTooltipObject.SetActive(false);
            }



            // Trophy Tooltips

            static GameObject __m_trophyTooltipObject = null;
            static GameObject __m_trophyTooltipBackground = null;
            static TextMeshProUGUI __m_trophyTooltip;
            static Vector2 __m_trophyTooltipWindowSize = new Vector2(240, 110);
            static Vector2 __m_trophyTooltipTextOffset = new Vector2(5, 2);
            static Vector2 __m_trophyTooltipAllTrophyStatsWindowSize = new Vector2(240, 180);

            public static void CreateTrophyTooltip()
            {
                Debug.LogWarning("Creating Tooltip object");

                Vector2 tooltipWindowSize = __m_trophyTooltipWindowSize;
                if (__m_showAllTrophyStats)
                {
                    tooltipWindowSize = __m_trophyTooltipAllTrophyStatsWindowSize;
                }

                // Tooltip Background
                __m_trophyTooltipBackground = new GameObject("Tooltip Background");

                // Set %the parent to the HUD
                Transform hudrootTransform = Hud.instance.transform;
                __m_trophyTooltipBackground.transform.SetParent(hudrootTransform, false);

                RectTransform bgTransform = __m_trophyTooltipBackground.AddComponent<RectTransform>();
                bgTransform.sizeDelta = tooltipWindowSize;

                // Add an Image component for the background
                UnityEngine.UI.Image backgroundImage = __m_trophyTooltipBackground.AddComponent<UnityEngine.UI.Image>();
                backgroundImage.color = new Color(0, 0, 0, 0.85f); // Semi-transparent black background

                __m_trophyTooltipBackground.SetActive(false);

                // Create a new GameObject for the tooltip
                __m_trophyTooltipObject = new GameObject("Tooltip Text");
                __m_trophyTooltipObject.transform.SetParent(__m_trophyTooltipBackground.transform, false);

                // Add a RectTransform component for positioning
                RectTransform rectTransform = __m_trophyTooltipObject.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(tooltipWindowSize.x - __m_trophyTooltipTextOffset.x, tooltipWindowSize.y - __m_trophyTooltipTextOffset.y);

                // Add a TextMeshProUGUI component for displaying the tooltip text
                __m_trophyTooltip = __m_trophyTooltipObject.AddComponent<TextMeshProUGUI>();
                __m_trophyTooltip.fontSize = 14;
                __m_trophyTooltip.alignment = TextAlignmentOptions.TopLeft;
                __m_trophyTooltip.color = Color.yellow;

                // Initially hide the tooltip
                __m_trophyTooltipObject.SetActive(false);
            }

            public static void DeleteTrophyTooltip()
            {
                if (__m_trophyTooltipObject != null)
                {
                    GameObject.DestroyImmediate(__m_trophyTooltipObject);
                    __m_trophyTooltipObject = null;
                }

                if (__m_trophyTooltipBackground)
                {
                    GameObject.DestroyImmediate(__m_trophyTooltipBackground);
                    __m_trophyTooltipBackground = null;
                }
            }

            public static void AddTooltipTriggersToTrophyIcon(GameObject trophyIconObject)
            {
                if (!COLLECT_DROP_RATES)
                {
                    return;
                }

                // Add EventTrigger component if not already present
                EventTrigger trigger = trophyIconObject.GetComponent<EventTrigger>();
                if (trigger != null)
                {
                    return;
                }

                trigger = trophyIconObject.AddComponent<EventTrigger>();

                // Mouse Enter event (pointer enters the icon area)
                EventTrigger.Entry entryEnter = new EventTrigger.Entry();
                entryEnter.eventID = EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((eventData) => ShowTrophyTooltip(trophyIconObject));
                trigger.triggers.Add(entryEnter);

                // Mouse Exit event (pointer exits the icon area)
                EventTrigger.Entry entryExit = new EventTrigger.Entry();
                entryExit.eventID = EventTriggerType.PointerExit;
                entryExit.callback.AddListener((eventData) => HideTrophyTooltip());
                trigger.triggers.Add(entryExit);
            }

            public static void CalculateDropPercentAndRating(TrophyHuntData trophyHuntData, DropInfo dropInfo, out string dropPercentStr, out string dropRatingStr)
            {
                dropPercentStr = "0";
                dropRatingStr = "<n/a>";

                if (dropInfo.m_numKilled > 0)
                {
                    float dropPercent = 0.0f;
                    float expectedDropPercent = trophyHuntData.m_dropPercent;

                    dropPercent = (100.0f * ((float)dropInfo.m_trophies / (float)dropInfo.m_numKilled));
                    dropPercentStr = dropPercent.ToString("0.0");

                    // Don't compute for 100% drop enemies
                    if (trophyHuntData.m_dropPercent < 100)
                    {
                        if (dropInfo.m_trophies > 0 &&
                            dropInfo.m_numKilled >= (100 / expectedDropPercent))
                        {
                            float ratingPercent = 100 * (dropPercent / expectedDropPercent);
                            dropRatingStr = GetLuckRatingUIString(ratingPercent);
                        }
                    }
                }
            }

            public static string BuildTrophyTooltipText(GameObject uiObject)
            {
                if (uiObject == null)
                {
                    return "Invalid";
                }

                string trophyName = uiObject.name;

                TrophyHuntData trophyHuntData = Array.Find(__m_trophyHuntData, element => element.m_name == trophyName);

                DropInfo allTrophyDropInfo = __m_allTrophyDropInfo[trophyName];
                DropInfo playerDropInfo = __m_playerTrophyDropInfo[trophyName];

                //                Debug.LogWarning($"dropped: {dropInfo.m_trophiesDropped} killed: {dropInfo.m_numKilled} percent:{trophyHuntData.m_dropPercent}");

                string playerDropPercentStr = "0";
                string playerDropRatingStr = "<n/a>";

                CalculateDropPercentAndRating(trophyHuntData, playerDropInfo, out playerDropPercentStr, out playerDropRatingStr);

                string allTrophyDropPercentStr = "0";
                string allTrophyDropRatingStr = "<n/a>";

                CalculateDropPercentAndRating(trophyHuntData, allTrophyDropInfo, out allTrophyDropPercentStr, out allTrophyDropRatingStr);

                string dropWikiPercentStr = trophyHuntData.m_dropPercent.ToString();

                string text =
                    $"<size=16><b><color=#FFB75B>{trophyHuntData.m_prettyName}</color><b></size>\n" +
                    $"<color=white>Player Kills: </color><color=orange>{playerDropInfo.m_numKilled}</color>\n" +
                    $"<color=white>Trophies Picked Up: </color><color=orange>{playerDropInfo.m_trophies}</color>\n" +
                    $"<color=white>Kill/Pickup Rate: </color><color=orange>{playerDropPercentStr}%</color>\n" +
                    $"<color=white>Wiki Trophy Drop Rate: (<color=orange>{dropWikiPercentStr}%)</color>\n" +
                    $"<color=white>Player Luck Rating: <color=yellow>{playerDropRatingStr}</color>\n";

                if (__m_showAllTrophyStats)
                {
                    text = text +
                    $"<color=white>Actual Kills: </color><color=orange>{allTrophyDropInfo.m_numKilled}</color>\n" +
                    $"<color=white>Actual Trophies: </color><color=orange>{allTrophyDropInfo.m_trophies}</color>\n" +
                    $"<color=white>Actual Drop Rate: </color><color=orange>{allTrophyDropPercentStr}%</color> (<color=yellow>{dropWikiPercentStr}%)</color>\n" +
                    $"<color=white>Actual Luck Rating: <color=yellow>{allTrophyDropRatingStr}</color>\n";

                }
                return text;
            }

            public static void ShowTrophyTooltip(GameObject uiObject)
            {
                if (uiObject == null)
                    return;

                string text = BuildTrophyTooltipText(uiObject);

                __m_trophyTooltip.text = text;

                __m_trophyTooltipBackground.SetActive(true);
                __m_trophyTooltipObject.SetActive(true);

                Vector2 tooltipSize = __m_trophyTooltipWindowSize;
                if (__m_showAllTrophyStats)
                    tooltipSize = __m_trophyTooltipAllTrophyStatsWindowSize;

                Vector3 tooltipOffset = new Vector3(tooltipSize.x / 2, tooltipSize.y, 0);
                Vector3 mousePosition = Input.mousePosition;
                Vector3 desiredPosition = mousePosition + tooltipOffset;

                // Clamp the tooltip window onscreen
                if (desiredPosition.x < 0) desiredPosition.x = 0;
                if (desiredPosition.y < 0) desiredPosition.y = 0;
                if (desiredPosition.x > Screen.width - tooltipSize.x)
                    desiredPosition.x = Screen.width - tooltipSize.x;
                if (desiredPosition.y > Screen.height - tooltipSize.y)
                    desiredPosition.y = Screen.height - tooltipSize.y;

                __m_trophyTooltipBackground.transform.position = desiredPosition;
                __m_trophyTooltipObject.transform.position = new Vector3(desiredPosition.x + __m_trophyTooltipTextOffset.x, desiredPosition.y - __m_trophyTooltipTextOffset.y, 0f);
            }

            public static void HideTrophyTooltip()
            {
                __m_trophyTooltipBackground.SetActive(false);
                __m_trophyTooltipObject.SetActive(false);
            }

            #endregion

            public static bool CharacterCanDropTrophies(string characterName)
            {
                int index = Array.FindIndex(__m_trophyHuntData, element => element.m_enemies.Contains(characterName));
                if (index >= 0) return true;
                return false;
            }

            public static void RecordDroppedTrophy(string characterName, string trophyName)
            {
                DropInfo drop = __m_allTrophyDropInfo[trophyName];
                drop.m_trophies++;
                __m_allTrophyDropInfo[trophyName] = drop;
            }

            public static string EnemyNameToTrophyName(string enemyName)
            {
                int index = Array.FindIndex(__m_trophyHuntData, element => element.m_enemies.Contains(enemyName));
                if (index < 0) return "Not Found";

                return __m_trophyHuntData[index].m_name;
            }

            public static bool RecordPlayerPickedUpTrophy(string trophyName)
            {
                if (__m_playerTrophyDropInfo.ContainsKey(trophyName))
                {
                    DropInfo drop = __m_playerTrophyDropInfo[trophyName];
                    drop.m_trophies++;
                    __m_playerTrophyDropInfo[trophyName] = drop;

                    return true;
                }

                return false;
            }

            public static void RecordTrophyCapableKill(string characterName, bool killedByPlayer)
            {
                string trophyName = EnemyNameToTrophyName(characterName);

                if (killedByPlayer)
                {
                    Debug.Log($"{characterName} killed by Player");

                    DropInfo drop = __m_playerTrophyDropInfo[trophyName];
                    drop.m_numKilled++;

                    __m_playerTrophyDropInfo[trophyName] = drop;

                }
                else
                {
                    Debug.Log($"{characterName} killed not by Player");

                    DropInfo drop = __m_allTrophyDropInfo[trophyName];
                    drop.m_numKilled++;

                    __m_allTrophyDropInfo[trophyName] = drop;
                }
            }

            // Watch character drops and see what characters drop what items (actual dropped items)
            //
            [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
            class CharacterDrop_GenerateDropList_Patch
            {
                static void Postfix(CharacterDrop __instance, ref List<KeyValuePair<GameObject, int>> __result)
                {
                    if (!COLLECT_DROP_RATES)
                    {
                        return;
                    }

                    if (__instance != null)
                    {
                        Character character = __instance.GetComponent<Character>();

                        string characterName = character.m_name;

                        if (!CharacterCanDropTrophies(characterName))
                        {
                            return;
                        }

                        Debug.Log($"Trophy-capable character {characterName} has dropped items:");

                        RecordTrophyCapableKill(characterName, false);

                        bool droppedTrophy = false;

                        // Check if there are any dropped items
                        if (__result != null)
                        {
                            foreach (KeyValuePair<GameObject, int> droppedItem in __result)
                            {
                                // Get the item's name
                                string itemName = droppedItem.Key.name;

                                // Log or process the dropped item
                                Debug.Log($"Dropped item: {itemName} count: {droppedItem.Value}");

                                if (itemName.Contains("Trophy"))
                                {
                                    Debug.Log($"Trophy {itemName} Dropped by {characterName}");

                                    RecordDroppedTrophy(characterName, itemName);

                                    droppedTrophy = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Debug.Log($"Trophy-capable character {characterName} had null drop list");
                        }

//                        if (__m_trophyRushEnabled && !droppedTrophy)
                        if (!droppedTrophy)
                        {
                            float dropPercentage = 0f;

                            if (GetGameMode() == TrophyGameMode.TrophyRush)
                            {
                                dropPercentage = 100f;
                            }
                            else if (GetGameMode() == TrophyGameMode.TrophySaga)
                            {
                                int index = Array.FindIndex(__m_trophyHuntData, element => element.m_enemies.Contains(characterName));
                                if (index >= 0)
                                {
                                    float wikiDropPercent = __m_trophyHuntData[index].m_dropPercent;

                                    // Cap at 50% drop rate
                                    dropPercentage = Math.Min(wikiDropPercent * TROPHY_SAGA_DROP_MULTIPLIER, 50f);
                                }
                            }

                            // Roll the dice
                            System.Random randomizer = new System.Random();
                            float randValue = (float)randomizer.NextDouble() * 100f;

                            // If we rolled below drop percentage, drop a trophy
                            if (randValue < dropPercentage)
                            {
                                string trophyName = EnemyNameToTrophyName(characterName);

                                List<Drop> dropList = __instance.m_drops;

                                Drop trophyDrop = dropList.Find(theDrop => theDrop.m_prefab.name == trophyName);

                                if (trophyDrop != null)
                                {
                                    KeyValuePair<GameObject, int> newDropItem = new KeyValuePair<GameObject, int>(trophyDrop.m_prefab, 1);

                                    if (__result == null)
                                    {
                                        __result = new List<KeyValuePair<GameObject, int>>();
                                    }
                                    __result.Add(newDropItem);

                                    RecordDroppedTrophy(characterName, trophyName);
                                }
                            }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
            public class Character_OnDeath_Patch
            {
                static void Postfix(Character __instance)
                {
                    Character character = __instance;
                    // Check if the attacker is the local player
                    bool playerHit = false;
                    if (Player.m_localPlayer != null &&
                        character.m_lastHit != null &&
                        character.m_lastHit.GetAttacker() == Player.m_localPlayer)
                    {
                        playerHit = true;
                    }

                    if (playerHit)
                    {
                        // The local player killed this enemy
                        Debug.Log($"Player killed {__instance.name}");

                        string characterName = __instance.m_name;
                        if (CharacterCanDropTrophies(characterName))
                        {
                            Debug.Log($"Trophy-capable character {characterName} was killed by Player.");

                            RecordTrophyCapableKill(characterName, true);
                        }
                    }
                }
            }

            public static Dictionary<string, string> __m_metalConversions = new Dictionary<string, string>()
            {
                { "CopperOre",          "Copper" },
                { "TinOre",             "Tin" },
                { "IronScrap",          "Iron" },
                { "SilverOre",          "Silver" },
                { "BlackMetalScrap",    "BlackMetal" },
                { "FlametalOreNew",     "FlametalNew" },
                { "BronzeScrap",        "Bronze" },
                { "CopperScrap",        "Copper" }
            };

            public static void ConvertMetal(GameObject go)
            {
                ZNetScene zNetScene = ZNetScene.instance;
                if (zNetScene == null)
                {
                    return;
                }

                if (go == null)
                    return;


                ItemDrop itemDrop = go.GetComponent<ItemDrop>();
                if (itemDrop == null)
                    return;

                ItemDrop.ItemData item = itemDrop.m_itemData;

                string cookedMetalName;

                if (__m_metalConversions.TryGetValue(item.m_dropPrefab.name, out cookedMetalName))
                {
                    GameObject metalPrefab = zNetScene.GetPrefab(cookedMetalName);
                    if (metalPrefab != null)
                    {
                        Debug.LogWarning($"Found {metalPrefab.name} prefab");
                    }
                    
                    GameObject tempMetalObject = UnityEngine.Object.Instantiate<GameObject>(metalPrefab);
                    if (tempMetalObject)
                    {
                        Debug.LogWarning("Created new Copper game object");

                        ItemDrop tempItemDrop = tempMetalObject.GetComponent<ItemDrop>();
                        ItemDrop.ItemData copperItem = tempItemDrop.m_itemData;

                        int stackSize = itemDrop.m_itemData.m_stack;

                        // Replace the ore/scrap itemdata with the cooked metal itemdata
                        itemDrop.m_itemData = copperItem.Clone();
                        itemDrop.m_itemData.m_stack = stackSize;

//                        GameObject.Destroy(tempMetalObject);
                    }
                }
            }

            // Called when an item is added to the player's inventory
            [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.Pickup))]
            public class Humanoid_Pickup_Patch
            {
                // Used in Trophy Saga to auto-convert metals on pickup
                static void Prefix(Humanoid __instance, GameObject go, bool autoequip, bool autoPickupDelay, bool __result)
                {
                    // Before pickup occurs, see if it's auto-smeltable ore and convert it

                    // It's not a trophy pickup, see if it's a smeltable ore
                    if (GetGameMode() != TrophyGameMode.TrophySaga)
                    {
                        return;
                    }

                    if (__instance == null || __instance != Player.m_localPlayer)
                    {
                        return;
                    }

                    ItemDrop itemDrop = go.GetComponent<ItemDrop>();
                    if (itemDrop == null)
                        return;
                    
                    ConvertMetal(go);
                }

                // Check picked up item to see if Trophy
                static void Postfix(Humanoid __instance, GameObject go, bool autoequip, bool autoPickupDelay, bool __result)
                {

                    ItemDrop component = go.GetComponent<ItemDrop>();
                    ItemDrop.ItemData item = component.m_itemData;

                    if (__result && item != null && item.m_dropPrefab != null)
                    {
                        // Log the item name to the console when the player picks it up
                        // You can add further logic here to check the item type or trigger specific events
                        if (RecordPlayerPickedUpTrophy(item.m_dropPrefab.name))
                        {
                            return;
                        }
                    }
                }
            }
/*
            public static void AddShowAllTrophyStatsButton(Transform parentTransform)
            {
                // Clone the existing button
                GameObject trophyRushButton = new GameObject("TrophyRushButton");
                trophyRushButton.transform.SetParent(parentTransform);

                // The UI RectTransform for the button
                RectTransform rectTransform = trophyRushButton.AddComponent<RectTransform>();
                rectTransform.localScale = Vector3.one;
                rectTransform.anchorMin = new Vector2(1.0f, 0.0f);
                rectTransform.anchorMax = new Vector2(1.0f, 0.0f);
                rectTransform.pivot = new Vector2(1.0f, 0.0f);
                rectTransform.anchoredPosition = new Vector2(0, -50); // Position below the logo
                rectTransform.sizeDelta = new Vector2(200, 20);

                // Add the Button component
                UnityEngine.UI.Button button = trophyRushButton.AddComponent<UnityEngine.UI.Button>();

                ColorBlock cb = button.colors;
                cb.normalColor = Color.black;
                cb.highlightedColor = Color.yellow;  // When hovering
                cb.pressedColor = Color.red;      // When pressed
                cb.selectedColor = Color.white;   // When selected
                button.colors = cb;

                // Add an Image component for the button background
                UnityEngine.UI.Image image = trophyRushButton.AddComponent<UnityEngine.UI.Image>();
                image.color = Color.white; // Set background color

                // Create a sub-object for the text because the GameObject can't have an Image and a Text object
                GameObject textObject = new GameObject("ShowAllTrophyStatsButtonText");
                textObject.transform.SetParent(trophyRushButton.transform);

                // Set the Text RectTransform
                RectTransform textRect = textObject.AddComponent<RectTransform>();
                //                textRect.sizeDelta = new Vector2(130, 40);
                textRect.anchoredPosition = new Vector2(0, 0);

                // Change the button's text
                TextMeshProUGUI buttonText = textObject.AddComponent<TextMeshProUGUI>();
                buttonText.text = "<b>Show All TrophyStats<b>";
                buttonText.fontSize = 12;
                buttonText.color = Color.black;
                buttonText.alignment = TextAlignmentOptions.Center;

                // Set up the click listener
                button.onClick.AddListener(ShowAllTrophyStatsButtonClick);
            }

            public static void ShowAllTrophyStatsButtonClick()
            {
                ToggleShowAllTrophyStats();
            }
*/
            public static void AddToggleGameModeButton(Transform parentTransform)
            {
                // Clone the existing button
                GameObject trophyRushButton = new GameObject("ToggleGameModeButton");
                trophyRushButton.transform.SetParent(parentTransform);

                // The UI RectTransform for the button
                RectTransform rectTransform = trophyRushButton.AddComponent<RectTransform>();
                rectTransform.localScale = Vector3.one;
                rectTransform.anchorMin = new Vector2(1.0f, 0.0f);
                rectTransform.anchorMax = new Vector2(1.0f, 0.0f);
                rectTransform.pivot = new Vector2(1.0f, 0.0f);
                rectTransform.anchoredPosition = new Vector2(-80, -85); // Position below the logo
                rectTransform.sizeDelta = new Vector2(200, 25);

                // Add the Button component
                UnityEngine.UI.Button button = trophyRushButton.AddComponent<UnityEngine.UI.Button>();

                ColorBlock cb = button.colors;
                cb.normalColor = Color.black;
                cb.highlightedColor = Color.yellow;  // When hovering
                cb.pressedColor = Color.red;      // When pressed
                cb.selectedColor = Color.white;   // When selected
                button.colors = cb;

                Navigation nav = new Navigation();
                nav.mode = Navigation.Mode.None;
                button.navigation = nav;

                // Add an Image component for the button background
                UnityEngine.UI.Image image = trophyRushButton.AddComponent<UnityEngine.UI.Image>();
                image.color = Color.white; // Set background color

                // Create a sub-object for the text because the GameObject can't have an Image and a Text object
                GameObject textObject = new GameObject("ToggleGameModeButtonText");
                textObject.transform.SetParent(trophyRushButton.transform);

                // Set the Text RectTransform
                RectTransform textRect = textObject.AddComponent<RectTransform>();
                textRect.anchoredPosition = new Vector2(0, 0);

                // Change the button's text
                TextMeshProUGUI buttonText = textObject.AddComponent<TextMeshProUGUI>();
                buttonText.text = "<b>Toggle Game Mode<b>";
                buttonText.fontSize = 18;
                buttonText.color = Color.black;
                buttonText.alignment = TextAlignmentOptions.Center;
                buttonText.fontStyle = FontStyles.Bold;

                // Set up the click listener
                button.onClick.AddListener(TrophyRushButtonClick);
            }

            public static void TrophyRushButtonClick()
            {
                ToggleTrophyRush();
            }

            [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
            public class FejdStartup_Start_Patch
            {
                static void Postfix()
                {
                    Debug.LogError("Main Menu Start method called");

                    GameObject mainMenu = GameObject.Find("Menu");
                    if (mainMenu != null)
                    {
                        Transform logoTransform = mainMenu.transform.Find("Logo");
                        if (logoTransform != null)
                        {
                            GameObject textObject = new GameObject("TrophyHuntModLogoText");
                            textObject.transform.SetParent(logoTransform.parent);

                            // Set up the RectTransform for positioning
                            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
                            rectTransform.localScale = Vector3.one;
                            rectTransform.anchorMin = new Vector2(0.5f, 0.6f);
                            rectTransform.anchorMax = new Vector2(1.0f, 0.6f);
                            rectTransform.pivot = new Vector2(1.0f, 1.0f);
                            rectTransform.anchoredPosition = new Vector2(-20, 0); // Position below the logo
                            rectTransform.sizeDelta = new Vector2(-650, 185);

                            // Add a TextMeshProUGUI component
                            __m_trophyHuntMainMenuText = textObject.AddComponent<TextMeshProUGUI>();
                            __m_trophyHuntMainMenuText.text = GetTrophyHuntMainMenuText();
                            __m_trophyHuntMainMenuText.alignment = TextAlignmentOptions.Left;
                            // Enable outline
                            __m_trophyHuntMainMenuText.fontMaterial.EnableKeyword("OUTLINE_ON");
                            __m_trophyHuntMainMenuText.lineSpacingAdjustment = -5;
                            // Set outline color and thickness
                            __m_trophyHuntMainMenuText.outlineColor = Color.black;
                            __m_trophyHuntMainMenuText.outlineWidth = 0.05f; // Adjust the thickness

                            __m_trophyHuntMainMenuText.font = Resources.Load<TMP_FontAsset>("Valheim-AveriaSerifLibre");
                            __m_trophyHuntMainMenuText.fontStyle = FontStyles.Bold;

                            AddToggleGameModeButton(textObject.transform);

                            // Don't bother adding this button to the main menu, but keep the code around for new buttons
                            //
                            //AddShowAllTrophyStatsButton(textObject.transform);

                            // HACK
                            //GameObject copperPrefab = GameObject.Find("Copper");
                        }
                        else
                        {
                            Debug.LogWarning("Valheim logo not found!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Main menu not found!");
                    }
                }
            }
            

            // Oh, this is sketchy, but it seems to work.
            //
            // Patch the New World creation dialogue to poke in world defaults for trophy rush automatically

            [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.OnNewWorldDone), new[] {typeof(bool)})]
            public class FejdStartup_OnNewWorldDone_Patch
            { 
                static void Postfix(FejdStartup __instance, bool forceLocal)
                {
                    Debug.LogError("FejdStartup.OnNewWorldDone:");

                    if (FejdStartup.m_instance.m_world != null)
                    {
                        if (GetGameMode() == TrophyGameMode.TrophyRush)
                        {
                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Clear();

                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Add("playerdamage 70");
                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Add("enemydamage 200");
                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Add("enemyspeedsize 120");
                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Add("enemyleveluprate 140");
                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Add("resourcerate 200");
                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Add("preset combat_veryhard:deathpenalty_default: resources_muchmore: raids_default: portals_default");
                            FejdStartup.m_instance.m_world.SaveWorldMetaData(DateTime.Now);
                            __instance.UpdateWorldList(centerSelection: true);
                        }
                        else if (GetGameMode() == TrophyGameMode.TrophySaga)
                        {
                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Clear();

                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Add("playerdamage 85");
                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Add("enemydamage 150");
                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Add("enemyspeedsize 110");
                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Add("enemyleveluprate 120");
                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Add("resourcerate 150");
                            FejdStartup.m_instance.m_world.m_startingGlobalKeys.Add("preset combat_hard:deathpenalty_default: resources_more: raids_default: portals_default");
                            FejdStartup.m_instance.m_world.SaveWorldMetaData(DateTime.Now);
                            __instance.UpdateWorldList(centerSelection: true);
                        }
                    }
                }
            }

// Uncomment to inspect current world modifiers when hitting World Modifiers button
 /*           
                        [HarmonyPatch (typeof(FejdStartup), nameof(FejdStartup.OnServerOptions))]
                        public class ServerOptionsGUI_Initizalize_Patch
                        {
                            static void Postfix(FejdStartup __instance)
                            {
                                ServerOptionsGUI serverOptionsGUI = __instance.m_serverOptions;

                                Debug.LogError("OnServerOptions:");

                                foreach (KeyUI entry in ServerOptionsGUI.m_modifiers)
                                {
                                    Debug.LogWarning($"  KeyUI: {entry.ToString()}");
                                    if (entry.GetType() == typeof(KeySlider))
                                    {
                                        KeySlider slider = entry as KeySlider;


                                        Debug.LogWarning($"  {slider.m_modifier.ToString()}");

                                        foreach (KeySlider.SliderSetting setting in slider.m_settings)
                                        {
                                            Debug.LogWarning($"    {setting.m_name}, {setting.m_modifierValue.ToString()}");

                                            foreach(string key in setting.m_keys)
                                            {
                                                Debug.LogWarning($"      {key}");
                                            }
                                        }
                                    }
                                }

                                World world = FejdStartup.m_instance.m_world;
                                if (world != null)
                                {
                                    Debug.LogWarning("FejdStartup.m_instance.m_world.m_startingGlobalKeys");
                                    foreach (string key in world.m_startingGlobalKeys)
                                    {
                                        Debug.LogWarning($"  world key: {key}");
                                    }
                                }
                            }
                        }
*/            

// Future Trophy Fiesta patches
/*
            // Inventory Tracking
            [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), new[] { typeof(ItemDrop.ItemData) })]
            public static class Inventory_AddItem_Patch
            {
                static void Postfix(Inventory __instance, ItemDrop.ItemData item, bool __result)
                {
                    Debug.LogError($"Inventory.AddItem() attempt: {item.m_shared.m_name} ({item.m_dropPrefab.name})");

                    if (__instance != null && Player.m_localPlayer != null
                        && __instance == Player.m_localPlayer.GetInventory())
                    {
                        if (__result)
                        {
                            // Item successfully added to inventory
                            Debug.LogError($"Item added to inventory: {item.m_shared.m_name}");
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), new[] { typeof(ItemDrop.ItemData), typeof(int) })]
            public static class Inventory_RemoveItem_Patch
            {
                static void Postfix(Inventory __instance, ItemDrop.ItemData item, int amount, bool __result)
                {
                    if (__instance == Player.m_localPlayer.GetInventory())
                    {
                        if (__result)
                        {
                            // Item successfully removed from inventory
                            Debug.LogError($"Item removed from inventory: {item.m_shared.m_name} (Amount: {amount})");
                        }

                        Inventory playerInventory = Player.m_localPlayer.GetInventory();
                        bool hasItem = playerInventory.ContainsItem(item);

                        if (hasItem)
                        {
                            Debug.LogError($"Player has item: {item.m_shared.m_name}");
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(Inventory), nameof(Inventory.Changed))]
            public static class Inventory_Changed_Patch
            {
                static void Postfix(Inventory __instance)
                {
                    if (__instance != null && Player.m_localPlayer != null
                        && __instance == Player.m_localPlayer.GetInventory())
                    {
                        Debug.LogWarning($"Player Inventory Changed");
                        //foreach(ItemDrop.ItemData item in __instance.m_inventory)
                        //{
                        //    Debug.LogWarning($"  item: {item.m_shared.m_name} ({item.m_dropPrefab.name}) {item.m_stack}");
                        //}
                    }
                }
            }
*/

            // Catch /die console command to track it
            [HarmonyPatch(typeof(ConsoleCommand), nameof(ConsoleCommand.RunAction), new[] { typeof(ConsoleEventArgs)})]
            public static class ConsoleCommand_RunAction_Patch
            {
                static void Postfix(Inventory __instance, ConsoleEventArgs args)
                {
                    if (__instance != null)
                    {
                        if (args.Length > 0 && args[0] == "die")
                        {
                            __m_slashDieCount += 1;
                        }
                    }
                }
            }

            // Increase sailing speed
            //
            // Informed by "Sailing Speed" mod by Smoothbrain
            [HarmonyPatch(typeof(Ship), nameof(Ship.GetSailForce))]
            public class Ship_GetSailForce_Patch
            {
                static void Postfix(ref Vector3 __result)
                {
                    if (GetGameMode() == TrophyGameMode.TrophySaga)
                    {
                        __result *= TROPHY_SAGA_SAILING_SPEED_MULTIPLIER;
                    }
                }
            }

        }
    }
}


/*

Trophy Saga

* Trophy drop rate increased by 50%, capped at 50%
* All metal ores and scrap insta-smelt upon pickup
* All boats are twice as fast as normal
* Combat on Hard
* Resources at 1.5x

The goal would be to push through the slow points in the progression and encourage exploration and travel. Right now the first hour in Hunt and the first two hours in Rush seem to drag.

Any thoughts?   
Maybe also:

* No biome bonuses

? Two star enemies have a chance to drop Megingjord

*/



