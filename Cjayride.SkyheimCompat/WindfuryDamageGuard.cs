using System.Reflection;
using HarmonyLib;
using skyheim;
using UnityEngine;

namespace Cjayride.SkyheimCompat
{
    internal static class WindfuryDamageGuard
    {
        private static readonly FieldInfo WindfuryHash =
            AccessTools.Field(typeof(SkyheimPlugin), "_statusEffectWindfury");

        internal static void RemoveOriginal(Harmony unused)
        {
            Unpatch(typeof(Character), nameof(Character.Damage), HarmonyPatchType.Postfix, "SkyheimPlugin");
            Unpatch(typeof(HitData), nameof(HitData.SetAttacker), HarmonyPatchType.Postfix, "SkyheimPlugin");
            MethodInfo rpc = AccessTools.DeclaredMethod(typeof(Character), "RPC_Damage");
            if (rpc != null)
            {
                Unpatch(typeof(Character), "RPC_Damage", HarmonyPatchType.Prefix, "SkyheimPlugin");
            }
        }

        private static void Unpatch(System.Type originalType, string methodName, HarmonyPatchType kind, string declaringTypeName)
        {
            MethodInfo original = AccessTools.DeclaredMethod(originalType, methodName);
            if (original == null)
            {
                return;
            }

            Patches info = Harmony.GetPatchInfo(original);
            if (info == null)
            {
                return;
            }

            System.Collections.Generic.IEnumerable<Patch> patches =
                kind == HarmonyPatchType.Prefix ? info.Prefixes : info.Postfixes;
            foreach (Patch patch in patches)
            {
                if (patch.PatchMethod?.DeclaringType?.Name != declaringTypeName)
                {
                    continue;
                }

                new Harmony(patch.owner).Unpatch(original, patch.PatchMethod);
                Plugin.Log($"Removed Skyheim {declaringTypeName} {originalType.Name}.{methodName} null crash.");
            }
        }

        internal static int WindfuryStatusHash()
        {
            return WindfuryHash == null ? 0 : (int)WindfuryHash.GetValue(null);
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
    internal static class Character_Damage_Windfury_Patch
    {
        private static void Postfix(Character __instance, HitData hit)
        {
            if (!__instance || hit == null || !__instance.m_nview || !__instance.m_nview.IsValid())
            {
                return;
            }

            Character attacker = hit.GetAttacker();
            if (!attacker || attacker != Player.m_localPlayer)
            {
                return;
            }

            SEMan seman = attacker.GetSEMan();
            int hash = WindfuryDamageGuard.WindfuryStatusHash();
            if (seman == null || hash == 0 || !seman.HaveStatusEffect(hash))
            {
                return;
            }

            SE_Windfury windfury = seman.GetStatusEffect(hash) as SE_Windfury;
            if (windfury == null)
            {
                return;
            }

            windfury.OnDamageDealt(__instance, hit);
        }
    }

    [HarmonyPatch(typeof(HitData), nameof(HitData.SetAttacker))]
    internal static class HitData_SetAttacker_Patch
    {
        private static void Postfix(HitData __instance, Character attacker)
        {
            if (!attacker || !attacker.IsPlayer())
            {
                return;
            }

            Player player = attacker as Player;
            ItemDrop.ItemData weapon = player != null ? player.GetCurrentWeapon() : null;
            if (weapon?.m_dropPrefab == null)
            {
                return;
            }

            SkyheimItemData data = weapon.m_dropPrefab.GetComponent<SkyheimItemData>();
            if (data != null && data.Beneficial)
            {
                __instance.m_attacker = ZDOID.None;
            }
        }
    }

    [HarmonyPatch(typeof(Character), "RPC_Damage")]
    internal static class Character_RPC_Damage_Patch
    {
        private static void Prefix(Character __instance, HitData hit)
        {
            if (!__instance || hit == null || hit.m_statusEffectHash == 0 || __instance.m_seman == null)
            {
                return;
            }

            __instance.m_seman.RemoveStatusEffect(hit.m_statusEffectHash, true);
        }
    }
}
