using Clean_Cash.NPCScripts;
using UnityEngine;
using UnityEngine.UIElements;
using MelonLoader;
#if IL2CPP
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.UI;
using Il2CppSystem.Linq;
#else
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.UI;
using ScheduleOne.Linq;
#endif

namespace Clean_Cash.UI
{
    public class LaundererUI
    {
        private bool isOpen = false;
        private Launderer launderer;
        public GameObject gameObject;
#if IL2CPP
        Il2CppAssetBundle bundle;
#else
        AssetBundle bundle = null;
#endif
        public LaundererUI(Launderer launderer)
        {
            this.launderer = launderer;
#if IL2CPP
            var loadedBundles = Il2CppAssetBundleManager.GetAllLoadedAssetBundles().ToArray();
            if (loadedBundles.Length == 1) bundle = loadedBundles[0];
#else
            var loadedBundles = AssetBundle.GetAllLoadedAssetBundles().ToList();
            if (loadedBundles.Length == 1) bundle = loadedBundles[0];
#endif

            else
            {
                foreach (var loaded in loadedBundles)
                {
#if IL2CPP
                    if (loaded.Contains("assets/laundererui.prefab"))
#else
                    if (loaded.name == "launderers.assetbundle")
#endif
                    {
                        bundle = loaded;
                        break;
                    }
                }
            }
        }

        public void SetLaunderer(Launderer launderer)
        {
            this.launderer = launderer;
        }

        public void Open()
        {
            if (isOpen) return;
            this.Show();
            isOpen = true;
        }

        public void Close()
        {
            if (!isOpen) return;
            this.Hide();
            isOpen = false;
        }
        private void Show()
        {
            this.gameObject = bundle.LoadAsset<GameObject>("assets/laundererui.prefab");
            this.gameObject = GameObject.Instantiate(this.gameObject);
            UIDocument doc = this.gameObject.GetComponent<UIDocument>();
            MelonLogger.Msg(7);
            VisualElement rootVisual = doc.rootVisualElement;
            MelonLogger.Msg(6);
            VisualElement content = rootVisual.Q<VisualElement>("LaundererContent");

            MelonLogger.Msg(5);
            VisualElement buttons = content.Q<VisualElement>("buttons");
            MelonLogger.Msg(4);
            Button closeButton = buttons.Q<Button>("btn_close");
            Button submitButton = buttons.Q<Button>("btn_submit");
            MelonLogger.Msg(1);
            IntegerField amountField = content.Q<IntegerField>("NumberInput");
            MelonLogger.Msg(2);
            Label npcName = amountField.Q<Label>("NpcName");
            MelonLogger.Msg(3);

            amountField.label = amountField.label.Replace("<MaxLaunder>", $"{this.launderer.laundererData.InstanceMaxLaunderAmount}");
            npcName.text = npcName.text.Replace("<NPCNAME>", this.launderer.nPC.fullName);

            submitButton.RegisterCallback<ClickEvent>(new Action<ClickEvent>(ev =>
            {
                MoneyManager moneyManager = NetworkSingleton<MoneyManager>.Instance;
                try
                {
                    float inputValue = (float)amountField.value;
                    if (inputValue < 1f) return;
                    if (moneyManager.cashBalance < inputValue)
                    {
                        MelonLogger.Error($"Input value {inputValue} exceeds cash balance {moneyManager.cashBalance}");
                        Singleton<NotificationsManager>.Instance.SendNotification(this.launderer.nPC.FirstName, $"<color=#f54c4c>You don't have enough money</color>", NetworkSingleton<MoneyManager>.Instance.LaunderingNotificationIcon, 5f, true);
                        return;
                    }
                    if (inputValue > this.launderer.laundererData.InstanceMaxLaunderAmount)
                    {
                        MelonLogger.Error($"Input value {inputValue} exceeds max launder amount {this.launderer.laundererData.InstanceMaxLaunderAmount}");
                        Singleton<NotificationsManager>.Instance.SendNotification(this.launderer.nPC.FirstName, $"<color=#f54c4c>You can only launder ${this.launderer.laundererData.InstanceMaxLaunderAmount}</color>", NetworkSingleton<MoneyManager>.Instance.LaunderingNotificationIcon, 5f, true);
                        return;
                    }
                    moneyManager.ChangeCashBalance(-inputValue, true, true);
                    this.launderer.laundererData.CurrentLaunderAmount = inputValue;
                    this.launderer.laundererData.CurrentTimeLeftSeconds = 10; // 720; // Half-day in-game
                    string message = $"Laundering <color=#16F01C>${inputValue}</color>.I will transfer <color=#38f577>${this.launderer.laundererData.ReturnLaunderAmount}</color> to your account in 12 hours.";
                    this.launderer.nPC.SendTextMessage(message);
                    this.Close();
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"Failed to load LaundererUI: {e}");
                    if (e is FormatException)
                    {
                        Singleton<NotificationsManager>.Instance.SendNotification(this.launderer.nPC.FirstName, $"<color=#f54c4c>Invalid input value</color>", NetworkSingleton<MoneyManager>.Instance.LaunderingNotificationIcon, 5f, true);
                    }
                }
            }));

            closeButton.RegisterCallback<ClickEvent>(new Action<ClickEvent>(ev =>
            {
                this.Close();
            }));
        }

        private void Hide()
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
