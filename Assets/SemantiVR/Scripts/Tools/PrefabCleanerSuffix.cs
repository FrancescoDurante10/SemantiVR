using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;

public class PrefabCleanerSuffix
{
    [MenuItem("Tools/Clean Prefabs (Suffix + Hyphen Rule)")]
    public static void CleanPrefabs()
    {
        GameObject[] allPrefabs = Resources.LoadAll<GameObject>("- Prefabs_M");
        int deletedCount = 0;
        int renamedCount = 0;

        foreach (GameObject prefab in allPrefabs)
        {
            if (prefab == null) continue;

            string prefabName = prefab.name.Trim();
            string prefabNameLower = prefabName.ToLower();
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(assetPath)) continue;

            // --- Prima parte: PrefabCleanerSuffix ---
            // Caso 1: rinomina quelli con "-a"
            if (prefabNameLower.EndsWith("-a"))
            {
                string newName = prefabName.Substring(0, prefabName.Length - 2); // rimuove "-a"
                Debug.Log($"Rinominando prefab: {prefabName} -> {newName}");
                string result = AssetDatabase.RenameAsset(assetPath, newName);
                if (string.IsNullOrEmpty(result)) renamedCount++;
                else Debug.LogWarning($"Errore nel rinominare {prefabName}: {result}");
            }
            // Caso 2: elimina quelli con "-[b-z]"
            else if (Regex.IsMatch(prefabNameLower, @"-[b-z]$"))
            {
                Debug.Log($"Eliminando prefab (suffisso): {prefabName} at {assetPath}");
                AssetDatabase.DeleteAsset(assetPath);
                deletedCount++;
                continue; // salta la parte sotto
            }

            int hyphenCount = prefabName.Count(c => c == '-');
            if (hyphenCount >= 3)
            {
                Debug.Log($"Eliminando prefab (troppe '-'): {prefabName} at {assetPath}");
                AssetDatabase.DeleteAsset(assetPath);
                deletedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Operazione completata. Rinominati: {renamedCount}, Eliminati: {deletedCount} prefab.");
    }
}
