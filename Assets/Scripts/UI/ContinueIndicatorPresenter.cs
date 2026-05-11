using DatingSim.Dialogue;
using TMPro;
using UnityEngine;

namespace DatingSim.UI
{
    public class ContinueIndicatorPresenter : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private TypewriterEffect typewriterEffect;

        [Header("UI")]
        [SerializeField] private GameObject indicatorRoot;
        [SerializeField] private TextMeshProUGUI indicatorText;
        [SerializeField] private string indicatorLabel = ">>";
        [SerializeField] private Transform choicesContainer;

        private void Awake()
        {
            if (indicatorText != null)
            {
                indicatorText.text = indicatorLabel;
            }

            SetIndicatorVisible(false);
        }

        private void OnEnable()
        {
            if (typewriterEffect != null)
            {
                typewriterEffect.TypewriterStarted += HandleTypingStarted;
                typewriterEffect.TypewriterCompleted += HandleTypingCompleted;
            }
        }

        private void OnDisable()
        {
            if (typewriterEffect != null)
            {
                typewriterEffect.TypewriterStarted -= HandleTypingStarted;
                typewriterEffect.TypewriterCompleted -= HandleTypingCompleted;
            }
        }

        private void Update()
        {
            RefreshIndicator();
        }

        private void HandleTypingStarted()
        {
            SetIndicatorVisible(false);
        }

        private void HandleTypingCompleted()
        {
            RefreshIndicator();
        }

        private void RefreshIndicator()
        {
            if (dialogueManager == null)
            {
                SetIndicatorVisible(false);
                return;
            }

            bool hasChoicesVisible = choicesContainer != null && choicesContainer.childCount > 0;
            bool shouldShow = dialogueManager.CanAdvance && !hasChoicesVisible;
            SetIndicatorVisible(shouldShow);
        }

        private void SetIndicatorVisible(bool visible)
        {
            if (indicatorRoot != null)
            {
                indicatorRoot.SetActive(visible);
            }
        }
    }
}
