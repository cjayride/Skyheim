using HarmonyLib;
using UnityEngine;

namespace Cjayride.SkyheimCompat
{
    /// <summary>
    /// Skyheim rune recipes must use the Rune Altar crafting station. The altar click
    /// still opens the imbue chest (Piece is not tagged Usage.Crafting). Standing in
    /// range of the station is what unlocks the Craft tab recipes.
    /// </summary>
    [HarmonyPatch]
    internal static class RuneAltarRecipes
    {
        private static bool _bound;

        internal static void Apply()
        {
            if (ObjectDB.instance?.m_recipes == null)
            {
                return;
            }

            CraftingStation station = FindAltarStation();
            if (!station)
            {
                return;
            }

            int count = 0;
            foreach (Recipe recipe in ObjectDB.instance.m_recipes)
            {
                if (recipe?.m_item == null || recipe.m_item.GetComponent<SkyheimItemData>() == null)
                {
                    continue;
                }

                recipe.m_craftingStation = station;
                recipe.m_repairStation = station;
                count++;
            }

            if (!_bound && count > 0)
            {
                _bound = true;
                Plugin.Log($"Bound {count} Skyheim rune recipe(s) to crafting station '{station.m_name}'.");
            }

            AltarChestText.ApplyNames();
        }

        private static CraftingStation FindAltarStation()
        {
            if (ZNetScene.instance?.m_prefabs != null)
            {
                foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
                {
                    CraftingStation station = StationOn(prefab);
                    if (station)
                    {
                        return station;
                    }
                }
            }

            System.Type manager = AccessTools.TypeByName("AssetManager");
            object[] objects = manager != null
                ? AccessTools.Field(manager, "AssetBundleObjects")?.GetValue(null) as object[]
                : null;
            if (objects == null)
            {
                return null;
            }

            foreach (object obj in objects)
            {
                CraftingStation station = obj as CraftingStation;
                if (IsAltar(station))
                {
                    return station;
                }

                GameObject go = obj as GameObject;
                station = StationOn(go);
                if (station)
                {
                    return station;
                }
            }

            return null;
        }

        private static CraftingStation StationOn(GameObject go)
        {
            if (!go)
            {
                return null;
            }

            CraftingStation station = go.GetComponent<CraftingStation>();
            if (IsAltar(station))
            {
                return station;
            }

            station = go.GetComponentInChildren<CraftingStation>(true);
            return IsAltar(station) ? station : null;
        }

        private static bool IsAltar(CraftingStation station)
        {
            if (!station)
            {
                return false;
            }

            string name = station.m_name;
            return name == "Rune Altar" || name == "$sh_piece_altar";
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        [HarmonyPriority(Priority.Last)]
        private static class ObjectDBAwake
        {
            private static void Postfix()
            {
                Apply();
            }
        }

        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
        [HarmonyPriority(Priority.Last)]
        private static class ZNetSceneAwake
        {
            private static void Postfix()
            {
                Apply();
            }
        }
    }
}
