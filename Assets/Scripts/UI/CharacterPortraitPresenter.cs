using System;
using System.Collections.Generic;
using DatingSim.Characters;
using DatingSim.Dialogue;
using UnityEngine;
using UnityEngine.UI;

namespace DatingSim.UI
{
    public class CharacterPortraitPresenter : MonoBehaviour
    {
        [Serializable]
        private class PortraitSlot
        {
            public string characterId;
            public Image portraitImage;
        }

        [Header("Dependencies")]
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private CharacterDatabase characterDatabase;

        [Header("Portrait Slots")]
        [SerializeField] private List<PortraitSlot> portraitSlots = new List<PortraitSlot>();

        private readonly Dictionary<string, Image> imageByCharacterId = new Dictionary<string, Image>();

        private void Awake()
        {
            RebuildSlotLookup();
            ApplyDefaultPortraits();
        }

        private void OnEnable()
        {
            if (dialogueManager != null)
            {
                dialogueManager.CharacterEmotionChanged += HandleEmotionChanged;
            }
        }

        private void OnDisable()
        {
            if (dialogueManager != null)
            {
                dialogueManager.CharacterEmotionChanged -= HandleEmotionChanged;
            }
        }

        [ContextMenu("Rebuild Slot Lookup")]
        public void RebuildSlotLookup()
        {
            imageByCharacterId.Clear();
            for (int i = 0; i < portraitSlots.Count; i++)
            {
                PortraitSlot slot = portraitSlots[i];
                if (slot == null || slot.portraitImage == null || string.IsNullOrWhiteSpace(slot.characterId))
                {
                    continue;
                }

                imageByCharacterId[slot.characterId] = slot.portraitImage;
            }
        }

        [ContextMenu("Apply Default Portraits")]
        public void ApplyDefaultPortraits()
        {
            if (characterDatabase == null)
            {
                return;
            }

            for (int i = 0; i < portraitSlots.Count; i++)
            {
                PortraitSlot slot = portraitSlots[i];
                if (slot == null || slot.portraitImage == null || string.IsNullOrWhiteSpace(slot.characterId))
                {
                    continue;
                }

                if (characterDatabase.TryGetCharacter(slot.characterId, out CharacterProfile profile))
                {
                    slot.portraitImage.sprite = profile.DefaultPortrait;
                }
            }
        }

        private void HandleEmotionChanged(string characterId, string emotionId)
        {
            if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(emotionId))
            {
                return;
            }

            if (characterDatabase == null || !characterDatabase.TryGetCharacter(characterId, out CharacterProfile profile))
            {
                return;
            }

            if (!imageByCharacterId.TryGetValue(characterId, out Image targetImage) || targetImage == null)
            {
                return;
            }

            if (profile.TryGetEmotionSprite(emotionId, out Sprite emotionSprite))
            {
                targetImage.sprite = emotionSprite;
            }
            else
            {
                targetImage.sprite = profile.DefaultPortrait;
            }
        }
    }
}
