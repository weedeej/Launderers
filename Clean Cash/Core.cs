#if IL2CPP
using Il2CppInterop.Runtime.Injection;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.GameTime;
#else
using ScheduleOne.Persistence;
using ScheduleOne.GameTime;
using UnityEngine.Events;
#endif
using Clean_Cash.NPCScripts;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(Clean_Cash.Core), "Clean Cash", "1.0.0", "weedeej", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace Clean_Cash
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
#if IL2CPP
            ClassInjector.RegisterTypeInIl2Cpp<Launderer>();
            ClassInjector.RegisterTypeInIl2Cpp<LaundererData>();
            try
            {
                Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream("Clean_Cash.Assets.launderers.assetbundle");
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
            try {
                var stream = MelonAssembly.Assembly.GetManifestResourceStream("Clean_Cash.Assets.launderers.assetbundle");

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
            Action weekPassListener = new Action(() => {
                foreach (Launderer launderer in GameObject.FindObjectsOfType<Launderer>())
                {
                    launderer.WeekPass();
                }
            });
            SaveManager.Instance.onSaveStart.AddListener(saveListener);
            TimeManager.Instance.onWeekPass += weekPassListener;
        }

    }
}