using UnityEditor;
using UnityEngine;

public class OutlineAdder : MonoBehaviour
{
    [MenuItem("Tools/Apply Outline + Hover to Grabbable Prefabs")]
    public static void ApplyOutline()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("- Prefabs_M");
        int updated = 0;

        foreach (var prefab in prefabs)
        {
            if (prefab == null || !prefab.CompareTag("Grabbable"))
                continue;

            string path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path))
                continue;

            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(path);

            var outline = prefabInstance.GetComponent<Outline>();
            if (outline == null)
                outline = prefabInstance.AddComponent<Outline>();

            outline.OutlineColor = Color.cyan;
            outline.OutlineWidth = 4f;

            if (prefabInstance.GetComponent<OutlineOnHover>() == null)
                prefabInstance.AddComponent<OutlineOnHover>();

            PrefabUtils.EnsureMeshesAreReadable(prefabInstance);
            PrefabUtils.FixMeshColliders(prefabInstance);

            PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
            PrefabUtility.UnloadPrefabContents(prefabInstance);

            updated++;
            Debug.Log($"✅ Reimpostato Outline + Hover per: {prefab.name}");
        }

        Debug.Log($"✨ Completato: {updated} prefab aggiornati.");
    }
}

