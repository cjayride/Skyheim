using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Cjayride.SkyheimEitr
{
    /// <summary>
    /// Applies the runtime asset changes: the replaced skill icons (AltarIconPatch).
    ///
    /// All three asset-field edits from the Ukrainian translation contribution were
    /// reverted after in-game testing:
    ///
    /// - The altar station name was NOT renamed. Recipes bind to the station by the
    ///   literal string "Rune Altar" (Recipe.m_craftingStation.m_name, compared in
    ///   Player.HaveRequirements), and the modpack's WackysDatabase rune recipes use
    ///   that literal. Renaming it made every rune recipe fail its station check and
    ///   left player save dictionaries (m_knownStations) pointing at the old name.
    ///
    /// - The chest name was NOT renamed. Vanilla builds the container hover text from
    ///   Container.m_name, and the Compat mod's altar instructions render under that
    ///   title; the rename stripped the instructions down to "x slots, used x slots".
    ///
    /// - The altar Piece's m_usage was NOT given the Crafting (2) tag. With it set the
    ///   altar opened the vanilla Craft panel listing every recipe, replacing its own
    ///   imbue panel.
    ///
    /// The translation itself needs none of this: it arrives through the localization
    /// file and Localization.AddWord, which the Compat mod loads independently.
    /// </summary>
    [HarmonyPatch]
    internal static class SkyheimAssetPatches
    {
        static bool _applied;

        static MethodBase TargetMethod()
        {
            Type manager = AccessTools.TypeByName("AssetManager");
            return manager == null ? null : AccessTools.Method(manager, "LoadAssetsFromBundle");
        }

        static void Postfix()
        {
            Apply();
        }

        internal static void Apply()
        {
            if (_applied)
            {
                return;
            }

            Plugin.Log("Altar assets: applying skill icons.");
            if (!AltarIconPatch.Apply())
            {
                return;
            }

            _applied = true;
        }

        internal static Texture2D FindTexture(string textureName)
        {
            Texture2D found = FindIn(BundleObjects(), textureName);
            if (found != null)
            {
                return found;
            }

            Texture2D[] loaded = Resources.FindObjectsOfTypeAll<Texture2D>();
            foreach (Texture2D texture in loaded)
            {
                if (NameMatches(texture, textureName))
                {
                    return texture;
                }
            }

            return null;
        }

        private static Texture2D FindIn(object[] objects, string textureName)
        {
            if (objects == null)
            {
                return null;
            }

            foreach (object obj in objects)
            {
                Texture2D texture = obj as Texture2D;
                if (NameMatches(texture, textureName))
                {
                    return texture;
                }
            }

            return null;
        }

        private static bool NameMatches(Texture2D texture, string textureName)
        {
            if (texture == null || string.IsNullOrEmpty(texture.name))
            {
                return false;
            }

            if (texture.name.IndexOf("rune_atlas", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return !string.IsNullOrEmpty(textureName)
                && texture.name.IndexOf(textureName, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static object[] BundleObjects()
        {
            Type manager = AccessTools.TypeByName("AssetManager");
            if (manager == null)
            {
                return null;
            }

            FieldInfo field = AccessTools.Field(manager, "AssetBundleObjects");
            return field == null ? null : field.GetValue(null) as object[];
        }

        private static object[] AssetBundleObjects()
        {
            return BundleObjects();
        }
    }
}
