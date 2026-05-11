using System;
using System.Collections;
using System.Collections.Generic;
using DatingSim.Dialogue;
using DatingSim.SaveSystem;
using UnityEngine;

namespace DatingSim.Core
{
    public class GameFlowStateMachine : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private SaveSystemManager saveSystemManager;

        [Header("Initialization")]
        [SerializeField] private GameFlowStateId initialState = GameFlowStateId.MainMenu;
        [SerializeField] private bool initializeOnEnable = true;

        [Header("Transition")]
        [SerializeField, Min(0f)] private float transitionDuration = 0.2f;

        [Header("Debug")]
        [SerializeField] private bool logStateChanges = true;
        [SerializeField] private GameFlowStateId currentStateDebug;
        [SerializeField] private GameFlowStateId previousStateDebug;

        public event Action<GameFlowStateId, GameFlowStateId> StateChanged;
        public event Action<GameFlowStateId> StateTransitionRequested;

        public GameFlowStateId CurrentStateId => currentState != null ? currentState.Id : initialState;
        public GameFlowStateId PreviousStateId => previousState != null ? previousState.Id : initialState;

        private readonly Dictionary<GameFlowStateId, IGameFlowState> states = new Dictionary<GameFlowStateId, IGameFlowState>();
        private IGameFlowState currentState;
        private IGameFlowState previousState;
        private Coroutine transitionRoutine;
        private bool initialized;

        private void Awake()
        {
            if (dialogueManager == null)
            {
                dialogueManager = FindFirstObjectByType<DialogueManager>();
            }

            if (saveSystemManager == null)
            {
                saveSystemManager = FindFirstObjectByType<SaveSystemManager>();
            }

            RegisterStates();
        }

        private void OnEnable()
        {
            SubscribeEvents();

            if (initializeOnEnable)
            {
                Initialize();
            }
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            ForceState(initialState);
        }

        public bool RequestState(GameFlowStateId nextState)
        {
            StateTransitionRequested?.Invoke(nextState);

            if (!states.TryGetValue(nextState, out IGameFlowState state))
            {
                Warn($"Requested unknown state: {nextState}");
                return false;
            }

            if (currentState != null && currentState.Id == nextState)
            {
                return true;
            }

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }

