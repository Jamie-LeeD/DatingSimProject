using System;
using System.Collections;
using System.Collections.Generic;
using DatingSim.Dialogue;
using UnityEngine;
using UnityEngine.UI;

namespace DatingSim.UI
{
    public class BackgroundTransitionPresenter : MonoBehaviour
    {
        [Serializable]
        private class BackgroundEntry
        {
            public string backgroundId;
            public Sprite sprite;
        }

        [Header("Dependencies")]
        [SerializeField] private DialogueManager dialogueManager;

        [Header("Crossfade Images")]
        [SerializeField] private Image fromImage;
        [SerializeField] private Image toImage;

        [Header("Mapping")]
        [SerializeField] private List<BackgroundEntry> backgrounds = new List<BackgroundEntry>();
        [SerializeField, Min(0f)] private float fadeDuration = 0.35f;

        private readonly Dictionary<string, Sprite> spriteById = new Dictionary<string, Sprite>();
        private Coroutine fadeRoutine;
        private string currentBackgroundId;

        private void Awake()
        {
            RebuildLookup();
            if (fromImage != null)
            {
                SetImageAlpha(fromImage, 1f);
            }

            if (toImage != null)
            {
                SetImageAlpha(toImage, 0f);
            }
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

        [ContextMenu("Rebuild Background Transition Lookup")]
        public void RebuildLookup()
        {
            spriteById.Clear();
            for (int i = 0; i < backgrounds.Count; i++)
            {
                BackgroundEntry entry = backgrounds[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.backgroundId) || entry.sprite == null)
                {
                    continue;
                }

                spriteById[entry.backgroundId] = entry.sprite;
            }
        }

        private void HandleBackgroundChanged(string backgroundId)
        {
            if (string.IsNullOrWhiteSpace(backgroundId) || backgroundId == currentBackgroundId)
            {
                return;
            }

            if (!spriteById.TryGetValue(backgroundId, out Sprite nextSprite) || nextSprite == null)
            {
                return;
            }

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = StartCoroutine(FadeToBackgroundRoutine(backgroundId, nextSprite));
        }

        private IEnumerator FadeToBackgroundRoutine(string backgroundId, Sprite nextSprite)
        {
            if (fromImage == null || toImage == null)
            {
                yield break;
            }

            toImage.sprite = nextSprite;
            SetImageAlpha(toImage, 0f);

            if (fadeDuration <= 0f)
            {
                fromImage.sprite = nextSprite;
                SetImageAlpha(fromImage, 1f);
                SetImageAlpha(toImage, 0f);
                currentBackgroundId = backgroundId;
                fadeRoutine = null;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                SetImageAlpha(toImage, t);
                yield return null;
            }

            fromImage.sprite = nextSprite;
            SetImageAlpha(fromImage, 1f);
            SetImageAlpha(toImage, 0f);
            currentBackgroundId = backgroundId;
            fadeRoutine = null;
        }

        private static void SetImageAlpha(Image image, float alpha)
        {
            if (image == null)
            {
                return;
            }

            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}
