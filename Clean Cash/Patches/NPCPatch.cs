using Clean_Cash.NPCScripts;
using Clean_Cash.LaundererSaveManager;
using HarmonyLib;


#if IL2CPP
using Il2CppScheduleOne.NPCs;
#else
using ScheduleOne.NPCs;
#endif

namespace Clean_Cash.Patches
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

        [HarmonyPostfix]
        [HarmonyPatch(nameof(NPC.MinPass))]
        public static void MinPass(NPC __instance)
        {
            if (!Launderers.TierOf.ContainsKey(__instance.FirstName)) return;
            Launderer component = __instance.GetComponent<Launderer>();
            if (component == null) return;
            if (component.laundererData.isUnlocked && component.laundererData.CurrentLaunderAmount > 0f)
            {
                if (component.laundererData.CurrentTimeLeftSeconds > 0f)
                {
                    component.laundererData.CurrentTimeLeftSeconds -= 1;
                    return;
                }
                component.GiveLaunderedPay();
                return;
            }
            if (component.isDialogueInitReady || component.isMainDialogueReady) return;
            component.Start();
        }
    }
}
