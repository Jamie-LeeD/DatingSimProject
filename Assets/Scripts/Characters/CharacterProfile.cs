using System;
using System.Collections.Generic;
using UnityEngine;

namespace DatingSim.Characters
{
    [Serializable]
    public class CharacterEmotionSprite
    {
        public string emotionId;
        public Sprite sprite;
    }

    [Serializable]
    public class CharacterRouteInfo
    {
        public string routeId;
        public int requiredAffection = 1;
        [TextArea] public string description;
    }

    [CreateAssetMenu(menuName = "DatingSim/Characters/Character Profile", fileName = "CharacterProfile_")]
    public class CharacterProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string characterId;
        [SerializeField] private string characterName;

        [Header("Visuals")]
        [SerializeField] private Sprite defaultPortrait;
        [SerializeField] private List<CharacterEmotionSprite> emotionSprites = new List<CharacterEmotionSprite>();

        [Header("Relationship Settings")]
        [SerializeField] private int startingAffection;
        [SerializeField] private int minAffection = -100;
        [SerializeField] private int maxAffection = 100;
        [SerializeField] private List<CharacterRouteInfo> routes = new List<CharacterRouteInfo>();

        public string CharacterId => characterId;
        public string CharacterName => characterName;
        public Sprite DefaultPortrait => defaultPortrait;
        public int StartingAffection => startingAffection;
        public int MinAffection => minAffection;
        public int MaxAffection => maxAffection;
        public IReadOnlyList<CharacterRouteInfo> Routes => routes;
        public IReadOnlyList<CharacterEmotionSprite> EmotionSprites => emotionSprites;

        public bool TryGetEmotionSprite(string emotionId, out Sprite sprite)
        {
            sprite = null;
            if (string.IsNullOrWhiteSpace(emotionId))
            {
                return false;
            }

            for (int i = 0; i < emotionSprites.Count; i++)
            {
                CharacterEmotionSprite entry = emotionSprites[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.emotionId))
                {
                    continue;
                }

                if (string.Equals(entry.emotionId, emotionId, StringComparison.OrdinalIgnoreCase))
                {
                    sprite = entry.sprite;
                    return sprite != null;
                }
            }

            return false;
        }
    }
}
