using System;
using System.Collections.Generic;

namespace DatingSim.SaveSystem
{
    [Serializable]
    public class DialogueProgressSaveData
    {
        public string dialogueId;
        public string currentLineId;
        public bool isDialogueActive;
        public List<string> selectedChoiceIds = new List<string>();
    }
}
