using DatingSim.Dialogue;
using UnityEditor;
using UnityEngine;

namespace DatingSim.EditorTools
{
    public class DialogueJsonValidatorWindow : EditorWindow
    {
        private TextAsset dialogueJson;
        private Vector2 scroll;
        private string validationReport = "Select a JSON TextAsset and run validation.";
        private bool isValid;

        [MenuItem("Tools/Dating Sim/Dialogue JSON Validator")]
        public static void Open()
        {
            GetWindow<DialogueJsonValidatorWindow>("Dialogue JSON Validator");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Dialogue JSON Validator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            dialogueJson = (TextAsset)EditorGUILayout.ObjectField("Dialogue JSON", dialogueJson, typeof(TextAsset), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Selected Asset"))
                {
                    dialogueJson = Selection.activeObject as TextAsset;
                }

                if (GUILayout.Button("Validate"))
                {
                    ValidateAsset();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(isValid ? "Status: Valid" : "Status: Not Valid", EditorStyles.helpBox);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.TextArea(validationReport, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void ValidateAsset()
        {
            if (!DialogueJsonLoader.TryLoad(dialogueJson, out DialogueSequence sequence, out string error))
            {
                isValid = false;
                validationReport = $"Validation failed.\n\n{error}";
                return;
            }

            isValid = true;
            validationReport =
                $"Validation succeeded.\n\n" +
                $"Dialogue ID: {sequence.dialogueId}\n" +
                $"Start Line ID: {sequence.startLineId}\n" +
                $"Line Count: {sequence.lines.Count}\n" +
                $"Asset Path: {AssetDatabase.GetAssetPath(dialogueJson)}";
        }
    }
}
