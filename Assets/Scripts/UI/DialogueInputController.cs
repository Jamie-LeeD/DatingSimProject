using DatingSim.Core;
using DatingSim.Dialogue;
using UnityEngine;

namespace DatingSim.UI
{
    public class DialogueInputController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private AudioManager audioManager;

        [Header("Input")]
        [SerializeField] private KeyCode primaryAdvanceKey = KeyCode.Space;
        [SerializeField] private KeyCode secondaryAdvanceKey = KeyCode.Return;
        [SerializeField] private bool allowMouseClickAdvance = true;

        [Header("Audio")]
        [SerializeField] private AudioClip dialogueAdvanceSfx;

        private void Update()
        {
            if (dialogueManager == null || !dialogueManager.IsDialogueActive)
            {
                return;
            }

            bool advanceRequested =
                Input.GetKeyDown(primaryAdvanceKey) ||
                Input.GetKeyDown(secondaryAdvanceKey) ||
                (allowMouseClickAdvance && Input.GetMouseButtonDown(0));

            if (!advanceRequested)
            {
                return;
            }

            dialogueManager.Advance();

            if (audioManager != null && dialogueAdvanceSfx != null)
            {
                audioManager.PlaySfx(dialogueAdvanceSfx);
            }
        }
    }
}
