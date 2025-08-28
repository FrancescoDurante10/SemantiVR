using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrefabCleaner_ByExtendedNames
{
    [MenuItem("Tools/Delete Extended Prefabs With Similar Base Names")]

    
    public static void DeleteExtendedPrefabsWithSimilarBaseNames()
    {
        /*
            // Modifica del percorso per includere Resources/- Prefabs_M
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/SemantiVR/Resources/- Prefabs_M" });
            List<string> prefabNames = new List<string>();
            int deletedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                string prefabName = prefab.name.ToLower().Trim();
                bool isDuplicate = false;

                // Controlla se esiste già un prefab base che contiene il nome corrente come prefisso
                foreach (var existingName in prefabNames)
                {
                    if (prefabName.StartsWith(existingName) && prefabName != existingName)
                    {
                        // Il nome base esiste già e il nome corrente è una "versione estesa"
                        Debug.Log($"Deleting extended version: '{prefabName}' (base name: '{existingName}')");
                        AssetDatabase.DeleteAsset(path);
                        deletedCount++;
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate)
                {
                    prefabNames.Add(prefabName);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Pulizia completata. Rimossi {deletedCount} prefab con suffisso.");
        */
    }

}
