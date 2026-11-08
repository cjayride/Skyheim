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
        private const string PieceNameKey = "$sh_piece_altar";
        /// <summary>WackyDB / recipes / HaveRequirements use this literal on CraftingStation.m_name.
        /// UI calls Localization.Localize on that string — see skyheim.json key "Rune Altar".</summary>
        private const string PieceNameLiteral = "Rune Altar";
        private const string DirectionsKey = "$sh_piece_altar_desc";
        private static readonly FieldInfo HeaderField = AccessTools.Field(typeof(SkyheimAltarPanel), "_header");
        private static bool _named;

        internal static void ApplyNames()
        {
            int chests = 0;
            int pieces = 0;

            object[] objects = AccessTools.TypeByName("AssetManager") is System.Type manager
                ? AccessTools.Field(manager, "AssetBundleObjects")?.GetValue(null) as object[]
                : null;
            if (objects != null)
            {
                foreach (object obj in objects)
                {
                    if (ApplyChest(obj as SkyheimAltarContainer) || ApplyChest(ComponentOn(obj as GameObject)))
                    {
                        chests++;
                    }

                    if (ApplyPiece(obj as Piece) || ApplyPiece(PieceOn(obj as GameObject)))
                    {
                        pieces++;
                    }
                }
            }

            if (ZNetScene.instance?.m_prefabs != null)
            {
                foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
                {
                    if (ApplyChest(ComponentOn(prefab)))
                    {
                        chests++;
                    }

                    if (ApplyPiece(PieceOn(prefab)))
                    {
                        pieces++;
                    }
                }
            }

            if (!_named && (chests > 0 || pieces > 0))
            {
                _named = true;
                Plugin.Log($"Rune Altar uses {PieceNameKey} in Misc; chest uses {ChestNameKey} ({pieces} piece(s), {chests} container(s)).");
            }
        }

        private static SkyheimAltarContainer ComponentOn(GameObject go)
        {
            return go != null ? go.GetComponent<SkyheimAltarContainer>() : null;
        }

        private static Piece PieceOn(GameObject go)
        {
            if (!go)
            {
                return null;
            }

            Piece piece = go.GetComponent<Piece>();
            return piece ? piece : go.GetComponentInChildren<Piece>(true);
        }

        private static bool ApplyChest(SkyheimAltarContainer chest)
        {
            if (chest == null)
            {
                return false;
            }

            chest.m_name = ChestNameKey;
            return true;
        }

        private static bool ApplyPiece(Piece piece)
        {
            if (!piece || !IsAltarPiece(piece))
            {
                return false;
            }

            piece.m_name = PieceNameKey;
            piece.m_description = DirectionsKey;
            piece.m_category = Piece.PieceCategory.Misc;
            // UsageTagFlags.Crafting (2) made clicking the altar open the vanilla Craft
            // list instead of the imbue chest. Misc still puts it on a hammer tab.
            piece.m_usage = Piece.UsageTagFlags.Misc;
            return true;
        }

        private static bool IsAltarPiece(Piece piece)
        {
            if (piece.m_name == PieceNameLiteral || piece.m_name == PieceNameKey)
            {
                return true;
            }

            CraftingStation station = piece.GetComponent<CraftingStation>();
            if (!station)
            {
                station = piece.GetComponentInChildren<CraftingStation>(true);
            }

            if (!station)
            {
                return false;
            }

            string name = station.m_name;
            return name == PieceNameLiteral || name == PieceNameKey;
        }

        private static bool IsAltarStation(CraftingStation station)
        {
            if (!station)
            {
                return false;
            }

            string name = station.m_name;
            return name == PieceNameLiteral || name == PieceNameKey;
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

        /// <summary>
        /// InventoryGui.UpdateRecipe does Localize(CraftingStation.m_name). The station id
        /// must stay the literal "Rune Altar" for WackyDB / HaveRequirements, and vanilla
        /// Localize only substitutes $keys, so the craft-panel title stayed English.
        /// </summary>
        private static bool _rewritingLiteral;

        [HarmonyPatch(typeof(Localization), nameof(Localization.Localize), typeof(string))]
        [HarmonyPostfix]
        private static void LocalizeLiteral_Postfix(ref string __result)
        {
            RewriteAltarLiteral(ref __result);
        }

        [HarmonyPatch(typeof(Localization), nameof(Localization.Localize), typeof(string), typeof(string[]))]
        [HarmonyPostfix]
        private static void LocalizeLiteralArgs_Postfix(ref string __result)
        {
            RewriteAltarLiteral(ref __result);
        }

        private static void RewriteAltarLiteral(ref string result)
        {
            if (_rewritingLiteral || string.IsNullOrEmpty(result)
                || result.IndexOf(PieceNameLiteral, System.StringComparison.Ordinal) < 0
                || Localization.instance == null)
            {
                return;
            }

            _rewritingLiteral = true;
            try
            {
                string localized = Localization.instance.Localize(PieceNameKey);
                if (!string.IsNullOrEmpty(localized) && localized != PieceNameKey)
                {
                    result = result.Replace(PieceNameLiteral, localized);
                }
            }
            finally
            {
                _rewritingLiteral = false;
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "UpdateRecipe")]
        [HarmonyPostfix]
        private static void UpdateRecipe_Postfix(InventoryGui __instance)
        {
            Player player = Player.m_localPlayer;
            CraftingStation station = player != null ? player.GetCurrentCraftingStation() : null;
            if (!IsAltarStation(station) || Localization.instance == null)
            {
                return;
            }

            object nameLabel = AccessTools.Field(typeof(InventoryGui), "m_craftingStationName")?.GetValue(__instance);
            if (nameLabel == null)
            {
                return;
            }

            AccessTools.Property(nameLabel.GetType(), "text")?.SetValue(
                nameLabel,
                Localization.instance.Localize(PieceNameKey));
        }

        [HarmonyPatch(typeof(CraftingStation), nameof(CraftingStation.GetHoverName))]
        [HarmonyPostfix]
        private static void StationHoverName_Postfix(CraftingStation __instance, ref string __result)
        {
            if (IsAltarStation(__instance))
            {
                __result = PieceNameKey;
            }
        }

        [HarmonyPatch(typeof(CraftingStation), nameof(CraftingStation.GetHoverText))]
        [HarmonyPostfix]
        private static void StationHoverText_Postfix(CraftingStation __instance, ref string __result)
        {
            if (!IsAltarStation(__instance) || Localization.instance == null)
            {
                return;
            }

            __result = Localization.instance.Localize(
                PieceNameKey + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
        }
    }
}
