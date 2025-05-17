#if IL2CPP
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.Dialogue;
using static Il2CppScheduleOne.Dialogue.DialogueController;
using Il2CppScheduleOne.Product;
using Il2CppScheduleOne.Quests;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.ItemFramework;
#else
using ScheduleOne.NPCs;
using ScheduleOne.Dialogue;
using static ScheduleOne.Dialogue.DialogueController;
using ScheduleOne.Product;
using ScheduleOne.Quests;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.UI;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.ItemFramework;
#endif
using UnityEngine;
using Clean_Cash.LaundererSaveManager;
using Clean_Cash.UI;
using MelonLoader;
using UnityEngine.Events;

namespace Clean_Cash.NPCScripts
{
    public class Launderer : MonoBehaviour
    {
        public LaundererData laundererData;
        public bool isMainDialogueReady = false;
        public bool isDialogueInitReady = false;
        public NPC nPC;
        public DialogueController dialogueController;
        private LaundererUI laundererUI;
        private float upfrontInstance;

#if IL2CPP
        public Launderer(IntPtr pointer) : base(pointer) { }
#endif
        public void Awake()
        {
            NPC npc = GetComponent<NPC>();
            if (npc == null)
            {
                MelonLogger.Error("Launderer component is not attached to an NPC.");
                return;
            }
            this.laundererData = (new LaundererData(npc)).Load();
            this.nPC = npc;
        }

        public void Start()
        {
            if (this.laundererData.isUnlocked && !this.isMainDialogueReady)
            {
                SetupLaunderDialog();
                return;
            }
            if (
                this.laundererData == null ||
                this.nPC.RelationData.RelationDelta < 4f
                ) return;
            this.dialogueController = this.nPC.dialogueHandler.GetComponent<DialogueController>();
            if (this.dialogueController == null) return;
            this.upfrontInstance = Mathf.Floor(UnityEngine.Random.Range(this.laundererData.MinLaunderAmount, this.laundererData.MaxLaunderAmount));
            AddInitChoice();
        }

        private void AddInitChoice()
        {
            DialogueChoice newChoice = new DialogueChoice() { Enabled = true, ChoiceText = "How about a partnership?" }; DialogueNodeData nodeData = NPCUtilities.CreateDialogueInitNodeData(this.laundererData.MinLaunderAmount, this.laundererData.MaxLaunderAmount, this.laundererData.CutPercentage, this.upfrontInstance, this);
#if IL2CPP
            Il2CppSystem.Collections.Generic.List<DialogueNodeData> nodeDataList = new Il2CppSystem.Collections.Generic.List<DialogueNodeData>();
            nodeDataList.Add(nodeData);
            newChoice.Conversation = new DialogueContainer() { DialogueNodeData = nodeDataList, name = "LAUNDER_INIT_CONTAINER" };
            newChoice.onChoosen.AddListener(new Action(() =>
            {

                this.nPC.dialogueHandler.InitializeDialogue(newChoice.Conversation, true, "LAUNDER_INIT_NODE");
                this.nPC.dialogueHandler.onDialogueChoiceChosen.AddListener(new Action<string>((label) => {
                    if (label == "ACCEPT_LAUNDER_TERMS") {
                        this.laundererData.isUnlocked = true;
                        this.dialogueController.Choices.Remove(newChoice);
                        this.laundererData.CutPercentage = 10.0;
                        this.SendInitialMessage();
                    }
                }));
            }));
#else


            List<DialogueNodeData> nodeDataList = new List<DialogueNodeData>();
            nodeDataList.Add(nodeData);
            newChoice.Conversation = new DialogueContainer() { DialogueNodeData = nodeDataList, name = "LAUNDER_INIT_CONTAINER" };
            newChoice.onChoosen.AddListener(() =>
            {

                this.nPC.dialogueHandler.InitializeDialogue(newChoice.Conversation, true, "LAUNDER_INIT_NODE");
                this.nPC.dialogueHandler.onDialogueChoiceChosen.AddListener((label) => {
                    if (label == "ACCEPT_LAUNDER_TERMS") {
                        MoneyManager moneyManager = NetworkSingleton<MoneyManager>.Instance;
                        moneyManager.ChangeCashBalance(-upfrontInstance, true, true);
                        this.laundererData.isUnlocked = true;
                        this.dialogueController.Choices.Remove(newChoice);
                        this.laundererData.CutPercentage = 10.0;
                        this.SendInitialMessage();
                    }
                });
            });
#endif
            this.dialogueController.AddDialogueChoice(newChoice);
            this.isDialogueInitReady = true;
        }

        private void SendInitialMessage()
        {
            if (this.nPC == null || this.dialogueController == null || !this.laundererData.isUnlocked) return;
            string message = $"I can help you launder your money. I'll take a cut of <color=#ff6b6b>{this.laundererData.CutPercentage}%</color> of the amount for this week.";
            this.nPC.SendTextMessage(message);
            SetupLaunderDialog();
        }

