using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Cjayride.SkyheimCompat
{
    /// <summary>
    /// The Runic Chest uses vanilla Container hover text (name + slot/open line). That
    /// never includes Piece.m_description, so the altar directions were invisible unless
    /// the Skyheim imbue panel was open. Keep the chest as a chest (do not tag the altar
    /// Piece as a vanilla crafting station), localize its name, and put the directions
    /// on the world hover and the imbue panel header.
    /// </summary>
    [HarmonyPatch]
    internal static class AltarChestText
    {
        private const string ChestNameKey = "$sh_piece_altar_chest";
        private const string DirectionsKey = "$sh_piece_altar_desc";
        private static readonly FieldInfo HeaderField = AccessTools.Field(typeof(SkyheimAltarPanel), "_header");
        private static bool _named;

        internal static void ApplyNames()
        {
            object[] objects = AccessTools.TypeByName("AssetManager") is System.Type manager
                ? AccessTools.Field(manager, "AssetBundleObjects")?.GetValue(null) as object[]
                : null;
            if (objects == null || objects.Length == 0)
            {
                return;
            }

            int chests = 0;
            foreach (object obj in objects)
            {
                SkyheimAltarContainer chest = obj as SkyheimAltarContainer;
                if (chest == null)
                {
                    GameObject go = obj as GameObject;
                    chest = go != null ? go.GetComponent<SkyheimAltarContainer>() : null;
                }

                if (chest == null)
                {
                    continue;
                }

                chest.m_name = ChestNameKey;
                chests++;
            }

            if (!_named && chests > 0)
            {
                _named = true;
                Plugin.Log($"Rune chest names use {ChestNameKey} ({chests} container(s)).");
            }
        }

        [HarmonyPatch(typeof(Container), nameof(Container.GetHoverText))]
        [HarmonyPostfix]
        private static void GetHoverText_Postfix(Container __instance, ref string __result)
        {
            if (!(__instance is SkyheimAltarContainer))
            {
                return;
            }

            string name = Localization.instance.Localize(ChestNameKey);
            string directions = Localization.instance.Localize(DirectionsKey);
            __result = Localization.instance.Localize(
                name + "\n" + directions +
                "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_container_open");
        }

        [HarmonyPatch(typeof(Container), nameof(Container.GetHoverName))]
        [HarmonyPostfix]
        private static void GetHoverName_Postfix(Container __instance, ref string __result)
        {
            if (__instance is SkyheimAltarContainer)
            {
                __result = ChestNameKey;
            }
        }

        [HarmonyPatch(typeof(SkyheimAltarPanel), nameof(SkyheimAltarPanel.Show))]
        [HarmonyPostfix]
        private static void Show_Postfix(SkyheimAltarPanel __instance)
        {
            if (HeaderField?.GetValue(__instance) is not Text header)
            {
                return;
            }

            header.text = Localization.instance.Localize(ChestNameKey);
        }
    }
}
