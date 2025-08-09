using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

public class ScenarioFitnessFunctionLogic : MonoBehaviour
{
    public bool enableSimilar = true;
    public bool enableWinner = true;
    public Transform centerSpawnPoint;
    public GameObject groundPlane;
    public SemanticCompatibilityChecker compatibilityChecker;

    private List<GameObject> allPrefabs;
    private Dictionary<string, int> recentWinners = new();
    private int populationSize = 100;
    private int generations = 40;
    private float mutationRate = 0.2f;
    private int elitism = 10;
    private const int recentPenaltyLifetime = 5;
    private string saveFilePath;

    async void Start()
    {
        if (enableWinner)
        {
            saveFilePath = Path.Combine(Application.persistentDataPath, "recent_winners.json");
            Debug.Log($"📂 File salvato in: {saveFilePath}");

            recentWinners = RecentWinnersStore.Load(saveFilePath);
        }

        allPrefabs = PrefabUtils.LoadAllPrefabsRecursively("- Prefabs_M");

        if (allPrefabs.Count < 2)
        {
            Debug.LogWarning("Non ci sono abbastanza prefab.");
            return;
        }

        await RunGeneticAlgorithm();
    }

    async Task RunGeneticAlgorithm()
    {
        List<(int, int)> population = GenerateInitialPopulation();

        for (int gen = 0; gen < generations; gen++)
        {
            var fitnessResults = new List<(int, int, float)>();

            foreach (var pair in population)
            {
                float score = await Fitness(pair.Item1, pair.Item2);
                fitnessResults.Add((pair.Item1, pair.Item2, score));
            }

            var sortedByFitness = fitnessResults.OrderByDescending(p => p.Item3).ToList();
            var newPopulation = sortedByFitness.Take(elitism).Select(p => (p.Item1, p.Item2)).ToList();
            var seenPairs = new HashSet<string>(newPopulation.Select(p => GetPairKey(p.Item1, p.Item2)));

            while (newPopulation.Count < populationSize)
            {
                var parent1 = TournamentSelection(fitnessResults);
                var parent2 = TournamentSelection(fitnessResults);
                var child = Crossover(parent1, parent2);

                if (Random.value < mutationRate)
                    child = Mutate(child);

                if (child.Item1 != child.Item2)
                {
                    string key = GetPairKey(child.Item1, child.Item2);
                    if (!seenPairs.Contains(key))
                    {
                        seenPairs.Add(key);
                        newPopulation.Add(child);
                    }
                }
            }

            population = newPopulation;
        }

        // Seleziona vincitore finale
        var finalFitness = new List<(int, int, float)>();
        foreach (var pair in population)
            finalFitness.Add((pair.Item1, pair.Item2, await Fitness(pair.Item1, pair.Item2)));

        var best = finalFitness.OrderByDescending(p => p.Item3).First();
        string finalKey = GetPairKey(best.Item1, best.Item2);

        if (enableWinner)
        {
            // ✅ Azzera penalità della coppia finale
            recentWinners[finalKey] = 0;

            // ✅ Invecchia tutte le altre coppie
            var keys = recentWinners.Keys.ToList();
            foreach (var key in keys)
            {
                if (key != finalKey)
                {
                    recentWinners[key]++;

                    if (recentWinners[key] > recentPenaltyLifetime)
                    {
                        (int i, int j) = PrefabUtils.SplitKey(key);
                        if (i >= 0 && j >= 0 && i < allPrefabs.Count && j < allPrefabs.Count)
                        {
                            string name1 = allPrefabs[i].name;
                            string name2 = allPrefabs[j].name;
                            Debug.Log($"🧹 Rimozione per età: {name1} + {name2}");
                        }
                        else
                        {
                            Debug.Log($"🧹 Rimozione per età (Key malformata): {key}");
                        }

                        recentWinners.Remove(key);
                    }
                }
            }

            // ✅ Salva su file solo se penalità è abilitata
            RecentWinnersStore.Save(saveFilePath, recentWinners);
        }

        Debug.Log($"🏆 Coppia selezionata: {allPrefabs[best.Item1].name} + {allPrefabs[best.Item2].name}");

        GameObject prefab1 = PrefabUtils.TryGetRigVersion(allPrefabs[best.Item1]);
        GameObject prefab2 = PrefabUtils.TryGetRigVersion(allPrefabs[best.Item2]);

        SpawnBalancedPrefabs(prefab1, prefab2);
    }


    List<(int, int)> GenerateInitialPopulation()
    {
        var pop = new List<(int, int)>();
        var seen = new HashSet<string>();

        while (pop.Count < populationSize)
        {
            int i = Random.Range(0, allPrefabs.Count);
            int j;
            do { j = Random.Range(0, allPrefabs.Count); } while (j == i);

            string key = GetPairKey(i, j);
            if (!seen.Contains(key))
            {
                seen.Add(key);
                pop.Add((i, j));
            }
        }

        return pop;
    }

    async Task<float> Fitness(int index1, int index2)
    {
        string name1 = allPrefabs[index1].name;
        string name2 = allPrefabs[index2].name;
        string pairKey = GetPairKey(index1, index2);

        float score = await compatibilityChecker.CalculateSemanticCompatibility(name1, name2);

        if (enableSimilar && AreTooSimilar(name1, name2))
        {
            score *= 0.75f;
            Debug.Log($"[Penalized (Too Similar)] {name1} + {name2} → {score:F2}");
        }

        if (enableWinner && recentWinners.ContainsKey(pairKey))
        {
            score *= 0.90f;
            Debug.Log($"[Penalized (Recent Winner)] {name1} + {name2} → {score:F2}");
        }

        Debug.Log($"{name1} + {name2} → {score:F2}");
        return score;
    }

    (int, int) TournamentSelection(List<(int, int, float)> fitnessList)
    {
        var a = fitnessList[Random.Range(0, fitnessList.Count)];
        var b = fitnessList[Random.Range(0, fitnessList.Count)];
        return a.Item3 > b.Item3 ? (a.Item1, a.Item2) : (b.Item1, b.Item2);
    }

    (int, int) Crossover((int, int) p1, (int, int) p2)
    {
        return (p1.Item1, p2.Item2);
    }

    (int, int) Mutate((int, int) pair)
    {
        int mutateIndex = Random.Range(0, 2);
        int newVal;
        do
        {
            newVal = Random.Range(0, allPrefabs.Count);
        } while (newVal == pair.Item1 || newVal == pair.Item2);

        return mutateIndex == 0 ? (newVal, pair.Item2) : (pair.Item1, newVal);
    }

    bool AreTooSimilar(string name1, string name2)
    {
        var words1 = name1.ToLower().Split(' ');
        var words2 = name2.ToLower().Split(' ');

        int shared = words1.Intersect(words2).Count();
        return shared > 0;
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

    string GetPairKey(int a, int b)
    {
        return a < b ? $"{a}-{b}" : $"{b}-{a}";
    }
}

