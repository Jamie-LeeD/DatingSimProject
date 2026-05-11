using System;
using System.Collections.Generic;

namespace DatingSim.SaveSystem
{
    [Serializable]
    public class CharacterAffectionSaveEntry
    {
        public string characterId;
        public int affection;
    }

    [Serializable]
    public class RelationshipSaveData
    {
        public List<CharacterAffectionSaveEntry> affectionValues = new List<CharacterAffectionSaveEntry>();
        public List<string> unlockedRouteIds = new List<string>();
    }
}