            transitionRoutine = StartCoroutine(TransitionRoutine(state));
            return true;
        }

        public void ForceState(GameFlowStateId nextState)
        {
            if (!states.TryGetValue(nextState, out IGameFlowState state))
            {
                Warn($"Cannot force unknown state: {nextState}");
                return;
            }

            ApplyState(state);
        }

        public void TogglePause()
        {
            if (CurrentStateId == GameFlowStateId.Pause)
            {
                RequestState(previousState != null ? previousState.Id : GameFlowStateId.Dialogue);
            }
            else
            {
                RequestState(GameFlowStateId.Pause);
            }
        }

        public void EnterSaveLoad()
        {
            RequestState(GameFlowStateId.SaveLoad);
        }

        public bool SaveToActiveSlot()
        {
            RequestState(GameFlowStateId.SaveLoad);
            bool result = saveSystemManager != null && saveSystemManager.SaveToActiveSlot();
            RequestState(dialogueManager != null && dialogueManager.IsDialogueActive ? GameFlowStateId.Dialogue : GameFlowStateId.MainMenu);
            return result;
        }

        public bool LoadFromActiveSlot()
        {
            RequestState(GameFlowStateId.SaveLoad);
            bool result = saveSystemManager != null && saveSystemManager.LoadActiveSlot();
            RequestState(dialogueManager != null && dialogueManager.IsDialogueActive ? GameFlowStateId.Dialogue : GameFlowStateId.MainMenu);
            return result;
        }

        private IEnumerator TransitionRoutine(IGameFlowState nextState)
        {
            ApplyState(states[GameFlowStateId.Transition]);
            if (transitionDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(transitionDuration);
            }

            ApplyState(nextState);
            transitionRoutine = null;
        }

        private void ApplyState(IGameFlowState nextState)
        {
            GameFlowStateId from = currentState != null ? currentState.Id : nextState.Id;

            currentState?.Exit(this);
            previousState = currentState;
            currentState = nextState;
            currentState.Enter(this);

            previousStateDebug = previousState != null ? previousState.Id : nextState.Id;
            currentStateDebug = currentState.Id;
            StateChanged?.Invoke(from, currentState.Id);

            if (logStateChanges)
            {
                Debug.Log($"[GameFlowStateMachine] State changed: {from} -> {currentState.Id}", this);
            }
        }

        private void RegisterStates()
        {
            states.Clear();
            states[GameFlowStateId.MainMenu] = new MainMenuState();
            states[GameFlowStateId.Dialogue] = new DialogueState();
            states[GameFlowStateId.Choice] = new ChoiceState();
            states[GameFlowStateId.Transition] = new TransitionState();
            states[GameFlowStateId.Pause] = new PauseState();
            states[GameFlowStateId.SaveLoad] = new SaveLoadState();
        }

        private void SubscribeEvents()
        {
            if (dialogueManager == null)
            {
                return;
            }

            dialogueManager.DialogueStarted += HandleDialogueStarted;
            dialogueManager.DialogueEnded += HandleDialogueEnded;
            dialogueManager.LineCompleted += HandleLineCompleted;
            dialogueManager.ChoiceSelected += HandleChoiceSelected;
        }

        private void UnsubscribeEvents()
        {
            if (dialogueManager == null)
            {
                return;
            }

            dialogueManager.DialogueStarted -= HandleDialogueStarted;
            dialogueManager.DialogueEnded -= HandleDialogueEnded;
            dialogueManager.LineCompleted -= HandleLineCompleted;
            dialogueManager.ChoiceSelected -= HandleChoiceSelected;
        }

        private void HandleDialogueStarted(string _)
        {
            RequestState(GameFlowStateId.Dialogue);
        }

        private void HandleDialogueEnded(string _)
        {
            RequestState(GameFlowStateId.MainMenu);
        }

        private void HandleLineCompleted(DialogueLine line)
        {
            if (line == null)
            {
                return;
            }

            bool hasChoices = line.choices != null && line.choices.Count > 0;
            RequestState(hasChoices ? GameFlowStateId.Choice : GameFlowStateId.Dialogue);
        }

        private void HandleChoiceSelected(DialogueChoice _)
        {
            RequestState(GameFlowStateId.Dialogue);
        }

        private void Warn(string message)
        {
            Debug.LogWarning($"[GameFlowStateMachine] {message}", this);
        }

        private sealed class MainMenuState : IGameFlowState
        {
            public GameFlowStateId Id => GameFlowStateId.MainMenu;
            public void Enter(GameFlowStateMachine machine)
            {
                Time.timeScale = 1f;
            }

            public void Exit(GameFlowStateMachine machine)
            {
            }
        }

        private sealed class DialogueState : IGameFlowState
        {
            public GameFlowStateId Id => GameFlowStateId.Dialogue;
            public void Enter(GameFlowStateMachine machine)
            {
                Time.timeScale = 1f;
            }

            public void Exit(GameFlowStateMachine machine)
            {
            }
        }

        private sealed class ChoiceState : IGameFlowState
        {
            public GameFlowStateId Id => GameFlowStateId.Choice;
            public void Enter(GameFlowStateMachine machine)
            {
                Time.timeScale = 1f;
            }

            public void Exit(GameFlowStateMachine machine)
            {
            }
        }

        private sealed class TransitionState : IGameFlowState
        {
            public GameFlowStateId Id => GameFlowStateId.Transition;
            public void Enter(GameFlowStateMachine machine)
            {
                Time.timeScale = 1f;
            }

            public void Exit(GameFlowStateMachine machine)
            {
            }
        }

        private sealed class PauseState : IGameFlowState
        {
            public GameFlowStateId Id => GameFlowStateId.Pause;
            public void Enter(GameFlowStateMachine machine)
            {
                Time.timeScale = 0f;
            }

            public void Exit(GameFlowStateMachine machine)
            {
                Time.timeScale = 1f;
            }
        }

        private sealed class SaveLoadState : IGameFlowState
        {
            public GameFlowStateId Id => GameFlowStateId.SaveLoad;
            public void Enter(GameFlowStateMachine machine)
            {
                Time.timeScale = 1f;
            }

            public void Exit(GameFlowStateMachine machine)
            {
            }
        }
    }
}
