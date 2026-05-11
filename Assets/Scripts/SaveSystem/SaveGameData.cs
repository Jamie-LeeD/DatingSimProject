using System;
using System.Collections.Generic;

namespace DatingSim.SaveSystem
{
    [Serializable]
    public class SaveExtensionData
    {
        public string key;
        public string jsonPayload;
    }

    [Serializable]
    public class SaveGameData
    {
        public string saveVersion = "1.0.0";
        public string savedAtUtc;
        public int slotIndex;

        public DialogueProgressSaveData dialogueProgress = new DialogueProgressSaveData();
        public RelationshipSaveData relationshipData = new RelationshipSaveData();

        // Reserved for future systems (inventory, quests, flags, etc.) without schema breakage.
        public List<SaveExtensionData> extensionData = new List<SaveExtensionData>();
    }
}
