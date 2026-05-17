using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using com.pyroduck.eggheads.Runtime.Scripts.Data;

namespace com.pyroduck.eggheads.Editor
{
    public class EggHeadsURPUpgrader : EditorWindow
    {
        [MenuItem("Tools/EggHeads URP 2D Upgrader")]
        public static void ShowWindow()
        {
            GetWindow<EggHeadsURPUpgrader>("EggHeads URP 2D Upgrader");
        }

        private void OnGUI()
        {
            GUILayout.Label("URP 2D Lighting Upgrader", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool makes all Sprite materials in the package compatible with the URP 2D Lighting (Sprite-Lit) system, or reverts them to default.\n\n" +
                "Please ensure that the 'Universal RP' (URP) package is installed in your project and the 2D Renderer is being used.", 
                MessageType.Info
            );

            EditorGUILayout.Space();

            if (GUILayout.Button("1. Convert to URP 2D Lit Material (Receives Light)", GUILayout.Height(40)))
            {
                UpgradeToURP();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("2. Convert to Standard (Unlit) Material (Default)", GUILayout.Height(30)))
            {
                DowngradeToStandard();
            }
        }

        private void UpgradeToURP()
        {
            Shader urpShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
            if (urpShader == null)
            {
                EditorUtility.DisplayDialog("Error", "URP 2D Shader not found! Please make sure the URP 2D library is installed in your project.", "OK");
                return;
            }

            Material urpMaterial = new Material(urpShader);
            ApplyMaterialToAllPrefabs(urpMaterial);
            EditorUtility.DisplayDialog("Success", "All parts have been successfully made URP 2D Lighting compatible!", "OK");
        }

        private void DowngradeToStandard()
        {
            Shader standardShader = Shader.Find("Sprites/Default");
            if (standardShader == null) return;

            Material standardMaterial = new Material(standardShader);
            ApplyMaterialToAllPrefabs(standardMaterial);
            EditorUtility.DisplayDialog("Success", "All parts have been reverted to Standard (Unlit) material.", "OK");
        }

        private void ApplyMaterialToAllPrefabs(Material newMaterial)
        {
            string[] guids = AssetDatabase.FindAssets("t:VisualGroupsDataSO");
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var group = AssetDatabase.LoadAssetAtPath<VisualGroupsDataSO>(path);

                if (group != null)
                {
                    foreach (var visual in group.VisualList)
                    {
                        if (visual.BodyPartPrefab != null)
                        {
                            string prefabPath = AssetDatabase.GetAssetPath(visual.BodyPartPrefab);
                            if (string.IsNullOrEmpty(prefabPath)) continue;

                            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
                            {
                                var prefabRoot = editingScope.prefabContentsRoot;
                                var renderers = prefabRoot.GetComponentsInChildren<SpriteRenderer>(true);

                                foreach (var renderer in renderers)
                                {
                                    if (renderer.sharedMaterial == null || renderer.sharedMaterial.shader != newMaterial.shader)
                                    {
                                        renderer.sharedMaterial = newMaterial;
                                        count++;
                                    }
                                }
                            } // Automatically saves the prefab when disposed
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[EggHeads] Process Completed. Updated Editor Part Count: {count}");
        }
    }
}

