using HarmonyLib;
using MelonLoader;
using ScheduleOne.Dialogue;
using ScheduleOne.Economy;
using ScheduleOne.Law;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.PlayerScripts;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace BetterFiends.Patching
{
    [HarmonyPatch(typeof(RequestProductBehaviour))]
    public static class NPC_Request_Product_Patch
    {

        [HarmonyPatch("Begin")]
        [HarmonyPostfix]
        public static void BeginPostfix(RequestProductBehaviour __instance)
        {
            try
            {
                if (BetterFiends.fiendList.Contains(__instance.Npc))
                {
                    MelonLogger.Msg("Setting greeting now");
                    var greetField = AccessTools.Field(typeof(RequestProductBehaviour), "requestGreeting");
                    var greetValue = greetField.GetValue(__instance) as DialogueController.GreetingOverride;

                    if (greetValue != null)
                    {
                        greetValue.Greeting = "Yo I need more of your shit... NOW!";
                        greetValue.PlayVO = true;
                        greetValue.VOType = ScheduleOne.VoiceOver.EVOLineType.Alerted;
                        MelonLogger.Msg("Greeting set");
                    }
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error in Begin Postfix: {e.Message}");
            }
        }

        [HarmonyPatch("RequestRejected")]
        [HarmonyPostfix]
        public static void RequestRejectedPostFix(RequestProductBehaviour __instance)
        {
            
            var npc = __instance.Npc;

            if (BetterFiends.fiendList.Contains(npc))
            {
                var customer = npc.GetComponent<Customer>();

                var narcRisk = CalculateNarcProbability(customer, npc);

                if (UnityEngine.Random.value < narcRisk)
                {
                    npc.PlayVO(EVOLineType.Angry);
                    npc.dialogueHandler.ShowWorldspaceDialogue(npc.dialogueHandler.Database.GetLine(EDialogueModule.Customer, "sample_offer_rejected_police"), 5f);
                    npc.actions.SetCallPoliceBehaviourCrime(new AttemptingToSell());
                    npc.actions.CallPolice_Networked(__instance.TargetPlayer);
                    npc.SendTextMessage("Better get me that product next time!");
                }
                else
                {
                    npc.behaviour.CombatBehaviour.SetTarget(null, Player.GetClosestPlayer(npc.transform.position, out var _).NetworkObject);
                    npc.behaviour.CombatBehaviour.Enable_Networked(null);
                }

                MelonLogger.Msg($"Npc is part of list? {BetterFiends.fiendList.Contains(npc)}");
                BetterFiends.fiendList.Remove(npc);
            }
        }

        [HarmonyPatch("RequestAccepted")]
        [HarmonyPostfix]
        public static void RequestAcceptedPostFix(RequestProductBehaviour __instance)
        {
            var npc = __instance.Npc;

            if(BetterFiends.fiendList.Contains(npc))
            {
                var customer = npc.GetComponent<Customer>();

                var narcChance = CalculateNarcProbability(customer, npc);

                if(UnityEngine.Random.value < narcChance)
                {
                    npc.PlayVO(EVOLineType.Angry);
                    npc.dialogueHandler.ShowWorldspaceDialogue(npc.dialogueHandler.Database.GetLine(EDialogueModule.Customer, "sample_offer_rejected_police"), 5f);
                    npc.actions.SetCallPoliceBehaviourCrime(new AttemptingToSell());
                    npc.actions.CallPolice_Networked(__instance.TargetPlayer);
                    npc.SendTextMessage("No hard feelings man, they said they would pay me!");
                }

                BetterFiends.fiendList.Remove(npc);
            }
        }

        private static float CalculateNarcProbability(Customer customer, NPC npc)
        {
            float baseChance = 0.05f;

            float addictionLevel= Mathf.Clamp01(customer.CurrentAddiction);
            float addictionFactor = 1.0f - (addictionLevel * 0.8f);

            float spendingFactor = 100f / (100f + customer.CustomerData.MaxWeeklySpend);

            float narcChance = baseChance * addictionFactor * spendingFactor;

            MelonLogger.Msg($"Narc chance: {narcChance}");

            return Mathf.Clamp01(narcChance);
        }
    }
}
