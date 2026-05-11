using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DatingSim.EditorTools
{
    public class MissingReferenceCheckerWindow : EditorWindow
    {
        private readonly List<Object> missingReferenceOwners = new List<Object>();
        private readonly List<string> issues = new List<string>();
        private Vector2 scroll;

        [MenuItem("Tools/Dating Sim/Missing Reference Checker")]
        public static void Open()
        {
            GetWindow<MissingReferenceCheckerWindow>("Missing Reference Checker");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Missing Reference Checker", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Scans open scenes for missing script components and broken object references.", MessageType.Info);

            if (GUILayout.Button("Scan Open Scenes"))
            {
                ScanOpenScenes();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Issues Found: {issues.Count}", EditorStyles.boldLabel);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < issues.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(issues[i], GUILayout.ExpandWidth(true));
                    if (i < missingReferenceOwners.Count && missingReferenceOwners[i] != null && GUILayout.Button("Ping", GUILayout.Width(60)))
                    {
                        EditorGUIUtility.PingObject(missingReferenceOwners[i]);
                        Selection.activeObject = missingReferenceOwners[i];
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void ScanOpenScenes()
        {
            issues.Clear();
            missingReferenceOwners.Clear();

            for (int sceneIndex = 0; sceneIndex < EditorSceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = EditorSceneManager.GetSceneAt(sceneIndex);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    continue;
                }

                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    ScanGameObjectRecursive(roots[rootIndex], scene.path);
                }
            }

            if (issues.Count == 0)
            {
                issues.Add("No missing references found in open scenes.");
                missingReferenceOwners.Add(null);
            }
        }

        private void ScanGameObjectRecursive(GameObject go, string scenePath)
        {
            int missingScripts = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (missingScripts > 0)
            {
                issues.Add($"[{scenePath}] {go.name}: Missing Scripts = {missingScripts}");
                missingReferenceOwners.Add(go);
            }

            Component[] components = go.GetComponents<Component>();
            for (int compIndex = 0; compIndex < components.Length; compIndex++)
            {
                Component component = components[compIndex];
                if (component == null)
                {
                    continue;
                }

                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty iterator = serializedObject.GetIterator();
                while (iterator.NextVisible(true))
                {
                    if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                    {
                        continue;
                    }

                    if (iterator.objectReferenceValue == null && iterator.objectReferenceInstanceIDValue != 0)
                    {
                        string issue = $"[{scenePath}] {go.name}/{component.GetType().Name}: Missing reference in '{iterator.displayName}'";
                        issues.Add(issue);
                        missingReferenceOwners.Add(component);
                    }
                }
            }

            for (int childIndex = 0; childIndex < go.transform.childCount; childIndex++)
            {
                ScanGameObjectRecursive(go.transform.GetChild(childIndex).gameObject, scenePath);
            }
        }
    }
}
