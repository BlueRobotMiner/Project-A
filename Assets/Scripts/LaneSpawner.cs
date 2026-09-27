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
    public GameObject[] shinyPrefabs;
    [Range(0f, 1f)] public float shinyChance = 0.1f;
    public GameObject[] rainbowPrefabs;
    [Range(0f, 1f)] public float rainbowChance = 0.02f;
    public GameObject[] cratePrefabs;
    public float minCrateDelay = 10f;
    public float maxCrateDelay = 20f;
    public int maxSameLaneInARow = 2;
    public float minLaneGap = 3f;
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
        if (spawnTimer >= GetCurrentSpawnDelay())
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

    float GetCurrentSpawnDelay()
    {
        float maxMultiplier = lanes != null ? lanes.maxSpeedMultiplier : 1f;
        float t = maxMultiplier > 1f ? Mathf.InverseLerp(1f, maxMultiplier, BackGroundScroller.SpeedMultiplier) : 0f;
        float slowDelay = Mathf.Lerp(minSpawnDelay, maxSpawnDelay, spawnRoll);
        float fastDelay = Mathf.Lerp(minSpawnDelayAtMaxSpeed, maxSpawnDelayAtMaxSpeed, spawnRoll);
        return Mathf.Lerp(slowDelay, fastDelay, t);
    }

    GameObject PickFrom(GameObject[] list)
    {
        if (list == null || list.Length == 0) return null;
        return list[Random.Range(0, list.Length)];
    }

    GameObject PickAsteroid()
    {
        float roll = Random.value;
        GameObject prefab = null;
        if (roll < rainbowChance) prefab = PickFrom(rainbowPrefabs);
        else if (roll < rainbowChance + shinyChance) prefab = PickFrom(shinyPrefabs);
        return prefab != null ? prefab : PickFrom(spawnPrefabs);
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
        SpawnInLane(PickAsteroid());
    }

    void SpawnInLane(GameObject prefab)
    {
        if (lanes == null || prefab == null) return;

        float dist = Mathf.Abs(cam.transform.position.z);
        float spawnX = cam.ViewportToWorldPoint(new Vector3(1f, 0f, dist)).x + spawnOffsetX;

        int lane = PickLane(spawnX);
        if (lane < 0) return;

        Vector3 spawnPos = new Vector3(spawnX, lanes.GetLaneY(lane), 0f);
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

        lastInLane[lane] = obj.transform;
        sameLaneCount = lane == lastLane ? sameLaneCount + 1 : 1;
        lastLane = lane;
    }

    int PickLane(float spawnX)
    {
        System.Collections.Generic.List<int> valid = new System.Collections.Generic.List<int>();
        for (int i = 0; i < lastInLane.Length; i++)
        {
            if (i == lastLane && sameLaneCount >= maxSameLaneInARow) continue;
            if (lastInLane[i] != null && spawnX - lastInLane[i].position.x < minLaneGap) continue;
            valid.Add(i);
        }
        return valid.Count > 0 ? valid[Random.Range(0, valid.Count)] : -1;
    }
}
