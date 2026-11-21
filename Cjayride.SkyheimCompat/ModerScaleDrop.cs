using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Cjayride.SkyheimCompat
{
    /// <summary>
    /// Skyheim's DropModifier adds unused Dragon Scale (moder_drop / $sh_item_moder_drop)
    /// to Moder (Dragon). No rune recipe uses it. Default off; vanilla TrophyDragonQueen
    /// and DragonTear are not touched.
    /// </summary>
    internal static class ModerScaleDrop
    {
        private const string PrefabName = "moder_drop";
        private const string LocKey = "$sh_item_moder_drop";

        private static ConfigEntry<bool> _enabled;

        internal static void Bind(ConfigFile config)
        {
            _enabled = config.Bind(
                "Drops",
                "ModerDragonScale",
                false,
                "Add Skyheim's unused Dragon Scale (moder_drop) to Moder. Off by default. Vanilla trophy and Dragon Tears stay. Set true to restore the extra drop.");
        }

        internal static bool ScaleEnabled => _enabled != null && _enabled.Value;

        internal static bool IsScalePrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                return false;
            }

            string name = prefab.name.Replace("(Clone)", string.Empty);
            if (string.Equals(name, PrefabName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            ItemDrop drop = prefab.GetComponent<ItemDrop>();
            string shared = drop != null ? drop.m_itemData?.m_shared?.m_name : null;
            return shared == LocKey || shared == "sh_item_moder_drop";
        }

        internal static bool IsScaleItemName(string itemDrop)
        {
            if (string.IsNullOrEmpty(itemDrop))
            {
                return false;
            }

            return string.Equals(itemDrop, PrefabName, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemDrop, LocKey, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemDrop, "sh_item_moder_drop", System.StringComparison.OrdinalIgnoreCase);
        }

        internal static int StripFrom(CharacterDrop drops)
        {
            if (drops?.m_drops == null)
            {
                return 0;
            }

            int removed = 0;
            for (int i = drops.m_drops.Count - 1; i >= 0; i--)
            {
                CharacterDrop.Drop row = drops.m_drops[i];
                if (row == null || !IsScalePrefab(row.m_prefab))
                {
                    continue;
                }

                drops.m_drops.RemoveAt(i);
                removed++;
            }

            return removed;
        }
    }

    [HarmonyPatch(typeof(DropModifier), nameof(DropModifier.Apply))]
    internal static class DropModifier_Apply_ModerScale_Patch
    {
        private static bool Prefix(DropModifier __instance)
        {
            if (__instance == null || ModerScaleDrop.ScaleEnabled)
            {
                return true;
            }

            if (!ModerScaleDrop.IsScaleItemName(__instance.ItemDrop))
            {
                return true;
            }

            if (!_logged)
            {
                _logged = true;
                Plugin.Log("Skipped Skyheim DropModifier for Dragon Scale (moder_drop). [Drops] ModerDragonScale = true restores it.");
            }

            return false;
        }

        private static bool _logged;
    }

    [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
    [HarmonyPriority(Priority.Last)]
    internal static class CharacterDrop_GenerateDropList_ModerScale_Patch
    {
        private static void Prefix(CharacterDrop __instance)
        {
            if (ModerScaleDrop.ScaleEnabled || __instance == null)
            {
                return;
            }

            int removed = ModerScaleDrop.StripFrom(__instance);
            if (removed > 0 && !_logged)
            {
                _logged = true;
                Plugin.Log("Removed " + removed + " leftover Dragon Scale row(s) from " + __instance.gameObject.name + " before GenerateDropList.");
            }
        }

        private static bool _logged;
    }
}
