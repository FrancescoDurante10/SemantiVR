using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScenarioFitnessFunctionSimple : MonoBehaviour
{
    public Transform centerSpawnPoint;
    public GameObject groundPlane;

    private List<GameObject> allPrefabs;

    private int populationSize = 100;
    private int generations = 40;
    private float mutationRate = 0.2f;
    private int elitism = 10;

    void Start()
    {
        allPrefabs = PrefabUtils.LoadAllPrefabsRecursively("- Prefabs_M");

        if (allPrefabs.Count < 2)
        {
            Debug.LogWarning("Non ci sono abbastanza prefab.");
            return;
        }

        RunGeneticAlgorithm();
    }

    void RunGeneticAlgorithm()
    {
        List<(int, int)> population = GenerateInitialPopulation();

        for (int gen = 0; gen < generations; gen++)
        {
            List<(int, int)> newPopulation = new();
            var sortedByFitness = population.OrderBy(p => Fitness(p.Item1, p.Item2)).ToList();

            newPopulation.AddRange(sortedByFitness.Take(elitism));

            while (newPopulation.Count < populationSize)
            {
                (int, int) parent1 = TournamentSelection(sortedByFitness);
                (int, int) parent2 = TournamentSelection(sortedByFitness);
                (int, int) child = Crossover(parent1, parent2);

                if (Random.value < mutationRate)
                    child = Mutate(child);

                if (child.Item1 != child.Item2)
                    newPopulation.Add(child);
            }

            population = newPopulation;
        }

        var best = population.OrderBy(p => Fitness(p.Item1, p.Item2)).First();
        GameObject prefab1 = allPrefabs[best.Item1];
        GameObject prefab2 = allPrefabs[best.Item2];

        SpawnBalancedPrefabs(prefab1, prefab2);
    }

    List<(int, int)> GenerateInitialPopulation()
    {
        List<(int, int)> pop = new();
        while (pop.Count < populationSize)
        {
            int i = Random.Range(0, allPrefabs.Count);
            int j;
            do { j = Random.Range(0, allPrefabs.Count); } while (j == i);
            pop.Add((i, j));
        }
        return pop;
    }

    float Fitness(int index1, int index2)
    {
        Bounds b1 = PrefabUtils.GetPrefabBounds(allPrefabs[index1]);
        Bounds b2 = PrefabUtils.GetPrefabBounds(allPrefabs[index2]);
        return Mathf.Abs(b1.size.magnitude - b2.size.magnitude);
    }

    (int, int) TournamentSelection(List<(int, int)> population)
    {
        int a = Random.Range(0, population.Count);
        int b = Random.Range(0, population.Count);
        return Fitness(population[a].Item1, population[a].Item2) < Fitness(population[b].Item1, population[b].Item2) ? population[a] : population[b];
    }

    (int, int) Crossover((int, int) p1, (int, int) p2)
    {
        return (p1.Item1, p2.Item2);
    }

    (int, int) Mutate((int, int) pair)
    {
        int mutateIndex = Random.Range(0, 2);
        int newVal;
        do { newVal = Random.Range(0, allPrefabs.Count); } while (newVal == pair.Item1 || newVal == pair.Item2);
        return mutateIndex == 0 ? (newVal, pair.Item2) : (pair.Item1, newVal);
    }

    void SpawnBalancedPrefabs(GameObject prefab1, GameObject prefab2)
    {
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
