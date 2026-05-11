using System;
using System.Collections.Generic;
using UnityEngine;

namespace DatingSim.Characters
{
    [CreateAssetMenu(menuName = "DatingSim/Characters/Character Database", fileName = "CharacterDatabase")]
    public class CharacterDatabase : ScriptableObject
    {
        [SerializeField] private List<CharacterProfile> characters = new List<CharacterProfile>();

        private readonly Dictionary<string, CharacterProfile> charactersById = new Dictionary<string, CharacterProfile>();

        public IReadOnlyList<CharacterProfile> Characters => characters;

        private void OnEnable()
        {
            RebuildLookup();
        }

        private void OnValidate()
        {
            RebuildLookup();
        }

        public bool TryGetCharacter(string characterId, out CharacterProfile profile)
        {
            profile = null;
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return false;
            }

            if (charactersById.Count != characters.Count)
            {
                RebuildLookup();
            }

            return charactersById.TryGetValue(characterId, out profile);
        }

        [ContextMenu("Rebuild Character Lookup")]
        public void RebuildLookup()
        {
            charactersById.Clear();

            for (int i = 0; i < characters.Count; i++)
            {
                CharacterProfile profile = characters[i];
                if (profile == null || string.IsNullOrWhiteSpace(profile.CharacterId))
                {
                    continue;
                }

                // Last entry wins to keep behavior deterministic in case of duplicates.
                charactersById[profile.CharacterId] = profile;
            }
        }
    }
}
