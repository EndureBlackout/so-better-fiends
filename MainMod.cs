#if(MONO)
using ScheduleOne.NPCs;
#elif(IL2CPP)
using Il2CppScheduleOne.NPCs;
#endif

using MelonLoader;
using UnityEngine;
using static MelonLoader.MelonLogger;
using MelonLoader.Utils;
using ExampleMod.Services;
using ExampleMod.Objects;
using BetterFiends.Configuration;
using Newtonsoft.Json;

[assembly: MelonInfo(typeof(BetterFiends.BetterFiends), BetterFiends.BuildInfo.Name, BetterFiends.BuildInfo.Version, BetterFiends.BuildInfo.Author, BetterFiends.BuildInfo.DownloadLink)]
[assembly: MelonColor()]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace BetterFiends
{
    public static class BuildInfo
    {
        public const string Name = "BetterFiends";
        public const string Description = "Makes NPCs fiend for your products or become narcs";
        public const string Author = "EndureBlackout";
        public const string Company = null;
        public const string Version = "1.0";
        public const string DownloadLink = "https://thunderstore.io/c/schedule-i/p/EndureBlackout/BetterFiends/";
    }

    public class BetterFiends : MelonMod
    {
        public static HarmonyLib.Harmony harmony;
        public static bool appUpdate = false;
        public static AssetBundle testAppBundle;
        public static UnityEngine.Object test;
        public static GameObject mainCanvasPrefab;

        private string fiendDataPath;
        public static JsonDataStoreService<Fiend> fiendData;

        private static string configPath;
        public static Config config;

        public static List<NPC> fiendList = new();

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            if (harmony == null)
            {
                harmony = new HarmonyLib.Harmony("com.endureblackout.betterfiends");

                try
                {
                    fiendDataPath = System.IO.Path.Combine(MelonEnvironment.UserDataDirectory, "fiend_data.json");
                    fiendData = new JsonDataStoreService<Fiend>(fiendDataPath);

                    LoadConfig();

                    MelonLogger.Msg("BetterFiends: Patches applied successfully.");
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"BetterFiends: Failed to apply patches. Error: {e}");
                }
            }
        }

        private void LoadConfig()
        {
            configPath = Path.Combine(MelonEnvironment.UserDataDirectory, "config.json");

            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<Config>(json);
                MelonLogger.Msg("[BetterFiends]: Config loaded successfully.");
            } 
            else
            {
                var newConfig = new Config();

                SaveConfig(newConfig);
            }
        }

        private static void SaveConfig(Config newConfig)
        {
            try
            {
                var serializedConfig = JsonConvert.SerializeObject(newConfig, Formatting.Indented);

                File.WriteAllText(configPath, serializedConfig);
                MelonLogger.Msg("[BetterFiends]: New config filed created!");

                config = newConfig;
            } catch (Exception e)
            {
                MelonLogger.Error($"[BetterFiends]: There was an issue saving the config file: {e.Message}");
            }
        }
    }
}
