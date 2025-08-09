using UnityEngine;
using UnityEditor;
using System.Linq;

public class AssetCleaner
{
    [MenuItem("Tools/Delete Prefabs With Multiple Hyphens")]
    public static void DeletePrefabsWithMultipleHyphens()
    {
        // Carica tutti i prefab da Resources/- Prefabs_M
        GameObject[] allPrefabs = Resources.LoadAll<GameObject>("- Prefabs_M");

        foreach (GameObject prefab in allPrefabs)
        {
            if (prefab == null) continue;

            string prefabName = prefab.name;

            // Conta quanti trattini "-"
            int hyphenCount = prefabName.Count(c => c == '-');

            if (hyphenCount >= 3)
            {
                // Trova il path dell'asset
                string assetPath = AssetDatabase.GetAssetPath(prefab);

                if (!string.IsNullOrEmpty(assetPath))
                {
                    Debug.Log($"Deleting asset: {prefabName} at {assetPath}");
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Pulizia completata.");
    }
}
