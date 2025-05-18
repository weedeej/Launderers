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
using UnityEngine.Events;
#endif
using UnityEngine;
using NPCLaunderers.LaundererSaveManager;
using NPCLaunderers.UI;
using MelonLoader;

namespace NPCLaunderers.NPCScripts
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
        public bool withContract;
        private bool isCheckingLaundry = false;
        private bool isCheckingDialouge = false;
        private bool isCheckingSavedLaundry = false;
        private bool isUpdatingFriendly = false;
        private bool isFriendly = false;
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
                    Singleton<NotificationsManager>.Instance.SendNotification(this.nPC.fullName, $"Laundering <color=#16F01C>${this.laundererData.CurrentLaunderAmount}</color>", this.nPC.MugshotSprite, 5f, true);
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
            Singleton<NotificationsManager>.Instance.SendNotification(this.nPC.fullName, $"<color=#16F01C>${this.laundererData.CurrentLaunderAmount}</color> Laundered", this.nPC.MugshotSprite, 5f, true);
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
            if (weeklyCutPayDiff > Mathf.Floor(UnityEngine.Random.Range(this.laundererData.MinLaunderAmount, this.laundererData.MaxLaunderAmount)) && shouldRandomizeChance < 30f)
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

        public void RequireMix(string productId = null)
        {
            if (this.nPC == null || this.dialogueController == null || !this.laundererData.isUnlocked) return;
            ProductDefinition randomProduct;
            if (productId == null)
            {
                int randomIndex = UnityEngine.Random.Range(0, ProductManager.DiscoveredProducts.Count - 1);
                randomProduct = ProductManager.DiscoveredProducts[randomIndex];
            } else
            {
                randomProduct = ProductManager.DiscoveredProducts
#if IL2CPP
                    ._items.FirstOrDefault(p => p.ID == productId);
#else
                    .Find(p => p.ID == productId);
#endif
                if (randomProduct == null)
                {
                    MelonLogger.Error($"Product with ID {productId} not found.");
                    Singleton<NotificationsManager>.Instance.SendNotification(this.nPC.fullName, $"<color=#f54c4c>Product not found</color>", this.nPC.MugshotSprite, 5f, true);
                    return;
                }
            }

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
            // Not adding listener on Mono
            entries.Add(entry);
            ProductList productList = new ProductList() { entries = entries };

            DeliveryLocation[] locations = DeliveryLocation.FindObjectsOfType<DeliveryLocation>();
            DeliveryLocation deliveryLocation = locations[UnityEngine.Random.Range(0, locations.Length)];

            ContractInfo contractInfo = new ContractInfo(0, new ProductList() { entries = entries }, deliveryLocation.StaticGUID, new QuestWindowConfig { WindowStartTime = 600, WindowEndTime = 1200, IsEnabled = true }, false, 1300, 0, false);
            
            QuestManager.Instance.CreateContract_Networked(Player.Local.Connection, Guid.NewGuid().ToString(), true, this.nPC.NetworkObject, contractInfo, new GameDateTime(1300), TimeManager.Instance.GetDateTime());
            
            string message = $"Hey, I wanna strengthen our partnership. How about we do a session.\n\nI need you to bring me <color=#6b9cff>3x {randomProduct.Name}</color> in <b>BEST</b> quality you can get at <color=#6b9cff>6:00am to 12:00pm</color>.\nMeet me <color=#6b9cff>{deliveryLocation.LocationDescription}</color>.";
            //MelonCoroutines.Start(this.AddContractListener());

             this.nPC.SendTextMessage(message);
        }

        private System.Collections.IEnumerator AddContractListener(Contract contract)
        {
            yield return new WaitForSecondsRealtime(3f);
            if (this.withContract) yield break;
            string contractName = $"Deal for {this.nPC.FirstName}";

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
                        this.laundererData.RequiredProductID = "";
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
                        this.laundererData.RequiredProductID = "";
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
                        this.laundererData.RequiredProductID = "";
                        message = $"Hey, I appreciate you bringing me the goods. I'll reduce my cut to <color=#ff6b6b>{this.laundererData.CutPercentage}%</color>.";
                        this.nPC.SendTextMessage(message);
                        break;

                    default:
                        MelonLogger.Msg(result);
                        break;
                }
                this.laundererData.RequiredProductID = "";
                contract.onQuestEnd.RemoveAllListeners();
                this.withContract = false;
            });
            contract.onQuestEnd.AddListener(questEndListener);
            this.withContract = true;

            QuestHUDUI contractUI = contract.hudUI;
            if (!contractUI.MainLabel.text.Contains("EVENT"))
            {
                contractUI.MainLabel.text = $"<color=#16F01C>[EVENT]</color> {contractUI.MainLabel.text}";
            }
            MelonLogger.Msg($"{this.nPC.FirstName}: Added active contract listner");
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

        public void OnDestroy()
        {
            this.laundererData = null;
            this.isMainDialogueReady = false;
            this.isDialogueInitReady = false;
            this.nPC = null;
            this.dialogueController = null;
            this.laundererUI = null;
            this.upfrontInstance = 0f;
            this.withContract = false;
        }

        private void Update()
        {
            if (!this.isCheckingLaundry)
                MelonCoroutines.Start(StartLaundryCheck());
            if (!this.isCheckingDialouge)
                MelonCoroutines.Start(StartDialougeCheck());
            if (!this.isCheckingSavedLaundry)
                MelonCoroutines.Start(StartSavedLaundryCheck());
            if (!this.isUpdatingFriendly)
                MelonCoroutines.Start(UpdateFriendlyState());
        }

        // Initializers and Updaters
        private System.Collections.IEnumerator StartLaundryCheck()
        {
            this.isCheckingLaundry = true;
            yield return new WaitForSecondsRealtime(1f);
            if (this.laundererData.isUnlocked && this.laundererData.CurrentLaunderAmount > 0f)
            {
                if (this.laundererData.CurrentTimeLeftSeconds > 0f)
                {
                    this.laundererData.CurrentTimeLeftSeconds -= 1;
                    this.isCheckingLaundry = false;
                    yield break;
                }
                this.GiveLaunderedPay();
            }
            this.isCheckingLaundry = false;
        }
        private System.Collections.IEnumerator StartDialougeCheck()
        {
            Debug.Log($"{this.nPC.FirstName} - Unlocked: {this.laundererData.isUnlocked}, Init Ready: {this.isDialogueInitReady}, Is Friendly: {this.isFriendly}, Main Ready: {this.isMainDialogueReady}");
            this.isCheckingDialouge = true;
            yield return new WaitForSecondsRealtime(1f);
            if (this.dialogueController == null)
            {
                this.dialogueController = this.nPC.dialogueHandler.GetComponent<DialogueController>();
            }
            if (!this.laundererData.isUnlocked && !this.isDialogueInitReady && this.isFriendly)
            {
                this.nPC.SendTextMessage($"Hey, Let me know if you need a partnership.");
                this.upfrontInstance = Mathf.Floor(UnityEngine.Random.Range(this.laundererData.MinLaunderAmount, this.laundererData.MaxLaunderAmount));
                this.AddInitChoice();
            }
            if (this.laundererData.isUnlocked && !this.isMainDialogueReady && this.isFriendly)
            {
                Debug.Log("Setting up launder");
                this.SetupLaunderDialog();
                Debug.Log("DONE????");
            }
            this.isCheckingDialouge = false;
        }
        private System.Collections.IEnumerator StartSavedLaundryCheck()
        {
            this.isCheckingSavedLaundry = true;
            yield return new WaitForSecondsRealtime(1f);
            if (this.laundererData.isUnlocked && this.laundererData.RequiredProductID != "" && !this.withContract)
            {
                this.LoadLaundererData();
            }
            this.isCheckingSavedLaundry = false;
        }
        private System.Collections.IEnumerator UpdateFriendlyState()
        {
            this.isUpdatingFriendly = true;
            yield return new WaitForSecondsRealtime(5f); // Updates every 5s
            this.isFriendly = this.nPC.RelationData.RelationDelta >= 4f;
            this.isUpdatingFriendly = false;
        }


        private void LoadLaundererData()
        {
            if (this.laundererData == null || this.laundererData.RequiredProductID == "") return;
            Contract activeContract = Contract.FindObjectsOfType<Contract>()

                .FirstOrDefault(c => {
                    return c.QuestState == EQuestState.Active && c.GetQuestTitle().Contains(this.nPC.FirstName);
                }
#if IL2CPP
                ,null
#endif
                ); // Don't remove pls T.T

            if (
                activeContract != null &&
                !this.withContract
                )
            {
                MelonCoroutines.Start(this.AddContractListener(activeContract));
            }
        }

    }
}
