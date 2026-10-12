using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Cjayride.SkyheimCompat
{
    /// <summary>
    /// Skyheim effect clips play at the prefab volume, which is much louder than the rest of Valheim.
    /// Each distinct sound gets an Audio slider, default 0.05.
    /// </summary>
    internal static class SkyheimAudio
    {
        private const float DefaultVolume = 0.5f;

        private static readonly Dictionary<string, ConfigEntry<float>> Volumes =
            new Dictionary<string, ConfigEntry<float>>(StringComparer.Ordinal);

        private static readonly HashSet<int> Scaled = new HashSet<int>();
        private static readonly HashSet<int> BundleObjects = new HashSet<int>();

        private static int _depth;

        internal static void Bind(ConfigFile config)
        {
            Discover(config);
        }

        internal static void Apply(Harmony harmony)
        {
            foreach (Type nested in typeof(ChainAoe).GetNestedTypes(BindingFlags.NonPublic))
            {
                if (nested.Name.IndexOf("StaggerVisuals", StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                MethodInfo moveNext = nested.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (moveNext == null)
                {
                    continue;
                }

                harmony.Patch(moveNext, prefix: new HarmonyMethod(typeof(SkyheimAudio), nameof(Enter)), finalizer: new HarmonyMethod(typeof(SkyheimAudio), nameof(Exit)));
            }
        }

        internal static void Enter()
        {
            _depth++;
        }

        internal static void Exit()
        {
            if (_depth > 0)
            {
                _depth--;
            }
        }

        internal static void ScaleObject(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            ZSFX[] sounds = instance.GetComponentsInChildren<ZSFX>(true);
            for (int i = 0; i < sounds.Length; i++)
            {
                Scale(sounds[i]);
            }
        }

        private static void Discover(ConfigFile config)
        {
            if (AssetManager.AssetBundleObjects == null)
            {
                return;
            }

            for (int i = 0; i < AssetManager.AssetBundleObjects.Length; i++)
            {
                if (AssetManager.AssetBundleObjects[i] is GameObject go)
                {
                    RememberBundleObject(go);
                }
            }

            for (int i = 0; i < AssetManager.AssetBundleObjects.Length; i++)
            {
                if (!(AssetManager.AssetBundleObjects[i] is GameObject root))
                {
                    continue;
                }

                ZSFX[] sounds = root.GetComponentsInChildren<ZSFX>(true);
                for (int s = 0; s < sounds.Length; s++)
                {
                    EnsureMark(sounds[s]);
                    BindVolume(config, sounds[s].gameObject.name);
                }

                Component[] components = root.GetComponentsInChildren<Component>(true);
                for (int c = 0; c < components.Length; c++)
                {
                    Component component = components[c];
                    if (component == null)
                    {
                        continue;
                    }

                    FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    for (int f = 0; f < fields.Length; f++)
                    {
                        if (fields[f].FieldType != typeof(EffectList))
                        {
                            continue;
                        }

                        if (fields[f].GetValue(component) is EffectList list)
                        {
                            RegisterEffectList(config, list);
                        }
                    }
                }
            }

            BindVolume(config, "vfx_Potion_health_medium");
            BindVolume(config, "vfx_Heal");
        }

        private static void RememberBundleObject(GameObject go)
        {
            BundleObjects.Add(go.GetInstanceID());
            Transform[] children = go.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                BundleObjects.Add(children[i].gameObject.GetInstanceID());
            }
        }

        private static void RegisterEffectList(ConfigFile config, EffectList list)
        {
            if (list?.m_effectPrefabs == null)
            {
                return;
            }

            for (int i = 0; i < list.m_effectPrefabs.Length; i++)
            {
                GameObject prefab = list.m_effectPrefabs[i].m_prefab;
                if (prefab == null)
                {
                    continue;
                }

                ZSFX[] sounds = prefab.GetComponentsInChildren<ZSFX>(true);
                if (sounds.Length == 0)
                {
                    BindVolume(config, prefab.name);
                    continue;
                }

                bool owned = BundleObjects.Contains(prefab.GetInstanceID());
                for (int s = 0; s < sounds.Length; s++)
                {
                    if (owned || BundleObjects.Contains(sounds[s].gameObject.GetInstanceID()))
                    {
                        EnsureMark(sounds[s]);
                    }

                    BindVolume(config, sounds[s].gameObject.name);
                }
            }
        }

        private static void EnsureMark(ZSFX sound)
        {
            if (sound != null && sound.GetComponent<SkyheimSfxMark>() == null)
            {
                sound.gameObject.AddComponent<SkyheimSfxMark>();
            }
        }

        private static void BindVolume(ConfigFile config, string objectName)
        {
            if (string.IsNullOrEmpty(objectName) || Volumes.ContainsKey(objectName))
            {
                return;
            }

            Volumes[objectName] = config.Bind(
                "Audio",
                objectName,
                DefaultVolume,
                new ConfigDescription(
                    "Volume scale for this Skyheim sound. 1 is the prefab's full volume.",
                    new AcceptableValueRange<float>(0f, 1f)));
        }

        private static void Scale(ZSFX sound)
        {
            if (sound == null || !Scaled.Add(sound.GetInstanceID()))
            {
                return;
            }

            if (!Volumes.TryGetValue(sound.gameObject.name, out ConfigEntry<float> entry) || entry == null)
            {
                if (Plugin.Instance != null)
                {
                    BindVolume(Plugin.Instance.Config, sound.gameObject.name);
                }

                if (!Volumes.TryGetValue(sound.gameObject.name, out entry) || entry == null)
                {
                    return;
                }
            }

            float volume = Mathf.Clamp01(entry.Value);
            sound.m_minVol *= volume;
            sound.m_maxVol *= volume;
            AudioSource source = sound.GetComponent<AudioSource>();
            if (source != null)
            {
                source.volume *= volume;
            }
        }

        private static bool ShouldScale(ZSFX sound)
        {
            if (sound == null)
            {
                return false;
            }

            if (sound.GetComponent<SkyheimSfxMark>() != null)
            {
                return true;
            }

            return _depth > 0;
        }

        [HarmonyPatch(typeof(ZSFX), "Play")]
        private static class ZSFX_Play_Patch
        {
            private static void Prefix(ZSFX __instance)
            {
                if (ShouldScale(__instance))
                {
                    Scale(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(Attack), nameof(Attack.Start))]
        [HarmonyPatch(typeof(Attack), nameof(Attack.OnAttackTrigger))]
        [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
        [HarmonyPatch(typeof(Attack), nameof(Attack.ProjectileAttackTriggered))]
        [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]
        [HarmonyPatch(typeof(Attack), nameof(Attack.DoAreaAttack))]
        private static class Attack_AudioScope_Patch
        {
            private static void Prefix(Attack __instance, ref bool __state)
            {
                __state = IsSkyheimWeapon(__instance);
                if (__state)
                {
                    Enter();
                }
            }

            private static void Finalizer(bool __state)
            {
                if (__state)
                {
                    Exit();
                }
            }
        }

        [HarmonyPatch(typeof(Aoe), "OnHit")]
        private static class Aoe_OnHit_AudioScope_Patch
        {
            private static void Prefix(Aoe __instance, ref bool __state)
            {
                __state = IsSkyheimObject(__instance != null ? __instance.gameObject : null);
                if (__state)
                {
                    Enter();
                }
            }

            private static void Finalizer(bool __state)
            {
                if (__state)
                {
                    Exit();
                }
            }
        }

        [HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.Setup))]
        private static class StatusEffect_Setup_AudioScope_Patch
        {
            private static void Prefix(StatusEffect __instance, ref bool __state)
            {
                __state = IsSkyheimStatus(__instance);
                if (__state)
                {
                    Enter();
                }
            }

            private static void Finalizer(bool __state)
            {
                if (__state)
                {
                    Exit();
                }
            }
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
        private static class Projectile_OnHit_AudioScope_Patch
        {
            private static void Prefix(Projectile __instance, ref bool __state)
            {
                __state = IsSkyheimObject(__instance != null ? __instance.gameObject : null);
                if (__state)
                {
                    Enter();
                }
            }

            private static void Finalizer(bool __state)
            {
                if (__state)
                {
                    Exit();
                }
            }
        }

        private static bool IsSkyheimWeapon(Attack attack)
        {
            if (!(attack?.m_character is Humanoid humanoid))
            {
                return false;
            }

            ItemDrop.ItemData weapon = humanoid.GetCurrentWeapon();
            return IsSkyheimObject(weapon?.m_dropPrefab);
        }

        private static bool IsSkyheimStatus(StatusEffect effect)
        {
            if (effect == null)
            {
                return false;
            }

            string assembly = effect.GetType().Assembly.GetName().Name ?? string.Empty;
            return assembly.IndexOf("skyheim", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsSkyheimObject(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            string name = go.name ?? string.Empty;
            if (name.StartsWith("rune_", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("sh_", StringComparison.OrdinalIgnoreCase)
                || name.IndexOf("skyheim", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            MonoBehaviour[] behaviours = go.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                string assembly = behaviour.GetType().Assembly.GetName().Name ?? string.Empty;
                if (assembly.IndexOf("skyheim", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal sealed class SkyheimSfxMark : MonoBehaviour
    {
    }
}
