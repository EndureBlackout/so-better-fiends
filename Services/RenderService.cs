#if (MONO)
using ScheduleOne.NPCs;
#elif (IL2CPP)
using Il2CppScheduleOne.NPCs;
#endif

using UnityEngine;

namespace BetterFiends.Services
{
    public static class RenderService
    {
#if (MONO)
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

        public static ScheduleOne.UI.WorldspaceDialogueRenderer GetWorldspaceDialogueRenderer(NPC npc)
        {
            var spine2 = FindChildRecursive(npc.Avatar.BodyContainer.transform, "mixamorig:Spine2");
            if (spine2 != null)
            {
                var renderer = spine2.GetComponentInChildren<ScheduleOne.UI.WorldspaceDialogueRenderer>();
                if (renderer != null)
                {
                    MelonLoader.MelonLogger.Msg($"Found WorldspaceDialogueRenderer on {npc.fullName}");
                    return renderer;
                }
                else
                {
                    MelonLoader.MelonLogger.Warning($"No WorldspaceDialogueRenderer found under Spine2 on {npc.fullName}");
                }
            }
            return null;
        }
#elif (IL2CPP)
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
#endif
    }
}
