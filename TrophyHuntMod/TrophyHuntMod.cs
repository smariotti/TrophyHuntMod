using BepInEx;
//using Jotunn.Entities;
//using Jotunn.Managers;
//using Jotunn.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using System.Configuration;
using System.Security.Policy;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using UnityEngine.UIElements;
using System.Collections;

namespace TrophyHuntMod
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    internal class TrophyHuntMod : BaseUnityPlugin
    {
        public const string PluginGUID = "com.oathorse.TrophyHuntMod";
        public const string PluginName = "TrophyHuntMod";
        public const string PluginVersion = "0.0.5";
        private readonly Harmony harmony = new Harmony(PluginGUID);

        private const Boolean DUMP_TROPHY_DATA = false;

        static TrophyHuntMod __m_trophyHuntMod;

        private void Awake()
        {
            Debug.LogWarning("TrophyHuntMod has landed");

            __m_trophyHuntMod = this;

            // Patch with Harmony
            harmony.PatchAll();
        }

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
            public TrophyHuntData(string name, Biome biome, int value)
            {
                m_name = name;
                m_biome = biome;
                m_value = value;
            }

            public string m_name;
            public Biome m_biome;
            public int m_value;
        }

        const int DEATH_PENALTY = 20;

        //
        // Trophy Scores updated from Discord chat 08/18/24
        // Archy:
        //  *eik/elder/bonemass/moder/yag     -   40/60/80/100/120 pts 
        //  *hildir bosses trophies respectively -   25/45/65 pts
        //

        static public TrophyHuntData[] __m_trophyHuntData = new TrophyHuntData[]
        {
            new TrophyHuntData("TrophyAbomination", Biome.Swamp, 20),
            new TrophyHuntData("TrophyAsksvin", Biome.Ashlands, 50),
            new TrophyHuntData("TrophyBlob", Biome.Swamp, 20),
            new TrophyHuntData("TrophyBoar", Biome.Meadows, 10),
            new TrophyHuntData("TrophyBonemass", Biome.Swamp, 80),
            new TrophyHuntData("TrophyBonemawSerpent", Biome.Ashlands, 50),
            new TrophyHuntData("TrophyCharredArcher", Biome.Ashlands, 50),
            new TrophyHuntData("TrophyCharredMage", Biome.Ashlands, 50),
            new TrophyHuntData("TrophyCharredMelee", Biome.Ashlands, 50),
            new TrophyHuntData("TrophyCultist", Biome.Mountains, 30),
            new TrophyHuntData("TrophyCultist_Hildir", Biome.Mountains, 45),
            new TrophyHuntData("TrophyDeathsquito", Biome.Plains, 30),
            new TrophyHuntData("TrophyDeer", Biome.Meadows, 10),
            new TrophyHuntData("TrophyDragonQueen", Biome.Mountains, 100),
            new TrophyHuntData("TrophyDraugr", Biome.Swamp, 20),
            new TrophyHuntData("TrophyDraugrElite", Biome.Swamp, 20),
            new TrophyHuntData("TrophyDraugrFem", Biome.Swamp, 20),
            new TrophyHuntData("TrophyDvergr", Biome.Mistlands, 40),
            new TrophyHuntData("TrophyEikthyr", Biome.Meadows, 40),
            new TrophyHuntData("TrophyFader", Biome.Ashlands, 1000),
            new TrophyHuntData("TrophyFallenValkyrie", Biome.Ashlands, 50),
            new TrophyHuntData("TrophyFenring", Biome.Mountains, 30),
            new TrophyHuntData("TrophyForestTroll", Biome.Forest, 20),
            new TrophyHuntData("TrophyFrostTroll", Biome.Forest, 20),
            new TrophyHuntData("TrophyGjall", Biome.Mistlands, 40),
            new TrophyHuntData("TrophyGoblin", Biome.Plains, 30),
            new TrophyHuntData("TrophyGoblinBrute", Biome.Plains, 30),
            new TrophyHuntData("TrophyGoblinBruteBrosBrute", Biome.Plains, 65),
            new TrophyHuntData("TrophyGoblinBruteBrosShaman", Biome.Plains, 65),
            new TrophyHuntData("TrophyGoblinKing", Biome.Plains, 120),
            new TrophyHuntData("TrophyGoblinShaman", Biome.Plains, 30),
            new TrophyHuntData("TrophyGreydwarf", Biome.Forest, 20),
            new TrophyHuntData("TrophyGreydwarfBrute", Biome.Forest, 20),
            new TrophyHuntData("TrophyGreydwarfShaman", Biome.Forest, 20),
            new TrophyHuntData("TrophyGrowth", Biome.Plains, 30),
            new TrophyHuntData("TrophyHare", Biome.Mistlands, 40),
            new TrophyHuntData("TrophyHatchling", Biome.Mountains, 30),
            new TrophyHuntData("TrophyLeech", Biome.Swamp, 20),
            new TrophyHuntData("TrophyLox", Biome.Plains, 30),
            new TrophyHuntData("TrophyMorgen", Biome.Ashlands, 50),
            new TrophyHuntData("TrophyNeck", Biome.Meadows, 10),
            new TrophyHuntData("TrophySeeker", Biome.Mistlands, 40),
            new TrophyHuntData("TrophySeekerBrute", Biome.Mistlands, 40),
            new TrophyHuntData("TrophySeekerQueen", Biome.Mistlands, 1000),
            new TrophyHuntData("TrophySerpent", Biome.Ocean, 25),
            new TrophyHuntData("TrophySGolem", Biome.Mountains, 30),
            new TrophyHuntData("TrophySkeleton", Biome.Forest, 20),
            new TrophyHuntData("TrophySkeletonHildir", Biome.Forest, 25),
            new TrophyHuntData("TrophySkeletonPoison", Biome.Forest, 20),
            new TrophyHuntData("TrophySurtling", Biome.Swamp, 20),
            new TrophyHuntData("TrophyTheElder", Biome.Forest, 60),
            new TrophyHuntData("TrophyTick", Biome.Mistlands, 40),
            new TrophyHuntData("TrophyUlv", Biome.Mountains, 30),
            new TrophyHuntData("TrophyVolture", Biome.Ashlands, 50),
            new TrophyHuntData("TrophyWolf", Biome.Mountains, 30),
            new TrophyHuntData("TrophyWraith", Biome.Swamp, 20)
        };

        static public Color[] __m_biomeColors = new Color[]
        {
            new Color(0.2f, 0.2f, 0.1f, 0.3f),  // Biome.Meadows
            new Color(0.0f, 0.2f, 0.0f, 0.3f),  // Biome.Forest   
            new Color(0.1f, 0.1f, 0.2f, 0.3f),  // Biome.Ocean    
            new Color(0.2f, 0.0f, 0.0f, 0.3f),  // Biome.Swamp
            new Color(0.2f, 0.2f, 0.2f, 0.3f),  // Biome.Mountains
            new Color(0.2f, 0.2f, 0.0f, 0.3f),  // Biome.Plains 
            new Color(0.1f, 0.2f, 0.1f, 0.3f),  // Biome.Mistlands
            new Color(0.2f, 0.0f, 0.0f, 0.3f)   // Biome.Ashlands 
        };

        // UI Elements
        static GameObject __m_scoreTextElement = null;
        static GameObject __m_deathsTextElement = null;
        static GameObject __m_trophyTrayElement = null;
        static List<GameObject> __m_iconList = null;

        // TrophyHuntData list
        static List<string> __m_trophiesInObjectDB = new List<string>();
        
        // Cache for detecting newly arrived trophies and flashing the new ones
        static List<string> __m_trophyCache = new List<string>();

        // Death counter
        static int __m_deaths = 0;

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
                __m_trophiesInObjectDB.Clear();
                foreach (GameObject item in ObjectDB.m_instance.m_items)
                {
                    ItemDrop component = item.GetComponent<ItemDrop>();

                    if (component != null && component.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophy)
                    {
                        __m_trophiesInObjectDB.Add(component.gameObject.name);
                    }
                }
                Debug.Log($"{__m_trophiesInObjectDB.Count} trophies discovered");

                if (__m_trophiesInObjectDB.Count != __m_trophyHuntData.Length)
                {
                    Debug.LogError($"Valheim's list of Trophies ({__m_trophiesInObjectDB.Count}) doesn't match the mod's Trophy data ({__m_trophyHuntData.Length}), this mod is out of date.");
                }

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
            }

            static void BuildUIElements()
            {
                if (Hud.instance == null || Hud.instance.m_rootObject == null)
                {
                    Debug.LogError("TrophyHuntMod: Hud.instance.m_rootObject is NOT valid");

                    return;
                }

                if (__m_deathsTextElement == null && __m_scoreTextElement == null && __m_trophyTrayElement == null)
                {
                    Transform healthPanelTransform = Hud.instance.transform.Find("hudroot/healthpanel");
                    if (healthPanelTransform == null)
                    {
                        Debug.LogError("Health panel transform not found.");

                        return;
                    }

                    // TODO: do something with trophyTray? like parent everything under it?
                    //if (__m_trophyTrayElement == null)
                    //{
                    //    __m_trophyTrayElement = CreateTrophyTrayElement(healthPanelTransform);
                    //}

                    if (__m_scoreTextElement == null)
                    {
                        __m_scoreTextElement = CreateScoreTextElement(healthPanelTransform);
                    }

                    if (__m_deathsTextElement == null)
                    {
                        __m_deathsTextElement = CreateDeathsElement(healthPanelTransform);
                    }

                    if (__m_iconList == null)
                    {
                        __m_iconList = new List<GameObject>();
                        CreateTrophyIconElements(healthPanelTransform, __m_trophyHuntData, __m_iconList);
                    }
                }
            }
            static GameObject CreateTrophyTrayElement(Transform parentTransform)
            {
                GameObject trophyTray = new GameObject("TrophyTray");

                trophyTray.transform.SetParent(parentTransform);
                RectTransform trophyTrayRectTransform = trophyTray.AddComponent<RectTransform>();
                trophyTrayRectTransform.sizeDelta = new Vector2(3600, 40);
                trophyTrayRectTransform.anchoredPosition = new Vector2(500, -140);

                UnityEngine.UI.Image trayImage = trophyTray.AddComponent<UnityEngine.UI.Image>();
                trayImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
                trayImage.raycastTarget = false;

                return trophyTray;
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
                rectTransform.anchoredPosition = new Vector2(-80, -80); // Set position

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

                    GameObject iconElement = CreateIconElement(parentTransform, trophySprite, trophy.m_name, trophy.m_biome, iconList.Count);
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

            static GameObject CreateIconElement(Transform parentTransform, Sprite iconSprite, string iconName, Biome iconBiome, int index)
            {

                int iconSize = 33;
                int iconBorderSize = 0;
                int xOffset = -20;
                int yOffset = -140;

                int biomeIndex = (int)iconBiome;
                Color backgroundColor = __m_biomeColors[biomeIndex];

                // Create a new GameObject for the icon background
                GameObject iconBackgroundElement = new GameObject(iconName);
                iconBackgroundElement.transform.SetParent(parentTransform);

                // AddRectTransform for sprite background
                RectTransform iconBackgroundRectTransform = iconBackgroundElement.AddComponent<RectTransform>();
                iconBackgroundRectTransform.sizeDelta = new Vector2(iconSize, iconSize); // Set size
                iconBackgroundRectTransform.anchoredPosition = new Vector2(xOffset + index * (iconSize + iconBorderSize), yOffset); // Set position

                UnityEngine.UI.Image iconBackgroundImage = iconBackgroundElement.AddComponent<UnityEngine.UI.Image>();
                iconBackgroundImage.color = backgroundColor;

                iconBackgroundImage.raycastTarget = false;

                // Create a new GameObject for the icon
                GameObject iconElement = new GameObject(iconName);
                iconElement.transform.SetParent(parentTransform);

                // Add RectTransform component for positioning Sprite
                RectTransform iconRectTransform = iconElement.AddComponent<RectTransform>();
                iconRectTransform.sizeDelta = new Vector2(iconSize, iconSize); // Set size
                iconRectTransform.anchoredPosition = new Vector2(xOffset + index * (iconSize + iconBorderSize), yOffset); // Set position

                // Add an Image component for Sprite
                UnityEngine.UI.Image iconImage = iconElement.AddComponent<UnityEngine.UI.Image>();
                iconImage.sprite = iconSprite;
                iconImage.color = Color.black;
                iconImage.raycastTarget = false;

                return iconElement;
            }

            static void EnableTrophyHuntIcon(string trophyName)
            {
                // Find the UI element and bold it
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
                        int __m_deaths = (int)stats[PlayerStatType.Deaths];

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

                // Update the Score string
                __m_scoreTextElement.GetComponent<TMPro.TextMeshProUGUI>().text = score.ToString();
            }

            static IEnumerator FlashImage(UnityEngine.UI.Image targetImage, RectTransform imageRect)
            {
                float flashDuration = 0.3f;
                int numFlashes = 5;

                Vector3 originalScale = imageRect.localScale;

                for (int i = 0; i < numFlashes; i++)
                {
                    for (float t = 0.0f; t < flashDuration; t += Time.deltaTime)
                    {
                        float interpValue = Math.Min(1.0f, t / flashDuration);
                        targetImage.color = new Color(1, 1, 1, interpValue);
                        imageRect.localScale = new Vector3(1 + interpValue, 1 + interpValue, 1 + interpValue);

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
        }
    }
}




