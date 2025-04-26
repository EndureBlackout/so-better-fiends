#if (MONO)
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Law;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Handover;
using ScheduleOne.VoiceOver;
#elif(IL2CPP)
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Dialogue;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Law;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.UI.Handover;
using Il2CppScheduleOne.VoiceOver;
#endif

using UnityEngine;
using BetterFiends.Services;
using HarmonyLib;
using MelonLoader;
using static MelonLoader.MelonLogger;

namespace BetterFiends.Patching
{
    [HarmonyPatch(typeof(RequestProductBehaviour))]
    public static class NPC_Request_Product_Patch
    {
#if (MONO)
        [HarmonyPatch("Begin")]
        [HarmonyPostfix]
        public static void BeginPostfix(RequestProductBehaviour __instance)
        {
            try
            {
                if (BetterFiends.fiendList.Contains(__instance.Npc))
                {
                    var greetField = AccessTools.Field(typeof(RequestProductBehaviour), "requestGreeting");
                    var greetValue = greetField.GetValue(__instance) as DialogueController.GreetingOverride;

                    var fiend = BetterFiends.fiendData.FindFirst(x => x.Id == __instance.Npc.BakedGUID);
                    var productName = fiend?.LastConsumed?.Name ?? null;

                    if (greetValue != null)
                    {
                        greetValue.Greeting = productName != null ? $"Yo I need more of that {productName}... NOW!" : "Yo I need more of your shit... NOW!";
                        greetValue.PlayVO = true;
                        greetValue.VOType = ScheduleOne.VoiceOver.EVOLineType.Alerted;
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
                    DoNarcBehavior(npc, __instance.TargetPlayer, "Better get me that product next time!");
                }
                else
                {
                    DoRejectBehavior(npc, "I really needed that shit!");
                }

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
                    DoNarcBehavior(npc, __instance.TargetPlayer, "No hard feelings man, they said I would do hard time!");
                }

                BetterFiends.fiendList.Remove(npc);
            }
        }

        [HarmonyPatch("HandoverClosed")]
        [HarmonyPrefix]
        public static bool HandoverClosedPrefix(RequestProductBehaviour __instance, HandoverScreen.EHandoverOutcome outcome, List<ItemInstance> items, float askingPrice)
        {
            if (BetterFiends.fiendList.Contains(__instance.Npc))
            {
                var npc = __instance.Npc;
                var customer = npc.GetComponent<Customer>();

                var fiend = BetterFiends.fiendData.FindFirst(x => x.Id == npc.BakedGUID);

                if (fiend != null && fiend.LastConsumed != null)
                {
                    var product = items.FirstOrDefault(x => x.ID == fiend.LastConsumed.Id);

                    if (product == null)
                    {
                        DoRejectBehavior(npc, "This ain't the shit I was looking for!");

                        Singleton<HandoverScreen>.Instance.ClearCustomerSlots(returnToOriginals: true);
                        return false;
                    }
                }
            }

            return true;
        }

        private static float CalculateNarcProbability(Customer customer, NPC npc)
        {
            float baseChance = 0.05f;

            float addictionLevel= Mathf.Clamp01(customer.CurrentAddiction);
            float addictionFactor = 1.0f - (addictionLevel * 0.8f);

            float spendingFactor = 100f / (100f + customer.CustomerData.MaxWeeklySpend);

            float narcChance = baseChance * addictionFactor * spendingFactor;

            MelonLogger.Msg($"Narc chance: {narcChance}");

            return Mathf.Clamp01(narcChance * BetterFiends.config.FiendProbabilityMultiplier);
        }

        private static void DoNarcBehavior(NPC npc, Player player, string message)
        {
            npc.PlayVO(EVOLineType.Angry);
            npc.dialogueHandler.ShowWorldspaceDialogue(npc.dialogueHandler.Database.GetLine(EDialogueModule.Customer, "sample_offer_rejected_police"), 5f);
            npc.actions.SetCallPoliceBehaviourCrime(new AttemptingToSell());
            npc.actions.CallPolice_Networked(player);

            var renderer = RenderService.GetWorldspaceDialogueRenderer(npc);
            renderer.ShowText(message);
            try { npc.PlayVO(ScheduleOne.VoiceOver.EVOLineType.Acknowledge); } catch { }
        }

