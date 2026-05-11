using System;

namespace DatingSim.Characters
{
    [Serializable]
    public class RelationshipThreshold
    {
        public string characterId;
        public string routeId;
        public int requiredAffection = 1;
    }
}
