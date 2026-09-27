using UnityEngine;

public class LaneSpawner : MonoBehaviour
{
    public PlayerLaneMovement lanes;
    public GameObject[] spawnPrefabs;
    public float minSpawnDelay = 1f;
    public float maxSpawnDelay = 2.5f;
    public float minSpawnDelayAtMaxSpeed = 0.3f;
    public float maxSpawnDelayAtMaxSpeed = 0.8f;
    public float spawnOffsetX = 1f;
    [Range(0f, 1f)] public float shinyChance = 0.1f;
    public Color shinyColor = Color.yellow;
    public GameObject shinyDropPrefab;
    [Range(0f, 1f)] public float rainbowChance = 0.02f;
    public float rainbowCycleSpeed = 0.5f;
    public GameObject rainbowDropPrefab;
    public GameObject[] cratePrefabs;
    public float minCrateDelay = 10f;
    public float maxCrateDelay = 20f;
    public int maxSameLaneInARow = 2;
    public float minLaneGap = 3f;
    public float minLaneGapAtMaxSpeed = 6f;
    public float minSpacing = 2f;
    public float minSpacingAtMaxSpeed = 5f;
    [Range(0f, 1f)] public float clusterChance = 0.12f;
    public int clusterMinSize = 2;
    public int clusterMaxSize = 3;
    public float clusterSpreadX = 1f;
    public float clusterBreatherMultiplier = 2f;
    public GameObject[] turretPrefabs;
    public float minTurretDelay = 6f;
    public float maxTurretDelay = 12f;
    public float turretYOffset = 0.5f;

    private Camera cam;
    private float spawnTimer;
    private float spawnRoll;
    private Transform[] lastInLane;
    private int lastLane = -1;
    private int sameLaneCount;
    private Transform lastSpawned;
    private float breatherMultiplier = 1f;
    private float nextTurretTime;
    private float nextCrateTime;

    void Start()
    {
        cam = Camera.main;
        if (lanes == null)
        {
            lanes = FindObjectOfType<PlayerLaneMovement>();
        }
        spawnRoll = Random.value;
        lastInLane = new Transform[lanes != null ? lanes.laneCount : 0];
        nextTurretTime = Time.time + Random.Range(minTurretDelay, maxTurretDelay);
        nextCrateTime = Time.time + Random.Range(minCrateDelay, maxCrateDelay);
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= GetCurrentSpawnDelay() && HasSpacing())
        {
            spawnTimer = 0f;
            spawnRoll = Random.value;
            Spawn();
        }

        if (Time.time >= nextTurretTime)
        {
            nextTurretTime = Time.time + Random.Range(minTurretDelay, maxTurretDelay);
            SpawnTurret();
        }

        if (Time.time >= nextCrateTime)
        {
            nextCrateTime = Time.time + Random.Range(minCrateDelay, maxCrateDelay);
            SpawnInLane(PickFrom(cratePrefabs));
        }
    }

    float SpeedT()
    {
        float maxMultiplier = lanes != null ? lanes.maxSpeedMultiplier : 1f;
        return maxMultiplier > 1f ? Mathf.InverseLerp(1f, maxMultiplier, BackGroundScroller.SpeedMultiplier) : 0f;
    }

    float GetSpawnX()
    {
        float dist = Mathf.Abs(cam.transform.position.z);
        return cam.ViewportToWorldPoint(new Vector3(1f, 0f, dist)).x + spawnOffsetX;
    }

    bool HasSpacing()
    {
        if (lastSpawned == null) return true;
        float required = Mathf.Lerp(minSpacing, minSpacingAtMaxSpeed, SpeedT()) * breatherMultiplier;
        return GetSpawnX() - lastSpawned.position.x >= required;
    }

    float GetCurrentSpawnDelay()
    {
        float t = SpeedT();
        float slowDelay = Mathf.Lerp(minSpawnDelay, maxSpawnDelay, spawnRoll);
        float fastDelay = Mathf.Lerp(minSpawnDelayAtMaxSpeed, maxSpawnDelayAtMaxSpeed, spawnRoll);
        return Mathf.Lerp(slowDelay, fastDelay, t);
    }

    GameObject PickFrom(GameObject[] list)
    {
        if (list == null || list.Length == 0) return null;
        return list[Random.Range(0, list.Length)];
    }

    void ApplyRarity(GameObject obj)
    {
        float roll = Random.value;
        BreakableDropper dropper = obj.GetComponent<BreakableDropper>();
        SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>();

        if (roll < rainbowChance)
        {
            if (dropper != null)
            {
                dropper.rarity = BreakableDropper.Rarity.Rainbow;
                if (rainbowDropPrefab != null) dropper.dropPrefab = rainbowDropPrefab;
            }
            obj.AddComponent<RainbowTint>().cycleSpeed = rainbowCycleSpeed;
        }
        else if (roll < rainbowChance + shinyChance)
        {
            if (dropper != null)
            {
                dropper.rarity = BreakableDropper.Rarity.Shiny;
                if (shinyDropPrefab != null) dropper.dropPrefab = shinyDropPrefab;
            }
            if (sr != null) sr.color = shinyColor;
        }
    }

    void SpawnTurret()
    {
        if (turretPrefabs == null || turretPrefabs.Length == 0) return;

        GameObject prefab = turretPrefabs[Random.Range(0, turretPrefabs.Length)];
        float dist = Mathf.Abs(cam.transform.position.z);
        Vector3 bottomRight = cam.ViewportToWorldPoint(new Vector3(1f, 0f, dist));

        Vector3 spawnPos = new Vector3(bottomRight.x + spawnOffsetX, bottomRight.y + turretYOffset, 0f);
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    void Spawn()
    {
        int count = 1;
        if (Random.value < clusterChance)
        {
            count = Random.Range(clusterMinSize, clusterMaxSize + 1);
        }
        count = Mathf.Clamp(count, 1, lanes != null ? Mathf.Max(1, lanes.laneCount - 1) : 1);

        for (int i = 0; i < count; i++)
        {
            float offset = i == 0 ? 0f : Random.Range(0f, clusterSpreadX);
            GameObject obj = SpawnInLane(PickFrom(spawnPrefabs), offset);
            if (obj != null) ApplyRarity(obj);
        }

        breatherMultiplier = count > 1 ? clusterBreatherMultiplier : 1f;
    }

    GameObject SpawnInLane(GameObject prefab, float xOffset = 0f)
    {
        if (lanes == null || prefab == null) return null;

        float spawnX = GetSpawnX() + xOffset;

        int lane = PickLane(spawnX);
        if (lane < 0) return null;

        Vector3 spawnPos = new Vector3(spawnX, lanes.GetLaneY(lane), 0f);
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

        lastInLane[lane] = obj.transform;
        lastSpawned = obj.transform;
        sameLaneCount = lane == lastLane ? sameLaneCount + 1 : 1;
        lastLane = lane;
        return obj;
    }

    int PickLane(float spawnX)
    {
        System.Collections.Generic.List<int> valid = new System.Collections.Generic.List<int>();
        float laneGap = Mathf.Lerp(minLaneGap, minLaneGapAtMaxSpeed, SpeedT());
        for (int i = 0; i < lastInLane.Length; i++)
        {
            if (i == lastLane && sameLaneCount >= maxSameLaneInARow) continue;
            if (lastInLane[i] != null && spawnX - lastInLane[i].position.x < laneGap) continue;
            valid.Add(i);
        }
        return valid.Count > 0 ? valid[Random.Range(0, valid.Count)] : -1;
    }
}
