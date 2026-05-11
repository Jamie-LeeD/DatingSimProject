using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DatingSim.Dialogue
{
    public class TypewriterEffect : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float charactersPerSecond = 45f;
        [SerializeField] private bool useUnscaledTime = true;

        public event Action TypewriterStarted;
        public event Action TypewriterCompleted;

        public bool IsTyping { get; private set; }

        private Coroutine typingRoutine;
        private TextMeshProUGUI activeTarget;
        private string activeText;
        private Action completionCallback;

        public void Play(TextMeshProUGUI target, string text, Action onComplete = null)
        {
            if (target == null)
            {
                return;
            }

            StopCurrentRoutine();

            activeTarget = target;
            activeText = text ?? string.Empty;
            completionCallback = onComplete;
            typingRoutine = StartCoroutine(TypeRoutine());
        }

        public void Skip()
        {
            if (!IsTyping || activeTarget == null)
            {
                return;
            }

            StopCoroutine(typingRoutine);
            typingRoutine = null;

            activeTarget.maxVisibleCharacters = int.MaxValue;
            activeTarget.text = activeText;

            IsTyping = false;
            TypewriterCompleted?.Invoke();
            completionCallback?.Invoke();
            completionCallback = null;
        }

        private IEnumerator TypeRoutine()
        {
            IsTyping = true;
            TypewriterStarted?.Invoke();

            activeTarget.text = activeText;
            activeTarget.maxVisibleCharacters = 0;
            activeTarget.ForceMeshUpdate();

            int totalCharacters = activeTarget.textInfo.characterCount;
            float secondsPerCharacter = 1f / charactersPerSecond;
            float elapsed = 0f;
            int visibleCharacters = 0;

            while (visibleCharacters < totalCharacters)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                while (elapsed >= secondsPerCharacter && visibleCharacters < totalCharacters)
                {
                    elapsed -= secondsPerCharacter;
                    visibleCharacters++;
                    activeTarget.maxVisibleCharacters = visibleCharacters;
                }

                yield return null;
            }

            IsTyping = false;
            typingRoutine = null;
            TypewriterCompleted?.Invoke();
            completionCallback?.Invoke();
            completionCallback = null;
        }

        private void StopCurrentRoutine()
        {
            if (typingRoutine != null)
            {
                StopCoroutine(typingRoutine);
                typingRoutine = null;
            }

            IsTyping = false;
            completionCallback = null;
        }
    }
}
