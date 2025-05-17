#if IL2CPP
using Il2CppNewtonsoft.Json.Linq;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.Persistence;

#else
using Newtonsoft.Json.Linq;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence;
#endif
using MelonLoader.Utils;
using UnityEngine;

namespace NPCLaunderers.LaundererSaveManager
{

    [System.Serializable]
#if IL2CPP
    public class LaundererData :  Il2CppSystem.Object
#else
    public class LaundererData
#endif
    {
        public NPC NPC;
        public float MinLaunderAmount;
        public float MaxLaunderAmount;
        public LaundererTier LaundererTier;
        public double CutPercentage;
        public bool isUnlocked;
        public float CurrentTimeLeftSeconds;
        public float CurrentLaunderAmount;
        public float WeekLaunderReturn;
        public float WeekCutAmount;
        public string RequiredProductID;
        public float InstanceMaxLaunderAmount;
        public float ReturnLaunderAmount { get => Mathf.Floor(this.CurrentLaunderAmount - (this.CurrentLaunderAmount * (float)(this.CutPercentage / 100))); }
        public float CutAmount { get => Mathf.Floor(this.CurrentLaunderAmount * (float)(this.CutPercentage / 100)); }
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
            this.WeekLaunderReturn = 0f;
            this.WeekCutAmount = 0f;
            this.CurrentLaunderAmount = 0f;
            this.CurrentTimeLeftSeconds = 0f;
            this.isUnlocked = false;
            this.InstanceMaxLaunderAmount = 0f;
            this.RequiredProductID = "";
            this.CutPercentage = 10f;

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
            JObject jObject = new JObject();
            jObject.Add(new JProperty("FirstName", this.NPC.FirstName));
            jObject.Add(new JProperty("LastName", this.NPC.LastName));
            jObject.Add(new JProperty("ID", this.NPC.ID));
            jObject.Add(new JProperty("Tier", this.LaundererTier.ToString()));
            jObject.Add(new JProperty("MinLaunderAmount", this.MinLaunderAmount));
            jObject.Add(new JProperty("MaxLaunderAmount", this.MaxLaunderAmount));
            jObject.Add(new JProperty("isUnlocked", this.isUnlocked));
            jObject.Add(new JProperty("CutPercentage", this.CutPercentage));
            jObject.Add(new JProperty("CurrentTimeLeftSeconds", this.CurrentTimeLeftSeconds));
            jObject.Add(new JProperty("CurrentLaunderAmount", this.CurrentLaunderAmount));
            jObject.Add(new JProperty("WeekLaunderReturn", this.WeekLaunderReturn));
            jObject.Add(new JProperty("WeekCutAmount", this.WeekCutAmount));
            jObject.Add(new JProperty("RequiredProductID", this.RequiredProductID));
            jObject.Add(new JProperty("InstanceMaxLaunderAmount", this.InstanceMaxLaunderAmount));

            File.WriteAllText(this.SaveFilePath, jObject.ToString());
            Debug.Log($"{this.NPC.FirstName} - Saved to: {this.SaveFilePath}");
        }

        public LaundererData Load()
        {
            if (!File.Exists(this.SaveFilePath)) return this;
            string json = File.ReadAllText(this.SaveFilePath);
            JObject jObject = JObject.Parse(json);

            this.MinLaunderAmount = jObject.Value<float>("MinLaunderAmount");
            this.MaxLaunderAmount = jObject.Value<float>("MaxLaunderAmount");
            this.CutPercentage = jObject.Value<double>("CutPercentage");
            this.isUnlocked = jObject.Value<bool>("isUnlocked");
            this.CurrentTimeLeftSeconds = jObject.Value<float>("CurrentTimeLeftSeconds");
            this.CurrentLaunderAmount = jObject.Value<float>("CurrentLaunderAmount");
            this.LaundererTier = (LaundererTier)Enum.Parse(typeof(LaundererTier), jObject.Value<string>("Tier"));
            this.WeekLaunderReturn = jObject.Value<float>("WeekLaunderReturn");
            this.WeekCutAmount = jObject.Value<float>("WeekCutAmount");
            this.InstanceMaxLaunderAmount = jObject.Value<float>("InstanceMaxLaunderAmount");
            this.RequiredProductID = jObject.Value<string>("RequiredProductID");
            return this;
        }
    }
}
