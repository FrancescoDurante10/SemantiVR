using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class BenchmarkLogger : MonoBehaviour
{
    [Header("Durate (secondi)")]
    public float warmupSeconds = 10f;
    public float recordSeconds = 60f;

    [Header("Campionamento")]
    public bool logEveryFrame = false;
    public float samplePeriod = 0.25f;

    [Header("Uscita")]
    public string fileName = "benchmark.csv";

    float timer, sampleTimer;
    bool recording;
    bool hasFinalized;            // <-- NEW
    string filePath;

    readonly List<float> dt = new();
    readonly List<float> cpuMs = new();
    readonly List<float> gcKb = new();

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;

        filePath = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Debug.Log($"[Benchmark] Rimosso file precedente: {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Benchmark] Impossibile cancellare file precedente: {e.Message}");
            }
        }

        File.WriteAllText(filePath, "time,dt,fps,cpu_ms,gc_alloc_kb,main_ms\n");

        timer = 0f;
        sampleTimer = 0f;
        hasFinalized = false;

        Debug.Log($"[Benchmark] Scrivo su: {filePath}");
    }

    void Update()
    {
        if (hasFinalized) return; // <-- evita qualsiasi lavoro dopo la fine

        timer += Time.unscaledDeltaTime;

        if (!recording)
        {
            if (timer >= warmupSeconds)
            {
                recording = true;
                timer = 0f;
                sampleTimer = 0f;
                Debug.Log("[Benchmark] Inizio registrazione.");
            }
            return;
        }

        // Raccolta
        float dtNow = Time.unscaledDeltaTime;
        float cpuNow = Time.deltaTime * 1000f;
        float gcNow = Profiler.GetTempAllocatorSize() / 1024f; // KB allocati (indicativo)

        dt.Add(dtNow);
        cpuMs.Add(cpuNow);
        gcKb.Add(gcNow);

        // Scrittura periodica
        bool shouldWrite = logEveryFrame;
        sampleTimer += Time.unscaledDeltaTime;
        if (!logEveryFrame && sampleTimer >= samplePeriod)
        {
            shouldWrite = true;
            sampleTimer = 0f;
        }

        if (shouldWrite)
        {
            float t = Time.realtimeSinceStartup;
            float fps = (dtNow > 0f) ? (1f / dtNow) : 0f;
            float mainMs = Time.deltaTime * 1000f;

            File.AppendAllText(filePath, $"{t:F3},{dtNow:F4},{fps:F1},{cpuNow:F2},{gcNow:F1},{mainMs:F2}\n");
        }

        // Stop e summary UNA VOLTA
        if (recording && timer >= recordSeconds && !hasFinalized)
        {
            FinalizeReport();
            hasFinalized = true;   // <-- blocca chiamate successive
#if UNITY_EDITOR
            Debug.Log("[Benchmark] Fine. File:\n" + filePath);
            enabled = false;       // opzionale: disattiva il componente
#else
            Application.Quit();
#endif
        }
    }

    void FinalizeReport()
    {
        float PctToFps(float p)
        {
            if (dt.Count == 0) return 0f;
            var worst = dt.OrderByDescending(x => x).ToList(); // dt più alti = frame peggiori
            int idx = Mathf.Clamp(Mathf.CeilToInt(worst.Count * p) - 1, 0, worst.Count - 1);
            float dtWorst = worst[idx];
            return (dtWorst > 0f) ? 1f / dtWorst : 0f;
        }

        float avgFps = (dt.Count > 0) ? (1f / dt.Average()) : 0f;
        float fps1Low = PctToFps(0.01f);
        float fps01Low = PctToFps(0.001f);
        float avgCpu = cpuMs.Where(x => x > 0).DefaultIfEmpty(0).Average();
        float avgGc = gcKb.DefaultIfEmpty(0).Average();

        File.AppendAllText(filePath,
            $"\n# Summary\n# avg_fps,{avgFps:F1}\n# fps_1pct_low,{fps1Low:F1}\n# fps_0.1pct_low,{fps01Low:F1}\n# avg_cpu_ms,{avgCpu:F2}\n# avg_gc_kb,{avgGc:F1}\n");
    }
}