        private void SetupLaunderDialog()
        {
            if (this.nPC == null || this.dialogueController == null || !this.laundererData.isUnlocked) return;
            if (this.laundererUI == null) {
                this.laundererUI = new LaundererUI(this);
            }
            DialogueChoice newChoice = new DialogueChoice() { Enabled = true, ChoiceText = "Launder Dirties" };
#if IL2CPP
            newChoice.onChoosen.AddListener(new Action(() =>
#else

            newChoice.onChoosen.AddListener((() =>
#endif
            {
                if (this.laundererData.CurrentLaunderAmount > 0f || this.laundererData.CurrentTimeLeftSeconds > 0)
                {
                    Singleton<NotificationsManager>.Instance.SendNotification(this.nPC.fullName, $"Laundering <color=#16F01C>${this.laundererData.CurrentLaunderAmount}</color>", NetworkSingleton<MoneyManager>.Instance.LaunderingNotificationIcon, 5f, true);
                    return;
                }
                this.laundererData.InstanceMaxLaunderAmount = Mathf.Floor(UnityEngine.Random.Range(this.laundererData.MinLaunderAmount, this.laundererData.MaxLaunderAmount));
                this.laundererUI.Open();
            }));
            this.dialogueController.AddDialogueChoice(newChoice);
            this.isMainDialogueReady = true;
        }

        public void GiveLaunderedPay()
        {
            if (this.nPC == null || this.dialogueController == null || !this.laundererData.isUnlocked) return;
            NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction(
                $"Launderer_${this.nPC.FirstName}_${this.laundererData.CurrentLaunderAmount}_${this.laundererData.CutPercentage}",
                this.laundererData.ReturnLaunderAmount, 1, "");
            string message = $"Hey, I sent <color=#2e75d9>${this.laundererData.ReturnLaunderAmount}</color> to your bank. \n\nMy Cut: <color=#eb4034>${this.laundererData.CutAmount}</color>";
            this.nPC.SendTextMessage(message);
            Singleton<NotificationsManager>.Instance.SendNotification(this.nPC.fullName, $"<color=#16F01C>${this.laundererData.CurrentLaunderAmount}</color> Laundered", NetworkSingleton<MoneyManager>.Instance.LaunderingNotificationIcon, 5f, true);
            this.laundererData.WeekLaunderReturn += this.laundererData.ReturnLaunderAmount;
            this.laundererData.WeekCutAmount += this.laundererData.CutAmount;
            this.laundererData.CurrentLaunderAmount = 0f;
            this.laundererData.CurrentTimeLeftSeconds = 0f;
            this.laundererData.InstanceMaxLaunderAmount = 0f;
        }

        public void RandomizeCut()
        {
            if (this.nPC == null || this.dialogueController == null || !this.laundererData.isUnlocked) return;
            double shouldRandomizeChance = UnityEngine.Random.Range(0f, 100f);
            float weeklyCutPayDiff = this.laundererData.WeekLaunderReturn - this.laundererData.WeekCutAmount;
            // if weeklycutpaydiff > 80% of the laundered amount, then randomize cut
            if (weeklyCutPayDiff > (this.laundererData.CurrentLaunderAmount * 0.8f) && shouldRandomizeChance < 10f)
            {
                if (this.laundererData.CutPercentage <= 30.0)
                {
                    double randomCutIncrease = Mathf.Floor(UnityEngine.Random.Range(1, 10));
                    while (this.laundererData.CutPercentage + randomCutIncrease > 60)
                    {
                        randomCutIncrease = Mathf.Floor(UnityEngine.Random.Range(1, 10));
                    }
                    this.laundererData.CutPercentage += randomCutIncrease;
                    string message = $"Hey, You seem to be doing so well. To make things fair, I'm changing my cut to <color=#ff6b6b>{this.laundererData.CutPercentage}%</color>.";
                    this.nPC.SendTextMessage(message);
                }
            }
        }

        public void RequireMix()
        {
            if (this.nPC == null || this.dialogueController == null || !this.laundererData.isUnlocked) return;
            int randomIndex = UnityEngine.Random.Range(0, ProductManager.DiscoveredProducts.Count - 1);
            ProductDefinition randomProduct = ProductManager.DiscoveredProducts[randomIndex];
            this.laundererData.RequiredProductID = randomProduct.ID;

#if IL2CPP
            Il2CppSystem.Collections.Generic.List<ProductList.Entry> entries = new Il2CppSystem.Collections.Generic.List<ProductList.Entry>();
#else
            List<ProductList.Entry> entries = new List<ProductList.Entry>();
#endif
            ProductList.Entry entry = new ProductList.Entry {
                ProductID = randomProduct.ID,
                Quality = randomProduct.DrugType == EDrugType.Methamphetamine ? EQuality.Premium : EQuality.Heavenly,
                Quantity = 3
            };
            entries.Add(entry);
            ProductList productList = new ProductList() { entries = entries };

            DeliveryLocation[] locations = DeliveryLocation.FindObjectsOfType<DeliveryLocation>();
            DeliveryLocation deliveryLocation = locations[UnityEngine.Random.Range(0, locations.Length)];

            ContractInfo contractInfo = new ContractInfo(0, new ProductList() { entries = entries }, deliveryLocation.StaticGUID, new QuestWindowConfig { WindowStartTime = 600, WindowEndTime = 1200, IsEnabled = true }, true, 1300, 0,  false);
            
            QuestManager.Instance.CreateContract_Networked(Player.Local.Connection, Guid.NewGuid().ToString(), true, this.nPC.NetworkObject, contractInfo, new GameDateTime(1100), TimeManager.Instance.GetDateTime());
            
            string message = $"Hey, I wanna strengthen our partnership. How about we do a session.\n\nI need you to bring me <color=#6b9cff>3x {randomProduct.Name}</color> in <b>BEST</b> quality you can get at <color=#6b9cff>6:00am to 12:00pm</color>.\nMeet me <color=#6b9cff>{deliveryLocation.LocationDescription}</color>.";
            string contractName = $"Deal for {this.nPC.FirstName}";
            MelonCoroutines.Start(this.AddContractListener(contractName));

             this.nPC.SendTextMessage(message);
        }

        private System.Collections.IEnumerator AddContractListener(string contractName)
        {
            yield return new WaitForSecondsRealtime(3f);
            Contract contract = Contract.Contracts
#if IL2CPP
                ._items
#endif
                .FirstOrDefault(c => (c.name == contractName || c.title == contractName) && c.QuestState == EQuestState.Active);

            while (contract == null)
            {
                yield return new WaitForSeconds(1f);
                contract = Contract.Contracts
#if IL2CPP
                ._items
#endif
                .FirstOrDefault(c => (c.name == contractName || c.title == contractName) && c.QuestState == EQuestState.Active);
            }
#if IL2CPP
            Action<EQuestState> questEndListener = new Action<EQuestState>((result) =>
#else
            UnityAction<EQuestState> questEndListener = new UnityAction<EQuestState>((result) =>
#endif
            {
                string message;
                double randomCutIncrease;
                switch (result)
                {
                    case EQuestState.Expired:
                        randomCutIncrease = Mathf.Floor(UnityEngine.Random.Range(1, 10));
                        if (this.laundererData.CutPercentage >= 60)
                        {
                            message = ".";
                            this.nPC.SendTextMessage(message);
                            break;
                        }
                        while (this.laundererData.CutPercentage + randomCutIncrease > 60)
                        {
                            randomCutIncrease = Mathf.Floor(UnityEngine.Random.Range(1, 10));
                        }
                        this.laundererData.CutPercentage += randomCutIncrease;
                        this.laundererData.RequiredProductID = string.Empty;
                        message = $"Ignoring me huh. I'm increasing my cut to <color=#ff6b6b>{this.laundererData.CutPercentage}%</color>";
                        this.nPC.SendTextMessage(message);
                        break;
                    case EQuestState.Failed:
                        randomCutIncrease = Mathf.Floor(UnityEngine.Random.Range(1, 5));
                        if (this.laundererData.CutPercentage >= 60) {
                            message = "Sometimes, I don't feel like working with you at all.";
                            this.nPC.SendTextMessage(message);
                            break;
                        }
                        while (this.laundererData.CutPercentage + randomCutIncrease > 60)
                        {
                            randomCutIncrease = Mathf.Floor(UnityEngine.Random.Range(1, 10));
                        }
                        this.laundererData.CutPercentage += randomCutIncrease;
                        this.laundererData.RequiredProductID = string.Empty;
                        message = $"I don't like the way you acted earlier. I'm increasing my cut to <color=#ff6b6b>{this.laundererData.CutPercentage}%</color>";
                        this.nPC.SendTextMessage(message);
                        break;
                    case EQuestState.Completed:
                        double randomReduceCut = Mathf.Floor(UnityEngine.Random.Range(1, 10));
                        if (this.laundererData.CutPercentage == 5.0) {
                            message = "Thanks!";
                            this.nPC.SendTextMessage(message);
                            break;
                        }
                        while (this.laundererData.CutPercentage - randomReduceCut < 5)
                        {
                            randomReduceCut = Mathf.Floor(UnityEngine.Random.Range(1, 10));
                        }
                        this.laundererData.CutPercentage -= randomReduceCut;
                        this.laundererData.RequiredProductID = string.Empty;
                        message = $"Hey, I appreciate you bringing me the goods. I'll reduce my cut to <color=#ff6b6b>{this.laundererData.CutPercentage}%</color>.";
                        this.nPC.SendTextMessage(message);
                        break;

                    default:
                        MelonLogger.Msg(result);
                        break;
                }
                contract.onQuestEnd.RemoveAllListeners();
            });
            contract.onQuestEnd.AddListener(questEndListener);
            yield break;
        }

        public void WeekPass()
        {
            if (this.nPC == null || this.dialogueController == null || !this.laundererData.isUnlocked) return;
            this.RandomizeCut();

            int randomChance = UnityEngine.Random.Range(0, 100);
            if (randomChance >= 60)
            {
                this.RequireMix();
            }
        }
    }
}
