using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using skyheim;
using UnityEngine;
using UnityEngine.UI;

namespace Cjayride.SkyheimCompat
{
    internal static class SenealCooldownOverlay
    {
        private static FieldInfo _hotbarSlots;
        private static FieldInfo _hotbarActions;
        private static FieldInfo _slotRoot;
        private static MethodInfo _actionRow;
        private static bool _loggedHotbar;
        private static bool _loggedMissing;

        // Tick runs every frame; these avoid allocating a delegate and an argument array per call.
        private static readonly Func<int, Vector2i> HotbarLocation = i => new Vector2i(i, 0);
        private static readonly object[] ActionRowArgs = new object[1];
        private static int _lastActionRow = -1;
        private static Func<int, Vector2i> _actionLocation;

        internal static void Init(Harmony harmony)
        {
            if (!Plugin.SenealUiLoaded())
            {
                return;
            }

            Type hotbarView = AccessTools.TypeByName("SeneaLUI.Hud.HotbarView");
            Type slotType = AccessTools.TypeByName("SeneaLUI.Components.Slot");
            Type slotsType = AccessTools.TypeByName("SeneaLUI.Inv.Slots");
            if (hotbarView == null || slotType == null)
            {
                Plugin.LogWarning("SeneaL UI types were not found; rune cooldown overlay on the custom HUD is unavailable.");
                return;
            }

            _hotbarSlots = AccessTools.Field(hotbarView, "_slots");
            _hotbarActions = AccessTools.Field(hotbarView, "_act");
            _slotRoot = AccessTools.Field(slotType, "Root");
            _actionRow = AccessTools.Method(slotsType, "ActionRow", new[] { typeof(Inventory) });

            MethodInfo tick = AccessTools.Method(hotbarView, "Tick");
            if (tick != null)
            {
                harmony.Patch(tick, postfix: new HarmonyMethod(typeof(SenealCooldownOverlay), nameof(HotbarTick_Postfix)));
            }
        }

        private static void HotbarTick_Postfix(object __instance)
        {
            try
            {
                Player player = Player.m_localPlayer;
                Inventory inv = player != null ? player.GetInventory() : null;
                if (inv == null || __instance == null)
                {
                    return;
                }

                AttachSlotArray(_hotbarSlots?.GetValue(__instance) as Array, HotbarLocation);

                if (_actionRow != null)
                {
                    ActionRowArgs[0] = inv;
                    int actionRow = (int)_actionRow.Invoke(null, ActionRowArgs);
                    if (actionRow != _lastActionRow)
                    {
                        _lastActionRow = actionRow;
                        int row = actionRow;
                        _actionLocation = i => new Vector2i(i, row);
                    }

                    AttachSlotArray(_hotbarActions?.GetValue(__instance) as Array, _actionLocation);
                }

                if (!_loggedHotbar)
                {
                    _loggedHotbar = true;
                    Plugin.Log("Attached Skyheim rune cooldown overlays to SeneaL hotbar and action slots.");
                }
            }
            catch (Exception ex)
            {
                if (!_loggedMissing)
                {
                    _loggedMissing = true;
                    Plugin.LogWarning("Could not attach Skyheim cooldown overlays to SeneaL slots: " + ex.Message);
                }
            }
        }

        private static void AttachSlotArray(Array slots, Func<int, Vector2i> locationForIndex)
        {
            if (slots == null || _slotRoot == null)
            {
                return;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                object slot = slots.GetValue(i);
                if (slot == null)
                {
                    continue;
                }

                RectTransform root = _slotRoot.GetValue(slot) as RectTransform;
                if (root)
                {
                    CooldownPrefab.EnsureOn(root, locationForIndex(i));
                }
            }
        }
    }

    internal static class CooldownPrefab
    {
        private static readonly FieldInfo LocationField =
            AccessTools.Field(typeof(SkyheimCooldownItem), "<Location>k__BackingField");

        // HotkeyBar.Update and InventoryGrid.UpdateGui run every frame, and every slot used to
        // walk its whole child hierarchy looking for the overlay. One cache lookup per slot per
        // frame replaces that walk, which is where the UI frame cost was.
        private static readonly Dictionary<Transform, SkyheimCooldownItem> Attached =
            new Dictionary<Transform, SkyheimCooldownItem>();

        private static FieldInfo _prefabField;
        private static GameObject _prefab;

        internal static Vector2i GetLocation(SkyheimCooldownItem item)
        {
            return item == null ? new Vector2i(-1, -1) : item.Location;
        }

        internal static void SetLocation(SkyheimCooldownItem item, Vector2i location)
        {
            LocationField?.SetValue(item, location);
        }

