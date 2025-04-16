using BOSSFramework.BehaviorTree.Tasks;
using BOSSFramework.BehaviorTree;
using BOSSFramework;
using MelonLoader;
using ScheduleOne.Economy;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.NPCs;
using ScheduleOne.Product;
using System.Collections;
using UnityEngine;
using ScheduleOne.PlayerScripts;
using HarmonyLib;
using ScheduleOne.ItemFramework;

namespace BetterFiends.Patching
{
    [HarmonyPatch(typeof(ConsumeProductBehaviour), "TryConsume")]
    public static class NPC_ConsumeProduct_Patch
    {
        public static void Postfix(ConsumeProductBehaviour __instance)
        {
            try
            {
                var productFieldInfo = AccessTools.Field(typeof(ConsumeProductBehaviour), "product");
                var productValue = productFieldInfo.GetValue(__instance) as ProductItemInstance;

                if (productValue != null)
                {
                    MelonLogger.Msg($"NPC consumed product: {productValue.Definition.Name}");

                    Player[] playerArray = UnityEngine.Object.FindObjectsOfType<Player>();
                    var npc = __instance.Npc;

                    foreach (Player player in playerArray)
                    {
                        if (player.IsLocalPlayer)
                        {
                            var customer = npc.GetComponent<Customer>();
                            var currentAddiction = customer.CurrentAddiction;
                            var productAddictiveness = productValue.GetAddictiveness();
                            var fiendChance = CalculateFiendProbability(currentAddiction, productAddictiveness);
                            MelonLogger.Msg($"[BetterFiends]: {npc.name} has a {fiendChance} chance to fiend for more product.");

                            if (UnityEngine.Random.value < fiendChance)
                            {
                                MelonLogger.Msg($"[BetterFiends]: {npc.name} is fiending for more product. If denied, they will act out in violence or narc on you.");
                                MelonCoroutines.Start(HandlePlayerInteraction(npc, player));
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error in TryConsume Postfix: {e.Message}");
            }
        }

        private static IEnumerator HandlePlayerInteraction(NPC npc, Player player)
        {
            yield return new WaitForSeconds(90f);
            BehaviorRegistry.PauseExistingBehaviours(npc);

            var enabledProp = HarmonyLib.AccessTools.Property(typeof(RequestProductBehaviour), "Enabled");
            enabledProp.SetValue(npc.behaviour.RequestProductBehaviour, true);
            npc.behaviour.RequestProductBehaviour.SendEnable();

            var targetProp = HarmonyLib.AccessTools.Property(typeof(RequestProductBehaviour), "TargetPlayer");
            targetProp.SetValue(npc.behaviour.RequestProductBehaviour, player);

            var addEnabled = HarmonyLib.AccessTools.Method(typeof(NPCBehaviour), "AddEnabledBehaviour");
            addEnabled.Invoke(npc.behaviour, new object[] { npc.behaviour.RequestProductBehaviour });

            var field = HarmonyLib.AccessTools.Field(typeof(NPCBehaviour), "enabledBehaviours");
            var existing = field.GetValue(npc.behaviour) as List<ScheduleOne.NPCs.Behaviour.Behaviour>;
            foreach (var behaviour in existing)
            {
                if (!behaviour.Active)
                {
                    var activeField = HarmonyLib.AccessTools.Property(typeof(ScheduleOne.NPCs.Behaviour.Behaviour), "Active");
                    activeField.SetValue(behaviour, true);
                    if (npc.LocalConnection != null)
                    {
                        behaviour.Enable_Networked(npc.LocalConnection);
                        behaviour.Begin_Networked(npc.LocalConnection);
                        BetterFiends.fiendList.Add(npc);
                    }
                }
            }

            if (existing.Count > 0)
            {
                npc.behaviour.activeBehaviour = existing.Where(x => x is RequestProductBehaviour).First();
            }
        }

        private static float CalculateFiendProbability(float currentAddiction, float productAddictiveness)
        {
            float baseProbability = productAddictiveness;

            float addictionMultiplier = Mathf.Pow(currentAddiction, 1.5f);

            float combined = baseProbability * (1f + addictionMultiplier);

            float targetDailyProb = 1f / 2f;

            float sigmoidOffset = 5f + Mathf.Log(1f / targetDailyProb - 1f);

            float probability = 1f / (1f + Mathf.Exp(-combined + sigmoidOffset));

            return Mathf.Clamp01(probability);
        }
    }
}
