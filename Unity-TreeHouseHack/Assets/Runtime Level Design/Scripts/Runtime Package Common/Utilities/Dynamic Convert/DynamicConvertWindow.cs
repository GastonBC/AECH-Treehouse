#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RLD
{
    public class DynamicConvertWindow : RLDEditorWindow
    {
        private class PrefabFolderDropHandler : DragAndDropHandler
        {
            protected override void PerformDrop()
            {
                RLDApp rldApp = RLDApp.Get;
                if (rldApp == null) return;

                string[] paths = DragAndDrop.paths;
                if (paths == null || paths.Length == 0) return;

                string prefabFolder = paths[0];
                if (string.IsNullOrEmpty(prefabFolder)) return;

                EditorUndoEx.Record(rldApp);
                rldApp.DynamicConvertSettings.PrefabFolder = prefabFolder;
            }
        }

        private PrefabFolderDropHandler _prefabFolderDropHandler = new PrefabFolderDropHandler();

        private void OnGUI()
        {
            RLDApp rldApp = RLDApp.Get;
            if (rldApp == null) return;

            DynamicConvertSettings settings = rldApp.DynamicConvertSettings;
            settings.RenderEditorGUI(rldApp);

            _prefabFolderDropHandler.Handle(Event.current, settings.PrefabFolderDropRect);

            const float btnWidth = 110.0f;
            GUIContent content = new GUIContent();

            EditorGUILayout.BeginHorizontal();
            content.text = "Convert scene";
            content.tooltip = "Converts the obejcts in the scene. Note: Only the objects which are of a convertable type will be converted.";
            if (GUILayout.Button(content, GUILayout.Width(btnWidth)))
            {
                ConvertScene(settings);
            }

            content.text = "Convert prefabs";
            content.tooltip = "Converts the prefabs in the specified folder. Note: Only the prefabs which are of a convertable type will be converted.";
            if (GUILayout.Button(content, GUILayout.Width(btnWidth)))
            {
                ConvertPrefabs(settings);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ConvertScene(DynamicConvertSettings settings)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.rootCount == 0) return;

            List<GameObject> sceneRoots = new List<GameObject>(activeScene.rootCount);
            activeScene.GetRootGameObjects(sceneRoots);

            foreach (GameObject root in sceneRoots)
            {
                if (root.HierarchyHasObjectsOfType(settings.ConvertableObjectTypes))
                    root.SetStatic(false, true);
            }

            RLDApp.Get.gameObject.SetStatic(true, true);
            EditorUtility.DisplayDialog("Done", "Scene objects were successfully converted!", "Ok");
        }

        private void ConvertPrefabs(DynamicConvertSettings settings)
        {
            if (string.IsNullOrEmpty(settings.PrefabFolder)) return;

            List<GameObject> allPrefabs = AssetDatabaseEx.LoadPrefabsInFolder(settings.PrefabFolder, settings.ProcessPrefabSubfolders, true);
            foreach (GameObject prefab in allPrefabs)
            {
                if (prefab.HierarchyHasObjectsOfType(settings.ConvertableObjectTypes))
                    prefab.SetStatic(false, true);
            }

            EditorUtility.DisplayDialog("Done", "Prefabs were successfully converted!", "Ok");
        }
    }
}
#endif