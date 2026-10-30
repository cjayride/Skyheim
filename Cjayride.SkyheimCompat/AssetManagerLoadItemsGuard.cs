using HarmonyLib;
using skyheim;

namespace Cjayride.SkyheimCompat
{
    /// <summary>
    /// Skyheim registers bundle items in ObjectDB.Awake and again from FejdStartup.SetupObjectDB.
    /// After leaving a multiplayer session, SetupObjectDB runs on an ObjectDB that still lists those items,
    /// so LoadItems AddRange + UpdateRegisters throws duplicate stable-hash keys (e.g. bonemass_drop).
    /// </summary>
    [HarmonyPatch(typeof(AssetManager), nameof(AssetManager.LoadItems))]
    internal static class AssetManager_LoadItems_Guard
    {
        private const string SentinelPrefab = "bonemass_drop";

        static bool Prefix(ObjectDB db)
        {
            if (db == null)
            {
                return true;
            }

            if (db.GetItemPrefab(SentinelPrefab) != null)
            {
                Plugin.Log("Skipped Skyheim LoadItems; ObjectDB already has bonemass_drop.");
                return false;
            }

            return true;
        }
    }
}
