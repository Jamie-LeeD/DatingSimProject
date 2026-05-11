using System.Collections;
using DatingSim.Core;
using DatingSim.SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DatingSim.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        [Header("Settings")]
        [SerializeField] private GameObject settingsPanelRoot;
        [SerializeField] private bool openSettingsOnStart;

        [Header("Scene Setup")]
        [SerializeField] private string gameSceneName = "GameScene";
        [SerializeField] private int loadSlotIndex;

        [Header("Transition")]
        [SerializeField] private CanvasGroup transitionCanvasGroup;
        [SerializeField] private Animator transitionAnimator;
        [SerializeField] private string transitionTriggerName = "PlayTransition";
        [SerializeField, Min(0f)] private float transitionDuration = 0.35f;

        [Header("Audio")]
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip buttonClickSfx;

        [Header("Optional UI")]
        [SerializeField] private TextMeshProUGUI loadButtonLabel;
        [SerializeField] private string loadAvailableLabel = "Load Game";
        [SerializeField] private string loadUnavailableLabel = "Load Game (No Save)";
        [SerializeField] private SaveSystemManager saveSystemManager;

        private bool isTransitioning;

        private void Awake()
        {
            BindButtons();
            SetSettingsVisible(openSettingsOnStart);
            SetTransitionAlpha(0f);
            SetTransitionBlockInput(false);
            RefreshLoadButtonState();
        }

        private void Start()
        {
            if (audioManager != null && menuMusic != null)
            {
                audioManager.PlayBgm(menuMusic, true);
            }
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        public void StartGame()
        {
            if (isTransitioning)
            {
                return;
            }

            PlayClickSfx();
            PendingSaveLoadRequest.Clear();
            StartCoroutine(LoadSceneRoutine(gameSceneName));
        }

        public void LoadGame()
        {
            if (isTransitioning)
            {
                return;
            }

            bool canLoad = saveSystemManager == null || saveSystemManager.SlotExists(loadSlotIndex);
            if (!canLoad)
            {
                return;
            }

            PlayClickSfx();
            PendingSaveLoadRequest.RequestLoad(loadSlotIndex);
            StartCoroutine(LoadSceneRoutine(gameSceneName));
        }

        public void OpenSettings()
        {
            PlayClickSfx();
            SetSettingsVisible(true);
        }

        public void CloseSettings()
        {
            PlayClickSfx();
            SetSettingsVisible(false);
        }

        public void ExitGame()
        {
            if (isTransitioning)
            {
                return;
            }

            PlayClickSfx();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            isTransitioning = true;

            TriggerTransitionAnimator();
            SetTransitionBlockInput(true);

            if (transitionCanvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(1f, transitionDuration));
            }
            else if (transitionDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(transitionDuration);
            }

            SceneManager.LoadScene(sceneName);
        }

        private IEnumerator FadeCanvasGroup(float targetAlpha, float duration)
        {
            if (transitionCanvasGroup == null)
            {
                yield break;
            }

            float startAlpha = transitionCanvasGroup.alpha;
            float elapsed = 0f;

            if (duration <= 0f)
            {
                transitionCanvasGroup.alpha = targetAlpha;
                yield break;
            }

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transitionCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            transitionCanvasGroup.alpha = targetAlpha;
        }

        private void BindButtons()
        {
            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(StartGame);
            }

            if (loadGameButton != null)
            {
                loadGameButton.onClick.AddListener(LoadGame);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OpenSettings);
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(ExitGame);
            }
        }

        private void UnbindButtons()
        {
            if (startGameButton != null)
            {
                startGameButton.onClick.RemoveListener(StartGame);
            }

            if (loadGameButton != null)
            {
                loadGameButton.onClick.RemoveListener(LoadGame);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(OpenSettings);
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveListener(ExitGame);
            }
        }

        private void RefreshLoadButtonState()
        {
            bool canLoad = saveSystemManager == null || saveSystemManager.SlotExists(loadSlotIndex);

            if (loadGameButton != null)
            {
                loadGameButton.interactable = canLoad;
            }

            if (loadButtonLabel != null)
            {
                loadButtonLabel.text = canLoad ? loadAvailableLabel : loadUnavailableLabel;
            }
        }

        private void SetSettingsVisible(bool visible)
        {
            if (settingsPanelRoot != null)
            {
                settingsPanelRoot.SetActive(visible);
            }
        }

        private void TriggerTransitionAnimator()
        {
            if (transitionAnimator == null || string.IsNullOrWhiteSpace(transitionTriggerName))
            {
                return;
            }

            transitionAnimator.ResetTrigger(transitionTriggerName);
            transitionAnimator.SetTrigger(transitionTriggerName);
        }

        private void SetTransitionAlpha(float alpha)
        {
            if (transitionCanvasGroup != null)
            {
                transitionCanvasGroup.alpha = alpha;
            }
        }

        private void SetTransitionBlockInput(bool value)
        {
            if (transitionCanvasGroup != null)
            {
                transitionCanvasGroup.blocksRaycasts = value;
                transitionCanvasGroup.interactable = value;
            }
        }

        private void PlayClickSfx()
        {
            if (audioManager != null && buttonClickSfx != null)
            {
                audioManager.PlaySfx(buttonClickSfx);
            }
        }
    }
}
