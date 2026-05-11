using System.Collections.Generic;
using DatingSim.Characters;
using DatingSim.Dialogue;
using DatingSim.SaveSystem;
using UnityEngine;

namespace DatingSim.Core
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Manager References (Scene or Prefab)")]
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private SaveSystemManager saveSystemManager;
        [SerializeField] private RelationshipManager relationshipManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private GameFlowStateMachine gameFlowStateMachine;

        [Header("Fallback Prefabs (Optional)")]
        [SerializeField] private DialogueManager dialogueManagerPrefab;
        [SerializeField] private SaveSystemManager saveSystemManagerPrefab;
        [SerializeField] private RelationshipManager relationshipManagerPrefab;
        [SerializeField] private AudioManager audioManagerPrefab;
        [SerializeField] private GameFlowStateMachine gameFlowStateMachinePrefab;

        [Header("Behavior")]
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private bool persistManagers = true;
        [SerializeField] private bool logInitialization = true;

        private static GameInitializer instance;
        private bool hasInitialized;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (initializeOnAwake)
            {
                InitializeSystems();
            }
        }

        [ContextMenu("Initialize Systems")]
        public void InitializeSystems()
        {
            if (hasInitialized)
            {
                return;
            }

            dialogueManager = EnsureSingleManager(dialogueManager, dialogueManagerPrefab);
            relationshipManager = EnsureSingleManager(relationshipManager, relationshipManagerPrefab);
            saveSystemManager = EnsureSingleManager(saveSystemManager, saveSystemManagerPrefab);
            audioManager = EnsureSingleManager(audioManager, audioManagerPrefab);
            gameFlowStateMachine = EnsureSingleManager(gameFlowStateMachine, gameFlowStateMachinePrefab);

            if (saveSystemManager != null)
            {
                saveSystemManager.SetDependencies(dialogueManager, relationshipManager);
            }

            if (gameFlowStateMachine != null)
            {
                gameFlowStateMachine.Initialize();
            }

            hasInitialized = true;
            Log("Core systems initialized.");
        }

        private T EnsureSingleManager<T>(T configuredReference, T prefab) where T : MonoBehaviour
        {
            List<T> existingManagers = new List<T>(FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            T managerToKeep = configuredReference;

            if (managerToKeep == null)
            {
                managerToKeep = existingManagers.Count > 0 ? existingManagers[0] : null;
            }

            if (managerToKeep == null && prefab != null)
            {
                managerToKeep = Instantiate(prefab);
                managerToKeep.name = prefab.name;
                Log($"Instantiated missing manager: {typeof(T).Name}");
            }

            if (managerToKeep == null)
            {
                Debug.LogWarning($"[GameInitializer] Missing manager and no prefab configured for {typeof(T).Name}.", this);
                return null;
            }

            for (int i = 0; i < existingManagers.Count; i++)
            {
                if (existingManagers[i] != null && existingManagers[i] != managerToKeep)
                {
                    Destroy(existingManagers[i].gameObject);
                    Log($"Removed duplicate manager: {typeof(T).Name}");
                }
            }

            if (persistManagers)
            {
                DontDestroyOnLoad(managerToKeep.gameObject);
            }

            return managerToKeep;
        }

        private void Log(string message)
        {
            if (logInitialization)
            {
                Debug.Log($"[GameInitializer] {message}", this);
            }
        }
    }
}
