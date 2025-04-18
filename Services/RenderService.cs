using Il2CppScheduleOne.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterFiends.Services
{
    public static class RenderService
    {
        public static void SetWorldspaceDialogueRenderer(NPC npc, string message)
        {
            var spine2 = FindChildRecursive(npc.Avatar.BodyContainer.transform, "mixamorig:Spine2");
            if (spine2 != null)
            {
                var renderer = GetWorldspaceDialogueRenderer(npc);
                if (renderer != null)
                {
                    UnityEngine.Object.Destroy(renderer);
                }
            }
        }

        public static Transform FindChildRecursive(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == name)
                {
                    return child;
                }
                var result = FindChildRecursive(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static Il2CppScheduleOne.UI.WorldspaceDialogueRenderer GetWorldspaceDialogueRenderer(NPC npc)
        {
            var spine2 = FindChildRecursive(npc.Avatar.BodyContainer.transform, "mixamorig:Spine2");
            if (spine2 != null)
            {
                var renderer = spine2.GetComponentInChildren<Il2CppScheduleOne.UI.WorldspaceDialogueRenderer>();
                if (renderer != null)
                {
                    return renderer;
                }
                else
                {
                    MelonLoader.MelonLogger.Warning($"No WorldspaceDialogueRenderer found under Spine2 on {npc.name}");
                }
            }
            return null;
        }
    }
}
