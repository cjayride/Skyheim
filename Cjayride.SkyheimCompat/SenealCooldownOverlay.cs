using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
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
        private static bool _loggedStrip;

        // Counted on the pass that logs (Tick runs every frame, so both are reset per pass).
        private static int _attachedCount;
        private static int _slotCount;

        // SeneaL HUD Layout draws its OWN cooldown square + countdown on the SeneaL hotbar and
        // action slots, so 1.2.7 turned Skyheim's overlay there OFF by default. THE LOG PROVED THAT
        // CHANGE MOVED NOTHING (1.2.8 session, 2026-11-11): the strip pass walks every SeneaL
        // hotbar/action slot root every 2s and logged "Removed N ... overlay(s)" ZERO times, and
        // the attach path logged its success line ZERO times - Skyheim's overlay was never
        // attaching to SeneaL UI's slots in the first place (the field lookups no-op on this SeneaL
        // build). The hotbar "ring" and centred number the user saw were therefore NOT Skyheim,
        // and my 1.2.7 attribution was a guess, now falsified by my own log line.
        // 1.2.9 restores the pre-1.2.7 behaviour: default TRUE, attach on the custom HUD exactly as
        // 1.2.6 did. The gate and the strip stay so the behaviour is switchable and, above all,
        // SELF-REPORTING: setting it false prints the removal count, so the claim can be checked.
        private static ConfigEntry<bool> _onSenealHotbar;

        // The strip pass walks each slot's children, so it runs on a 2s cadence, not every frame:
        // clones are only ever created by the attach path above, which is off.
        private static float _nextStrip;

        internal static void Bind(ConfigFile config)
        {
            _onSenealHotbar = config.Bind(
                "Cooldown overlay",
                "OnSenealHotbar",
                true,
                "Show Skyheim's cooldown square and number on SeneaL UI's hotbar and action slots too (the 1.2.6 behaviour; the default). Set false to let SeneaL HUD Layout own that space: Skyheim's overlays are then destroyed on the SeneaL slots and the log prints how many it removed. The inventory grid always keeps its overlays. Client-only.");
        }

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
                if (_onSenealHotbar != null && !_onSenealHotbar.Value)
                {
                    // HUD Layout owns the hotbar cooldown timer: drop any overlay a previous
                    // session (or the config default) left on the SeneaL slot roots.
                    if (Time.unscaledTime >= _nextStrip)
                    {
                        _nextStrip = Time.unscaledTime + 2f;
                        StripSenealHotbarOverlays(__instance);
                    }
                    return;
                }

                Player player = Player.m_localPlayer;
                Inventory inv = player != null ? player.GetInventory() : null;
                if (inv == null || __instance == null)
                {
                    return;
                }

                _attachedCount = 0;
                _slotCount = 0;
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
                    // The count is the point: 1.2.6 attached silently, so "Skyheim's overlay was
                    // the second square/number on the custom HUD" was never checkable from the log
                    // and turned out to be a guess. State exactly how many overlays exist now.
                    Plugin.Log(
                        "Skyheim cooldown overlays on SeneaL UI's hotbar/action slots: " +
                        _attachedCount + " of " + _slotCount + " slots carry one.");
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

        private static void StripSenealHotbarOverlays(object __instance)
        {
            int removed = 0;
            removed += DestroyOnSlotArray(_hotbarSlots?.GetValue(__instance) as Array);
            removed += DestroyOnSlotArray(_hotbarActions?.GetValue(__instance) as Array);

            // Proof over assertion: the strip runs silently, so "Skyheim's overlay was the second
            // square/number on the custom HUD" was not checkable from the log. Say it once, with
            // the count, so the line either appears (Skyheim really was stacking an overlay on the
            // SeneaL slots) or never does (it was not the cause).
            if (removed > 0 && !_loggedStrip)
            {
                _loggedStrip = true;
                Plugin.Log(
                    "Removed " + removed +
                    " Skyheim cooldown overlay(s) from SeneaL UI's hotbar/action slots; " +
                    "SeneaL HUD Layout draws the cooldown square and countdown there. " +
                    "[Cooldown overlay] OnSenealHotbar = true restores Skyheim's own square.");
            }
        }

        private static int DestroyOnSlotArray(Array slots)
        {
            if (slots == null)
            {
                return 0;
            }

            int removed = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                object slot = slots.GetValue(i);
                RectTransform root = slot != null ? _slotRoot?.GetValue(slot) as RectTransform : null;
                if (!root)
                {
                    continue;
                }

                SkyheimCooldownItem existing = root.GetComponentInChildren<SkyheimCooldownItem>(true);
                if (existing)
                {
                    CooldownPrefab.Strip(root);
                    removed++;
                }
            }

            return removed;
        }

        private static void AttachSlotArray(Array slots, Func<int, Vector2i> locationForIndex)
        {
            if (slots == null || _slotRoot == null)
            {
                return;
            }

            _slotCount += slots.Length;
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
                    _attachedCount += CooldownPrefab.EnsureOn(root, locationForIndex(i));
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

        // Removes our overlay from a slot we no longer own (SeneaL HUD Layout draws the hotbar
        // cooldown timer). Drop it from _updateables first: an entry whose GameObject is destroyed
        // feeds a dead item to Skyheim's own update every frame.
        internal static void Strip(Transform parent)
        {
            SkyheimCooldownItem item = parent.GetComponentInChildren<SkyheimCooldownItem>(true);
            if (!item)
            {
                return;
            }

            Attached.Remove(parent);
            Unregister(item);
            UnityEngine.Object.Destroy(item.gameObject);
        }

        private static void Unregister(SkyheimCooldownItem item)
        {
            if (SkyheimCooldown.Instance == null)
            {
                return;
            }

            if (UpdateablesField?.GetValue(SkyheimCooldown.Instance) is List<SkyheimCooldownItem> list)
            {
                list.RemoveAll(entry => !entry || entry == item);
            }
        }

        private static readonly FieldInfo UpdateablesField =
            AccessTools.Field(typeof(SkyheimCooldown), "_updateables");

        // Returns 1 when the slot ends up carrying an overlay, 0 when it does not (no prefab
        // available): the attach path's self-reporting count the 1.2.6 version never gave.
        internal static int EnsureOn(Transform parent, Vector2i location)
        {
            if (!parent)
            {
                return 0;
            }

            if (Attached.TryGetValue(parent, out SkyheimCooldownItem cached) && cached)
            {
                Reposition(cached, location);
                return 1;
            }

            SkyheimCooldownItem existing = parent.GetComponentInChildren<SkyheimCooldownItem>(true);
            if (existing)
            {
                Attached[parent] = existing;
                Reposition(existing, location);
                return 1;
            }

            GameObject prefab = Get();
            if (!prefab)
            {
                return 0;
            }

            GameObject clone = UnityEngine.Object.Instantiate(prefab, parent);
            clone.name = "skyheim_cooldown";
            Stretch(clone.transform as RectTransform);
            clone.SetActive(true);

            SkyheimCooldownItem item = clone.GetComponent<SkyheimCooldownItem>()
                ?? clone.GetComponentInChildren<SkyheimCooldownItem>(true);
            if (item == null)
            {
                return 0;
            }

            Attached[parent] = item;
            SetLocation(item, location);
            if (SkyheimCooldown.Instance != null)
            {
                SkyheimCooldown.Instance.RegisterUpdateable(item);
            }

            return 1;
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
