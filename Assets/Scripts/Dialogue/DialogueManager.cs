using System;
using System.Collections.Generic;
using DatingSim.SaveSystem;
using DatingSim.UI;
using TMPro;
using UnityEngine;

namespace DatingSim.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [Header("Data Source")]
        [SerializeField] private TextAsset dialogueJson;
        [SerializeField] private bool playOnStart = true;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private ChoiceButtonView choiceButtonPrefab;
        [SerializeField] private TypewriterEffect typewriterEffect;

        [Header("Debug")]
        [SerializeField] private bool logWarnings = true;

        public event Action<string> DialogueStarted;
        public event Action<string> DialogueEnded;
        public event Action<DialogueLine> LineStarted;
        public event Action<DialogueLine> LineCompleted;
        public event Action<DialogueChoice> ChoiceSelected;
        public event Action<string> BackgroundChanged;
        public event Action<string, string> CharacterEmotionChanged;

        public bool IsDialogueActive { get; private set; }
        public bool CanAdvance => IsDialogueActive && (typewriterEffect == null || !typewriterEffect.IsTyping);
        public string ActiveDialogueId => activeSequence != null ? activeSequence.dialogueId : string.Empty;
        public string CurrentLineId => currentLine != null ? currentLine.lineId : string.Empty;

        private readonly List<ChoiceButtonView> activeChoiceButtons = new List<ChoiceButtonView>();
        private readonly Dictionary<string, DialogueLine> linesById = new Dictionary<string, DialogueLine>();
        private readonly List<string> selectedChoiceHistory = new List<string>();

        private DialogueSequence activeSequence;
        private DialogueLine currentLine;

        private void Awake()
        {
            if (typewriterEffect == null)
            {
                typewriterEffect = GetComponent<TypewriterEffect>();
            }
        }

        private void Start()
        {
            if (playOnStart && dialogueJson != null)
            {
                StartDialogue(dialogueJson);
            }
        }

        public bool StartDialogue(TextAsset jsonAsset)
        {
            if (!DialogueJsonLoader.TryLoad(jsonAsset, out DialogueSequence sequence, out string error))
            {
                Warn(error);
                return false;
            }

            BuildLineLookup(sequence);
            activeSequence = sequence;
            IsDialogueActive = true;
            selectedChoiceHistory.Clear();
            DialogueStarted?.Invoke(sequence.dialogueId);

            DisplayLineById(sequence.startLineId);
            return true;
        }

        public void Advance()
        {
            if (!IsDialogueActive || currentLine == null)
            {
                return;
            }

            if (typewriterEffect != null && typewriterEffect.IsTyping)
            {
                typewriterEffect.Skip();
                return;
            }

            if (currentLine.choices != null && currentLine.choices.Count > 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(currentLine.nextLineId))
            {
                EndDialogue();
                return;
            }

            DisplayLineById(currentLine.nextLineId);
        }

        public void SelectChoice(DialogueChoice choice)
        {
            if (!IsDialogueActive || choice == null)
            {
                return;
            }

            ChoiceSelected?.Invoke(choice);
            if (!string.IsNullOrWhiteSpace(choice.choiceId))
            {
                selectedChoiceHistory.Add(choice.choiceId);
            }
            ClearChoices();

            if (string.IsNullOrWhiteSpace(choice.nextLineId))
            {
                EndDialogue();
                return;
            }

            DisplayLineById(choice.nextLineId);
        }

        public void EndDialogue()
        {
            if (!IsDialogueActive)
            {
                return;
            }

            IsDialogueActive = false;
            string endedDialogueId = activeSequence != null ? activeSequence.dialogueId : string.Empty;

            currentLine = null;
            ClearChoices();
            DialogueEnded?.Invoke(endedDialogueId);
        }

        public DialogueProgressSaveData CreateProgressSaveData()
        {
            return new DialogueProgressSaveData
            {
                dialogueId = ActiveDialogueId,
                currentLineId = CurrentLineId,
                isDialogueActive = IsDialogueActive,
                selectedChoiceIds = new List<string>(selectedChoiceHistory)
            };
        }

        public bool RestoreProgress(DialogueProgressSaveData progressData)
        {
            if (progressData == null)
            {
                Warn("Progress data is null. Cannot restore dialogue progress.");
                return false;
            }

            if (dialogueJson == null)
            {
                Warn("Dialogue JSON reference is missing. Cannot restore dialogue progress.");
                return false;
            }

            if (activeSequence == null || linesById.Count == 0)
            {
                if (!StartDialogue(dialogueJson))
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(progressData.dialogueId) &&
                !string.Equals(progressData.dialogueId, ActiveDialogueId, StringComparison.Ordinal))
            {
                Warn($"Saved dialogue id '{progressData.dialogueId}' does not match active dialogue '{ActiveDialogueId}'.");
            }

            selectedChoiceHistory.Clear();
            if (progressData.selectedChoiceIds != null)
            {
                selectedChoiceHistory.AddRange(progressData.selectedChoiceIds);
            }

            if (!string.IsNullOrWhiteSpace(progressData.currentLineId))
            {
                DisplayLineById(progressData.currentLineId);
            }

            if (!progressData.isDialogueActive)
            {
                EndDialogue();
            }

            return true;
        }

        private void DisplayLineById(string lineId)
        {
            if (!linesById.TryGetValue(lineId, out DialogueLine line))
            {
                Warn($"Line '{lineId}' was not found. Ending dialogue.");
                EndDialogue();
                return;
            }

            currentLine = line;
            ClearChoices();
            ApplyLineVisualState(line);
            LineStarted?.Invoke(line);

            if (typewriterEffect != null)
            {
                typewriterEffect.Play(dialogueText, line.text, OnLineTypewriterCompleted);
            }
            else if (dialogueText != null)
            {
                dialogueText.text = line.text ?? string.Empty;
                OnLineTypewriterCompleted();
            }
        }

        private void OnLineTypewriterCompleted()
        {
            if (currentLine == null)
            {
                return;
            }

            LineCompleted?.Invoke(currentLine);

            if (currentLine.choices == null || currentLine.choices.Count == 0)
            {
                return;
            }

            for (int i = 0; i < currentLine.choices.Count; i++)
            {
                DialogueChoice choice = currentLine.choices[i];
                if (choice == null || choiceButtonPrefab == null || choicesContainer == null)
                {
                    continue;
                }

                ChoiceButtonView choiceButton = Instantiate(choiceButtonPrefab, choicesContainer);
                choiceButton.Bind(choice, SelectChoice);
                activeChoiceButtons.Add(choiceButton);
            }
        }

        private void ApplyLineVisualState(DialogueLine line)
        {
            if (characterNameText != null)
            {
                characterNameText.text = line.characterName ?? string.Empty;
            }

            if (dialogueText != null)
            {
                dialogueText.text = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(line.backgroundId))
            {
                BackgroundChanged?.Invoke(line.backgroundId);
            }

            if (!string.IsNullOrWhiteSpace(line.emotion))
            {
                CharacterEmotionChanged?.Invoke(line.characterId, line.emotion);
            }
        }

        private void BuildLineLookup(DialogueSequence sequence)
        {
            linesById.Clear();

            for (int i = 0; i < sequence.lines.Count; i++)
            {
                DialogueLine line = sequence.lines[i];
                linesById[line.lineId] = line;
            }
        }

        private void ClearChoices()
        {
            for (int i = 0; i < activeChoiceButtons.Count; i++)
            {
                if (activeChoiceButtons[i] != null)
                {
                    Destroy(activeChoiceButtons[i].gameObject);
                }
            }

            activeChoiceButtons.Clear();
        }

        private void Warn(string message)
        {
            if (logWarnings)
            {
                Debug.LogWarning($"[DialogueManager] {message}", this);
            }
        }
    }
}
