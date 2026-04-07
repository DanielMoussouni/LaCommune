using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class AssetBakerUtility
{
    public static void BakeAssetsFromFolder<T>(string folderPath, List<T> targetList, UnityEngine.Object assetToSave = null) where T : UnityEngine.Object
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError($"[AssetBaker] The folder '{folderPath}' does not exist. Please check the spelling and ensure it starts with 'Assets/'.");
            return;
        }

        targetList.Clear();
        
        string typeName = typeof(T).Name; 
        
        string[] guids = AssetDatabase.FindAssets($"t:{typeName}", new[] { folderPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T loadedAsset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            
            if (loadedAsset != null && !targetList.Contains(loadedAsset))
            {
                targetList.Add(loadedAsset);
            }
        }

        if (assetToSave != null)
        {
            EditorUtility.SetDirty(assetToSave);
            AssetDatabase.SaveAssets();
        }
        
        Debug.Log($"[AssetBaker] Success! Loaded {targetList.Count} assets of type '{typeName}' from '{folderPath}'.");
    }
}
