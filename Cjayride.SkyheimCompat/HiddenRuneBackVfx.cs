using BepInEx.Configuration;
using HarmonyLib;
using skyheim;
using UnityEngine;

namespace Cjayride.SkyheimCompat
{
    /// <summary>
    /// Hide Weapon (R) moves the rune to the back/shoulder via VisEquipment.AttachBackItem.
    /// Rune prefabs carry held-state VFX that still run on that attachment and look wrong (e.g. Rune of Light).
    /// </summary>
    internal static class HiddenRuneBackVfx
    {
        private static ConfigEntry<bool> _showWhenHidden;

        internal static void Bind(ConfigFile config)
        {
            _showWhenHidden = config.Bind(
                "Runes",
                "Show Vfx When Weapon Hidden",
                false,
                "Spawn rune glow/particles on your back when the rune is hidden with R (Hide Weapon). Off by default. Does not change VFX while the rune is held in hand. Unequipping via the hotbar is unchanged.");
        }

        internal static void AfterAttach(GameObject attached, int itemHash)
        {
            if (attached == null || !IsSkyheimRune(itemHash))
            {
                return;
            }

            if (_showWhenHidden != null && _showWhenHidden.Value)
            {
                VisEquipment.EnableEquippedEffects(attached);
                return;
            }

            StripHeldVfx(attached);
        }

        private static bool IsSkyheimRune(int itemHash)
        {
            if (itemHash == 0 || ObjectDB.instance == null)
            {
                return false;
            }

            GameObject prefab = ObjectDB.instance.GetItemPrefab(itemHash);
            return prefab != null && prefab.GetComponent<SkyheimItemData>() != null;
        }

        private static void StripHeldVfx(GameObject root)
        {
            Transform equiped = root.transform.Find("equiped");
            if (equiped != null)
            {
                equiped.gameObject.SetActive(false);
            }

            Light[] lights = root.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].enabled = false;
            }

            AudioSource[] audio = root.GetComponentsInChildren<AudioSource>(true);
            for (int i = 0; i < audio.Length; i++)
            {
                audio[i].enabled = false;
            }

            ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles[i].gameObject.SetActive(false);
            }

            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform t = transforms[i];
                if (t == root.transform)
                {
                    continue;
                }

                string name = t.name;
                if (name.StartsWith("vfx_", System.StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("fx_", System.StringComparison.OrdinalIgnoreCase))
                {
                    t.gameObject.SetActive(false);
                }
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.AttachBackItem))]
    internal static class VisEquipment_AttachBackItem_HiddenRuneVfx_Patch
    {
        private static void Postfix(GameObject __result, int hash)
        {
            HiddenRuneBackVfx.AfterAttach(__result, hash);
        }
    }
}
