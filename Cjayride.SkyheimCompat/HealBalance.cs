using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using skyheim;
using UnityEngine;

namespace Cjayride.SkyheimCompat
{
    internal static class HealBalance
    {
        private const float TickInterval = 1f;
        private const string RuneHealNameToken = "rune_heal";
        private static readonly int HealRuneStatusHash = "se_rune_heal".GetStableHashCode();

        private static ConfigEntry<float> _totalMultiplier;
        private static ConfigEntry<float> _healDurationSeconds;
        private static ConfigEntry<float> _durationPerQuality;
        private static ConfigEntry<float> _minIconSeconds;
        private static ConfigEntry<bool> _showHealVfx;

        private static readonly FieldInfo BaseHealth = AccessTools.Field(typeof(SE_Heal), "_baseHealth");
        private static readonly FieldInfo HealthPerQuality = AccessTools.Field(typeof(SE_Heal), "_healthPerQuality");
        private static readonly FieldInfo DurationPerSkill = AccessTools.Field(typeof(SE_Heal), "_durationPerSkill");

        internal static void Bind(ConfigFile config)
        {
            _totalMultiplier = config.Bind(
                "Rune healing",
                "Total Heal Multiplier",
                0.9f,
                new ConfigDescription(
                    "Fraction of Skyheim's healing rune HP. 1 = 40 HP at quality 1 before Nature Magic; 0.9 is about 36 HP. Nature Magic adds +1% total heal per skill point (100 skill = double). Each rune quality still adds 5 HP before this multiplier.",
                    new AcceptableValueRange<float>(0.05f, 2f)));
            _healDurationSeconds = config.Bind(
                "Rune healing",
                "Heal Duration Seconds",
                8f,
                new ConfigDescription(
                    "HoT length at rune quality 1. Higher quality adds Extra Duration Per Quality.",
                    new AcceptableValueRange<float>(1f, 30f)));
            _durationPerQuality = config.Bind(
                "Rune healing",
                "Extra Duration Per Quality",
                2f,
                new ConfigDescription(
                    "Seconds added to the HoT for each rune quality above 1. Quality 1 = Heal Duration Seconds; quality 5 = that plus 8s at the default of 2.",
                    new AcceptableValueRange<float>(0f, 15f)));
            _minIconSeconds = config.Bind(
                "Rune healing",
                "Min Status Icon Seconds",
                10f,
                new ConfigDescription(
                    "How long the healing status icon stays on the effect bar (allies included).",
                    new AcceptableValueRange<float>(3f, 60f)));
            _showHealVfx = config.Bind(
                "Rune healing",
                "Show Heal Vfx On Allies",
                true,
                "Spawn a short heal burst VFX on each character when the healing rune HoT is applied. Its volume is the Audio slider for vfx_Potion_health_medium.");
        }

