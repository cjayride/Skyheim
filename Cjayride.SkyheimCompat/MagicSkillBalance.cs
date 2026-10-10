using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;

namespace Cjayride.SkyheimCompat
{
    internal static class MagicSkillBalance
    {
        private static readonly Skills.SkillType[] MagicSkills =
        {
            (Skills.SkillType)200, // Nature
            (Skills.SkillType)201, // Holy
            (Skills.SkillType)202, // Fire
            (Skills.SkillType)203  // Frost
        };

        private static ConfigEntry<float> _gain;
        private static ConfigEntry<int> _loss;

        internal static void Bind(ConfigFile config)
        {
            _gain = config.Bind(
                "Magic skills",
                "Skill Experience Gain Factor",
                1f,
                new ConfigDescription(
                    "Factor for experience gained for the rune magic skills.",
                    new AcceptableValueRange<float>(0.01f, 5f)));
            _loss = config.Bind(
                "Magic skills",
                "Skill Experience Loss",
                0,
                new ConfigDescription(
                    "How much experience to lose in the rune magic skills on death.",
                    new AcceptableValueRange<int>(0, 100)));
        }

        private static bool IsMagic(Skills.SkillType type)
        {
            int id = (int)type;
            return id >= 200 && id <= 203;
        }

        [HarmonyPatch(typeof(Skills), nameof(Skills.GetSkillDef))]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        private static void GetSkillDef_Postfix(Skills.SkillType type, Skills.SkillDef __result)
        {
            if (__result != null && IsMagic(type) && _gain != null)
            {
                __result.m_increseStep = _gain.Value;
            }
        }

        [HarmonyPatch(typeof(Skills), nameof(Skills.OnDeath))]
        [HarmonyPriority(Priority.First)]
        [HarmonyPrefix]
        private static void OnDeath_Prefix(Skills __instance, ref Dictionary<Skills.SkillType, Skills.Skill> __state)
        {
            __state = new Dictionary<Skills.SkillType, Skills.Skill>();
            if (__instance?.m_skillData == null || _loss == null)
            {
                return;
            }

            foreach (Skills.SkillType type in MagicSkills)
            {
                if (!__instance.m_skillData.TryGetValue(type, out Skills.Skill skill))
                {
                    continue;
                }

                __state[type] = skill;
                if (_loss.Value > 0)
                {
                    skill.m_level -= skill.m_level * _loss.Value / 100f;
                    skill.m_accumulator = 0f;
                }

                __instance.m_skillData.Remove(type);
            }
        }

        [HarmonyPatch(typeof(Skills), nameof(Skills.OnDeath))]
        [HarmonyFinalizer]
        private static void OnDeath_Finalizer(Skills __instance, ref Dictionary<Skills.SkillType, Skills.Skill> __state)
        {
            if (__state == null || __instance?.m_skillData == null)
            {
                return;
            }

            foreach (KeyValuePair<Skills.SkillType, Skills.Skill> entry in __state)
            {
                __instance.m_skillData[entry.Key] = entry.Value;
            }

            __state = null;
        }
    }
}
