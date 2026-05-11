using DatingSim.Core;
using DatingSim.Dialogue;
using UnityEngine;
using UnityEngine.InputSystem;

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

            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            bool advanceRequested =
                (keyboard != null && (IsKeyPressedThisFrame(keyboard, primaryAdvanceKey) || IsKeyPressedThisFrame(keyboard, secondaryAdvanceKey))) ||
                (allowMouseClickAdvance && mouse != null && mouse.leftButton.wasPressedThisFrame);

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

        private static bool IsKeyPressedThisFrame(Keyboard keyboard, KeyCode keyCode)
        {
            Key key = Key.None;
            switch (keyCode)
            {
                case KeyCode.Space:
                    key = Key.Space;
                    break;
                case KeyCode.Return:
                    key = Key.Enter;
                    break;
                case KeyCode.KeypadEnter:
                    key = Key.NumpadEnter;
                    break;
            }

            return key != Key.None && keyboard[key].wasPressedThisFrame;
        }
    }
}
