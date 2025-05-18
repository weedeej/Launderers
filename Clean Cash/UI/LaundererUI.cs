using NPCLaunderers.NPCScripts;
using UnityEngine;
using UnityEngine.UIElements;
using MelonLoader;

#if IL2CPP
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.UI;
using Il2CppSystem.Linq;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.UI;
using ScheduleOne.PlayerScripts;
#endif

namespace NPCLaunderers.UI
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
            if (loadedBundles.Count == 1) bundle = loadedBundles[0];
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
            VisualElement rootVisual = doc.rootVisualElement;
            VisualElement content = rootVisual.Q<VisualElement>("LaundererContent");
            VisualElement header = content.Q<VisualElement>("Header");
            Label version = header.Q<Label>("Version");
            version.text = MelonAssembly.FindMelonInstance<Core>().Info.Version;

            VisualElement buttons = content.Q<VisualElement>("buttons");
            Button closeButton = buttons.Q<Button>("btn_close");
            Button submitButton = buttons.Q<Button>("btn_submit");
            IntegerField amountField = content.Q<IntegerField>("NumberInput");
            Label npcName = amountField.Q<Label>("NpcName");

            amountField.label = amountField.label.Replace("<MaxLaunder>", $"{this.launderer.laundererData.InstanceMaxLaunderAmount}");
            npcName.text = npcName.text.Replace("<NPCNAME>", this.launderer.nPC.fullName);

#if IL2CPP
            submitButton.RegisterCallback<ClickEvent>(new Action<ClickEvent>(ev =>
#else
            submitButton.RegisterCallback<ClickEvent>(ev =>
#endif
            {
                MoneyManager moneyManager = NetworkSingleton<MoneyManager>.Instance;
                try
                {
                    float inputValue = (float)amountField.value;
                    if (inputValue < 1f) return;
                    if (moneyManager.cashBalance < inputValue)
                    {
                        MelonLogger.Error($"Input value {inputValue} exceeds cash balance {moneyManager.cashBalance}");
                        Singleton<NotificationsManager>.Instance.SendNotification(this.launderer.nPC.FirstName, $"<color=#f54c4c>You don't have enough money</color>", this.launderer.nPC.MugshotSprite, 5f, true);
                        return;
                    }
                    if (inputValue > this.launderer.laundererData.InstanceMaxLaunderAmount)
                    {
                        MelonLogger.Error($"Input value {inputValue} exceeds max launder amount {this.launderer.laundererData.InstanceMaxLaunderAmount}");
                        Singleton<NotificationsManager>.Instance.SendNotification(this.launderer.nPC.FirstName, $"<color=#f54c4c>Max of ${this.launderer.laundererData.InstanceMaxLaunderAmount}</color>", this.launderer.nPC.MugshotSprite, 5f, true);
                        return;
                    }
                    moneyManager.ChangeCashBalance(-inputValue, true, true);
                    this.launderer.laundererData.CurrentLaunderAmount = inputValue;
                    this.launderer.laundererData.CurrentTimeLeftSeconds = 720; // 720; // Half-day in-game
                    string message = $"Laundering <color=#16F01C>${inputValue}</color>.I will transfer <color=#6b9cff>${this.launderer.laundererData.ReturnLaunderAmount}</color> to your account in 12 hours.";
                    this.launderer.nPC.SendTextMessage(message);
                    this.Close();
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"Failed to load LaundererUI: {e}");
                    if (e is FormatException)
                    {
                        Singleton<NotificationsManager>.Instance.SendNotification(this.launderer.nPC.FirstName, $"<color=#f54c4c>Invalid input value</color>", this.launderer.nPC.MugshotSprite, 5f, true);
                    }
                }
#if IL2CPP
            }));
#else
            });
#endif

#if IL2CPP
            closeButton.RegisterCallback<ClickEvent>(new Action<ClickEvent>(ev =>
#else
            closeButton.RegisterCallback<ClickEvent>((ev =>
#endif
            {
                this.Close();
            }));

            PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(this.gameObject.name);
            Player.Deactivate(true);
        }

        private void Hide()
        {
            PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(this.gameObject.name);
            Player.Activate();
            GameObject.Destroy(this.gameObject);
        }
    }
}
