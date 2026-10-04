using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Cjayride.SkyheimEitr
{
    internal static class EitrShardDrops
    {
        internal const int StackSize = 50;
        const string RpcName = "CJEitrBossShards";

        static readonly string[] BossOrder =
        {
            "Eikthyr",
            "gd_king",
            "Bonemass",
            "Dragon",
            "GoblinKing",
            "SeekerQueen",
            "Fader",
            "SvartalfarQueen",
            "Jotunn",
            "HelDemon",
            "Spider_Boss",
            "AshHuldraQueen",
            "ML_AshHuldra",
            "Elaking",
            "Gammeltroll",
            "LordReto"
        };

        static readonly Dictionary<string, int> Defaults = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "Eikthyr", 5 },
            { "gd_king", 15 },
            { "Bonemass", 15 },
            { "Dragon", 20 },
            { "GoblinKing", 20 },
            { "SeekerQueen", 25 },
            { "Fader", 30 },
            { "SvartalfarQueen", 25 },
            { "Jotunn", 30 },
            { "HelDemon", 30 },
            { "Spider_Boss", 15 },
            { "AshHuldraQueen", 25 },
            { "ML_AshHuldra", 20 },
            { "Elaking", 20 },
            { "Gammeltroll", 20 },
            { "LordReto", 20 }
        };

        internal const string ShardPrefabName = "eitr_shard_drop";
        const string ShardPrefab = ShardPrefabName;
        static bool _missingLogged;

        static readonly Dictionary<string, ConfigEntry<int>> Bindings = new Dictionary<string, ConfigEntry<int>>(StringComparer.OrdinalIgnoreCase);
        static readonly Dictionary<string, int> Synced = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        static ConfigEntry<bool> Enabled;
        static ConfigEntry<int> OtherBosses;
        static bool _bound;

        internal static void Bind(ConfigFile config)
        {
            if (_bound)
                return;
            _bound = true;

            Enabled = config.Bind("EitrShards", "Enabled", true,
                "Raise eitr_shard_drop stack size and apply the per-boss amounts below. Dedicated server values are sent to clients.");
            OtherBosses = config.Bind("EitrShards", "OtherBosses", 0,
                "Shards per player on any other prefab that already drops eitr_shard_drop. 0 leaves that drop alone.");

            BindBoss(config, "Eikthyr", "Eikthyr");
            BindBoss(config, "TheElder", "gd_king");
            BindBoss(config, "Bonemass", "Bonemass");
            BindBoss(config, "Moder", "Dragon");
            BindBoss(config, "Yagluth", "GoblinKing");
            BindBoss(config, "TheQueen", "SeekerQueen");
            BindBoss(config, "Fader", "Fader");
            BindBoss(config, "SvartalfarQueen", "SvartalfarQueen");
            BindBoss(config, "Jotunn", "Jotunn");
            BindBoss(config, "HelDemon", "HelDemon");
            BindBoss(config, "SpiderBoss", "Spider_Boss");
            BindBoss(config, "AshHuldraQueen", "AshHuldraQueen");
            BindBoss(config, "ML_AshHuldra", "ML_AshHuldra");
            BindBoss(config, "Elaking", "Elaking");
            BindBoss(config, "Gammeltroll", "Gammeltroll");
            BindBoss(config, "LordReto", "LordReto");
        }

        static void BindBoss(ConfigFile config, string key, string prefab)
        {
            Bindings[prefab] = config.Bind("EitrShards", key, Defaults[prefab],
                "Eitr shards per player from " + key + " (" + prefab + "). 0 removes this shard drop.");
        }

        internal static void ApplyStack(GameObject go)
        {
            if (go == null || Enabled == null || !Enabled.Value)
                return;
            string prefab = go.name.Replace("(Clone)", string.Empty);
            if (prefab != ShardPrefab)
                return;
            var drop = go.GetComponent<ItemDrop>();
            if (drop?.m_itemData?.m_shared == null)
                return;
            if (drop.m_itemData.m_shared.m_maxStackSize < StackSize)
                drop.m_itemData.m_shared.m_maxStackSize = StackSize;
        }

        internal static void ApplyAllStacks()
        {
            if (ObjectDB.instance?.m_items != null)
            {
                for (int i = 0; i < ObjectDB.instance.m_items.Count; i++)
                    ApplyStack(ObjectDB.instance.m_items[i]);
            }

            if (ZNetScene.instance?.m_namedPrefabs != null)
            {
                foreach (var pair in ZNetScene.instance.m_namedPrefabs)
                    ApplyStack(pair.Value);
            }
        }

        internal static void ApplyToCharacterDrop(CharacterDrop drops)
        {
            if (drops == null || drops.m_drops == null || Enabled == null || !Enabled.Value)
                return;

            string prefab = drops.gameObject.name.Replace("(Clone)", string.Empty);
            bool listed = Bindings.ContainsKey(prefab);
            int amount = AmountFor(prefab);

            CharacterDrop.Drop shard = null;
            for (int i = drops.m_drops.Count - 1; i >= 0; i--)
            {
                var row = drops.m_drops[i];
                if (row == null || row.m_prefab == null)
                    continue;
                if (row.m_prefab.name.Replace("(Clone)", string.Empty) != ShardPrefab)
                    continue;
                if (amount == 0)
                {
                    drops.m_drops.RemoveAt(i);
                    continue;
                }
                shard = row;
            }

            if (amount <= 0)
                return;

            if (shard == null)
            {
                if (!listed)
                    return;
                GameObject item = FindShardPrefab();
                if (item == null)
                {
                    if (!_missingLogged)
                    {
                        _missingLogged = true;
                        Plugin.Log("No item prefab named " + ShardPrefab + ". Bosses will not drop eitr shards.");
                    }
                    return;
                }
                shard = new CharacterDrop.Drop
                {
                    m_prefab = item,
                    m_chance = 1f,
                    m_onePerPlayer = true,
                    m_levelMultiplier = false
                };
                drops.m_drops.Add(shard);
            }

            shard.m_amountMin = amount;
            shard.m_amountMax = amount;
            shard.m_chance = 1f;
            shard.m_onePerPlayer = true;
            shard.m_levelMultiplier = false;
        }

        static GameObject FindShardPrefab()
        {
            GameObject item = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab(ShardPrefab) : null;
            if (item != null)
                return item;
            if (ZNetScene.instance != null)
                item = ZNetScene.instance.GetPrefab(ShardPrefab);
            return item;
        }

        internal static int AmountForPublic(string prefab)
        {
            return AmountFor(prefab);
        }

        static int AmountFor(string prefab)
        {
            if (Bindings.TryGetValue(prefab, out ConfigEntry<int> bind))
            {
                int value = UseSynced() && Synced.TryGetValue(prefab, out int synced) ? synced : bind.Value;
                return Mathf.Clamp(value, 0, StackSize);
            }

            if (OtherBosses.Value <= 0)
                return -1;
            int other = UseSynced() && Synced.TryGetValue("*", out int otherSynced) ? otherSynced : OtherBosses.Value;
            return Mathf.Clamp(other, 0, StackSize);
        }

        static bool UseSynced()
        {
            return ZNet.instance != null && !ZNet.instance.IsServer() && Synced.Count > 0;
        }

        internal static void RegisterPeer(ZNetPeer peer)
        {
            if (peer?.m_rpc == null)
                return;
            peer.m_rpc.Register<string>(RpcName, (_, payload) => Receive(payload));
            if (ZNet.instance != null && ZNet.instance.IsServer())
                peer.m_rpc.Invoke(RpcName, Serialize());
        }

        static string Serialize()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < BossOrder.Length; i++)
            {
                if (i > 0)
                    sb.Append(',');
                string prefab = BossOrder[i];
                sb.Append(Bindings[prefab].Value);
            }
            sb.Append(',').Append(OtherBosses.Value);
            return sb.ToString();
        }

        static void Receive(string payload)
        {
            if (string.IsNullOrEmpty(payload))
                return;
            string[] parts = payload.Split(',');
            Synced.Clear();
            for (int i = 0; i < BossOrder.Length && i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], out int n))
                    Synced[BossOrder[i]] = n;
            }
            if (parts.Length > BossOrder.Length && int.TryParse(parts[BossOrder.Length], out int other))
                Synced["*"] = other;
            Plugin.Log("Received eitr shard boss amounts from the server.");
        }
    }

    // Ragdoll.Setup saves the loot list before CharacterDrop.OnDeath runs. Patching
    // GenerateDropList puts the shard on that saved list, which is what the chest receives.
    [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
    [HarmonyPriority(Priority.First)]
    internal static class CharacterDrop_GenerateDropList_Eitr
    {
        private static void Prefix(CharacterDrop __instance)
        {
            EitrShardDrops.ApplyToCharacterDrop(__instance);
            EitrShardDrops.ApplyAllStacks();
        }

        // Skyheim's own DropModifier row is amount 1 and can be what the roll keeps.
        // Rewrite the finished list so the chest gets the configured stack.
        private static void Postfix(CharacterDrop __instance, List<KeyValuePair<GameObject, int>> __result)
        {
            if (__result == null || __instance == null)
                return;

            string prefab = __instance.gameObject.name.Replace("(Clone)", string.Empty);
            int amount = EitrShardDrops.AmountForPublic(prefab);
            if (amount <= 0)
                return;

            int total = amount;
            EitrShardDrops.ApplyAllStacks();
            for (int i = 0; i < __result.Count; i++)
            {
                GameObject key = __result[i].Key;
                if (key == null)
                    continue;
                string name = key.name.Replace("(Clone)", string.Empty);
                if (name != EitrShardDrops.ShardPrefabName)
                    continue;
                EitrShardDrops.ApplyStack(key);
                __result[i] = new KeyValuePair<GameObject, int>(key, total);
            }
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    [HarmonyPriority(Priority.Last)]
    internal static class ObjectDB_Awake_EitrStack
    {
        private static void Postfix()
        {
            EitrShardDrops.ApplyAllStacks();
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [HarmonyPriority(Priority.Last)]
    internal static class ZNetScene_Awake_EitrStack
    {
        private static void Postfix()
        {
            EitrShardDrops.ApplyAllStacks();
        }
    }

    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Awake))]
    internal static class ItemDrop_Awake_EitrStack
    {
        private static void Postfix(ItemDrop __instance)
        {
            if (__instance != null)
                EitrShardDrops.ApplyStack(__instance.gameObject);
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
    internal static class ZNet_OnNewConnection_Eitr
    {
        private static void Postfix(ZNetPeer peer)
        {
            EitrShardDrops.RegisterPeer(peer);
        }
    }
}
