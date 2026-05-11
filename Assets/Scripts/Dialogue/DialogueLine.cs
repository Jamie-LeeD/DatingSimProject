using System;
using System.Collections.Generic;

namespace DatingSim.Dialogue
{
    [Serializable]
    public class DialogueLine
    {
        public string lineId;
        public string characterId;
        public string characterName;
        public string emotion;
        public string backgroundId;
        public string text;
        public string nextLineId;
        public List<DialogueChoice> choices = new List<DialogueChoice>();
    }
}
