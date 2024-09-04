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

namespace TrophyHuntMod
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    internal class TrophyHuntMod : BaseUnityPlugin
    {
        public const string PluginGUID = "com.oathorse.TrophyHuntMod";
        public const string PluginName = "TrophyHuntMod";
        public const string PluginVersion = "0.2.4";
        private readonly Harmony harmony = new Harmony(PluginGUID);

        // Configuration variables
        private const Boolean DUMP_TROPHY_DATA = false;
        private const Boolean UPDATE_LEADERBOARD = false;
        private const Boolean COLLECT_PLAYER_PATH = true;
        private const Boolean COLLECT_DROP_RATES = true;

        static TrophyHuntMod __m_trophyHuntMod;

        public enum Biome
        {
            Meadows = 0,
            Forest = 1,
            Ocean = 2,
            Swamp = 3,
            Mountains = 4,
            Plains = 5,
            Mistlands = 6,
            Ashlands = 7
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

        const int DEATH_PENALTY = 20;
        const int LOGOUT_PENALTY = 10;

        const float LOGOUT_PENALTY_GRACE_DISTANCE = 50.0f;  // total distance you're allowed to walk/run from initial spawn and get a free logout to clear wet debuff

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
            new TrophyHuntData("TrophyCultist_Hildir",          "Geirrhafa",        Biome.Mountains,    45,     100,    new List<string> { "$enemy_fenringcultist_hildir" }),
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
            new TrophyHuntData("TrophyGoblinBruteBrosBrute",    "Thungr",           Biome.Plains,       65,     100,    new List<string> { "$enemy_goblinbrute_hildircombined" }),
            new TrophyHuntData("TrophyGoblinBruteBrosShaman",   "Zil",              Biome.Plains,       65,     100,    new List<string> { "$enemy_goblin_hildir" }),
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
            new TrophyHuntData("TrophySkeletonHildir",          "Brenna",           Biome.Forest,       25,     100,    new List<string> { "$enemy_skeletonfire" }),
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
            new Color(0.1f, 0.1f, 0.2f, 0.3f),  // Biome.Ocean    
            new Color(0.2f, 0.1f, 0.0f, 0.3f),  // Biome.Swamp
            new Color(0.2f, 0.2f, 0.2f, 0.3f),  // Biome.Mountains
            new Color(0.2f, 0.2f, 0.0f, 0.3f),  // Biome.Plains 
            new Color(0.2f, 0.1f, 0.2f, 0.3f),  // Biome.Mistlands
            new Color(0.2f, 0.0f, 0.0f, 0.3f)   // Biome.Ashlands 
        };

        // UI Elements
        static GameObject __m_scoreTextElement = null;
        static GameObject __m_deathsTextElement = null;
        static List<GameObject> __m_iconList = null;

        static float __m_baseTrophyScale = 1.4f;
        static float __m_userTrophyScale = 1.0f;

        // TrophyHuntData list
        //        static List<string> __m_trophiesInObjectDB = new List<string>();

        // Cache for detecting newly arrived trophies and flashing the new ones
        static List<string> __m_trophyCache = new List<string>();

        // Death counter
        static int __m_deaths = 0;
        static int __m_logoutCount = 0;

        // Player Path
        static bool __m_pathAddedToMinimap = false;                                // are we showing the path on the minimap? 
        static List<Minimap.PinData> __m_pathPins = new List<Minimap.PinData>();   // keep track of the special pins we add to the minimap so we can remove them
        static List<Vector3> __m_playerPathData = new List<Vector3>();   // list of player positions during the session
        static bool __m_collectingPlayerPath = false;                           // are we actively asynchronously collecting the player position?
        static float __m_playerPathCollectionInterval = 8.0f;                   // seconds between checks to see if we can store the current player position
        static float __m_minPathPlayerMoveDistance = 50.0f;                     // the min distance the player has to have moved to consider storing the new path position
        static Vector3 __m_previousPlayerPos;                                   // last player position stored

        static bool __m_onlyModRunning = false;

        static bool __m_trophyRushEnabled = false;
        
        public struct DropInfo
        {
            public DropInfo()
            {
                m_numKilled = 0;
                m_trophiesDropped = 0;
            }

            public int m_numKilled = 0;
            public int m_trophiesDropped = 0;
        }

        static Dictionary<string, DropInfo> __m_trophyDropInfo = new Dictionary<string, DropInfo>();


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
                __m_trophyDropInfo.Clear();
                foreach (TrophyHuntData td in __m_trophyHuntData)
                {
                    __m_trophyDropInfo.Add(td.m_name, new DropInfo());
                }
            }
        }

        // New Console Commands for TrophyHuntMod
        #region Console Commands
        void PrintToConsole(string message)
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

                int score = 0;
                PrintToConsole($"Trophies:");
                foreach (TrophyHuntData thData in __m_trophyHuntData)
                {
                    if (__m_trophyCache.Contains(thData.m_name))
                    {
                        PrintToConsole($"  {thData.m_name}: Score: {thData.m_value} Biome: {thData.m_biome.ToString()}");
                        score += thData.m_value;
                    }
                }
                PrintToConsole($"Trophy Score Total: {score}");

                int deathScore = __m_deaths * DEATH_PENALTY * -1;
                int logoutScore = __m_logoutCount * LOGOUT_PENALTY * -1;

                PrintToConsole($"Penalties:");
                PrintToConsole($"  Deaths: {__m_deaths} Score: {deathScore}");
                PrintToConsole($"  Logouts: {__m_logoutCount} Score: {logoutScore}");

                score += deathScore;
                score += logoutScore;

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

            ConsoleCommand trophyRush = new ConsoleCommand("trophyrush", "Toggle Trophy Rush Mode on and off", delegate (ConsoleEventArgs args)
            {
                if (Game.instance)
                {
                    PrintToConsole("'/trophyrush' console command can only be used at the main menu via the F5 console.");
                    return;
                }

                __m_trophyRushEnabled = !__m_trophyRushEnabled;

                string enabledStr = "OFF";
                if (__m_trophyRushEnabled)
                {
                    enabledStr = "ON";
                }
                PrintToConsole($"Trophy Rush mode is set to {enabledStr}!");
                if (__m_trophyHuntMainMenuText != null)
                {
                    __m_trophyHuntMainMenuText.text = GetTrophyHuntMainMenuText();
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
                    Debug.Log(args);

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
        }

        public static TextMeshProUGUI __m_trophyHuntMainMenuText = null;

        public static string GetTrophyHuntMainMenuText()
        {
            string textStr = $"<b><size=52><color=#FFB75B>TrophyHuntMod</color></size></b>\n<size=24>Version {PluginVersion}</size>";

            if (__m_trophyRushEnabled)
            {
                textStr += "\n<size=30><color=red>Trophy Rush Enabled!</color></size>";
            }

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

                // Pull the list of trophies from the ObjectDB
                //
                //__m_trophiesInObjectDB.Clear();
                //foreach (GameObject item in ObjectDB.m_instance.m_items)
                //{
                //    ItemDrop component = item.GetComponent<ItemDrop>();

                //    if (component != null && component.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophy)
                //    {
                //        __m_trophiesInObjectDB.Add(component.gameObject.name);
                //    }
                //}
                //Debug.Log($"{__m_trophiesInObjectDB.Count} trophies discovered");
                //if (__m_trophiesInObjectDB.Count != __m_trophyHuntData.Length)
                //{
                //    Debug.LogError($"Valheim's list of Trophies ({__m_trophiesInObjectDB.Count}) doesn't match the mod's Trophy data ({__m_trophyHuntData.Length}), this mod is out of date.");
                //}

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

                // Create all the UI elements we need for this mod
                BuildUIElements();

                // Do initial update of all UI elements to the current state of the game
                UpdateTrophyHuntUI(Player.m_localPlayer);

                // Until the player has moved 10 meters, ignore logouts. This is a hack
                // to get around switching players and accounting for logouts in case the 
                // user was playing another character before starting the trophy hunt run
                //
                if (GetTotalOnFootDistance(Game.instance) < 10.0f)
                {
                    __m_logoutCount = 0;
                }

                Debug.LogWarning($"Total Logouts: {__m_logoutCount}");

                string workingDirectory = Directory.GetCurrentDirectory();
                Debug.Log($"Working Directory for Trophy Hunt Mod: {workingDirectory}");
                Debug.Log($"Steam username: {SteamFriends.GetPersonaName()}");

                // Start collecting player position map pin data
                if (COLLECT_PLAYER_PATH)
                {
                    ShowPlayerPath(false);
                    StopCollectingPlayerPath();
                    StartCollectingPlayerPath();
                }
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

                    __m_iconList = new List<GameObject>();
                    CreateTrophyIconElements(healthPanelTransform, __m_trophyHuntData, __m_iconList);

                    // Create the hover text object
                    if (COLLECT_DROP_RATES)
                    {
                        CreateDropRateTooltip();
                    }
                }
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
                rectTransform.sizeDelta = new Vector2(40, 40);
                rectTransform.anchoredPosition = new Vector2(-75, -75); // Set position

                // Add an Image component
                UnityEngine.UI.Image image = skullElement.AddComponent<UnityEngine.UI.Image>();
                image.sprite = skullSprite;
                image.color = Color.white;
                image.raycastTarget = false;

                GameObject deathsTextElement = new GameObject("DeathsText");
                deathsTextElement.transform.SetParent(parentTransform);

                RectTransform deathsTextTransform = deathsTextElement.AddComponent<RectTransform>();
                deathsTextTransform.sizeDelta = new Vector2(40, 40);
                deathsTextTransform.anchoredPosition = new Vector2(-60, -100); // Set position

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
                // Create a new GameObject for the text
                GameObject scoreTextElement = new GameObject("ScoreText");

                // Set the parent to the HUD canvas
                scoreTextElement.transform.SetParent(parentTransform);

                // Add RectTransform component for positioning
                RectTransform rectTransform = scoreTextElement.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(200, 50); // Set size
                rectTransform.anchoredPosition = new Vector2(-65, -140); // Set position

                int scoreValue = 9999;

                TMPro.TextMeshProUGUI tmText = scoreTextElement.AddComponent<TMPro.TextMeshProUGUI>();

                tmText.text = $"{scoreValue}";
                tmText.fontSize = 28;
                tmText.color = Color.yellow;
                tmText.alignment = TextAlignmentOptions.Center;
                tmText.raycastTarget = false;

                if (!__m_onlyModRunning)
                {
                    tmText.color = Color.cyan;
                }

                return scoreTextElement;
            }
            static void CreateTrophyIconElements(Transform parentTransform, TrophyHuntData[] trophies, List<GameObject> iconList)
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
            }

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
                iconRectTransform.anchoredPosition = new Vector2(xOffset + index * (iconSize + iconBorderSize), yOffset); // Set position
                iconRectTransform.localScale = new Vector3(__m_baseTrophyScale, __m_baseTrophyScale, __m_baseTrophyScale) * __m_userTrophyScale;

                // Add an Image component for Sprite
                UnityEngine.UI.Image iconImage = iconElement.AddComponent<UnityEngine.UI.Image>();
                iconImage.sprite = iconSprite;
                iconImage.color = Color.black;
                iconImage.raycastTarget = true;

                if (__m_trophyRushEnabled)
                {
                    iconImage.color = new Color(0.5f, 0.0f, 0.0f);
                }

                AddHoverTextTriggersToUIObject(iconElement);

                return iconElement;
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

                List<string> discoveredTrophies = player.GetTrophies();

                //Debug.Log($"__m_trophyData     length={__m_trophyData.Length}");
                //Debug.Log($"discoveredTrophies length={discoveredTrophies.Count}");
                //Debug.Log($"__m_iconList       length={__m_iconList.Count}");

                // Compute Score and enable all TrophyHuntIcons for trophies that have been discovered
                //
                int score = 0;
                foreach (string trophyName in discoveredTrophies)
                {
                    TrophyHuntData trophyHuntData = Array.Find(__m_trophyHuntData, element => element.m_name == trophyName);

                    if (trophyHuntData.m_name == trophyName)
                    {
                        // Add the value to our score
                        score += trophyHuntData.m_value;

                        EnableTrophyHuntIcon(trophyName);
                    }
                }

                // Update the deaths text and subtract deaths from score
                //
                PlayerProfile profile = Game.instance.GetPlayerProfile();
                if (profile != null)
                {
                    PlayerProfile.PlayerStats stats = profile.m_playerStats;
                    if (stats != null)
                    {
                        __m_deaths = (int)stats[PlayerStatType.Deaths];

                        //                            Debug.LogWarning($"Subtracing score for {__m_deaths} deaths.");
                        score -= __m_deaths * DEATH_PENALTY;

                        // Update the UI element
                        TMPro.TextMeshProUGUI deathsText = __m_deathsTextElement.GetComponent<TMPro.TextMeshProUGUI>();
                        if (deathsText != null)
                        {
                            deathsText.SetText(__m_deaths.ToString());
                        }
                    }
                }

                // Subtract points for logouts
                score -= __m_logoutCount * LOGOUT_PENALTY;

                // Update the Score string
                __m_scoreTextElement.GetComponent<TMPro.TextMeshProUGUI>().text = score.ToString();

                if (UPDATE_LEADERBOARD)
                {
                    // Send the score to the web page
                    SendScoreToLeaderboard(score);
                }
            }

            static IEnumerator FlashImage(UnityEngine.UI.Image targetImage, RectTransform imageRect)
            {
                float flashDuration = 0.5f;
                int numFlashes = 6;

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
                            targetImage.color = Color.black;
                        }

                        float flashScale = 1 + interpValue;
                        imageRect.localScale = new Vector3(__m_baseTrophyScale, __m_baseTrophyScale, __m_baseTrophyScale) * flashScale * __m_userTrophyScale;

                        yield return null;
                    }
                }

                targetImage.color = Color.white;
                imageRect.localScale = originalScale;
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
            }

            private static void SendScoreToLeaderboard(int score)
            {
                string steamName = SteamFriends.GetPersonaName();
                int seed = WorldGenerator.instance.GetSeed();
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
                    logouts = __m_logoutCount
                };

                // Start the coroutine to post the data
                __m_trophyHuntMod.StartCoroutine(PostLeaderboardDataCoroutine("https://oathorse.pythonanywhere.com/submit", leaderboardData));
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

                    __m_logoutCount++;
                }
            }

            #endregion

            static GameObject __m_tooltipObject = null;
            static GameObject __m_tooltipBackground = null;
            static TextMeshProUGUI __m_hoverText;
            static Vector2 __m_tooltipWindowSize = new Vector2(200, 80);
            static Vector2 __m_tooltipTextOffset = new Vector2(5, 2);

            public static void CreateDropRateTooltip()
            {
                Debug.LogWarning("Creating HoverText object");


                // Tooltip Background
                __m_tooltipBackground = new GameObject("Tooltip Background");

                // Set %the parent to the HUD
                Transform hudrootTransform = Hud.instance.transform;
                __m_tooltipBackground.transform.SetParent(hudrootTransform, false);

                RectTransform bgTransform = __m_tooltipBackground.AddComponent<RectTransform>();
                bgTransform.sizeDelta = __m_tooltipWindowSize;

                // Add an Image component for the background
                UnityEngine.UI.Image backgroundImage = __m_tooltipBackground.AddComponent<UnityEngine.UI.Image>();
                backgroundImage.color = new Color(0, 0, 0, 0.75f); // Semi-transparent black background

                __m_tooltipBackground.SetActive(false);

                // Create a new GameObject for the tooltip
                __m_tooltipObject = new GameObject("Tooltip Text");
                __m_tooltipObject.transform.SetParent(__m_tooltipBackground.transform, false);

                // Add a RectTransform component for positioning
                RectTransform rectTransform = __m_tooltipObject.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(__m_tooltipWindowSize.x - __m_tooltipTextOffset.x, __m_tooltipWindowSize.y - __m_tooltipTextOffset.y);

                // Add a TextMeshProUGUI component for displaying the tooltip text
                __m_hoverText = __m_tooltipObject.AddComponent<TextMeshProUGUI>();
                __m_hoverText.fontSize = 14;
                __m_hoverText.alignment = TextAlignmentOptions.TopLeft;
                __m_hoverText.color = Color.yellow;

                // Initially hide the tooltip
                __m_tooltipObject.SetActive(false);
            }

            public static void AddHoverTextTriggersToUIObject(GameObject uiObject)
            {
                if (!COLLECT_DROP_RATES)
                {
                    return;
                }

                // Add EventTrigger component if not already present
                EventTrigger trigger = uiObject.GetComponent<EventTrigger>();
                if (trigger == null)
                {
                    trigger = uiObject.AddComponent<EventTrigger>();
                }

                // Mouse Enter event (pointer enters the icon area)
                EventTrigger.Entry entryEnter = new EventTrigger.Entry();
                entryEnter.eventID = EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((eventData) => ShowTooltip(uiObject));
                trigger.triggers.Add(entryEnter);

                // Mouse Exit event (pointer exits the icon area)
                EventTrigger.Entry entryExit = new EventTrigger.Entry();
                entryExit.eventID = EventTriggerType.PointerExit;
                entryExit.callback.AddListener((eventData) => HideTooltip());
                trigger.triggers.Add(entryExit);
            }

            public static string BuildTooltipText(GameObject uiObject)
            {
                if (uiObject == null)
                {
                    return "Invalid";
                }

                string trophyName = uiObject.name;

                TrophyHuntData trophyHuntData = Array.Find(__m_trophyHuntData, element => element.m_name == trophyName);

                DropInfo dropInfo = __m_trophyDropInfo[trophyName];

                string dropPercentStr = "<n/a>";
                if (dropInfo.m_numKilled > 0)
                {
                    dropPercentStr = (100.0f * (float)dropInfo.m_trophiesDropped / (float)dropInfo.m_numKilled).ToString("0.0"); ;
                }

                string dropWikiPercentStr = trophyHuntData.m_dropPercent.ToString();

                string text = 
                    $"<size=16><b><color=#FFB75B>{trophyHuntData.m_prettyName}</color><b></size>\n" +
                    $"<color=white>Kills: </color><color=orange>{dropInfo.m_numKilled}</color>\n" +
                    $"<color=white>Trophies: </color><color=orange>{dropInfo.m_trophiesDropped}</color>\n" +
                    $"<color=white>Drop Rate: </color><color=orange>{dropPercentStr}%</color> (<color=yellow>{dropWikiPercentStr}%)</color>";

                return text;
            }

            public static void ShowTooltip(GameObject uiObject)
            {
                if (uiObject == null)
                    return;

                string text = BuildTooltipText(uiObject);
                
                __m_hoverText.text = text;

                __m_tooltipBackground.SetActive(true);
                __m_tooltipObject.SetActive(true);

                Vector3 tooltipOffset = new Vector3(__m_tooltipWindowSize.x / 2, __m_tooltipWindowSize.y, 0);
                Vector3 mousePosition = Input.mousePosition;
                Vector3 desiredPosition = mousePosition + tooltipOffset;
                
                // Clamp the tooltip window onscreen
                if (desiredPosition.x < 0) desiredPosition.x = 0;
                if (desiredPosition.y < 0) desiredPosition.y = 0;
                if (desiredPosition.x > Screen.width - __m_tooltipWindowSize.x)
                    desiredPosition.x = Screen.width - __m_tooltipWindowSize.x;
                if (desiredPosition.y > Screen.height - __m_tooltipWindowSize.y)
                    desiredPosition.y = Screen.height - __m_tooltipWindowSize.y;

                __m_tooltipBackground.transform.position = desiredPosition;
                __m_tooltipObject.transform.position = new Vector3(desiredPosition.x + __m_tooltipTextOffset.x, desiredPosition.y - __m_tooltipTextOffset.y, 0f);
            }

            public static void HideTooltip()
            {
                __m_tooltipBackground.SetActive(false);
                __m_tooltipObject.SetActive(false);
            }

            public static bool CharacterCanDropTrophies(string characterName)
            {
                int index = Array.FindIndex(__m_trophyHuntData, element => element.m_enemies.Contains(characterName));
                if (index >= 0) return true;
                return false;
            }

            public static void RecordDroppedTrophy(string characterName, string trophyName)
            {
                DropInfo drop = __m_trophyDropInfo[trophyName];
                drop.m_trophiesDropped++;
                __m_trophyDropInfo[trophyName] = drop;
            }
            public static string EnemyNameToTrophyName(string enemyName)
            {
                int index = Array.FindIndex(__m_trophyHuntData, element => element.m_enemies.Contains(enemyName));
                if (index < 0) return "Not Found";

                return __m_trophyHuntData[index].m_name;
            }

            public static void RecordTrophyCapableKill(string characterName)
            {
                string trophyName = EnemyNameToTrophyName(characterName);
                DropInfo drop = __m_trophyDropInfo[trophyName];
                drop.m_numKilled++;
                __m_trophyDropInfo[trophyName] = drop;
            }

            // Watch character drops and see what characters drop what items (actual dropped items)
            //
            [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
            class CharacterDrop_GenerateDropList_Patch
            {
                static void Postfix(CharacterDrop __instance, List<KeyValuePair<GameObject, int>> __result)
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

                        RecordTrophyCapableKill(characterName);

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
                                    
                                    break;
                                }
                                else
                                {
                                    if (__m_trophyRushEnabled)
                                    {
                                        string trophyName = EnemyNameToTrophyName(characterName);
                                        GameObject trophyObject = GameObject.Find(trophyName);
                                        if (trophyObject)
                                        {
                                            KeyValuePair<GameObject, int> trophyDrop = new KeyValuePair<GameObject, int>(trophyObject, 1);
                                            __result.Add(trophyDrop);

                                            RecordDroppedTrophy(characterName, trophyName);

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
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
                            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                            rectTransform.pivot = new Vector2(0.5f, 0.5f);
                            rectTransform.anchoredPosition = new Vector2(0, 0); // Position below the logo
                            rectTransform.sizeDelta = new Vector2(500, 200);
                            
                            // Add a TextMeshProUGUI component
                            __m_trophyHuntMainMenuText = textObject.AddComponent<TextMeshProUGUI>();
                            __m_trophyHuntMainMenuText.text = GetTrophyHuntMainMenuText();
//                            __m_trophyHuntMainMenuText.fontSize = 52;
                            __m_trophyHuntMainMenuText.alignment = TextAlignmentOptions.Center;
                            // Enable outline
                            __m_trophyHuntMainMenuText.fontMaterial.EnableKeyword("OUTLINE_ON");
                            __m_trophyHuntMainMenuText.lineSpacingAdjustment = -5;
                            // Set outline color and thickness
                            __m_trophyHuntMainMenuText.outlineColor = Color.black;
                            __m_trophyHuntMainMenuText.outlineWidth = 0.05f; // Adjust the thickness

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
        }
    }
}




