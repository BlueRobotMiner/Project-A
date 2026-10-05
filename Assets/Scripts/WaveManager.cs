// WaveManager
// Drives wave progression: distance from travel and kills fills the wave bar, and each
// wave break pauses the spawner briefly. On a boss wave the spawner stops and the boss
// prefab is spawned instead; the wave counter stays there until the boss dies.
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    public int currentWave = 1;
    public int bossEvery = 5;
    public float distancePerWave = 300f;
    public float baseTravelSpeed = 10f;
    public float distancePerKill = 10f;
    public float waveBreakDuration = 3f;
    public GameObject bossPrefab;
    public float bossSpawnOffsetX = 2f;

    [HideInInspector] public float waveProgress;

    private float distanceThisWave;
    private float pauseTimer;
    private bool bossSpawned;

    public int FinalWave
    {
        get { return bossEvery > 0 ? Mathf.CeilToInt((float)currentWave / bossEvery) * bossEvery : int.MaxValue; }
    }

    public bool IsPaused
    {
        get { return pauseTimer > 0f; }
    }

    void Awake()
    {
        Instance = this;
    }

    // Progresses waves, or holds on the boss wave while the boss fight runs.
    void Update()
    {
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }
        if (IsBossWave(currentWave))
        {
            if (!bossSpawned) SpawnBoss();
            return;
        }
        AddDistance(CurrentSpeed * Time.deltaTime);
    }

    public float CurrentSpeed
    {
        get { return baseTravelSpeed * BackGroundScroller.SpeedMultiplier; }
    }

    public void AddKill()
    {
        if (IsPaused) return;
        AddDistance(distancePerKill);
    }

    public bool IsBossWave(int wave)
    {
        return bossEvery > 0 && wave % bossEvery == 0;
    }

    // Spawns the boss just off the right edge and starts the boss music.
    void SpawnBoss()
    {
        bossSpawned = true;
        if (bossPrefab == null) return;
        AudioManager.PlayBossMusic();
        Camera cam = Camera.main;
        float dist = Mathf.Abs(cam.transform.position.z);
        Vector3 edge = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, dist));
        Instantiate(bossPrefab, new Vector3(edge.x + bossSpawnOffsetX, edge.y, 0f), Quaternion.identity);
    }

    // Adds travel distance and advances to the next wave when the bar fills.
    void AddDistance(float amount)
    {
        if (IsBossWave(currentWave)) return;
        distanceThisWave += amount;
        if (distanceThisWave >= distancePerWave)
        {
            distanceThisWave = 0f;
            currentWave++;
            pauseTimer = waveBreakDuration;
        }
        waveProgress = distanceThisWave / distancePerWave;
    }
}
