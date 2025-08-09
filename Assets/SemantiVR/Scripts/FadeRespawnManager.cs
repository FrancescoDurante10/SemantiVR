using UnityEngine;
using System.Collections;

public class FadeRespawnManager : MonoBehaviour
{
    [Header("Setup")]
    public Transform xrOrigin;           
    public Transform respawnPoint;       
    public Material fadeMaterial;       

    [Header("Impostazioni")]
    public float fallThreshold = -5f;    
    public float fadeDuration = 0.5f;    
    public float stayBlackDuration = 0.5f; 

    private bool isFading = false;

    void Start()
    {
       
        SetAlpha(0f);
    }

    void Update()
    {
        if (!isFading && xrOrigin.position.y < fallThreshold)
        {
            StartCoroutine(FadeAndRespawn());
        }
    }

    private IEnumerator FadeAndRespawn()
    {
        isFading = true;

       
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

      
        xrOrigin.position = respawnPoint.position;

        
        yield return new WaitForSeconds(stayBlackDuration);

       
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        isFading = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color c = fadeMaterial.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeMaterial.color = c;
            yield return null;
        }

        c.a = endAlpha;
        fadeMaterial.color = c;
    }

    private void SetAlpha(float alpha)
    {
        Color c = fadeMaterial.color;
        c.a = alpha;
        fadeMaterial.color = c;
    }
}
