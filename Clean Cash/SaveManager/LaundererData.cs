#if IL2CPP
using Il2CppNewtonsoft.Json.Linq;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.Persistence;
using Il2CppInterop.Runtime;
using Il2CppSystem;

using MelonLoader;

#else
using Newtonsoft.Json.Linq;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence;
#endif
using MelonLoader.Utils;

namespace Clean_Cash.SaveManager
{

    [System.Serializable]
#if IL2CPP
    public class LaundererData :  Il2CppSystem.Object
#else
    public class LaundererData
#endif
    {
        public NPC NPC;
        public string Name;
        public float MinLaunderAmount;
        public float MaxLaunderAmount;
        public LaundererTiers LaundererTier;
        public double CutPercentage;
        private string SaveFilePath;

#if IL2CPP
        public LaundererData(System.IntPtr ptr) : base(ptr) { }
#endif
        public LaundererData(NPC nPC)
        {
            this.NPC = nPC;
            this.LaundererTier = Launderers.TierOf[nPC.FirstName];
            float[] range = Launderers.LaunderAmountRanges[this.LaundererTier];
            this.MinLaunderAmount = range[0];
            this.MaxLaunderAmount = range[1];

            string saveDirectory = LoadManager.Instance.LoadedGameFolderPath;
            int separatorIndex = saveDirectory.IndexOf("Saves\\") + 5;
            string idSlotPath = saveDirectory.Substring(separatorIndex);
            string[] idSlotArr = idSlotPath.Split(new[] { '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
            this.SaveFilePath = Path.Combine(MelonEnvironment.UserDataDirectory, "Launderers", $"{idSlotArr[0]}_{idSlotArr[1]}_{nPC.ID}.json");
            // Initialize the save manager
            if (!Directory.Exists(Path.Combine(MelonEnvironment.UserDataDirectory, "Launderers")))
            {
                Directory.CreateDirectory(Path.Combine(MelonEnvironment.UserDataDirectory, "Launderers"));
            }
        }
        public void Save()
        {
            JObject jObject = new JObject() {
                ["FirstName"] = this.NPC.FirstName,
                ["LastName"] = this.NPC.LastName,
                ["ID"] = this.NPC.ID,
                ["Tier"] = this.LaundererTier.ToString(),
                ["MinLaunderAmount"] = this.MinLaunderAmount,
                ["MaxLaunderAmount"] = this.MaxLaunderAmount,
                ["CutPercentage"] = this.CutPercentage
            };
            File.WriteAllText(this.SaveFilePath, jObject.ToString());
        }
    }
}
