using System.Collections;
using DatingSim.SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DatingSim.UI
{
    public class SaveLoadUiController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private SaveSystemManager saveSystemManager;

        [Header("UI Buttons")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;

        [Header("Auto Save Indicator")]
        [SerializeField] private GameObject autoSaveIndicatorRoot;
        [SerializeField] private TextMeshProUGUI autoSaveIndicatorText;
        [SerializeField] private string savingLabel = "Auto-saving...";
        [SerializeField] private string savedLabel = "Saved";
        [SerializeField, Min(0.1f)] private float indicatorVisibleSeconds = 1.5f;

        private Coroutine indicatorRoutine;

        private void Awake()
        {
            if (saveButton != null)
            {
                saveButton.onClick.AddListener(HandleSaveClicked);
            }

            if (loadButton != null)
            {
                loadButton.onClick.AddListener(HandleLoadClicked);
            }

            SetIndicatorVisible(false);
        }

        private void OnEnable()
        {
            if (saveSystemManager != null)
            {
                saveSystemManager.SaveCompleted += HandleSaveCompleted;
                saveSystemManager.LoadCompleted += HandleLoadCompleted;
                saveSystemManager.SaveFailed += HandleSaveFailed;
                saveSystemManager.LoadFailed += HandleLoadFailed;
            }
        }

        private void OnDisable()
        {
            if (saveSystemManager != null)
            {
                saveSystemManager.SaveCompleted -= HandleSaveCompleted;
                saveSystemManager.LoadCompleted -= HandleLoadCompleted;
                saveSystemManager.SaveFailed -= HandleSaveFailed;
                saveSystemManager.LoadFailed -= HandleLoadFailed;
            }
        }

        private void OnDestroy()
        {
            if (saveButton != null)
            {
                saveButton.onClick.RemoveListener(HandleSaveClicked);
            }

            if (loadButton != null)
            {
                loadButton.onClick.RemoveListener(HandleLoadClicked);
            }
        }

        private void HandleSaveClicked()
        {
            ShowIndicator(savingLabel);
            saveSystemManager?.SaveToActiveSlot();
        }

        private void HandleLoadClicked()
        {
            saveSystemManager?.LoadActiveSlot();
        }

        private void HandleSaveCompleted(int _, SaveGameData __)
        {
            ShowIndicator(savedLabel);
        }

        private void HandleLoadCompleted(int _, SaveGameData __)
        {
            ShowIndicator("Loaded");
        }

        private void HandleSaveFailed(int _, string error)
        {
            ShowIndicator($"Save failed: {error}");
        }

        private void HandleLoadFailed(int _, string error)
        {
            ShowIndicator($"Load failed: {error}");
        }

        private void ShowIndicator(string label)
        {
            if (autoSaveIndicatorText != null)
            {
                autoSaveIndicatorText.text = label;
            }

            SetIndicatorVisible(true);

            if (indicatorRoutine != null)
            {
                StopCoroutine(indicatorRoutine);
            }

            indicatorRoutine = StartCoroutine(HideIndicatorRoutine());
        }

        private IEnumerator HideIndicatorRoutine()
        {
            yield return new WaitForSecondsRealtime(indicatorVisibleSeconds);
            SetIndicatorVisible(false);
            indicatorRoutine = null;
        }

        private void SetIndicatorVisible(bool visible)
        {
            if (autoSaveIndicatorRoot != null)
            {
                autoSaveIndicatorRoot.SetActive(visible);
            }
        }
    }
}
