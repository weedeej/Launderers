using NPCLaunderers.NPCScripts;
using NPCLaunderers.LaundererSaveManager;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

#if IL2CPP
using Il2CppScheduleOne.NPCs;
#else
using ScheduleOne.NPCs;
#endif

namespace NPCLaunderers.Patches
{

    [HarmonyPatch(typeof(NPC))]
    class NPCPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(NPC.Start))]
        public static void AddComponent(NPC __instance)
        {
            if (!Launderers.TierOf.ContainsKey(__instance.FirstName)) return;
            Launderer component = __instance.gameObject.AddComponent<Launderer>();
        }
    }
}
