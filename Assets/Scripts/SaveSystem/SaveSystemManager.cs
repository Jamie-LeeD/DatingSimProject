using System;
using System.Collections;
using System.IO;
using DatingSim.Characters;
using DatingSim.Dialogue;
using UnityEngine;

namespace DatingSim.SaveSystem
{
    public class SaveSystemManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private RelationshipManager relationshipManager;

        [Header("Save Slot Settings")]
        [SerializeField, Min(0)] private int activeSlotIndex;
        [SerializeField, Min(1)] private int maxSlots = 6;
        [SerializeField] private string saveFolderName = "SaveData";
        [SerializeField] private string saveFilePrefix = "slot_";

        [Header("Auto Save")]
        [SerializeField] private bool autoSaveEnabled = true;
        [SerializeField, Min(5f)] private float autoSaveIntervalSeconds = 30f;
        [SerializeField] private bool autoSaveOnApplicationPause = true;
        [SerializeField] private bool autoSaveOnApplicationQuit = true;

        [Header("Debug")]
        [SerializeField] private bool prettyPrintJson = true;
        [SerializeField] private bool logOperations = true;
        [SerializeField] private string lastSaveUtc;
        [SerializeField] private string lastLoadUtc;
        [SerializeField] private string lastError;

        public event Action<int, SaveGameData> SaveCompleted;
        public event Action<int, SaveGameData> LoadCompleted;
        public event Action<int, string> SaveFailed;
        public event Action<int, string> LoadFailed;

        private Coroutine autoSaveRoutine;

        public int ActiveSlotIndex => activeSlotIndex;

        public void SetDependencies(DialogueManager dialogue, RelationshipManager relationship)
        {
            dialogueManager = dialogue;
            relationshipManager = relationship;
        }

        private void Awake()
        {
            if (dialogueManager == null)
            {
                dialogueManager = FindFirstObjectByType<DialogueManager>();
            }

            if (relationshipManager == null)
            {
                relationshipManager = FindFirstObjectByType<RelationshipManager>();
            }
        }

        private void OnEnable()
        {
            if (autoSaveEnabled)
            {
                autoSaveRoutine = StartCoroutine(AutoSaveLoop());
            }
        }

        private void Start()
        {
            if (!PendingSaveLoadRequest.HasPendingLoad)
            {
                return;
            }

            int pendingSlot = PendingSaveLoadRequest.PendingSlotIndex;
            PendingSaveLoadRequest.Clear();

            SetActiveSlot(pendingSlot);
            LoadActiveSlot();
        }

        private void OnDisable()
        {
            if (autoSaveRoutine != null)
            {
                StopCoroutine(autoSaveRoutine);
                autoSaveRoutine = null;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && autoSaveEnabled && autoSaveOnApplicationPause)
            {
                SaveToSlot(activeSlotIndex);
            }
        }

        private void OnApplicationQuit()
        {
            if (autoSaveEnabled && autoSaveOnApplicationQuit)
            {
                SaveToSlot(activeSlotIndex);
            }
        }

        public bool SaveToActiveSlot()
        {
            return SaveToSlot(activeSlotIndex);
        }

        public bool SaveToSlot(int slotIndex)
        {
            if (!ValidateSlotIndex(slotIndex, out string validationError))
            {
                ReportSaveError(slotIndex, validationError);
                return false;
            }

            SaveGameData payload = BuildSavePayload(slotIndex);
            string json = JsonUtility.ToJson(payload, prettyPrintJson);
            string path = GetSlotPath(slotIndex);

            try
            {
                Directory.CreateDirectory(GetSaveDirectoryPath());
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                ReportSaveError(slotIndex, $"Failed writing save file: {ex.Message}");
                return false;
            }

            lastSaveUtc = payload.savedAtUtc;
            lastError = string.Empty;
            SaveCompleted?.Invoke(slotIndex, payload);
            Log($"Saved slot {slotIndex} to '{path}'.");
            return true;
        }

        public bool LoadActiveSlot()
        {
            return LoadFromSlot(activeSlotIndex);
        }