        internal static bool IsHealRuneItem(ItemDrop.ItemData item)
        {
            if (item?.m_shared == null)
            {
                return false;
            }

            GameObject prefab = item.m_dropPrefab;
            if (prefab != null && prefab.name.StartsWith(RuneHealNameToken, System.StringComparison.Ordinal))
            {
                return !prefab.name.StartsWith("rune_heal_aoe", System.StringComparison.Ordinal);
            }

            string shared = item.m_shared.m_name;
            return shared != null && shared.IndexOf(RuneHealNameToken, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static bool TryComputeHeal(int quality, float skillLevel, out int totalHp, out float hotDuration)
        {
            totalHp = 0;
            hotDuration = 0f;
            if (_totalMultiplier == null)
            {
                return false;
            }

            float baseHealth = 40f;
            float perQuality = 5f;
            float perSkill = 0.01f;
            int q = quality < 1 ? 1 : quality;
            float total = (baseHealth + perQuality * (q - 1)) * (1f + perSkill * skillLevel) * _totalMultiplier.Value;
            if (total <= 0f)
            {
                return false;
            }

            float extraDuration = _durationPerQuality != null ? _durationPerQuality.Value : 2f;
            hotDuration = Mathf.Max(TickInterval, _healDurationSeconds.Value + extraDuration * (q - 1));
            totalHp = Mathf.CeilToInt(total);
            return true;
        }

        internal static string FormatHotDuration(float seconds)
        {
            int total = Mathf.Max(1, Mathf.RoundToInt(seconds));
            if (total >= 60 && total % 60 == 0)
            {
                int minutes = total / 60;
                return minutes == 1 ? "1 minute" : minutes + " minutes";
            }

            if (total >= 60)
            {
                return (total / 60) + " min " + (total % 60) + " sec";
            }

            return total == 1 ? "1 second" : total + " seconds";
        }

        internal static string BuildDetailedHealLine(int totalHp, float hotDuration)
        {
            if (Localization.instance == null)
            {
                return "\nHeals for " + totalHp + " over " + FormatHotDuration(hotDuration) + ".";
            }

            return "\n" + Localization.instance.Localize(
                "$sh_se_rune_heal_desc_detailed",
                totalHp.ToString(),
                FormatHotDuration(hotDuration));
        }

        internal static void AppendItemTooltip(ref string tooltip, int quality, float skillLevel)
        {
            if (string.IsNullOrEmpty(tooltip) || !TryComputeHeal(quality, skillLevel, out int totalHp, out float hotDuration))
            {
                return;
            }

            string line = BuildDetailedHealLine(totalHp, hotDuration);
            if (tooltip.IndexOf(line.Trim(), System.StringComparison.Ordinal) >= 0)
            {
                return;
            }

            tooltip += line;
        }

        internal static bool IsSkyheimHealRune(SE_Heal heal)
        {
            if (heal == null)
            {
                return false;
            }

            if (heal.NameHash() == HealRuneStatusHash)
            {
                return true;
            }

            string name = heal.name ?? string.Empty;
            if (name.IndexOf(RuneHealNameToken, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            string loc = heal.m_name ?? string.Empty;
            return loc.IndexOf("sh_se_rune_heal", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static bool TryApplyPackHeal(SE_Heal instance, int itemLevel, float skillLevel)
        {
            if (instance == null || _totalMultiplier == null)
            {
                return false;
            }

            if (!TryComputeHeal(itemLevel, skillLevel, out int totalHp, out float hotDuration))
            {
                return false;
            }

            float total = totalHp;
            float iconTtl = Mathf.Max(hotDuration, _minIconSeconds.Value);
            int ticks = Mathf.Max(1, Mathf.CeilToInt(hotDuration / TickInterval));
            float tickHp = total / ticks;

            instance.m_ttl = iconTtl;
            instance.m_healthOverTimeDuration = hotDuration;
            instance.m_healthOverTimeInterval = TickInterval;
            instance.m_healthOverTimeTicks = ticks;
            instance.m_healthOverTimeTickHP = tickHp;
            instance.m_healthOverTime = 0f;

            if (instance.m_character != null)
            {
                HealDiagnostics.Log($"SE_Heal applied to {instance.m_character.m_name}: quality {itemLevel}, skill {skillLevel:0.#}, total {total:0.#} HP over {hotDuration:0.#}s ({ticks} ticks of {tickHp:0.##}), hp {instance.m_character.GetHealth():0}/{instance.m_character.GetMaxHealth():0}");
            }

            return true;
        }

        internal static void TryPlayHealVfx(Character character)
        {
            if (_showHealVfx == null || !_showHealVfx.Value || character == null)
            {
                return;
            }

            GameObject prefab = ZNetScene.instance?.GetPrefab("vfx_Potion_health_medium");
            if (prefab == null)
            {
                prefab = ZNetScene.instance?.GetPrefab("vfx_Heal");
            }

            if (prefab == null)
            {
                return;
            }

            SkyheimAudio.Enter();
            try
            {
                Object.Instantiate(prefab, character.GetCenterPoint(), Quaternion.identity);
            }
            finally
            {
                SkyheimAudio.Exit();
            }
        }
    }

    [HarmonyPatch(typeof(SE_Heal), nameof(SE_Heal.SetLevel))]
    internal static class SE_Heal_SetLevel_PackBalance_Patch
    {
        private static bool Prefix(SE_Heal __instance, int itemLevel, float skillLevel)
        {
            if (__instance == null || !HealBalance.IsSkyheimHealRune(__instance))
            {
                return true;
            }

            if (!HealBalance.TryApplyPackHeal(__instance, itemLevel, skillLevel))
            {
                return true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip), new[] { typeof(int) })]
    internal static class ItemDrop_HealRune_Tooltip_Patch
    {
        private static void Postfix(ItemDrop.ItemData __instance, ref string __result)
        {
            if (__instance == null || !HealBalance.IsHealRuneItem(__instance))
            {
                return;
            }

            int quality = __instance.m_quality;
            float skill = 0f;
            if (Player.m_localPlayer != null)
            {
                skill = Player.m_localPlayer.GetSkillLevel((Skills.SkillType)200);
            }

            HealBalance.AppendItemTooltip(ref __result, quality, skill);
        }
    }

    [HarmonyPatch(typeof(SE_Heal), nameof(SE_Heal.Setup))]
    internal static class SE_Heal_Setup_Vfx_Patch
    {
        private static void Postfix(SE_Heal __instance, Character character)
        {
            if (__instance == null || character == null || !HealBalance.IsSkyheimHealRune(__instance))
            {
                return;
            }

            HealBalance.TryPlayHealVfx(character);
        }
    }
}
