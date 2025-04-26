#if(MONO)
using ScheduleOne.Economy;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.NPCs;
using ScheduleOne.Product;
using System.Collections;
using ScheduleOne.PlayerScripts;
#elif(IL2CPP)
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.PlayerScripts;
#endif

using HarmonyLib;
using MelonLoader;
using System.Text.RegularExpressions;
using System.Collections;
using UnityEngine;

namespace BetterFiends.Patching
{
    [HarmonyPatch(typeof(ConsumeProductBehaviour), "TryConsume")]
    public static class NPC_ConsumeProduct_Patch
    {
#if (MONO)
        public static void Postfix(ConsumeProductBehaviour __instance)
        {
            try
            {
                var productFieldInfo = AccessTools.Field(typeof(ConsumeProductBehaviour), "product");
                var productValue = productFieldInfo.GetValue(__instance) as ProductItemInstance;

                var regexNameString = @"\s*\(.*[uU]npackaged\)";

                if (productValue != null)
                {
                    //Player[] playerArray = UnityEngine.Object.FindObjectsOfType<Player>();
                    var player = Player.Local;
                    var npc = __instance.Npc;

                    var fiendData = BetterFiends.fiendData.FindFirst(x => x.Id == npc.BakedGUID);

                    if (fiendData == null)
                    {
                        MelonLogger.Msg("[BetterFiends]: Fiend data not found, creating new fiend data");
                        fiendData = new ExampleMod.Objects.Fiend()
                        {
                            Id = npc.BakedGUID,
                            Name = npc.name,
                            LastConsumed = new ExampleMod.Objects.Product
                            {
                                Id = productValue.ID,
                                Name = Regex.Replace(productValue.Name, regexNameString, ""),
                                Addictiveness = productValue.GetAddictiveness(),
                                Quality = (int)productValue.Quality
                            }
                        };

                        BetterFiends.fiendData.Add(fiendData);
                    }
                    else
                    {

                        fiendData.LastConsumed = new ExampleMod.Objects.Product
                        {
                            Id = productValue.ID,
                            Name = Regex.Replace(productValue.Name, regexNameString, ""),
                            Addictiveness = productValue.GetAddictiveness(),
                            Quality = (int)productValue.Quality
                        };

                        BetterFiends.fiendData.Update(x => x.Id == npc.BakedGUID, y => y.LastConsumed = fiendData.LastConsumed);
                    }

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
            catch (Exception e)
            {
                MelonLogger.Error($"[BetterFiends]: Error in TryConsume Postfix: {e.Message}");
            }
        }

        private static IEnumerator HandlePlayerInteraction(NPC npc, Player player)
        {
            yield return new WaitForSeconds(90f);
            PauseBehaviors(npc);

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

        private static void PauseBehaviors(NPC npc)
        {
            var field = HarmonyLib.AccessTools.Field(typeof(NPCBehaviour), "enabledBehaviours");
            //var existing = npc.behaviour.enabledBehaviours;
            var existing = field.GetValue(npc.behaviour) as List<ScheduleOne.NPCs.Behaviour.Behaviour>;
            foreach (var behaviour in existing)
            {
                if (behaviour.Active)
                {
                    var activeField = HarmonyLib.AccessTools.Property(typeof(ScheduleOne.NPCs.Behaviour.Behaviour), "Active");
                    activeField.SetValue(behaviour, false);
                    behaviour.BehaviourUpdate();
                    if (npc.LocalConnection != null)
                        behaviour.End_Networked(npc.LocalConnection);
                }
            }
        }

        private static float CalculateFiendProbability(float currentAddiction, float productAddictiveness)
        {
            float baseProbability = productAddictiveness;

            float addictionMultiplier = Mathf.Pow(currentAddiction, 1.5f)
                * (1f + UnityEngine.Random.Range(-0.1f, 0.1f));

            float combined = baseProbability * (1f + addictionMultiplier);

            float probability = (1f / (1f + Mathf.Exp(-combined + 3f))) * 0.5f;

            return Mathf.Clamp01(probability * BetterFiends.config.FiendProbabilityMultiplier);
        }
#elif (IL2CPP)
        public static void Postfix(ConsumeProductBehaviour __instance)
        {
            try
            {
                var product = __instance.product;

                var regexNameString = @"\s*\(.*[uU]npackaged\)";

                if (product != null)
                {
                    //Player[] playerArray = UnityEngine.Object.FindObjectsOfType<Player>();
                    var player = Player.Local;
                    var npc = __instance.Npc;

                    var fiendData = BetterFiends.fiendData.FindFirst(x => x.Id == npc.BakedGUID);

                    if (fiendData == null)
                    {
                        MelonLogger.Msg("[BetterFiends]: Fiend data not found, creating new fiend data");
                        fiendData = new ExampleMod.Objects.Fiend()
                        {
                            Id = npc.BakedGUID,
                            Name = npc.name,
                            LastConsumed = new ExampleMod.Objects.Product
                            {
                                Id = product.ID,
                                Name = Regex.Replace(product.Name, regexNameString, ""),
                                Addictiveness = product.GetAddictiveness(),
                                Quality = (int)product.Quality
                            }
                        };

                        BetterFiends.fiendData.Add(fiendData);
                    }
                    else
                    {
                        fiendData.LastConsumed = new ExampleMod.Objects.Product
                        {
                            Id = product.ID,
                            Name = Regex.Replace(product.Name, regexNameString, ""),
                            Addictiveness = product.GetAddictiveness(),
                            Quality = (int)product.Quality
                        };

                        BetterFiends.fiendData.Update(x => x.Id == npc.BakedGUID, y => y.LastConsumed = fiendData.LastConsumed);
                    }

                    if (player.IsLocalPlayer)
                    {
                        var customer = npc.GetComponent<Customer>();
                        var currentAddiction = customer.CurrentAddiction;
                        var productAddictiveness = product.GetAddictiveness();
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
            catch (System.Exception e)
            {
                MelonLogger.Error($"[BetterFiends]: Error in TryConsume Postfix: {e.Message}");
            }
        }

        private static IEnumerator HandlePlayerInteraction(NPC npc, Player player)
        {
            yield return new WaitForSeconds(90f);
            PauseBehaviors(npc);

            npc.behaviour.RequestProductBehaviour.Enabled = true;
            npc.behaviour.RequestProductBehaviour.SendEnable();

            npc.behaviour.RequestProductBehaviour.TargetPlayer = player;

            npc.behaviour.AddEnabledBehaviour(npc.behaviour.RequestProductBehaviour);

            Il2CppSystem.Collections.Generic.List<Il2CppScheduleOne.NPCs.Behaviour.Behaviour> existing = npc.behaviour.enabledBehaviours;

            foreach (var behaviour in existing)
            {
                if (!behaviour.Active)
                {
                    behaviour.Active = true;

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
                foreach (var beh in existing)
                {
                    if (beh is RequestProductBehaviour)
                    {
                        npc.behaviour.activeBehaviour = beh;
                    }
                }
            }
        }

        private static void PauseBehaviors(NPC npc)
        {
            var existing = npc.behaviour.enabledBehaviours;
            foreach (var behaviour in existing)
            {
                if (behaviour.Active)
                {
                    behaviour.Active = false;
                    behaviour.BehaviourUpdate();
                    if (npc.LocalConnection != null)
                        behaviour.End_Networked(npc.LocalConnection);
                }
            }
        }

        private static float CalculateFiendProbability(float currentAddiction, float productAddictiveness)
        {
            float baseProbability = productAddictiveness;

            float addictionMultiplier = Mathf.Pow(currentAddiction, 1.5f)
                * (1f + UnityEngine.Random.Range(-0.1f, 0.1f));

            float combined = baseProbability * (1f + addictionMultiplier);

            float probability = (1f / (1f + Mathf.Exp(-combined + 3f))) * 0.5f;

            return Mathf.Clamp01(probability * BetterFiends.config.FiendProbabilityMultiplier);
        }
#endif
    }
}
