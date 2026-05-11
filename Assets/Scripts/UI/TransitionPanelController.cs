using System.Collections;
using DatingSim.Core;
using UnityEngine;

namespace DatingSim.UI
{
    public class TransitionPanelController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameFlowStateMachine gameFlowStateMachine;

        [Header("UI")]
        [SerializeField] private CanvasGroup transitionCanvasGroup;
        [SerializeField] private Animator transitionAnimator;

        [Header("Fade")]
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.2f;
        [SerializeField] private string transitionTriggerName = "PlayTransition";

        private Coroutine fadeRoutine;

        private void Awake()
        {
            SetAlpha(0f);
            SetInteractable(false);
        }

        private void OnEnable()
        {
            if (gameFlowStateMachine != null)
            {
                gameFlowStateMachine.StateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (gameFlowStateMachine != null)
            {
                gameFlowStateMachine.StateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameFlowStateId _, GameFlowStateId to)
        {
            if (to == GameFlowStateId.Transition)
            {
                PlayTransitionIn();
            }
            else
            {
                PlayTransitionOut();
            }
        }

        public void PlayTransitionIn()
        {
            TriggerAnimator();
            FadeTo(1f);
        }

        public void PlayTransitionOut()
        {
            FadeTo(0f);
        }

        private void TriggerAnimator()
        {
            if (transitionAnimator == null || string.IsNullOrWhiteSpace(transitionTriggerName))
            {
                return;
            }

            transitionAnimator.ResetTrigger(transitionTriggerName);
            transitionAnimator.SetTrigger(transitionTriggerName);
        }

        private void FadeTo(float targetAlpha)
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
        }

        private IEnumerator FadeRoutine(float targetAlpha)
        {
            SetInteractable(targetAlpha > 0.01f);

            if (transitionCanvasGroup == null)
            {
                yield break;
            }

            float start = transitionCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                SetAlpha(Mathf.Lerp(start, targetAlpha, t));
                yield return null;
            }

            SetAlpha(targetAlpha);
            SetInteractable(targetAlpha > 0.01f);
            fadeRoutine = null;
        }

        private void SetAlpha(float alpha)
        {
            if (transitionCanvasGroup != null)
            {
                transitionCanvasGroup.alpha = alpha;
            }
        }

        private void SetInteractable(bool value)
        {
            if (transitionCanvasGroup != null)
            {
                transitionCanvasGroup.interactable = value;
                transitionCanvasGroup.blocksRaycasts = value;
            }
        }
    }
}
