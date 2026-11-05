using BepInEx;
using HarmonyLib;

namespace Cjayride.SkyheimEitr
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("skyheim")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "cjayride.SkyheimEitr";
        public const string PluginName = "Skyheim Eitr Shards";
        public const string PluginVersion = "1.4.2";

        internal static Plugin Instance;
        Harmony _harmony;

        private void Awake()
        {
            Instance = this;
            EitrShardDrops.Bind(Config);
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();
            // skyheim's Awake runs first (BepInDependency), so its bundle is already loaded
            // and the postfix above never fires this session. Do the altar work now.
            SkyheimAssetPatches.Apply();
            Logger.LogInfo(PluginName + " " + PluginVersion + " — boss eitr amounts in this cfg, shipped with Skyheim Fix.");
        }

        internal static void Log(string message)
        {
            Instance?.Logger.LogInfo(message);
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
