using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Cjayride.SkyheimCompat
{
    internal static class AltarPanelFix
    {
        internal static void Apply(Harmony harmony)
        {
            MethodInfo original = AccessTools.DeclaredMethod(typeof(InventoryGui), "UpdateContainer");
            if (original == null)
            {
                Plugin.LogWarning("InventoryGui.UpdateContainer was not found; altar panel prefix was not replaced.");
                return;
            }

            harmony.Patch(original, prefix: new HarmonyMethod(typeof(AltarPanelFix), nameof(UpdateContainerPrefix)));
            Plugin.Log("Replaced Skyheim altar UpdateContainer prefix with a null-safe version.");
        }

        static bool UpdateContainerPrefix()
        {
            SkyheimAltarPanel panel = SkyheimAltarPanel.Instance;
            if (!panel)
            {
                return true;
            }

            return !panel.gameObject.activeSelf;
        }
    }
}
