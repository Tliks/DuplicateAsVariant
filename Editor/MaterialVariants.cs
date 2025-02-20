using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Aoyon.DuplicateAsVariant
{
    public class DuplicateWithMaterialVariants 
    {
        [MenuItem("GameObject/Convert to Material Variant", false, 0)]
        private static void DuplicateAsVariantMenu()
        {
            GameObject originalObject = Selection.activeGameObject;

            string folderPath = PrepareFolders(originalObject);

            CreateMaterialVariants(originalObject, folderPath);

            Debug.Log("Saved materials to " + folderPath);
        }

        private static string PrepareFolders(GameObject originalObject)
        {
            CreateFolder("Assets/Material Variants");
            string folderPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/Material Variants/{originalObject.name}");
            CreateFolder(folderPath);
            return folderPath;
        }

        private static void CreateMaterialVariants(GameObject instance, string folderPath)
        {
            var materialCache = new Dictionary<Material, Material>();
            var renderers = instance.GetComponentsInChildren<Renderer>();

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

        public static void CreateFolder(string path)
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
