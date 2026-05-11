using System;
using System.Collections.Generic;
using DatingSim.Dialogue;
using UnityEngine;
using UnityEngine.UI;

namespace DatingSim.UI
{
    public class BackgroundPresenter : MonoBehaviour
    {
        [Serializable]
        private class BackgroundEntry
        {
            public string backgroundId;
            public Sprite sprite;
        }

        [Header("Dependencies")]
        [SerializeField] private DialogueManager dialogueManager;

        [Header("UI")]
        [SerializeField] private Image backgroundImage;

        [Header("Background Mapping")]
        [SerializeField] private List<BackgroundEntry> backgrounds = new List<BackgroundEntry>();

        private readonly Dictionary<string, Sprite> spriteByBackgroundId = new Dictionary<string, Sprite>();

        private void Awake()
        {
            RebuildLookup();
        }

        private void OnEnable()
        {
            if (dialogueManager != null)
            {
                dialogueManager.BackgroundChanged += HandleBackgroundChanged;
            }
        }

        private void OnDisable()
        {
            if (dialogueManager != null)
            {
                dialogueManager.BackgroundChanged -= HandleBackgroundChanged;
            }
        }

        [ContextMenu("Rebuild Background Lookup")]
        public void RebuildLookup()
        {
            spriteByBackgroundId.Clear();

            for (int i = 0; i < backgrounds.Count; i++)
            {
                BackgroundEntry entry = backgrounds[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.backgroundId) || entry.sprite == null)
                {
                    continue;
                }

                spriteByBackgroundId[entry.backgroundId] = entry.sprite;
            }
        }

        private void HandleBackgroundChanged(string backgroundId)
        {
            if (backgroundImage == null || string.IsNullOrWhiteSpace(backgroundId))
            {
                return;
            }

            if (spriteByBackgroundId.Count != backgrounds.Count)
            {
                RebuildLookup();
            }

            if (spriteByBackgroundId.TryGetValue(backgroundId, out Sprite sprite))
            {
                backgroundImage.sprite = sprite;
            }
        }
    }
}
