#if IL2CPP
using Clean_Cash.NPCScripts;
using Clean_Cash.SaveManager;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.NPCs.CharacterClasses;
using Il2CppSystem;


#else
using Newtonsoft.Json.Linq;
#endif
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(Clean_Cash.Core), "Clean Cash", "1.0.0", "weedeej", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace Clean_Cash
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
#if IL2CPP
            ClassInjector.RegisterTypeInIl2Cpp<Launderer>();
#endif
            LoggerInstance.Msg("Initialized.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            if (sceneName == "Main")
            {
                MelonCoroutines.Start(DelayedStart());
            }
        }

        private System.Collections.IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(5.0f);
            try
            {
                LoggerInstance.Msg(1);
                Marco donna = GameObject.FindObjectOfType<Marco>();
                if (donna == null)
                {
                    LoggerInstance.Error("Donna not found.");
                    yield break;
                }
                LoggerInstance.Msg(2);
                LaundererData laundererData = new LaundererData(donna);
                LoggerInstance.Msg(3);
                laundererData.Save();
            } catch (System.Exception e)
            {
                LoggerInstance.Error($"Error: {e.Message}");
            }
        }

    }
}