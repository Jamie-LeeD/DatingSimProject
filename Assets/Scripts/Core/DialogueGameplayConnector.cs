using System.Collections;
using DatingSim.Characters;
using DatingSim.Dialogue;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DatingSim.Core
{
    public class DialogueGameplayConnector : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private RelationshipManager relationshipManager;
        [SerializeField] private GameFlowStateMachine gameFlowStateMachine;

        [Header("Scene Transition")]
        [SerializeField] private bool transitionOnDialogueEnd = true;
        [SerializeField] private string nextSceneName = "MainMenu";
        [SerializeField, Min(0f)] private float sceneLoadDelaySeconds = 0.35f;

        [Header("Debug")]
        [SerializeField] private bool logAppliedAffection;

        private Coroutine transitionRoutine;

        private void OnEnable()
        {
            if (dialogueManager != null)
            {
                dialogueManager.ChoiceSelected += HandleChoiceSelected;
                dialogueManager.DialogueEnded += HandleDialogueEnded;
            }
        }

        private void OnDisable()
        {
            if (dialogueManager != null)
            {
                dialogueManager.ChoiceSelected -= HandleChoiceSelected;
                dialogueManager.DialogueEnded -= HandleDialogueEnded;
            }
        }

        private void HandleChoiceSelected(DialogueChoice choice)
        {
            if (choice == null || relationshipManager == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(choice.affectionTargetId) || choice.affectionDelta == 0)
            {
                return;
            }

            relationshipManager.AddAffection(choice.affectionTargetId, choice.affectionDelta);
            if (logAppliedAffection)
            {
                Debug.Log($"[DialogueGameplayConnector] Applied affection delta {choice.affectionDelta} to '{choice.affectionTargetId}'.", this);
            }
        }

        private void HandleDialogueEnded(string _)
        {
            if (!transitionOnDialogueEnd || string.IsNullOrWhiteSpace(nextSceneName))
            {
                return;
            }

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }

            transitionRoutine = StartCoroutine(LoadNextSceneRoutine());
        }

        private IEnumerator LoadNextSceneRoutine()
        {
            if (gameFlowStateMachine != null)
            {
                gameFlowStateMachine.RequestState(GameFlowStateId.Transition);
            }

            if (sceneLoadDelaySeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(sceneLoadDelaySeconds);
            }

            SceneManager.LoadScene(nextSceneName);
            transitionRoutine = null;
        }
    }
}
