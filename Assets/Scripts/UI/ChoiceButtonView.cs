using System;
using DatingSim.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DatingSim.UI
{
    public class ChoiceButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI choiceTextLabel;

        private DialogueChoice boundChoice;
        private Action<DialogueChoice> callback;

        public void Bind(DialogueChoice choice, Action<DialogueChoice> onSelected)
        {
            boundChoice = choice;
            callback = onSelected;

            if (choiceTextLabel != null)
            {
                choiceTextLabel.text = choice != null ? choice.choiceText : string.Empty;
            }

            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
                button.onClick.AddListener(HandleClick);
                button.interactable = choice != null;
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            if (boundChoice == null)
            {
                return;
            }

            callback?.Invoke(boundChoice);
        }
    }
}
