using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

// Modello per inviare i dati
[System.Serializable]
public class DescriptionPair
{
    public string description1;
    public string description2;
}

// Modello per ricevere i dati
[System.Serializable]
public class CompatibilityResponse
{
    public float compatibility_score;
}

public class SemanticCompatibilityChecker : MonoBehaviour
{
    private string apiUrl = "http://127.0.0.1:8000/semantic-compatibility"; // Porta 8000!

    // Funzione asincrona per calcolare la compatibilità semantica
    public async Task<float> CalculateSemanticCompatibility(string description1, string description2)
    {
        // Prepara il corpo JSON correttamente
        DescriptionPair pair = new DescriptionPair
        {
            description1 = description1,
            description2 = description2
        };
        string jsonBody = JsonUtility.ToJson(pair);

        // Prepara la richiesta HTTP POST
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Manda la richiesta
        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Errore nella richiesta HTTP: " + request.error);
            request.Dispose();
            return 0.1f; // Valore neutro in caso di errore
        }

        // Logga la risposta per debug (opzionale)
        Debug.Log("Risposta server: " + request.downloadHandler.text);

        // Deserializza la risposta JSON
        try
        {
            CompatibilityResponse response = JsonUtility.FromJson<CompatibilityResponse>(request.downloadHandler.text);
            request.Dispose();
            return response.compatibility_score;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Errore durante la deserializzazione della risposta: " + e.Message);
            request.Dispose();
            return 0.1f; // Valore neutro in caso di errore nella deserializzazione
        }
    }
}
