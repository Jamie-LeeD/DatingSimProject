using System.Collections.Generic;
using DatingSim.Dialogue;
using UnityEditor;
using UnityEngine;

namespace DatingSim.EditorTools
{
    public class DialoguePreviewWindow : EditorWindow
    {
        private TextAsset dialogueJson;
        private DialogueSequence sequence;
        private readonly Dictionary<string, DialogueLine> lineLookup = new Dictionary<string, DialogueLine>();
        private DialogueLine currentLine;
        private Vector2 scroll;
        private string loadStatus = "Load a dialogue JSON to preview.";

        [MenuItem("Tools/Dating Sim/Dialogue Preview")]
        public static void Open()
        {
            GetWindow<DialoguePreviewWindow>("Dialogue Preview");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Dialogue Preview", EditorStyles.boldLabel);
            dialogueJson = (TextAsset)EditorGUILayout.ObjectField("Dialogue JSON", dialogueJson, typeof(TextAsset), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Selected Asset"))
                {
                    dialogueJson = Selection.activeObject as TextAsset;
                }

                if (GUILayout.Button("Load"))
                {
                    LoadSequence();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(loadStatus, MessageType.None);

            if (sequence == null || currentLine == null)
            {
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField($"Dialogue ID: {sequence.dialogueId}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Line ID: {currentLine.lineId}");
            EditorGUILayout.LabelField($"Character: {currentLine.characterName} ({currentLine.characterId})");
            EditorGUILayout.LabelField($"Emotion: {currentLine.emotion}");
            EditorGUILayout.LabelField($"Background: {currentLine.backgroundId}");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Text", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(currentLine.text ?? string.Empty, MessageType.None);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);
            if (currentLine.choices == null || currentLine.choices.Count == 0)
            {
                EditorGUILayout.LabelField("No choices on this line.");
            }
            else
            {
                for (int i = 0; i < currentLine.choices.Count; i++)
                {
                    DialogueChoice choice = currentLine.choices[i];
                    if (choice == null)
                    {
                        continue;
                    }

                    if (GUILayout.Button($"{choice.choiceText} -> {choice.nextLineId}"))
                    {
                        JumpToLine(choice.nextLineId);
                    }
                }
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Follow Next Line"))
                {
                    JumpToLine(currentLine.nextLineId);
                }

                if (GUILayout.Button("Jump to Start"))
                {
                    JumpToLine(sequence.startLineId);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void LoadSequence()
        {
            if (!DialogueJsonLoader.TryLoad(dialogueJson, out DialogueSequence loadedSequence, out string error))
            {
                sequence = null;
                currentLine = null;
                lineLookup.Clear();
                loadStatus = $"Failed to load dialogue JSON: {error}";
                return;
            }

            sequence = loadedSequence;
            lineLookup.Clear();
            for (int i = 0; i < sequence.lines.Count; i++)
            {
                DialogueLine line = sequence.lines[i];
                lineLookup[line.lineId] = line;
            }

            JumpToLine(sequence.startLineId);
            loadStatus = $"Loaded '{sequence.dialogueId}' with {sequence.lines.Count} lines.";
        }

        private void JumpToLine(string lineId)
        {
            if (string.IsNullOrWhiteSpace(lineId))
            {
                loadStatus = "Reached end of dialogue (empty nextLineId).";
                return;
            }

            if (!lineLookup.TryGetValue(lineId, out DialogueLine line))
            {
                loadStatus = $"Line '{lineId}' not found in loaded sequence.";
                return;
            }

            currentLine = line;
        }
    }
}
