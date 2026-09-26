using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Cjayride.SkyheimCompat
{
    internal static class RuneConfig
    {
        private static readonly Dictionary<string, RuneBindings> Bindings = new Dictionary<string, RuneBindings>(StringComparer.OrdinalIgnoreCase);
        private static bool _bound;

        internal static void DiscoverAndApply()
        {
            if (AssetManager.AssetBundleObjects == null || AssetManager.AssetBundleObjects.Length == 0)
            {
                return;
            }

            ConfigFile config = Plugin.Instance.Config;
            int count = 0;
            foreach (SkyheimItemData rune in AssetManager.GetGameObjects<SkyheimItemData>())
            {
                ItemDrop itemDrop = rune.GetComponent<ItemDrop>();
                if (itemDrop == null || itemDrop.m_itemData?.m_shared == null)
                {
                    continue;
                }

                string section = rune.gameObject.name;
                if (!Bindings.TryGetValue(section, out RuneBindings bind))
                {
                    bind = BindRune(config, section, rune, itemDrop);
                    bind.Enabled.SettingChanged += (_, __) => ApplyAll();
                    bind.Cooldown.SettingChanged += (_, __) => ApplyAll();
                    bind.ManaDrain.SettingChanged += (_, __) => ApplyAll();
                    bind.AttackEitr.SettingChanged += (_, __) => ApplyAll();
                    bind.AttackStamina.SettingChanged += (_, __) => ApplyAll();
                    bind.DamageBlunt.SettingChanged += (_, __) => ApplyAll();
                    bind.DamageSlash.SettingChanged += (_, __) => ApplyAll();
                    bind.DamagePierce.SettingChanged += (_, __) => ApplyAll();
                    bind.DamageFire.SettingChanged += (_, __) => ApplyAll();
                    bind.DamageFrost.SettingChanged += (_, __) => ApplyAll();
                    bind.DamageLightning.SettingChanged += (_, __) => ApplyAll();
                    bind.DamagePoison.SettingChanged += (_, __) => ApplyAll();
                    bind.DamageSpirit.SettingChanged += (_, __) => ApplyAll();
                    bind.DamageChop.SettingChanged += (_, __) => ApplyAll();
                    bind.AttackRange.SettingChanged += (_, __) => ApplyAll();
                    Bindings[section] = bind;
                }

                Apply(bind, rune, itemDrop);
                count++;
            }

            if (!_bound && count > 0)
            {
                _bound = true;
                Plugin.Log($"Skyheim rune config ready for {count} runes -> BepInEx/config/{Plugin.PluginGUID}.cfg");
            }
        }

        private static RuneBindings BindRune(ConfigFile config, string section, SkyheimItemData rune, ItemDrop itemDrop)
        {
            ItemDrop.ItemData.SharedData shared = itemDrop.m_itemData.m_shared;
            Attack attack = shared.m_attack;
            HitData.DamageTypes damages = shared.m_damages;

            string pretty = Localization.instance != null
                ? Localization.instance.Localize(shared.m_name)
                : section;

            return new RuneBindings
            {
                Enabled = config.Bind(section, "Enabled", true, $"Show/craft {pretty} ({section})."),
                Cooldown = config.Bind(section, "Cooldown", rune.Cooldown, $"Cooldown in seconds for {pretty}."),
                ManaDrain = config.Bind(section, "EitrDrainWhileHeld", rune.ManaDrain, $"Eitr drained per second while {pretty} is held."),
                AttackEitr = config.Bind(section, "AttackEitr", attack != null ? attack.m_attackEitr : 0f, $"Eitr cost to cast {pretty}."),
                AttackStamina = config.Bind(section, "AttackStamina", attack != null ? attack.m_attackStamina : 0f, $"Stamina cost to cast {pretty}."),
                AttackRange = config.Bind(section, "AttackRange", attack != null ? attack.m_attackRange : 0f, $"Attack/projectile range for {pretty}."),
                DamageBlunt = config.Bind(section, "DamageBlunt", damages.m_blunt, ""),
                DamageSlash = config.Bind(section, "DamageSlash", damages.m_slash, ""),
                DamagePierce = config.Bind(section, "DamagePierce", damages.m_pierce, ""),
                DamageFire = config.Bind(section, "DamageFire", damages.m_fire, ""),
                DamageFrost = config.Bind(section, "DamageFrost", damages.m_frost, ""),
                DamageLightning = config.Bind(section, "DamageLightning", damages.m_lightning, ""),
                DamagePoison = config.Bind(section, "DamagePoison", damages.m_poison, ""),
                DamageSpirit = config.Bind(section, "DamageSpirit", damages.m_spirit, ""),
                DamageChop = config.Bind(section, "DamageChop", damages.m_chop, "")
            };
        }

        private static void ApplyAll()
        {
            foreach (SkyheimItemData rune in AssetManager.GetGameObjects<SkyheimItemData>())
            {
                if (!Bindings.TryGetValue(rune.gameObject.name, out RuneBindings bind))
                {
                    continue;
                }

                ItemDrop itemDrop = rune.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    Apply(bind, rune, itemDrop);
                }
            }
        }

        private static void Apply(RuneBindings bind, SkyheimItemData rune, ItemDrop itemDrop)
        {
            rune.Cooldown = bind.Cooldown.Value;
            rune.ManaDrain = bind.ManaDrain.Value;

            ItemDrop.ItemData.SharedData shared = itemDrop.m_itemData.m_shared;
            if (shared.m_attack != null)
            {
                shared.m_attack.m_attackEitr = bind.AttackEitr.Value;
                shared.m_attack.m_attackStamina = bind.AttackStamina.Value;
                shared.m_attack.m_attackRange = bind.AttackRange.Value;
            }

            HitData.DamageTypes damages = shared.m_damages;
            damages.m_blunt = bind.DamageBlunt.Value;
            damages.m_slash = bind.DamageSlash.Value;
            damages.m_pierce = bind.DamagePierce.Value;
            damages.m_fire = bind.DamageFire.Value;
            damages.m_frost = bind.DamageFrost.Value;
            damages.m_lightning = bind.DamageLightning.Value;
            damages.m_poison = bind.DamagePoison.Value;
            damages.m_spirit = bind.DamageSpirit.Value;
            damages.m_chop = bind.DamageChop.Value;
            shared.m_damages = damages;

            if (ObjectDB.instance?.m_recipes == null)
            {
                return;
            }

            foreach (Recipe recipe in ObjectDB.instance.m_recipes)
            {
                if (recipe != null && recipe.m_item == itemDrop)
                {
                    recipe.m_enabled = bind.Enabled.Value;
                }
            }
        }

        private sealed class RuneBindings
        {
            public ConfigEntry<bool> Enabled;
            public ConfigEntry<float> Cooldown;
            public ConfigEntry<float> ManaDrain;
            public ConfigEntry<float> AttackEitr;
            public ConfigEntry<float> AttackStamina;
            public ConfigEntry<float> AttackRange;
            public ConfigEntry<float> DamageBlunt;
            public ConfigEntry<float> DamageSlash;
            public ConfigEntry<float> DamagePierce;
            public ConfigEntry<float> DamageFire;
            public ConfigEntry<float> DamageFrost;
            public ConfigEntry<float> DamageLightning;
            public ConfigEntry<float> DamagePoison;
            public ConfigEntry<float> DamageSpirit;
            public ConfigEntry<float> DamageChop;
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    [HarmonyPriority(Priority.Last)]
    internal static class ObjectDB_Awake_RuneConfig_Patch
    {
        private static void Postfix()
        {
            RuneConfig.DiscoverAndApply();
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [HarmonyPriority(Priority.Last)]
    internal static class ZNetScene_Awake_RuneConfig_Patch
    {
        private static void Postfix()
        {
            RuneConfig.DiscoverAndApply();
        }
    }
}
