using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Cjayride.SkyheimCompat
{
    /// <summary>
    /// Windfury proc VFX/SFX were authored with EffectList variant 0 (male body only).
    /// ApplyOnHitEffect passes Player.GetPlayerModel(), so female (1) never spawned them.
    /// -1 is Valheim's "any body" variant.
    /// </summary>
    [HarmonyPatch(typeof(SE_Windfury), "ApplyOnHitEffect")]
    internal static class WindfuryVfxFix
    {
        private static readonly FieldInfo OnHitEffects =
            AccessTools.Field(typeof(SE_Windfury), "_onDamageDealtEffects");

        private static void Prefix(SE_Windfury __instance)
        {
            if (__instance == null || OnHitEffects == null)
            {
                return;
            }

            EffectList list = OnHitEffects.GetValue(__instance) as EffectList;
            if (list?.m_effectPrefabs == null)
            {
                return;
            }

            bool changed = false;
            for (int i = 0; i < list.m_effectPrefabs.Length; i++)
            {
                if (list.m_effectPrefabs[i].m_variant == -1)
                {
                    continue;
                }

                list.m_effectPrefabs[i].m_variant = -1;
                changed = true;
            }

            if (changed)
            {
                Plugin.Log("Windfury proc VFX/SFX set to play on male and female player models.");
            }
        }
    }
}
