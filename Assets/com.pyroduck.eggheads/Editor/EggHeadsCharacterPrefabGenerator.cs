using System.Collections.Generic;
using System.IO;
using com.pyroduck.eggheads.Runtime.Scripts.Character;
using com.pyroduck.eggheads.Runtime.Scripts.Data;
using com.pyroduck.eggheads.Runtime.Scripts.Enums;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace com.pyroduck.eggheads.Editor
{
    public sealed class EggHeadsCharacterPrefabGenerator : EditorWindow
    {
        private const string PackageName = "com.pyroduck.eggheads";
        private const string DefaultOutputFolder = "Assets/EggHeads/GeneratedPrefabs";

        [SerializeField] private GameObject characterBase;
        [SerializeField] private GameObject body;
        [SerializeField] private GameObject eyes;
        [SerializeField] private GameObject eyebrows;
        [SerializeField] private GameObject nose;
        [SerializeField] private GameObject mouth;
        [SerializeField] private GameObject beardMustache;
        [SerializeField] private GameObject hairOrHat;
        [SerializeField] private GameObject weapon;
        [SerializeField] private Color eyesColor = Color.white;
        [SerializeField] private Color eyebrowsColor = Color.white;
        [SerializeField] private Color noseColor = Color.white;
        [SerializeField] private Color mouthColor = Color.white;
        [SerializeField] private Color beardMustacheColor = Color.white;
        [SerializeField] private Color hairOrHatColor = Color.white;
        [SerializeField] private Color weaponColor = Color.white;
        [SerializeField] private string outputFolder = DefaultOutputFolder;
        [SerializeField] private string prefabName = "GeneratedEggHeadCharacter";

        private static readonly Dictionary<string, VisualDataSO> VisualDataByPrefabPath = new();
        private static bool visualDataLookupInitialized;

        [MenuItem("Tools/PyroDuck/EggHeads/Character Prefab Generator")]
        private static void Open()
        {
            GetWindow<EggHeadsCharacterPrefabGenerator>("EggHeads Generator");
        }

        private void OnEnable()
        {
            RebuildVisualDataLookup();
            LoadDefaults();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Character Base", EditorStyles.boldLabel);
            characterBase = ObjectField("Base", characterBase);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Drag Prefabs", EditorStyles.boldLabel);
            body = ObjectField("Body", body);
            eyes = ObjectField("Eyes", eyes);
            DrawColorFieldIfColorable("Eyes Color", eyes, ref eyesColor);
            eyebrows = ObjectField("Eyebrows", eyebrows);
            DrawColorFieldIfColorable("Eyebrows Color", eyebrows, ref eyebrowsColor);
            nose = ObjectField("Nose", nose);
            DrawColorFieldIfColorable("Nose Color", nose, ref noseColor);
            mouth = ObjectField("Mouth", mouth);
            DrawColorFieldIfColorable("Mouth Color", mouth, ref mouthColor);
            beardMustache = ObjectField("Beard/Mustache", beardMustache);
            DrawColorFieldIfColorable("Beard/Mustache Color", beardMustache, ref beardMustacheColor);
            hairOrHat = ObjectField("Hair or Hat", hairOrHat);
            DrawColorFieldIfColorable("Hair or Hat Color", hairOrHat, ref hairOrHatColor);
            weapon = ObjectField("Weapon", weapon);
            DrawColorFieldIfColorable("Weapon Color", weapon, ref weaponColor);

            EditorGUILayout.Space(8);
            outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
            prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);

            EditorGUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Load Defaults"))
                    LoadDefaults();

                if (GUILayout.Button("Refresh Colorable Cache"))
                    RebuildVisualDataLookup();

                using (new EditorGUI.DisabledScope(characterBase == null || string.IsNullOrWhiteSpace(prefabName)))
                {
                    if (GUILayout.Button("Generate"))
                        GeneratePrefab();
                }
            }
        }

        private static GameObject ObjectField(string label, GameObject value)
        {
            return (GameObject)EditorGUILayout.ObjectField(label, value, typeof(GameObject), false);
        }

        private static void DrawColorFieldIfColorable(string label, GameObject prefab, ref Color color)
        {
            if (!IsColorable(prefab))
                return;

            EditorGUI.indentLevel++;
            color = EditorGUILayout.ColorField(label, color);
            EditorGUI.indentLevel--;
        }

        private void LoadDefaults()
        {
            characterBase = characterBase != null ? characterBase : LoadAsset("Runtime/Prefabs/Characters/CharacterBase.prefab");
            body = body != null ? body : LoadAsset("Runtime/Prefabs/Bodies/Body.prefab");
            eyes = eyes != null ? eyes : LoadAsset("Runtime/Prefabs/Eyes/Eyes_0.prefab");
            eyebrows = eyebrows != null ? eyebrows : LoadAsset("Runtime/Prefabs/Eyebrows/EyeBrowBase.prefab");
            nose = nose != null ? nose : LoadAsset("Runtime/Prefabs/Noses/Nose_0.prefab");
            mouth = mouth != null ? mouth : LoadAsset("Runtime/Prefabs/Mouths/Mouth_0.prefab");
            beardMustache = beardMustache != null ? beardMustache : LoadAsset("Runtime/Prefabs/BeardMustache/MustacheBase.prefab");
            hairOrHat = hairOrHat != null ? hairOrHat : LoadAsset("Runtime/Prefabs/HairAndHelmets/Hats/Hat_0.prefab");
            weapon = weapon != null ? weapon : LoadAsset("Runtime/Prefabs/Weapons/Melee/WeaponMelee_0.prefab");
        }

        private static GameObject LoadAsset(string packageRelativePath)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>($"Packages/{PackageName}/{packageRelativePath}")
                   ?? AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/{PackageName}/{packageRelativePath}");
        }

        private void GeneratePrefab()
        {
            outputFolder = NormalizeOutputFolder(outputFolder);
            EnsureFolder(outputFolder);

            var root = (GameObject)PrefabUtility.InstantiatePrefab(characterBase);
            if (root == null)
                root = Instantiate(characterBase);

            try
            {
                root.name = SanitizeFileName(prefabName);
                RemoveMissingScripts(root);

                var visualController = root.GetComponent<CharacterVisualController>();
                var bodyInstance = AddPart(root.transform, "BodyParent", body);
                AddVisualPart(visualController, VisualType.Eyes, eyes);
                ApplyVisualColor(visualController, VisualType.Eyes, eyes, eyesColor);
                AddVisualPart(visualController, VisualType.Eyebrow, eyebrows);
                ApplyVisualColor(visualController, VisualType.Eyebrow, eyebrows, eyebrowsColor);
                AddVisualPart(visualController, VisualType.Nose, nose);
                ApplyVisualColor(visualController, VisualType.Nose, nose, noseColor);
                AddVisualPart(visualController, VisualType.Mouth, mouth);
                ApplyVisualColor(visualController, VisualType.Mouth, mouth, mouthColor);
                AddVisualPart(visualController, VisualType.Moustache, beardMustache);
                ApplyVisualColor(visualController, VisualType.Moustache, beardMustache, beardMustacheColor);
                AddVisualPart(visualController, VisualType.HairOrHat, hairOrHat);
                ApplyVisualColor(visualController, VisualType.HairOrHat, hairOrHat, hairOrHatColor);
                AddVisualPart(visualController, VisualType.Weapon, weapon);
                ApplyVisualColor(visualController, VisualType.Weapon, weapon, weaponColor);
                ConfigureBodyReferences(root, bodyInstance);

                var path = AssetDatabase.GenerateUniqueAssetPath($"{outputFolder}/{root.name}.prefab");
                PrefabUtility.SaveAsPrefabAsset(root, path);
                AssetDatabase.SaveAssets();
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                EditorGUIUtility.PingObject(Selection.activeObject);
                Debug.Log($"EggHeads generated character prefab: {path}");
            }
            finally
            {
                DestroyImmediate(root);
            }
        }

        private static GameObject AddPart(Transform root, string slotName, GameObject prefab)
        {
            if (prefab == null)
                return null;

            var slot = FindDirectChild(root, slotName) ?? FindChild(root, slotName);
            if (slot == null)
            {
                Debug.LogWarning($"EggHeads generator could not find slot '{slotName}' on {root.name}.");
                return null;
            }

            return AddPart(slot, prefab);
        }

        private static GameObject AddPart(Transform slot, GameObject prefab)
        {
            if (slot == null || prefab == null)
                return null;

            ClearChildren(slot);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, slot);
            if (instance == null)
                instance = Object.Instantiate(prefab, slot);

            instance.name = prefab.name;
            instance.transform.localPosition = Vector3.zero;
            RemoveMissingScripts(instance);
            return instance;
        }

        private static GameObject AddVisualPart(CharacterVisualController visualController, VisualType visualType, GameObject prefab)
        {
            if (prefab == null)
                return null;

            if (visualController == null || visualController.VisualMappings == null)
            {
                Debug.LogWarning($"EggHeads generator could not find visual mappings for '{visualType}'.");
                return null;
            }

            foreach (var visualMapping in visualController.VisualMappings)
            {
                if (visualMapping.Type != visualType)
                    continue;

                if (visualMapping.Parent == null)
                {
                    Debug.LogWarning($"EggHeads generator could not find parent for visual type '{visualType}'.");
                    return null;
                }

                var instance = AddPart(visualMapping.Parent, prefab);
                visualMapping.CurrentVisual = instance;
                return instance;
            }

            Debug.LogWarning($"EggHeads generator could not find mapping for visual type '{visualType}'.");
            return null;
        }

        private static void ApplyVisualColor(
            CharacterVisualController visualController,
            VisualType visualType,
            GameObject prefab,
            Color color)
        {
            if (visualController == null || !IsColorable(prefab))
                return;

            var colorizer = visualController.GetComponent<CharacterColorizer>();
            if (colorizer == null)
                return;

            colorizer.bodyParts ??= new List<com.pyroduck.eggheads.Runtime.Scripts.Character.BodyPart>();
            colorizer.RefreshForVisualType(visualType);
            colorizer.SetColorForVisualType(visualType, color);
            EditorUtility.SetDirty(colorizer);
        }

        private static bool IsColorable(GameObject prefab)
        {
            return FindVisualData(prefab)?.isColorable == true;
        }

        private static VisualDataSO FindVisualData(GameObject prefab)
        {
            if (prefab == null)
                return null;

            if (!visualDataLookupInitialized)
                RebuildVisualDataLookup();

            var prefabPath = AssetDatabase.GetAssetPath(prefab);
            return !string.IsNullOrEmpty(prefabPath) && VisualDataByPrefabPath.TryGetValue(prefabPath, out var visualData)
                ? visualData
                : null;
        }

        private static void RebuildVisualDataLookup()
        {
            VisualDataByPrefabPath.Clear();

            var folders = GetExistingVisualDataFolders();
            var guids = FindAssetGuids("t:VisualDataSO", folders);
            if (guids.Length == 0)
                guids = FindAssetGuids("t:ScriptableObject", folders);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var visualData = AssetDatabase.LoadAssetAtPath<VisualDataSO>(path);
                if (visualData == null || visualData.BodyPartPrefab == null)
                    continue;

                AddVisualDataPath(AssetDatabase.GetAssetPath(visualData.BodyPartPrefab), visualData);
            }

            visualDataLookupInitialized = true;
        }

        private static void AddVisualDataPath(string prefabPath, VisualDataSO visualData)
        {
            if (string.IsNullOrEmpty(prefabPath) || visualData == null)
                return;

            AddVisualDataPathAlias(prefabPath, visualData);

            var assetRoot = $"Assets/{PackageName}/";
            var packageRoot = $"Packages/{PackageName}/";

            if (prefabPath.StartsWith(assetRoot))
            {
                AddVisualDataPathAlias(packageRoot + prefabPath.Substring(assetRoot.Length), visualData);
            }
            else if (prefabPath.StartsWith(packageRoot))
            {
                AddVisualDataPathAlias(assetRoot + prefabPath.Substring(packageRoot.Length), visualData);
            }
        }

        private static void AddVisualDataPathAlias(string prefabPath, VisualDataSO visualData)
        {
            if (!VisualDataByPrefabPath.ContainsKey(prefabPath))
                VisualDataByPrefabPath.Add(prefabPath, visualData);
        }

        private static string[] GetExistingVisualDataFolders()
        {
            var folders = new List<string>();
            AddFolderIfExists(folders, $"Assets/{PackageName}/Runtime/Data");
            AddFolderIfExists(folders, $"Packages/{PackageName}/Runtime/Data");
            return folders.ToArray();
        }

        private static void AddFolderIfExists(List<string> folders, string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                folders.Add(folder);
        }

        private static string[] FindAssetGuids(string filter, string[] folders)
        {
            return folders != null && folders.Length > 0
                ? AssetDatabase.FindAssets(filter, folders)
                : AssetDatabase.FindAssets(filter);
        }

        private static void ConfigureBodyReferences(GameObject root, GameObject bodyInstance)
        {
            if (root == null || bodyInstance == null) return;

            var visualController = root.GetComponent<CharacterVisualController>();
            if (visualController == null) return;

            var bodyTransform = FindDirectChild(bodyInstance.transform, "Body")
                                ?? FindChild(bodyInstance.transform, "Body");
            var bodyRotator = FindDirectChild(bodyInstance.transform, "BodyRotator")
                              ?? FindChild(bodyInstance.transform, "BodyRotator");

            if (bodyTransform != null)
                visualController.BodyVisual = bodyTransform;

            if (bodyRotator != null)
            {
                visualController.RotatingBodyParent = bodyRotator.gameObject;
                if (bodyRotator.GetComponent<RotationConstraint>() == null)
                    bodyRotator.gameObject.AddComponent<RotationConstraint>();
            }
        }

        private static void ClearChildren(Transform slot)
        {
            for (var i = slot.childCount - 1; i >= 0; i--)
                DestroyImmediate(slot.GetChild(i).gameObject);
        }

        private static void RemoveMissingScripts(GameObject root)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child.gameObject);
        }

        private static Transform FindDirectChild(Transform root, string name)
        {
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == name)
                    return child;
            }

            return null;
        }

        private static Transform FindChild(Transform root, string name)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                    return child;
            }

            return null;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            var normalized = folder.Replace('\\', '/').Trim('/');
            var parts = normalized.Split('/');
            var current = parts[0];

            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static string NormalizeOutputFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
                return DefaultOutputFolder;

            var normalized = folder.Replace('\\', '/').TrimEnd('/');
            return normalized == "Assets" || normalized.StartsWith("Assets/")
                ? normalized
                : DefaultOutputFolder;
        }

        private static string SanitizeFileName(string rawName)
        {
            var name = string.IsNullOrWhiteSpace(rawName) ? "GeneratedEggHeadCharacter" : rawName.Trim();
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                name = name.Replace(invalidChar, '_');
            return name;
        }
    }
}
