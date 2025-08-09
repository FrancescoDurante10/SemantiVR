using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PairPenaltyData
{
    public List<string> keys = new();
    public List<int> values = new();
}

public static class RecentWinnersStore
{
    public static void Save(string path, Dictionary<string, int> data)
    {
        var jsonData = new PairPenaltyData();
        foreach (var kv in data)
        {
            jsonData.keys.Add(kv.Key);
            jsonData.values.Add(kv.Value);
        }

        string json = JsonUtility.ToJson(jsonData);
        File.WriteAllText(path, json);
    }

    public static Dictionary<string, int> Load(string path)
    {
        if (!File.Exists(path))
            return new Dictionary<string, int>();

        string json = File.ReadAllText(path);
        var jsonData = JsonUtility.FromJson<PairPenaltyData>(json);

        var result = new Dictionary<string, int>();
        for (int i = 0; i < jsonData.keys.Count; i++)
        {
            result[jsonData.keys[i]] = jsonData.values[i];
        }

        return result;
    }
}
