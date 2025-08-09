using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class TestCompatibility : MonoBehaviour
{
    // Riferimento alla classe che calcola la compatibilit� semantica
    public SemanticCompatibilityChecker compatibilityChecker;

    // Esegui il test di compatibilit�
    void Start()
    {
        // Esegui una Coroutine per gestire la logica asincrona in Unity
        StartCoroutine(TestSemanticCompatibility());
    }

    // Coroutine per calcolare la compatibilit� semantica
    private IEnumerator TestSemanticCompatibility()
    {
        string asset1 = "gym";   // Esempio di asset1
        string asset2 = "house";   // Esempio di asset2

        // Frase generica migliorata per la compatibilit� logica
        string description1 = asset1;
        string description2 = asset2;

        // Calcola il punteggio di compatibilit�
        Task<float> compatibilityTask = compatibilityChecker.CalculateSemanticCompatibility(description1, description2);

        // Attendi il completamento del Task
        yield return new WaitUntil(() => compatibilityTask.IsCompleted);

        // Controlla se il Task ha avuto successo
        if (compatibilityTask.IsCompletedSuccessfully)
        {
            float score = compatibilityTask.Result;
            Debug.Log("Punteggio di compatibilit� logica (migliorato): " + score);
        }
        else
        {
            Debug.LogError("Errore nel calcolo della compatibilit� semantica.");
        }
    }
}
