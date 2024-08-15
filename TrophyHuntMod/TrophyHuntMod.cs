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

namespace TrophyHuntMod
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    internal class TrophyHuntMod : BaseUnityPlugin
    {
        public const string PluginGUID = "com.oathorse.TrophyHuntMod";
        public const string PluginName = "TrophyHuntMod";
        public const string PluginVersion = "0.0.1";
        private readonly Harmony harmony = new Harmony(PluginGUID);

        private const Boolean DUMP_TROPHY_DATA = false;

        private void Awake()
        {
            Debug.LogWarning("TrophyHuntMod has landed");

            // Patch with Harmony
            harmony.PatchAll();
        }

        public enum Biome
        {
            Meadows,
            Forest,
            Ocean,
            Swamp,
            Mountains,
            Plains,
            Mistlands,
            Ashlands
        };

        public struct TrophyData
        {
            public TrophyData(string name, Biome biome, int value)
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

        static public TrophyData[] __m_trophyData = new TrophyData[]
        {
            new TrophyData("TrophyAbomination", Biome.Swamp, 20),
            new TrophyData("TrophyAsksvin", Biome.Ashlands, 50),
            new TrophyData("TrophyBlob", Biome.Swamp, 20),
            new TrophyData("TrophyBoar", Biome.Meadows, 10),
            new TrophyData("TrophyBonemass", Biome.Swamp, 80),
            new TrophyData("TrophyBonemawSerpent", Biome.Ashlands, 50),
            new TrophyData("TrophyCharredArcher", Biome.Ashlands, 50),
            new TrophyData("TrophyCharredMage", Biome.Ashlands, 50),
            new TrophyData("TrophyCharredMelee", Biome.Ashlands, 50),
            new TrophyData("TrophyCultist", Biome.Mountains, 30),
            new TrophyData("TrophyCultist_Hildir", Biome.Mountains, 30),
            new TrophyData("TrophyDeathsquito", Biome.Plains, 30),
            new TrophyData("TrophyDeer", Biome.Meadows, 10),
            new TrophyData("TrophyDragonQueen", Biome.Mountains, 100),
            new TrophyData("TrophyDraugr", Biome.Swamp, 20),
            new TrophyData("TrophyDraugrElite", Biome.Swamp, 20),
            new TrophyData("TrophyDraugrFem", Biome.Swamp, 20),
            new TrophyData("TrophyDvergr", Biome.Mistlands, 40),
            new TrophyData("TrophyEikthyr", Biome.Meadows, 50),
            new TrophyData("TrophyFader", Biome.Ashlands, 1000),
            new TrophyData("TrophyFallenValkyrie", Biome.Ashlands, 50),
            new TrophyData("TrophyFenring", Biome.Mountains, 30),
            new TrophyData("TrophyForestTroll", Biome.Forest, 20),
            new TrophyData("TrophyFrostTroll", Biome.Forest, 20),
            new TrophyData("TrophyGjall", Biome.Mistlands, 40),
            new TrophyData("TrophyGoblin", Biome.Plains, 30),
            new TrophyData("TrophyGoblinBrute", Biome.Plains, 30),
            new TrophyData("TrophyGoblinBruteBrosBrute", Biome.Plains, 30),
            new TrophyData("TrophyGoblinBruteBrosShaman", Biome.Plains, 30),
            new TrophyData("TrophyGoblinKing", Biome.Plains, 100),
            new TrophyData("TrophyGoblinShaman", Biome.Plains, 30),
            new TrophyData("TrophyGreydwarf", Biome.Forest, 20),
            new TrophyData("TrophyGreydwarfBrute", Biome.Forest, 20),
            new TrophyData("TrophyGreydwarfShaman", Biome.Forest, 20),
            new TrophyData("TrophyGrowth", Biome.Plains, 30),
            new TrophyData("TrophyHare", Biome.Mistlands, 40),
            new TrophyData("TrophyHatchling", Biome.Mountains, 30),
            new TrophyData("TrophyLeech", Biome.Swamp, 20),
            new TrophyData("TrophyLox", Biome.Plains, 30),
            new TrophyData("TrophyMorgen", Biome.Ashlands, 50),
            new TrophyData("TrophyNeck", Biome.Meadows, 10),
            new TrophyData("TrophySeeker", Biome.Mistlands, 40),
            new TrophyData("TrophySeekerBrute", Biome.Mistlands, 40),
            new TrophyData("TrophySeekerQueen", Biome.Mistlands, 1000),
            new TrophyData("TrophySerpent", Biome.Ocean, 25),
            new TrophyData("TrophySGolem", Biome.Mountains, 30),
            new TrophyData("TrophySkeleton", Biome.Forest, 20),
            new TrophyData("TrophySkeletonHildir", Biome.Forest, 20),
            new TrophyData("TrophySkeletonPoison", Biome.Forest, 20),
            new TrophyData("TrophySurtling", Biome.Swamp, 20),
            new TrophyData("TrophyTheElder", Biome.Forest, 50),
            new TrophyData("TrophyTick", Biome.Mistlands, 40),
            new TrophyData("TrophyUlv", Biome.Mountains, 30),
            new TrophyData("TrophyVolture", Biome.Ashlands, 50),
            new TrophyData("TrophyWolf", Biome.Mountains, 30),
            new TrophyData("TrophyWraith", Biome.Swamp, 20)
        };

        static GameObject __m_scoreTextElement = null;
        static GameObject __m_deathsTextElement = null;
        static GameObject __m_trophyTrayElement = null;

        static List<string> __m_trophiesInObjectDB = new List<string>();
        static List<GameObject> __m_iconList = new List<GameObject>();
        static int __m_deaths = 0;

        [HarmonyPatch(typeof(Player), "Awake")]
        public class Player_Awake_Patch
        {
            static void Postfix(Player __instance)
            {

                Debug.LogWarning("Player is Awake()!");

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
                Debug.LogWarning($"{__m_trophiesInObjectDB.Count} trophies discovered");

                if (__m_trophiesInObjectDB.Count != __m_trophyData.Length)
                {
                    Debug.LogError($"Valheim's list of Trophies ({__m_trophiesInObjectDB.Count}) doesn't match the mod's Trophy data ({__m_trophyData.Length}), this mod is out of date.");
                }

                // Sort the trophies by biome, score and name
                Array.Sort<TrophyData>(__m_trophyData, (x, y) => x.m_biome.CompareTo(y.m_biome) * 100000 + x.m_value.CompareTo(y.m_value) * 10000 + x.m_name.CompareTo(y.m_name));

                // Dump loaded trophy data
                if (DUMP_TROPHY_DATA)
                {
                    foreach (var t in __m_trophyData)
                    {
                        Debug.LogWarning($"{t.m_biome.ToString()}, {t.m_name}, {t.m_value}");
                    }
                }

                // Create all the UI elements we need for this mod ahead of time and manipulate them at Player.Update() time
                BuildUIElements();
            }

            static void BuildUIElements()
            {
                if (Hud.instance == null || Hud.instance.m_rootObject == null)
                {
                    Debug.LogError("TrophyHuntMod: Hud.instance.m_rootObject is NOT valid");
                    
                    return;
                }

                Transform healthPanelTransform = Hud.instance.transform.Find("hudroot/healthpanel");

                if (healthPanelTransform == null)
                {
                    Debug.LogError("Health panel transform not found.");

                    return;
                }

                Transform baseHUDTransform = Hud.instance.transform.Find("hudroot/healthpanel");

                __m_trophyTrayElement = CreateTrophyTrayElement(healthPanelTransform);

                // TODO: do something with trophyTray? like parent everything under it?

                CreateScoreTextElement(healthPanelTransform);

                CreateTrophyIconElements(healthPanelTransform, __m_trophyData);

                __m_deathsTextElement = CreateDeathsElement(healthPanelTransform);
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
                rectTransform.anchoredPosition = new Vector2(-70, -90); // Set position

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

            static void CreateScoreTextElement(Transform parentTransform)
            {
                // Create a new GameObject for the text
                __m_scoreTextElement = new GameObject("ScoreText");

                // Set the parent to the HUD canvas
                __m_scoreTextElement.transform.SetParent(parentTransform);

                // Add RectTransform component for positioning
                RectTransform rectTransform = __m_scoreTextElement.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(200, 50); // Set size
                rectTransform.anchoredPosition = new Vector2(-65, -140); // Set position

                int scoreValue = 9999;

                TMPro.TextMeshProUGUI tmText = __m_scoreTextElement.AddComponent<TMPro.TextMeshProUGUI>();

                tmText.text = $"{scoreValue}";
                tmText.fontSize = 28;
                tmText.color = Color.yellow;
                tmText.alignment = TextAlignmentOptions.Center;
                tmText.raycastTarget = false;
                
            }
            static void CreateTrophyIconElements(Transform parentTransform, TrophyData[] trophies)
            {
                foreach (TrophyData trophy in trophies)
                {
                    Sprite trophySprite = GetTrophySprite(trophy.m_name);
                    if (trophySprite == null)
                    {
                        //ACK
                        Debug.LogError($"Unable to find trophy sprite for {trophy.m_name}");
                        continue;
                    }

                    GameObject iconElement = CreateIconElement(parentTransform, trophySprite, trophy.m_name, __m_iconList.Count);

                    __m_iconList.Add(iconElement);
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

            static GameObject CreateIconElement(Transform parentTransform, Sprite iconSprite, string iconName, int index)
            {
                // Create a new GameObject for the icon
                GameObject iconElement = new GameObject(iconName);

                // Set the parent to the HUD canvas
                iconElement.transform.SetParent(parentTransform);

                int iconSize = 33;
                int iconBorderSize = 0;
                int xOffset = -20;
                int yOffset = -140;

                // Add RectTransform component for positioning
                RectTransform rectTransform = iconElement.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(iconSize, iconSize); // Set size
                rectTransform.anchoredPosition = new Vector2(xOffset + index * (iconSize + iconBorderSize), yOffset); // Set position
 
                // Add an Image component
                UnityEngine.UI.Image image = iconElement.AddComponent<UnityEngine.UI.Image>();
                image.sprite = iconSprite;
                image.color = Color.black;
                image.raycastTarget = false;

                return iconElement;
            }
        }

        // Patch the Player.Update() function to do our own update of the trophy data displayed in the HUD
        //
        [HarmonyPatch(typeof(Player), "Update")]
        public class Player_Update_Patch
        {
            static void Postfix(ObjectDB __instance)
            {
                // If there's no Hud yet, don't do anything here
                if (Hud.instance == null || Hud.instance.m_rootObject == null)
                {
                    return;
                }

                // If there's no player yet, or no trophy list, don't do anything here
                if (Player.m_localPlayer == null || Player.m_localPlayer.m_trophies == null)
                {
                    return;
                }

                // Player.Update() has already occurred, now build the list of Trophies we've discovered
                List<string> discoveredTrophies = Player.m_localPlayer.m_trophies.ToList<string>();

                // Quick and dirty brute force lookup for trophies to update the UI
                //
                int score = 0;
                foreach (TrophyData td in __m_trophyData)
                {
                    foreach (string s in discoveredTrophies)
                    {
                        if (td.m_name == s)
                        {
                            // Add the value to our score
                            score += td.m_value;
                        
                            // Find the UI element and bold it
                            foreach (GameObject o in __m_iconList)
                            {
                                UnityEngine.UI.Image image = o.GetComponent<UnityEngine.UI.Image>();
                                if (image != null)
                                {
                                    if (image.name == s)
                                    {
                                        image.color = Color.white;

                                        break;
                                    }
                                }
                            }

                            break;
                        }
                    }
                }

                // Remove score for death penalty
                if (Game.instance == null)
                {
                    Debug.Log("Unable to retrieve the Game instance!");
                }
                else
                {
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
                            if (deathsText != null )
                            {
                                deathsText.SetText(__m_deaths.ToString());
                            }
                        }
                    }
                }

                // Update the Score string
                __m_scoreTextElement.GetComponent<TMPro.TextMeshProUGUI>().text = score.ToString();
            }
        }
    
        //[HarmonyPatch(typeof(Hud), "Awake")]
        //public class Hud_Awake_Patch
        //{
        //    static void Postfix(Hud __instance)
        //    {
        //        // This code runs after the Hud's Awake method
        //        Debug.Log("TrophyHuntMod: HUD has been instantiated!");
        //    }
        //}
    }

    //[HarmonyPatch(typeof(Player), nameof(Player.AddTrophy), new [] {typeof(ItemDrop.ItemData)} )]
    //public static class Player_AddTrophy_Patch
    //{
    //    public static void Postfix(Player __instance, ItemDrop.ItemData item)
    //    {
    //        var player = __instance;
    //        if (player != null)
    //        {
    //            if (item != null)
    //            {
    //                var name = item.m_shared.m_name;

    //                Debug.LogWarning(string.Format("TrophyHuntMod: Trophy added! '{0}'", name));

    //                foreach (var x in player.m_trophies)
    //                {
    //                    Debug.LogWarning(x);
    //                }
    //            }
    //        }
    //    }
    //}
}




