using System;
using System.Collections.Generic;

namespace DatingSim.Dialogue
{
    [Serializable]
    public class DialogueSequence
    {
        public string dialogueId;
        public string startLineId;
        public List<DialogueLine> lines = new List<DialogueLine>();
    }
}
