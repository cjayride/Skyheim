using System;
using System.Reflection;
using HarmonyLib;
using skyheim;
using UnityEngine;

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

                AttachSlotArray(_hotbarSlots?.GetValue(__instance) as Array, i => new Vector2i(i, 0));

                int actionRow = 0;
                bool haveActionRow = false;
                if (_actionRow != null)
                {
                    actionRow = (int)_actionRow.Invoke(null, new object[] { inv });
                    haveActionRow = true;
                }

                if (haveActionRow)
                {
                    AttachSlotArray(_hotbarActions?.GetValue(__instance) as Array, i => new Vector2i(i, actionRow));
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

            return AccessTools.Field(typeof(SkyheimCooldown), "_cooldownPrefab")
                ?.GetValue(SkyheimCooldown.Instance) as GameObject;
        }

        internal static void EnsureOn(Transform parent, Vector2i location)
        {
            if (!parent)
            {
                return;
            }

            SkyheimCooldownItem existing = parent.GetComponentInChildren<SkyheimCooldownItem>(true);
            if (existing != null)
            {
                Vector2i current = GetLocation(existing);
                if (current.x != location.x || current.y != location.y)
                {
                    SetLocation(existing, location);
                    if (SkyheimCooldown.Instance != null)
                    {
                        SkyheimCooldown.Instance.RegisterUpdateable(existing);
                    }
                }

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

            SkyheimCooldownItem item = clone.GetComponent<SkyheimCooldownItem>()
                ?? clone.GetComponentInChildren<SkyheimCooldownItem>(true);
            if (item == null)
            {
                return;
            }

            SetLocation(item, location);
            if (SkyheimCooldown.Instance != null)
            {
                SkyheimCooldown.Instance.RegisterUpdateable(item);
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
            if (!Plugin.SenealUiLoaded() || __instance == null || __instance.m_elements == null)
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
}
