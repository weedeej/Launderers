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
            MelonCoroutines.Start(MinPassCoroutine(__instance));
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(NPC.MinPass))]
        public static void MinPass(NPC __instance)
        {
            if (!Launderers.TierOf.ContainsKey(__instance.FirstName)) return;
            Launderer component = __instance.GetComponent<Launderer>();
            if (component == null) return;
            
        }

        private static System.Collections.IEnumerator MinPassCoroutine(NPC __instance)
        {
            yield return new WaitForSeconds(3f);
            Launderer component = __instance.gameObject.AddComponent<Launderer>();
            if (component.laundererData.isUnlocked)
            {
                if (!(component.isMainDialogueReady && component.withContract) || (component.laundererData.RequiredProductID != "" && !component.withContract))
                {
                    component.Start();
                }
            }
            else
            {
                if (!component.isDialogueInitReady)
                {
                    component.Start();
                }
            }
            if (component.laundererData.isUnlocked && component.laundererData.CurrentLaunderAmount > 0f)
            {
                if (component.laundererData.CurrentTimeLeftSeconds > 0f)
                {
                    component.laundererData.CurrentTimeLeftSeconds -= 1;
                    yield break;
                }
                component.GiveLaunderedPay();
            }
        }
    }
}
