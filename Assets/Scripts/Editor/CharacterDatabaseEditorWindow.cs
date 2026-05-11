using DatingSim.Characters;
using UnityEditor;
using UnityEngine;

namespace DatingSim.EditorTools
{
    public class CharacterDatabaseEditorWindow : EditorWindow
    {
        private CharacterDatabase database;
        private SerializedObject serializedDatabase;
        private SerializedProperty charactersProperty;
        private Vector2 scroll;

        [MenuItem("Tools/Dating Sim/Character Database Editor")]
        public static void Open()
        {
            GetWindow<CharacterDatabaseEditorWindow>("Character Database");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Character Database Editor", EditorStyles.boldLabel);
            database = (CharacterDatabase)EditorGUILayout.ObjectField("Database", database, typeof(CharacterDatabase), false);

            if (database == null)
            {
                EditorGUILayout.HelpBox("Assign a CharacterDatabase asset to edit it.", MessageType.Info);
                return;
            }

            if (serializedDatabase == null || serializedDatabase.targetObject != database)
            {
                serializedDatabase = new SerializedObject(database);
                charactersProperty = serializedDatabase.FindProperty("characters");
            }

            serializedDatabase.Update();
            EditorGUILayout.PropertyField(charactersProperty, true);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create New Character Profile"))
                {
                    CreateCharacterProfileAsset();
                }

                if (GUILayout.Button("Rebuild Lookup"))
                {
                    database.RebuildLookup();
                    EditorUtility.SetDirty(database);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Access", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < charactersProperty.arraySize; i++)
            {
                SerializedProperty entry = charactersProperty.GetArrayElementAtIndex(i);
                CharacterProfile profile = entry.objectReferenceValue as CharacterProfile;
                if (profile == null)
                {
                    continue;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.ObjectField(profile, typeof(CharacterProfile), false);
                    if (GUILayout.Button("Ping", GUILayout.Width(60)))
                    {
                        EditorGUIUtility.PingObject(profile);
                    }
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeObject = profile;
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            serializedDatabase.ApplyModifiedProperties();
        }

        private void CreateCharacterProfileAsset()
        {
            string selectedPath = AssetDatabase.GetAssetPath(database);
            string targetFolder = "Assets";

            if (!string.IsNullOrWhiteSpace(selectedPath))
            {
                targetFolder = System.IO.Path.GetDirectoryName(selectedPath).Replace("\\", "/");
            }

            CharacterProfile newProfile = CreateInstance<CharacterProfile>();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/CharacterProfile_New.asset");
            AssetDatabase.CreateAsset(newProfile, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            charactersProperty.arraySize++;
            charactersProperty.GetArrayElementAtIndex(charactersProperty.arraySize - 1).objectReferenceValue = newProfile;
            serializedDatabase.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);

            Selection.activeObject = newProfile;
            EditorGUIUtility.PingObject(newProfile);
        }
    }
}
