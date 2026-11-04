using System.Reflection;
using HarmonyLib;

namespace Cjayride.SkyheimCompat
{
    /// <summary>
    /// Skyheim only starts an imbue after the chest was in use and then closed
    /// (Container.IsInUse). SeneaL never calls SetInUse, so _inUse stayed false and
    /// closing the Runic Chest did nothing. Treat SeneaL's open container as in-use
    /// and start the process when that container is no longer current.
    /// </summary>
    [HarmonyPatch(typeof(SkyheimAltar), "Update")]
    internal static class AltarProcessFix
    {
        private static readonly FieldInfo InUseField = AccessTools.Field(typeof(SkyheimAltar), "_inUse");
        private static readonly MethodInfo StartProcess = AccessTools.Method(typeof(SkyheimAltar), "StartProcess", new[] { typeof(float) });

        private static void Postfix(SkyheimAltar __instance)
        {
            if (!__instance || __instance.Processing || InUseField == null)
            {
                return;
            }

            SkyheimAltarContainer chest = __instance.Container;
            if (!chest)
            {
                return;
            }

            bool senealOpen = Plugin.SenealUiLoaded()
                && AltarCostHint.SenealContainer() == (Container)chest;

            if (senealOpen)
            {
                InUseField.SetValue(__instance, true);
                return;
            }

            if (!(bool)InUseField.GetValue(__instance))
            {
                return;
            }

            InUseField.SetValue(__instance, false);
            SkyheimArmorRecipe recipe = __instance.GetActiveRecipe();
            if (recipe == null || !__instance.HasEitrShards(recipe) || StartProcess == null)
            {
                return;
            }

            float duration = Player.m_localPlayer != null && Player.m_localPlayer.InGodMode()
                ? 5f
                : recipe.CraftingTime;
            StartProcess.Invoke(__instance, new object[] { duration });
            Plugin.Log($"Altar imbue: started ({recipe.SourceItemDrop}, {duration:0}s).");
        }
    }
}
