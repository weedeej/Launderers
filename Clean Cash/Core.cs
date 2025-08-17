#if IL2CPP
using Il2CppInterop.Runtime.Injection;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.GameTime;
using NPCLaunderers.LaundererSaveManager;
#else
using ScheduleOne.Persistence;
using ScheduleOne.GameTime;
using UnityEngine.Events;
#endif
using NPCLaunderers.NPCScripts;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(NPCLaunderers.Core), "NPCLaunderers", "1.0.3", "weedeej", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace NPCLaunderers
{
    public class Core : MelonMod
    {
#if IL2CPP
        Il2CppAssetBundle bundle;
#else
        AssetBundle bundle;
#endif
        public override void OnInitializeMelon()
        {
            MelonPreferences.CreateCategory("NPCLaunderers", "NPCLaunderers Settings");
            MelonPreferences.CreateEntry("NPCLaunderers", "EnableEnvy", true, "Enable Envy", "Allows your launderers to get envy of what you earn and request more cut.");
            MelonPreferences.CreateEntry("NPCLaunderers", "EnableCutDecreaseEvent", true, "Enable Request Product", "Allows your launderers to request a product to reduce cut");
            MelonPreferences.CreateEntry("NPCLaunderers", "EnableNPCInvestment", true, "Enable NPC Investment",
                "Allows your launderers to invest in your business if you are on a streak with them. You will earn money, and they will too.");
            MelonPreferences.CreateEntry("NPCLaunderers", "EnableDividendAlerts", true, "Enable Dividends Alert",
                "[Works only with NPCInvestment ON]\nReceive daily notification for dividends you earned if you have very good relationship with launderers.");
            MelonPreferences.Save();
#if IL2CPP
            ClassInjector.RegisterTypeInIl2Cpp<Launderer>();
            ClassInjector.RegisterTypeInIl2Cpp<LaundererData>();

            try
            {
                Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream("NPCLaunderers.Assets.launderers.assetbundle");
                if (bundleStream == null)
                {
                    this.Unregister($"AssetBundle stream not found");
                    return;
                }
                byte[] bundleData;
                using (MemoryStream ms = new MemoryStream())
                {
                    bundleStream.CopyTo(ms);
                    bundleData = ms.ToArray();
                }
                Il2CppSystem.IO.Stream stream = new Il2CppSystem.IO.MemoryStream(bundleData);
                bundle = Il2CppAssetBundleManager.LoadFromStream(stream);
            }
            catch (Exception e)
            {
                this.Unregister($"Failed to load AssetBundle. Please report to dev: {e}");
                return;
            }
#else
            try
            {
                var stream = MelonAssembly.Assembly.GetManifestResourceStream("NPCLaunderers.Assets.launderers.assetbundle");

                if (stream == null)
                {
                    this.Unregister($"AssetBundle stream not found");
                    return;
                }
                bundle = AssetBundle.LoadFromStream(stream);
            }
            catch (Exception e)
            {
                this.Unregister($"Failed to load AssetBundle. Please report to dev: {e}");
                return;
            }
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
            yield return new WaitForSecondsRealtime(5f);
#if IL2CPP
            Action saveListener = new Action(() =>
#else
            UnityAction saveListener = new UnityAction(() =>
#endif
            {
                foreach (Launderer launderer in GameObject.FindObjectsOfType<Launderer>())
                {
                    launderer.laundererData.Save();
                }
            });
            // There's a better way to do this.
            Action weekPassListener = new Action(() => {
                foreach (Launderer launderer in GameObject.FindObjectsOfType<Launderer>())
                {
                    launderer.WeekPass();
                }
            });
            
            Action dayPassListener = new Action(() => {
                foreach (Launderer launderer in GameObject.FindObjectsOfType<Launderer>())
                {
                    launderer.DayPass();
                }
            });
            SaveManager.Instance.onSaveStart.AddListener(saveListener);
            TimeManager.Instance.onWeekPass += weekPassListener;
            TimeManager.Instance.onDayPass += dayPassListener;
        }

    }
}