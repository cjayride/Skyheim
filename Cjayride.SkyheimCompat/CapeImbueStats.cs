using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Cjayride.SkyheimCompat
{
    /// <summary>
    /// Skyheim only adds imbue eitr/regen from helmet, chest, and legs (SetMaxEitr prefix).
    /// Capes, backpacks, and SeneaL extra cape cells are other equipped items, so they
    /// showed the imbue tooltip but never changed the live eitr pool.
    /// </summary>
    [HarmonyPatch]
    internal static class CapeImbueStats
    {
        private const string EitrKey = "sh_mod_eitr";
        private const string RegenKey = "sh_mod_eitr_regen";
        private static FieldInfo _senealCapeItem;
        private static bool _senealCapeProbed;

        [HarmonyPatch(typeof(Player), "SetMaxEitr")]
        [HarmonyPostfix]
        private static void SetMaxEitr_Postfix(Player __instance)
        {
            float extra = SumEquipped(__instance, EitrKey);
            if (extra == 0f)
            {
                return;
            }

            __instance.m_maxEitr += extra;
            __instance.m_eitr = Mathf.Clamp(__instance.m_eitr, 0f, __instance.m_maxEitr);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetEquipmentEitrRegenModifier))]
        [HarmonyPostfix]
        private static void Regen_Postfix(Player __instance, ref float __result)
        {
            __result += SumEquipped(__instance, RegenKey);
        }

        private static float SumEquipped(Player player, string key)
        {
            if (player == null)
            {
                return 0f;
            }

            float total = 0f;
            HashSet<ItemDrop.ItemData> seen = new HashSet<ItemDrop.ItemData>();
            Inventory inventory = player.GetInventory();
            if (inventory != null)
            {
                List<ItemDrop.ItemData> equipped = inventory.GetEquippedItems();
                for (int i = 0; i < equipped.Count; i++)
                {
                    ItemDrop.ItemData item = equipped[i];
                    if (item == null || !seen.Add(item) || IsSkyheimBodySlot(item))
                    {
                        continue;
                    }

                    total += ReadModifier(item, key);
                }
            }

            AddIfNew(seen, player.m_shoulderItem, key, ref total);
            AddIfNew(seen, player.m_utilityItem, key, ref total);
            FieldInfo extraCape = SenealCapeField();
            if (extraCape != null)
            {
                AddIfNew(seen, extraCape.GetValue(null) as ItemDrop.ItemData, key, ref total);
            }

            return total;
        }

        private static FieldInfo SenealCapeField()
        {
            if (_senealCapeProbed)
            {
                return _senealCapeItem;
            }

            _senealCapeProbed = true;
            System.Type type = AccessTools.TypeByName("SeneaLUI.Mods.CapeSlot");
            _senealCapeItem = type != null ? AccessTools.Field(type, "Item") : null;
            return _senealCapeItem;
        }

        private static void AddIfNew(HashSet<ItemDrop.ItemData> seen, ItemDrop.ItemData item, string key, ref float total)
        {
            if (item == null || !seen.Add(item) || IsSkyheimBodySlot(item))
            {
                return;
            }

            total += ReadModifier(item, key);
        }

        private static bool IsSkyheimBodySlot(ItemDrop.ItemData item)
        {
            ItemDrop.ItemData.ItemType type = item.m_shared != null
                ? item.m_shared.m_itemType
                : ItemDrop.ItemData.ItemType.None;
            return type == ItemDrop.ItemData.ItemType.Helmet
                || type == ItemDrop.ItemData.ItemType.Chest
                || type == ItemDrop.ItemData.ItemType.Legs;
        }

        private static float ReadModifier(ItemDrop.ItemData item, string key)
        {
            if (item?.m_customData == null || !item.m_customData.TryGetValue(key, out string raw) || string.IsNullOrEmpty(raw))
            {
                return 0f;
            }

            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                return value;
            }

            return float.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value) ? value : 0f;
        }
    }
}
