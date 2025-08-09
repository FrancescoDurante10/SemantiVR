using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

[System.Serializable]
public class CheckWordResponse
{
    public string input;
    public List<string> found_words;
    public List<string> missing_words;
    public bool all_found;
}

public class Word2VecAssetsChecker : MonoBehaviour
{
    private string apiUrl = "http://127.0.0.1:8000/check-word";
    private List<GameObject> allPrefabs;

    void Start()
    {
        CheckAllPrefabNames();
    }

    async void CheckAllPrefabNames()
    {
        allPrefabs = PrefabUtils.LoadAllPrefabsRecursively("- Prefabs_M");
        Debug.Log($"Trovati {allPrefabs.Count} prefab.");

        foreach (GameObject prefab in allPrefabs)
        {
            string prefabName = prefab.name;
            bool result = await IsWordInModel(prefabName);

            if (result)
                Debug.Log($"✅ '{prefabName}' è presente completamente nel modello Word2Vec.");
            else
                Debug.LogWarning($"❌ '{prefabName}' contiene parole mancanti nel modello.");
        }
    }

    async Task<bool> IsWordInModel(string phrase)
    {
        string encoded = UnityWebRequest.EscapeURL(phrase);
        string url = $"{apiUrl}?word={encoded}";

        UnityWebRequest request = UnityWebRequest.Get(url);
        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Errore HTTP: " + request.error);
            return false;
        }

        try
        {
            CheckWordResponse response = JsonUtility.FromJson<CheckWordResponse>(request.downloadHandler.text);
            return response.all_found;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Errore deserializzazione JSON: " + e.Message);
            return false;
        }
    }
}
