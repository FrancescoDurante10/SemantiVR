using UnityEngine;
using UnityEditor;

public class AddWeightScriptsTool
{
    [MenuItem("Tools/Grabbable/Add Weight Scripts to Prefabs")]
    public static void AddWeightScripts()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("- Prefabs_M");
        int updatedCount = 0;

        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null || !prefab.CompareTag("Grabbable"))
                continue;

            string path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path)) continue;

            GameObject instance = PrefabUtility.LoadPrefabContents(path);
            bool modified = false;

            if (instance.GetComponent<AutoWeightBasedGrab>() == null)
            {
                instance.AddComponent<AutoWeightBasedGrab>();
                modified = true;
            }

            if (instance.GetComponent<GrabWeightResistor>() == null)
            {
                instance.AddComponent<GrabWeightResistor>();
                modified = true;
            }

            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(instance, path);
                updatedCount++;
            }

            PrefabUtility.UnloadPrefabContents(instance);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Completato. Prefab aggiornati: {updatedCount}");
    }
}
