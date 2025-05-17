#if IL2CPP
using Il2CppScheduleOne.Dialogue;
#else
using ScheduleOne.Dialogue;
#endif

namespace NPCLaunderers.NPCScripts
{
    public static class NPCUtilities
    {
        public static DialogueNodeData CreateDialogueInitNodeData(float amount_min, float amount_max, double cut, float upfront, Launderer launderer)
        {
            DialogueNodeData nodeData = new DialogueNodeData();
            nodeData.DialogueNodeLabel = "LAUNDER_INIT_NODE";
            nodeData.DialogueText = $"Alright, I can wash around <color=#16F01C>${amount_min} - ${amount_max}</color> of your dirties. But there will be some cuts, of course. I'll let you know how much every week. But for now, Let us start with <color=#6b9cff>{cut}%</color> and an upfront of <color=#16F01C>${upfront}</color>";
            DialogueChoiceData[] choices = {
                new DialogueChoiceData() { ChoiceLabel = "ACCEPT_LAUNDER_TERMS", ChoiceText = "Sure, Let's do it." },
                new DialogueChoiceData() { ChoiceLabel = "DECLINE_LAUNDER_TERMS", ChoiceText = "Nevermind" },
            };
            nodeData.choices = choices;

            return nodeData;
        }
    }
}
