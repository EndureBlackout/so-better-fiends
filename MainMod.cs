using HarmonyLib;
using ScheduleOne.Product;
using ScheduleOne.PlayerScripts;
using MelonLoader;
using UnityEngine;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using BOSSFramework.BehaviorTree;
using BOSSFramework.BehaviorTree.Tasks;
using BOSSFramework;
using System.Collections;
using ScheduleOne.Dialogue;
using static MelonLoader.MelonLogger;
using ScheduleOne.Economy;
using ScheduleOne.Law;
using MelonLoader.Utils;

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
        public const string DownloadLink = "google.com";
    }

    public class BetterFiends : MelonMod
    {
        public static HarmonyLib.Harmony harmony;
        public static bool appUpdate = false;
        public static AssetBundle testAppBundle;
        public static UnityEngine.Object test;
        public static GameObject mainCanvasPrefab;

        public static List<NPC> fiendList = new();

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            if (harmony == null)
            {
                harmony = new HarmonyLib.Harmony("com.endureblackout.betterfiends");

                try
                {
                    MelonLogger.Msg("BetterFiends: Patches applied successfully.");
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"BetterFiends: Failed to apply patches. Error: {e}");
                }
            }
        }
    }
}
