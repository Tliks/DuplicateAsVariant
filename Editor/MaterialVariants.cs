using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Aoyon.DuplicateAsVariant
{
    public class DuplicateWithMaterialVariants 
    {
        [MenuItem("GameObject/Duplicate as Variant", false, 0)]
        private static void DuplicateAsVariantMenu()
        {
            GameObject originalObject = Selection.activeGameObject;

            string folderPath = PrepareFolders(originalObject);

            GameObject prefabRoot = CreatePrefabVariant(originalObject, folderPath);

            CreateMaterialVariants(prefabRoot, folderPath);

            FinalizePrefab(prefabRoot, originalObject);

            Debug.Log("Saved prefab and materials to " + folderPath);
        }

        private static string PrepareFolders(GameObject originalObject)
        {
            CreateParent("Assets/Variants");
            string folderPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/Variants/{originalObject.name}_Variant");
            CreateParent(folderPath);
            return folderPath;
        }

        private static GameObject CreatePrefabVariant(GameObject originalObject, string folderPath)
        {
            string[] parts = folderPath.Split('/');
            return PrefabUtility.SaveAsPrefabAsset(originalObject, folderPath + "/" + parts[parts.Length - 1] + ".prefab");
        }

        private static void CreateMaterialVariants(GameObject prefabRoot, string folderPath)
        {
            var materialCache = new Dictionary<Material, Material>();
            var renderers = prefabRoot.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                var newMaterials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < newMaterials.Length; i++)
                {
                    var originalMaterial = renderer.sharedMaterials[i];
                    if (!materialCache.TryGetValue(originalMaterial, out var newMaterial))
                    {
                        // 元のマテリアルに対してMaterial Variantを生成
                        newMaterial = new Material(originalMaterial);
                        newMaterial.parent = originalMaterial;

                        string matSavePath = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + originalMaterial.name + ".mat");

                        AssetDatabase.CreateAsset(newMaterial, matSavePath);
                        materialCache[originalMaterial] = newMaterial;
                    }
                    newMaterials[i] = newMaterial;
                }
                renderer.sharedMaterials = newMaterials;
            }
        }

        private static void FinalizePrefab(GameObject variantRoot, GameObject originalObject)
        {
            PrefabUtility.SavePrefabAsset(variantRoot);
            GameObject instance = PrefabUtility.InstantiatePrefab(variantRoot) as GameObject;
            SceneManager.MoveGameObjectToScene(instance, originalObject.scene);
            EditorGUIUtility.PingObject(instance);

            Selection.activeGameObject = variantRoot;
            EditorUtility.FocusProjectWindow();
        }

        public static void CreateParent(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] folders = path.Split('/');
                string parentFolder = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    string folder = folders[i];
                    string newPath = parentFolder + "/" + folder;
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(parentFolder, folder);
                    }
                    parentFolder = newPath;
                }
            }
        }
    }
}
