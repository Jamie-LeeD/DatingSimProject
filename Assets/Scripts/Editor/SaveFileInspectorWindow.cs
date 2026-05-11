using System;
using System.IO;
using DatingSim.SaveSystem;
using UnityEditor;
using UnityEngine;

namespace DatingSim.EditorTools
{
    public class SaveFileInspectorWindow : EditorWindow
    {
        private readonly string[] slotFiles = new string[64];
        private int slotFileCount;
        private int selectedIndex = -1;
        private SaveGameData selectedSaveData;
        private string selectedSaveJson = string.Empty;
        private Vector2 scroll;

        [MenuItem("Tools/Dating Sim/Save File Inspector")]
        public static void Open()
        {
            GetWindow<SaveFileInspectorWindow>("Save File Inspector");
        }

        private void OnEnable()
        {
            RefreshFiles();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Save File Inspector", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Persistent Path: {Application.persistentDataPath}", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Refresh"))
            {
                RefreshFiles();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawFileList();
                DrawFileDetails();
            }
        }

        private void DrawFileList()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(280)))
            {
                EditorGUILayout.LabelField("Save Files", EditorStyles.boldLabel);

                if (slotFileCount == 0)
                {
                    EditorGUILayout.HelpBox("No save files found.", MessageType.Info);
                    return;
                }

                for (int i = 0; i < slotFileCount; i++)
                {
                    string fileName = Path.GetFileName(slotFiles[i]);
                    if (GUILayout.Toggle(selectedIndex == i, fileName, "Button"))
                    {
                        if (selectedIndex != i)
                        {
                            selectedIndex = i;
                            LoadSelectedFile();
                        }
                    }
                }
            }
        }

        private void DrawFileDetails()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);
                if (selectedSaveData == null)
                {
                    EditorGUILayout.HelpBox("Select a save file to inspect.", MessageType.None);
                    return;
                }

                scroll = EditorGUILayout.BeginScrollView(scroll);
                EditorGUILayout.LabelField($"Version: {selectedSaveData.saveVersion}");
                EditorGUILayout.LabelField($"Saved At (UTC): {selectedSaveData.savedAtUtc}");
                EditorGUILayout.LabelField($"Slot: {selectedSaveData.slotIndex}");
                EditorGUILayout.Space();

                if (selectedSaveData.dialogueProgress != null)
                {
                    EditorGUILayout.LabelField("Dialogue Progress", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Dialogue ID: {selectedSaveData.dialogueProgress.dialogueId}");
                    EditorGUILayout.LabelField($"Current Line: {selectedSaveData.dialogueProgress.currentLineId}");
                    EditorGUILayout.LabelField($"Active: {selectedSaveData.dialogueProgress.isDialogueActive}");
                    EditorGUILayout.LabelField($"Choices Recorded: {selectedSaveData.dialogueProgress.selectedChoiceIds?.Count ?? 0}");
                }

                EditorGUILayout.Space();
                if (selectedSaveData.relationshipData != null)
                {
                    EditorGUILayout.LabelField("Relationship Data", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Affection Entries: {selectedSaveData.relationshipData.affectionValues?.Count ?? 0}");
                    EditorGUILayout.LabelField($"Unlocked Routes: {selectedSaveData.relationshipData.unlockedRouteIds?.Count ?? 0}");
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Raw JSON", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(selectedSaveJson, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        private void RefreshFiles()
        {
            slotFileCount = 0;
            selectedIndex = -1;
            selectedSaveData = null;
            selectedSaveJson = string.Empty;

            string saveDirectory = Path.Combine(Application.persistentDataPath, "SaveData");
            if (!Directory.Exists(saveDirectory))
            {
                return;
            }

            string[] files = Directory.GetFiles(saveDirectory, "*.json");
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            slotFileCount = Mathf.Min(files.Length, slotFiles.Length);
            for (int i = 0; i < slotFileCount; i++)
            {
                slotFiles[i] = files[i];
            }
        }

        private void LoadSelectedFile()
        {
            if (selectedIndex < 0 || selectedIndex >= slotFileCount)
            {
                return;
            }

            string path = slotFiles[selectedIndex];
            if (!File.Exists(path))
            {
                selectedSaveData = null;
                selectedSaveJson = string.Empty;
                return;
            }

            selectedSaveJson = File.ReadAllText(path);
            selectedSaveData = JsonUtility.FromJson<SaveGameData>(selectedSaveJson);
        }
    }
}
