using System;
using System.Collections.Generic;
using DatingSim.Dialogue;
using UnityEngine;
using UnityEngine.UI;

namespace DatingSim.UI
{
    public class PortraitFocusPresenter : MonoBehaviour
    {
        [Serializable]
        private class PortraitFocusSlot
        {
            public string characterId;
            public Image portraitImage;
            public CanvasGroup canvasGroup;
        }

        [Header("Dependencies")]
        [SerializeField] private DialogueManager dialogueManager;

        [Header("Portrait Slots")]
        [SerializeField] private List<PortraitFocusSlot> slots = new List<PortraitFocusSlot>();

        [Header("Visibility / Focus")]
        [SerializeField, Range(0f, 1f)] private float speakingAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float nonSpeakingAlpha = 0.45f;
        [SerializeField] private bool hideUnusedPortraits = true;
        [SerializeField, Range(0f, 1f)] private float hiddenAlpha = 0f;

        private readonly Dictionary<string, PortraitFocusSlot> slotByCharacterId = new Dictionary<string, PortraitFocusSlot>();

        private void Awake()
        {
            RebuildLookup();
            ApplyIdleState();
        }

        private void OnEnable()
        {
            if (dialogueManager != null)
            {
                dialogueManager.LineStarted += HandleLineStarted;
                dialogueManager.DialogueEnded += HandleDialogueEnded;
            }
        }

        private void OnDisable()
        {
            if (dialogueManager != null)
            {
                dialogueManager.LineStarted -= HandleLineStarted;
                dialogueManager.DialogueEnded -= HandleDialogueEnded;
            }
        }

        [ContextMenu("Rebuild Portrait Focus Lookup")]
        public void RebuildLookup()
        {
            slotByCharacterId.Clear();
            for (int i = 0; i < slots.Count; i++)
            {
                PortraitFocusSlot slot = slots[i];
                if (slot == null || string.IsNullOrWhiteSpace(slot.characterId))
                {
                    continue;
                }

                if (slot.canvasGroup == null && slot.portraitImage != null)
                {
                    slot.canvasGroup = slot.portraitImage.GetComponent<CanvasGroup>();
                }

                slotByCharacterId[slot.characterId] = slot;
            }
        }

        private void HandleLineStarted(DialogueLine line)
        {
            string activeCharacterId = line != null ? line.characterId : string.Empty;

            for (int i = 0; i < slots.Count; i++)
            {
                PortraitFocusSlot slot = slots[i];
                if (slot == null || slot.canvasGroup == null)
                {
                    continue;
                }

                bool hasPortraitSprite = slot.portraitImage != null && slot.portraitImage.sprite != null;
                if (hideUnusedPortraits && !hasPortraitSprite)
                {
                    slot.canvasGroup.alpha = hiddenAlpha;
                    continue;
                }

                bool isSpeaking = !string.IsNullOrWhiteSpace(activeCharacterId) &&
                                  string.Equals(slot.characterId, activeCharacterId, StringComparison.Ordinal);

                slot.canvasGroup.alpha = isSpeaking ? speakingAlpha : nonSpeakingAlpha;
            }
        }

        private void HandleDialogueEnded(string _)
        {
            ApplyIdleState();
        }

        private void ApplyIdleState()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                PortraitFocusSlot slot = slots[i];
                if (slot == null || slot.canvasGroup == null)
                {
                    continue;
                }

                bool hasPortraitSprite = slot.portraitImage != null && slot.portraitImage.sprite != null;
                slot.canvasGroup.alpha = hideUnusedPortraits && !hasPortraitSprite ? hiddenAlpha : nonSpeakingAlpha;
            }
        }
    }
}
