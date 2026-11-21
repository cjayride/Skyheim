using System.Reflection;
using HarmonyLib;
using skyheim;
using UnityEngine;

namespace Cjayride.SkyheimCompat
{
    /// <summary>
    /// Unique Skyheim runes (max stack 1) were duplicated when a drop spawned a world item
    /// without removing the inventory copy — observed with Rune of Travel thrown on the ground.
    /// ItemDrop.DropItem is the common spawn; Humanoid.DropItem already removes first, so this
    /// is a no-op on the vanilla path. UI/backpack paths that spawn first keep the extra copy.
    /// </summary>
    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.DropItem))]
    internal static class ItemDrop_DropItem_UniqueRune_Patch
    {
        private static readonly MethodInfo GetEquippedBackpack = AccessTools.Method(
            "AdventureBackpacks.Extensions.PlayerExtensions:GetEquippedBackpack",
            new[] { typeof(Player) });

        private static void Prefix(ItemDrop.ItemData item, int amount)
        {
            if (item?.m_shared == null || item.m_shared.m_maxStackSize != 1 || amount == 0)
            {
                return;
            }

            if (!IsSkyheimRune(item))
            {
                return;
            }

            TryRemoveIfStillHeld(item, amount);
        }

        private static bool IsSkyheimRune(ItemDrop.ItemData item)
        {
            GameObject prefab = item.m_dropPrefab;
            if (prefab == null && ObjectDB.instance != null && item.m_shared != null)
            {
                prefab = ObjectDB.instance.GetItemPrefab(item.m_shared.m_name);
            }

            return prefab != null && prefab.GetComponent<SkyheimItemData>() != null;
        }

        private static void TryRemoveIfStillHeld(ItemDrop.ItemData item, int amount)
        {
            Player player = Player.m_localPlayer;
            TryRemoveFrom(player != null ? player.GetInventory() : null, item, amount);
            TryRemoveFrom(GetBackpackInventory(player), item, amount);

            InventoryGui gui = InventoryGui.instance;
            Container container = gui != null ? gui.m_currentContainer : null;
            TryRemoveFrom(container != null ? container.GetInventory() : null, item, amount);
        }

        private static void TryRemoveFrom(Inventory inventory, ItemDrop.ItemData item, int amount)
        {
            if (inventory == null || !inventory.ContainsItem(item))
            {
                return;
            }

            if (amount >= item.m_stack)
            {
                inventory.RemoveItem(item);
            }
            else
            {
                inventory.RemoveItem(item, amount);
            }
        }

        private static Inventory GetBackpackInventory(Player player)
        {
            if (player == null || GetEquippedBackpack == null)
            {
                return null;
            }

            object backpack = GetEquippedBackpack.Invoke(null, new object[] { player });
            if (backpack == null)
            {
                return null;
            }

            MethodInfo getInventory = AccessTools.Method(backpack.GetType(), "GetInventory");
            return getInventory != null ? getInventory.Invoke(backpack, null) as Inventory : null;
        }
    }
}
