using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PrefabUtils
{
    public static List<GameObject> LoadAllPrefabsRecursively(string path)
    {
        List<GameObject> allPrefabs = new();
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>(path);

        foreach (var prefab in loadedPrefabs)
        {
            if (prefab != null && !prefab.name.EndsWith("_Rig"))
            {
                allPrefabs.Add(prefab);
            }
        }

        return allPrefabs;
    }

    public static GameObject TryGetRigVersion(GameObject basePrefab)
    {
        string rigName = basePrefab.name + "_Rig";
        string rigPath = "- Prefabs_M/People_M/Rigs_M/" + rigName;
        GameObject rigPrefab = Resources.Load<GameObject>(rigPath);

        if (rigPrefab != null)
        {
            Debug.Log($"[PrefabUtils] Trovato rig: {rigPath}");
            return rigPrefab;
        }
        return basePrefab;
    }

    public static (int, int) SplitKey(string key)
    {
        var parts = key.Split('-');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int a) &&
            int.TryParse(parts[1], out int b))
        {
            return (a, b);
        }
        return (-1, -1); 
    }

    public static Bounds GetPrefabBounds(GameObject prefab)
    {
        GameObject temp = Object.Instantiate(prefab);
        Renderer[] renderers = temp.GetComponentsInChildren<Renderer>();

        Bounds combined = renderers[0].bounds;
        foreach (Renderer rend in renderers)
            combined.Encapsulate(rend.bounds);

        Object.DestroyImmediate(temp);
        return combined;
    }

    public static void EnsureMeshesAreReadable(GameObject prefab)
    {
        foreach (var mf in prefab.GetComponentsInChildren<MeshFilter>(true))
        {
            SetMeshReadable(mf.sharedMesh);
        }

        foreach (var smr in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            SetMeshReadable(smr.sharedMesh);
        }
    }

    private static void SetMeshReadable(Mesh mesh)
    {
        if (mesh == null) return;

        string meshPath = AssetDatabase.GetAssetPath(mesh);
        if (string.IsNullOrEmpty(meshPath)) return;

        var importer = AssetImporter.GetAtPath(meshPath) as ModelImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
            Debug.Log($"Mesh resa leggibile: {mesh.name}");
        }
    }

    public static void FixMeshColliders(GameObject prefab)
    {
        foreach (var mc in prefab.GetComponentsInChildren<MeshCollider>(true))
        {
            if (!mc.convex)
            {
                mc.convex = true;
                Debug.Log($"MeshCollider reso convex: {mc.name}");
            }
        }
    }

    public static void AdjustGroundPlane(GameObject groundPlane, Vector3 pos1, Vector3 pos2, Bounds b1, Bounds b2)
    {
        float maxX = Mathf.Max(b1.size.x, b2.size.x) + 4f;
        float totalZ = Mathf.Abs(pos2.z - pos1.z) + Mathf.Max(b1.size.z, b2.size.z) + 4f;
        float centerZ = (pos1.z + pos2.z) / 2f;

        Vector3 planePos = groundPlane.transform.position;
        planePos.z = centerZ;
        groundPlane.transform.position = planePos;

        float scaleX = Mathf.Max(maxX / 10f, 2f);
        float scaleY = Mathf.Max(groundPlane.transform.localScale.y, 2f);
        float scaleZ = Mathf.Max(totalZ / 10f, 2f);

        groundPlane.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }

    public static void SpawnPrefabAtHeightZero(GameObject prefab, Vector3 spawnCenter)
    {
        GameObject temp = Object.Instantiate(prefab); 

        Renderer[] renderers = temp.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Object.Destroy(temp);
            return;
        }

        Bounds bounds = renderers[0].bounds;
        foreach (var rend in renderers)
            bounds.Encapsulate(rend.bounds);

        float bottomY = bounds.min.y;
        float heightAdjustment = -bottomY;

        Vector3 spawnPosition = spawnCenter;
        spawnPosition.y = heightAdjustment;

        Object.DestroyImmediate(temp); 

        GameObject finalObject = Object.Instantiate(prefab, spawnPosition, Quaternion.identity);
        bool isGrabbable = finalObject.CompareTag("Grabbable");
        bool isSmall = bounds.size.x < 1f && bounds.size.y < 1f && bounds.size.z < 1f;

        if (isGrabbable && isSmall)
        {
            finalObject.transform.position += Vector3.up * 1.0f; 
            if (!finalObject.GetComponent<FloatingObject>())
                finalObject.AddComponent<FloatingObject>();
        }
    }



}
