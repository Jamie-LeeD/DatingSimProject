using System;

namespace DatingSim.Dialogue
{
    [Serializable]
    public class DialogueChoice
    {
        public string choiceId;
        public string choiceText;
        public string nextLineId;

        // Hook for future relationship systems.
        public string affectionTargetId;
        public int affectionDelta;
    }
}
