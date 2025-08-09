using UnityEngine;
using UnityEditor;

public class ColliderWrapperTool
{
    [MenuItem("Tools/Apply Collider Wrapper to Grabbable Prefabs")]
    public static void ApplyWrapper()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("- Prefabs_M");
        int updated = 0;

        foreach (var prefab in prefabs)
        {
            if (prefab == null || !prefab.CompareTag("Grabbable"))
                continue;

            string path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path)) continue;

            GameObject instance = PrefabUtility.LoadPrefabContents(path);

            if (instance.GetComponent<Collider>() == null)
            {
                Collider[] childColliders = instance.GetComponentsInChildren<Collider>();

                if (childColliders.Length > 0)
                {
                    Bounds bounds = childColliders[0].bounds;
                    for (int i = 1; i < childColliders.Length; i++)
                        bounds.Encapsulate(childColliders[i].bounds);

                    BoxCollider wrapper = instance.AddComponent<BoxCollider>();
                    wrapper.center = instance.transform.InverseTransformPoint(bounds.center);
                    wrapper.size = bounds.size;

                    updated++;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
        }

        Debug.Log($"✅ Collider Wrapper applicato a {updated} prefab con tag 'Grabbable'");
    }
}
