using System.Collections.Generic;
using UnityEngine;

public class ScenarioRandomizer : MonoBehaviour
{
    public Transform centerSpawnPoint;
    public GameObject groundPlane;

    private List<GameObject> allPrefabs;

    void Start()
    {
        allPrefabs = PrefabUtils.LoadAllPrefabsRecursively("- Prefabs_M");

        if (allPrefabs.Count < 2)
        {
            Debug.LogWarning("Non ci sono abbastanza prefab.");
            return;
        }

        SpawnTwoBalancedPrefabs();
    }

    void SpawnTwoBalancedPrefabs()
    {
        int index1 = Random.Range(0, allPrefabs.Count);
        int index2;
        do { index2 = Random.Range(0, allPrefabs.Count); } while (index2 == index1);

        GameObject prefab1 = allPrefabs[index1];
        GameObject prefab2 = allPrefabs[index2];

        Bounds bounds1 = PrefabUtils.GetPrefabBounds(prefab1);
        Bounds bounds2 = PrefabUtils.GetPrefabBounds(prefab2);

        float totalZ = (bounds1.size.z / 2f) + (bounds2.size.z / 2f) + 2f;
        Vector3 offset = Vector3.forward * (totalZ / 2f);

        Vector3 pos1 = centerSpawnPoint.position - offset;
        Vector3 pos2 = centerSpawnPoint.position + offset;

        PrefabUtils.AdjustGroundPlane(groundPlane, pos1, pos2, bounds1, bounds2);
        PrefabUtils.SpawnPrefabAtHeightZero(prefab1, pos1);
        PrefabUtils.SpawnPrefabAtHeightZero(prefab2, pos2);
    }
}