        public bool LoadFromSlot(int slotIndex)
        {
            if (!ValidateSlotIndex(slotIndex, out string validationError))
            {
                ReportLoadError(slotIndex, validationError);
                return false;
            }

            string path = GetSlotPath(slotIndex);
            if (!File.Exists(path))
            {
                ReportLoadError(slotIndex, $"Save file does not exist at '{path}'.");
                return false;
            }

            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                ReportLoadError(slotIndex, $"Failed reading save file: {ex.Message}");
                return false;
            }

            SaveGameData payload;
            try
            {
                payload = JsonUtility.FromJson<SaveGameData>(json);
            }
            catch (Exception ex)
            {
                ReportLoadError(slotIndex, $"Failed parsing save JSON: {ex.Message}");
                return false;
            }

            if (payload == null)
            {
                ReportLoadError(slotIndex, "Parsed save payload is null.");
                return false;
            }

            ApplyPayload(payload);
            activeSlotIndex = slotIndex;
            lastLoadUtc = DateTime.UtcNow.ToString("o");
            lastError = string.Empty;
            LoadCompleted?.Invoke(slotIndex, payload);
            Log($"Loaded slot {slotIndex} from '{path}'.");
            return true;
        }

        public bool DeleteSlot(int slotIndex)
        {
            if (!ValidateSlotIndex(slotIndex, out _))
            {
                return false;
            }

            string path = GetSlotPath(slotIndex);
            if (!File.Exists(path))
            {
                return true;
            }

            try
            {
                File.Delete(path);
                Log($"Deleted slot {slotIndex}.");
                return true;
            }
            catch (Exception ex)
            {
                lastError = $"Failed deleting slot {slotIndex}: {ex.Message}";
                Debug.LogWarning($"[SaveSystemManager] {lastError}", this);
                return false;
            }
        }

        public bool SlotExists(int slotIndex)
        {
            if (!ValidateSlotIndex(slotIndex, out _))
            {
                return false;
            }

            return File.Exists(GetSlotPath(slotIndex));
        }

        public void SetActiveSlot(int slotIndex)
        {
            if (!ValidateSlotIndex(slotIndex, out string error))
            {
                Debug.LogWarning($"[SaveSystemManager] {error}", this);
                return;
            }

            activeSlotIndex = slotIndex;
        }

        private IEnumerator AutoSaveLoop()
        {
            var wait = new WaitForSecondsRealtime(autoSaveIntervalSeconds);
            while (true)
            {
                yield return wait;
                SaveToSlot(activeSlotIndex);
            }
        }

        private SaveGameData BuildSavePayload(int slotIndex)
        {
            return new SaveGameData
            {
                saveVersion = "1.0.0",
                savedAtUtc = DateTime.UtcNow.ToString("o"),
                slotIndex = slotIndex,
                dialogueProgress = dialogueManager != null ? dialogueManager.CreateProgressSaveData() : new DialogueProgressSaveData(),
                relationshipData = relationshipManager != null ? relationshipManager.CreateSaveData() : new RelationshipSaveData()
            };
        }

        private void ApplyPayload(SaveGameData payload)
        {
            if (relationshipManager != null)
            {
                relationshipManager.LoadFromSaveData(payload.relationshipData);
            }

            if (dialogueManager != null)
            {
                dialogueManager.RestoreProgress(payload.dialogueProgress);
            }
        }

        private bool ValidateSlotIndex(int slotIndex, out string error)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots)
            {
                error = $"Slot index {slotIndex} is out of range. Valid range: 0 to {maxSlots - 1}.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void ReportSaveError(int slotIndex, string error)
        {
            lastError = error;
            SaveFailed?.Invoke(slotIndex, error);
            Debug.LogWarning($"[SaveSystemManager] Save failed for slot {slotIndex}: {error}", this);
        }

        private void ReportLoadError(int slotIndex, string error)
        {
            lastError = error;
            LoadFailed?.Invoke(slotIndex, error);
            Debug.LogWarning($"[SaveSystemManager] Load failed for slot {slotIndex}: {error}", this);
        }

        private string GetSlotPath(int slotIndex)
        {
            return Path.Combine(GetSaveDirectoryPath(), $"{saveFilePrefix}{slotIndex}.json");
        }

        private string GetSaveDirectoryPath()
        {
            return Path.Combine(Application.persistentDataPath, saveFolderName);
        }

        private void Log(string message)
        {
            if (logOperations)
            {
                Debug.Log($"[SaveSystemManager] {message}", this);
            }
        }
    }
}