        internal static GameObject Get()
        {
            if (SkyheimCooldown.Instance == null)
            {
                return null;
            }

            if (_prefab)
            {
                return _prefab;
            }

            _prefabField = _prefabField ?? AccessTools.Field(typeof(SkyheimCooldown), "_cooldownPrefab");
            _prefab = _prefabField?.GetValue(SkyheimCooldown.Instance) as GameObject;
            return _prefab;
        }

        internal static void EnsureOn(Transform parent, Vector2i location)
        {
            if (!parent)
            {
                return;
            }

            if (Attached.TryGetValue(parent, out SkyheimCooldownItem cached) && cached)
            {
                Reposition(cached, location);
                return;
            }

            SkyheimCooldownItem existing = parent.GetComponentInChildren<SkyheimCooldownItem>(true);
            if (existing)
            {
                Attached[parent] = existing;
                Reposition(existing, location);
                return;
            }

            GameObject prefab = Get();
            if (!prefab)
            {
                return;
            }

            GameObject clone = UnityEngine.Object.Instantiate(prefab, parent);
            clone.name = "skyheim_cooldown";
            Stretch(clone.transform as RectTransform);
            clone.SetActive(true);

            SkyheimCooldownItem item = clone.GetComponent<SkyheimCooldownItem>()
                ?? clone.GetComponentInChildren<SkyheimCooldownItem>(true);
            if (item == null)
            {
                return;
            }

            Attached[parent] = item;
            SetLocation(item, location);
            if (SkyheimCooldown.Instance != null)
            {
                SkyheimCooldown.Instance.RegisterUpdateable(item);
            }
        }

        private static void Reposition(SkyheimCooldownItem existing, Vector2i location)
        {
            GameObject existingGo = existing.gameObject;
            if (existingGo && !existingGo.activeSelf)
            {
                existingGo.SetActive(true);
            }

            Vector2i current = GetLocation(existing);
            if (current.x != location.x || current.y != location.y)
            {
                SetLocation(existing, location);
                if (SkyheimCooldown.Instance != null)
                {
                    SkyheimCooldown.Instance.RegisterUpdateable(existing);
                }
            }
        }

        private static void Stretch(RectTransform rt)
        {
            if (!rt)
            {
                return;
            }

            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.SetAsLastSibling();
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
    internal static class InventoryGrid_UpdateGui_Cooldown_Patch
    {
        private static void Postfix(InventoryGrid __instance)
        {
            if (__instance == null || __instance.m_elements == null)
            {
                return;
            }

            foreach (InventoryElement element in __instance.m_elements)
            {
                if (!element)
                {
                    continue;
                }

                CooldownPrefab.EnsureOn(element.transform, element.Position);
            }
        }
    }

    [HarmonyPatch(typeof(SkyheimCooldown), "Update")]
    internal static class SkyheimCooldown_Update_Patch
    {
        private static readonly FieldInfo Updateables =
            AccessTools.Field(typeof(SkyheimCooldown), "_updateables");

        private static void Prefix(SkyheimCooldown __instance)
        {
            if (Updateables?.GetValue(__instance) is not List<SkyheimCooldownItem> list)
            {
                return;
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!list[i])
                {
                    list.RemoveAt(i);
                }
            }
        }
    }

    [HarmonyPatch(typeof(SkyheimCooldownItem), nameof(SkyheimCooldownItem.Awake))]
    internal static class SkyheimCooldownItem_Awake_Patch
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(HotkeyBar), "Update")]
    internal static class HotkeyBar_Update_Cooldown_Patch
    {
        private static void Postfix(HotkeyBar __instance)
        {
            if (Plugin.SenealUiLoaded() || __instance == null || __instance.m_elements == null)
            {
                return;
            }

            for (int i = 0; i < __instance.m_elements.Count; i++)
            {
                GameObject slot = __instance.m_elements[i].m_go;
                if (slot)
                {
                    CooldownPrefab.EnsureOn(slot.transform, new Vector2i(i, 0));
                }
            }
        }
    }

    [HarmonyPatch(typeof(SkyheimCooldownItem), nameof(SkyheimCooldownItem.SetCooldown))]
    internal static class SkyheimCooldownItem_SetCooldown_Patch
    {
        private static readonly FieldInfo Swipe = AccessTools.Field(typeof(SkyheimCooldownItem), "_swipe");
        private static readonly FieldInfo Label = AccessTools.Field(typeof(SkyheimCooldownItem), "_text");

        private static bool Prefix(SkyheimCooldownItem __instance)
        {
            if (!__instance)
            {
                return false;
            }

            return Swipe?.GetValue(__instance) is Image && Label?.GetValue(__instance) is Text;
        }
    }
}
