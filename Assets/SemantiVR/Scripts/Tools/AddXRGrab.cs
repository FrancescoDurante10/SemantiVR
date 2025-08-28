using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AddXRGrab
{
    [MenuItem("Tools/Add XRGrabInteractable to Grabbable Prefabs")]
    public static void AddGrabComponent()
    {
        GameObject[] allPrefabs = Resources.LoadAll<GameObject>("- Prefabs_M");
        int updated = 0;

        foreach (GameObject prefab in allPrefabs)
        {
            if (prefab == null || !prefab.CompareTag("Grabbable"))
                continue;

            string path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path))
                continue;

            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(path);

            if (prefabInstance.GetComponent<XRGrabInteractable>() == null)
            {
                prefabInstance.AddComponent<XRGrabInteractable>();
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
                updated++;
            }

            PrefabUtility.UnloadPrefabContents(prefabInstance);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ Completato: {updated} prefab aggiornati con XRGrabInteractable.");
    }
}
