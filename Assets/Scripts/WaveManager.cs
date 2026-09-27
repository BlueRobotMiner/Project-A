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

    [HideInInspector] public float waveProgress;

    private float distanceThisWave;
    private float pauseTimer;

    public bool IsPaused
    {
        get { return pauseTimer > 0f; }
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
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

    void AddDistance(float amount)
    {
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
