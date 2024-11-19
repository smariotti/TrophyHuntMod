using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TrophyHuntMod
{
    class TrophyFiesta
    {
        static void AddItemToObjectDB(GameObject item)
        {
            Debug.LogWarning("AddItemToObjectDB()");
            if (ObjectDB.instance.m_items.Contains(item))
                return;

            ObjectDB.instance.m_items.Add(item);
//            ObjectDB.instance.UpdateItemHashes();
        }


        static GameObject customBomb = null;

        static void SetupBoarTrophy()
        {
            Debug.LogWarning("SetupBoarTrophy()");
            // Clone the Ooze Bomb prefab
            //var oozeBombPrefab = ObjectDB.instance.GetItemPrefab("BombOoze");
            //if (oozeBombPrefab != null)
            //{
            //    Debug.LogWarning("SetupBoarTrophy() - Found OozeBomb prefab");

            //    Component[] all = oozeBombPrefab.GetComponents<Component>();
            //    foreach (Component c in all)
            //    {
            //        Debug.LogWarning(c);
            //    }

            //    customBomb = GameObject.Instantiate(oozeBombPrefab);
            //    customBomb.name = "BoarTrophyBomb";

            //     Modify the ItemDrop component to change the ammo type
            //    var boarDrop = customBomb.GetComponent<ItemDrop>();
            //    boarDrop.m_itemData.m_shared.m_ammoType = "BoarTrophy"; // Set the ammo to BoarTrophy

            //    BoarTrophyBombBehavior bombScript = customBomb.AddComponent<BoarTrophyBombBehavior>();

            //     Add it to the ObjectDB
            //    AddItemToObjectDB(customBomb);

            //    GameObject go = ObjectDB.instance.GetItemPrefab(StringExtensionMethods.GetStableHashCode("BoarTrophyBomb"));
            //    Debug.LogWarning(go);
            //}

            GameObject boarTrophy = ObjectDB.instance.GetItemPrefab("TrophyBoar");
            if (boarTrophy == null)
            {
                Debug.LogError("boarTrophy is null");
                return;
            }

            GameObject flintSpear = ObjectDB.instance.GetItemPrefab("SpearFlint");
            if (flintSpear == null)
            {
                Debug.LogError("flintSpear is null");
                return;
            }

            ItemDrop spearItemDrop = flintSpear.GetComponent<ItemDrop>();

            //GameObject flintProjectile = ObjectDB.instance.GetItemPrefab("flintspear_projectile");
            //if (flintProjectile == null)
            //{
            //    Debug.LogError("flintProjectile is null");
            //    return;
            //}


            // Modify its ItemDrop data to mark it as throwable
            var boarTrophyItemDrop = boarTrophy.GetComponent<ItemDrop>();
            if (boarTrophyItemDrop != null)
            {
                //                itemDrop.m_itemData.m_shared.m_name = "Throwable Deer Trophy";
                //                itemDrop.m_itemData.m_shared.m_description = "A throwable version of the deer trophy";
                //                itemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable; // Mark it as a consumable (to throw it)
                // boarTrophyItemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.OneHandedWeapon;
                // boarTrophyItemDrop.m_itemData.m_shared.m_attack.m_attackType = Attack.AttackType.TriggerProjectile;

                //                itemDrop.m_itemData.m_shared.m_ammoType = "Grenade"; // Set it as grenade-type throwable

                ItemDrop.ItemData.SharedData boarSharedData = boarTrophyItemDrop.m_itemData.m_shared;
                ItemDrop.ItemData.SharedData spearSharedData= spearItemDrop.m_itemData.m_shared;

                boarSharedData.m_itemType = spearSharedData.m_itemType;

                boarSharedData.m_attack = spearSharedData.m_secondaryAttack;

//                boarSharedData.m_attack.m_attackProjectile = GameObject.Instantiate(spearSharedData.m_attack.m_attackProjectile);
                Projectile boarProjectile = boarSharedData.m_attack.m_attackProjectile.GetComponent<Projectile>();
                boarProjectile.m_visual = boarTrophy;

                //Projectile boarProjectile = boarTrophyItemDrop.m_itemData.m_shared.m_secondaryAttack.m_attackProjectile.GetComponent<Projectile>();

                boarSharedData.m_spawnOnHit = spearSharedData.m_spawnOnHit;
                boarSharedData.m_spawnOnHitTerrain = spearSharedData.m_spawnOnHitTerrain;
                boarSharedData.m_projectileToolTip = spearSharedData.m_projectileToolTip;

            }

            // Add the Projectile component to make it throwable
            //            var projectile = boarTrophy.AddComponent<Projectile>();
            //            projectile.m_gravity = 10f; // Adjust gravity for throw arc
            //            projectile.m_damage.m_damage = 30; // Set damage dealt
            GameObject hitEffect = Resources.Load<GameObject>("impact_sfx"); // Optional: add a hit effect
            if (!hitEffect)
            {
                Debug.LogError("hitEffect is null");
                return;
            }
            //projectile.m_hitEffects;
            // Add the modified throwable trophy to ObjectDB
            //            ObjectDB.instance.m_items.Add(boarTrophy);

        }

        public static void Initialize()
        {
            SetupBoarTrophy();

        }



//        [HarmonyPatch(typeof(Attack), "UseAmmo")]
//        public class Attack_UseAmmo_Patch
//        {
//            static bool Prefix(Attack __instance, ItemDrop.ItemData ammoItem)
//            {
////                Debug.LogWarning("Attack.UseAmmo");

//                // Check if the item is the custom bomb and use boar trophies as ammo
//                //if (ammoItem.m_shared.m_name == "BoarTrophyBomb")
//                //{
//                //    var boarTrophies = __instance.GetInventory().GetItem("BoarTrophy");
//                //    if (boarTrophies != null)
//                //    {
//                //        __instance.GetInventory().RemoveItem(boarTrophies, 1); // Consume 1 boar trophy
//                //        return true; // Allow ammo use
//                //    }

//                //    // No boar trophies in inventory
//                //    return false;
//                //}

//                return true; // Proceed for other items
//            }
//        }
    }

    public class BoarTrophyBombBehavior : MonoBehaviour
    {
        public GameObject explosionEffect;

        void Start()
        {
            Debug.LogWarning("BoarTrophyBombBehavior.Start");

            // Load troll's smash explosion effect from troll's prefab
            var trollPrefab = ObjectDB.instance.GetItemPrefab("Troll");
            if (trollPrefab != null)
            {
                var trollSmashEffect = trollPrefab.GetComponent<Character>().m_hitEffects;// m_onHitEffects;
                explosionEffect = trollSmashEffect.m_effectPrefabs[0].m_prefab; // Example, adjust as needed
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            Debug.LogWarning("BoarTrophyBombBehavior.OnCollisionEnter");

            // When the bomb hits something, trigger the explosion
            Explode();
        }

        void Explode()
        {
            Debug.LogWarning("BoarTrophyBombBehavior.Explode");

            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            // Add logic to deal damage or apply any other effects you need
            // Destroy the bomb after explosion
            Destroy(gameObject);
        }
    }
}
