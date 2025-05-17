#if IL2CPP
using Il2CppScheduleOne.Dialogue;
#else
using ScheduleOne.Dialogue;
#endif

namespace Clean_Cash.NPCScripts
{
    public static class NPCUtilities
    {
        public static DialogueNodeData CreateDialogueInitNodeData(float amount_min, float amount_max, double cut, Launderer launderer)
        {
            DialogueNodeData nodeData = new DialogueNodeData();
            nodeData.DialogueNodeLabel = "LAUNDER_INIT_NODE";
            nodeData.DialogueText = $"Alright, I can wash around <color=#38f577>${amount_min.ToString()} - ${amount_max.ToString()}</color> of your dirties. But there will be some cuts, of course. I'll let you know how much every week. But for now, Let us start with <color=#384bf5>{cut}%</color>";
            DialogueChoiceData[] choices = {
                new DialogueChoiceData() { ChoiceLabel = "ACCEPT_LAUNDER_TERMS", ChoiceText = "Sure, Let's do it." },
                new DialogueChoiceData() { ChoiceLabel = "DECLINE_LAUNDER_TERMS", ChoiceText = "Nevermind" },
            };
            nodeData.choices = choices;

            return nodeData;
        }
    }
}