        private static void DoRejectBehavior(NPC npc, string message)
        {
            if (BetterFiends.config.EnableAggressiveBehavior)
            {
                npc.behaviour.CombatBehaviour.SetTarget(null, Player.GetClosestPlayer(npc.transform.position, out var _).NetworkObject);
                npc.behaviour.CombatBehaviour.Enable_Networked(null);
            }

            var renderer = RenderService.GetWorldspaceDialogueRenderer(npc);
            renderer.ShowText(message);
            try { npc.PlayVO(ScheduleOne.VoiceOver.EVOLineType.Acknowledge); } catch { }
        }
#elif (IL2CPP)
        [HarmonyPatch("Begin")]
        [HarmonyPostfix]
        public static void BeginPostfix(RequestProductBehaviour __instance)
        {
            try
            {
                if (BetterFiends.fiendList.Contains(__instance.Npc))
                {
                    var greeting = __instance.requestGreeting;

                    var fiend = BetterFiends.fiendData.FindFirst(x => x.Id == __instance.Npc.BakedGUID);
                    var productName = fiend?.LastConsumed?.Name ?? null;

                    if (greeting != null)
                    {
                        greeting.Greeting = productName != null ? $"Yo I need more of that {productName}... NOW!" : "Yo I need more of your shit... NOW!";
                        greeting.PlayVO = true;
                        greeting.VOType = Il2CppScheduleOne.VoiceOver.EVOLineType.Alerted;
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
                    DoNarcBehavior(npc, __instance.TargetPlayer, "Better get me that product next time!");
                }
                else
                {
                    DoRejectBehavior(npc, "I really needed that shit!");
                }

                BetterFiends.fiendList.Remove(npc);
            }
        }

        [HarmonyPatch("RequestAccepted")]
        [HarmonyPostfix]
        public static void RequestAcceptedPostFix(RequestProductBehaviour __instance)
        {
            var npc = __instance.Npc;

            if (BetterFiends.fiendList.Contains(npc))
            {
                var customer = npc.GetComponent<Customer>();

                var narcChance = CalculateNarcProbability(customer, npc);

                if (UnityEngine.Random.value < narcChance)
                {
                    DoNarcBehavior(npc, __instance.TargetPlayer, "No hard feelings man, they said I would do hard time!");
                }

                BetterFiends.fiendList.Remove(npc);
            }
        }

        [HarmonyPatch("HandoverClosed")]
        [HarmonyPrefix]
        public static bool HandoverClosedPrefix(RequestProductBehaviour __instance, HandoverScreen.EHandoverOutcome outcome, Il2CppSystem.Collections.Generic.List<ItemInstance> items, float askingPrice)
        {
            if (BetterFiends.fiendList.Contains(__instance.Npc))
            {
                var npc = __instance.Npc;
                var customer = npc.GetComponent<Customer>();

                var fiend = BetterFiends.fiendData.FindFirst(x => x.Id == npc.BakedGUID);

                if (fiend != null && fiend.LastConsumed != null)
                {
                    ItemInstance product = null;

                    foreach (var item in items)
                    {
                        if (item.ID == fiend.LastConsumed.Id)
                        {
                            product = item;
                        }
                    }

                    if (product == null)
                    {
                        DoRejectBehavior(npc, "This ain't the shit I was looking for!");

                        Singleton<HandoverScreen>.Instance.ClearCustomerSlots(returnToOriginals: true);
                        return false;
                    }
                }
            }

            return true;
        }

        private static float CalculateNarcProbability(Customer customer, NPC npc)
        {
            float baseChance = 0.05f;

            float addictionLevel = Mathf.Clamp01(customer.CurrentAddiction);
            float addictionFactor = 1.0f - (addictionLevel * 0.8f);

            float spendingFactor = 100f / (100f + customer.CustomerData.MaxWeeklySpend);

            float narcChance = baseChance * addictionFactor * spendingFactor;

            MelonLogger.Msg($"Narc chance: {narcChance}");

            return Mathf.Clamp01(narcChance * BetterFiends.config.FiendProbabilityMultiplier);
        }

        private static void DoNarcBehavior(NPC npc, Player player, string message)
        {
            npc.PlayVO(EVOLineType.Angry);
            npc.dialogueHandler.ShowWorldspaceDialogue(npc.dialogueHandler.Database.GetLine(EDialogueModule.Customer, "sample_offer_rejected_police"), 5f);
            npc.actions.SetCallPoliceBehaviourCrime(new AttemptingToSell());
            npc.actions.CallPolice_Networked(player);

            var renderer = RenderService.GetWorldspaceDialogueRenderer(npc);
            renderer.ShowText(message);
            try { npc.PlayVO(Il2CppScheduleOne.VoiceOver.EVOLineType.Acknowledge); } catch { }
        }

        private static void DoRejectBehavior(NPC npc, string message)
        {
            if (BetterFiends.config.EnableAggressiveBehavior)
            {
                npc.behaviour.CombatBehaviour.SetTarget(null, Player.GetClosestPlayer(npc.transform.position, out var _).NetworkObject);
                npc.behaviour.CombatBehaviour.Enable_Networked(null);
            }

            var renderer = RenderService.GetWorldspaceDialogueRenderer(npc);
            renderer.ShowText(message);
        }
#endif
        }
    }
