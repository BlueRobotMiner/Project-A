// Boss
// The level boss. Flies in from off screen to a hold point (the player can't shoot it
// until it arrives), moves between lanes, and fires at the player from one or more fire
// points. On death it plays a cutscene: drifts to the centre lane while shaking,
// spinning and exploding, then spawns the portal. Its health scales with boss number.
using UnityEngine;

public class Boss : MonoBehaviour
{
    public static Boss Active;

    public float baseHealth = 250f;
    public float healthPerBoss = 0.5f;
    public float enterSpeed = 3f;
    public float holdViewportX = 0.8f;
    public float facingAngle = 90f;
    public float spawnMargin = 0.5f;
    public float moveSpeed = 3f;
    public float minMoveDelay = 1f;
    public float maxMoveDelay = 2.5f;
    public Transform[] firePoints;
    public bool fireAllAtOnce = true;
    public GameObject enemyProjectilePrefab;
    public float fireRate = 1.2f;
    public Color projectileColor = Color.red;
    public GameObject destroyEffect;
    public int deathExplosions = 8;
    public float deathDuration = 2f;
    public float deathExplosionScale = 0.6f;
    public float finalExplosionScale = 1.5f;
    public float explosionLifetime = 2f;
    public float shakeAmount = 0.15f;
    public float deathSettleTime = 0.8f;
    public float deathSpinSpeed = 60f;
    public GameObject portalPrefab;

    [HideInInspector] public float maxHealth;
    [HideInInspector] public float health;

    private PlayerLaneMovement lanes;
    private Transform player;
    private Camera cam;
    private float holdX;
    private float targetY;
    private float nextMoveTime;
    private float nextFireTime;
    private bool entered;
    private bool dying;

    public bool IsEngaged
    {
        get { return entered && !dying; }
    }
    private int fireIndex;
    private float halfWidth;

    // Awake runs before Start, so the boss re-anchors itself off the right edge and
    // rotates to face the player before it can be seen on the first frame.
    void Awake()
    {
        Active = this;
        cam = Camera.main;
        transform.rotation = Quaternion.Euler(0f, 0f, facingAngle);
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        halfWidth = sr != null ? sr.bounds.extents.x : 1f;
        float dist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 pos = transform.position;
        pos.x = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, dist)).x + halfWidth + spawnMargin;
        transform.position = pos;
    }

    void Start()
    {
        cam = Camera.main;
        lanes = FindObjectOfType<PlayerLaneMovement>();
        PlayerHealth ph = FindObjectOfType<PlayerHealth>();
        if (ph != null) player = ph.transform;

        WaveManager wm = WaveManager.Instance;
        int bossNumber = wm != null && wm.bossEvery > 0 ? Mathf.Max(1, wm.currentWave / wm.bossEvery) : 1;
        maxHealth = baseHealth * (1f + (bossNumber - 1) * healthPerBoss);
        health = maxHealth;

        float dist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        holdX = cam.ViewportToWorldPoint(new Vector3(holdViewportX, 0.5f, dist)).x;
        holdX = Mathf.Min(holdX, cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, dist)).x - halfWidth);
        targetY = transform.position.y;
    }

    // Entrance and combat: glide to the hold point, then pick lanes, fire, and
    // shoot at the player. IsEngaged turns true only once the entrance is done.
    void Update()
    {
        if (dying) return;
        Vector3 pos = transform.position;
        if (!entered)
        {
            pos.x = Mathf.MoveTowards(pos.x, holdX, enterSpeed * Time.deltaTime);
            transform.position = pos;
            if (pos.x <= holdX)
            {
                entered = true;
                nextMoveTime = Time.time + Random.Range(minMoveDelay, maxMoveDelay);
                nextFireTime = Time.time + fireRate;
            }
            return;
        }

        if (lanes != null && Time.time >= nextMoveTime)
        {
            nextMoveTime = Time.time + Random.Range(minMoveDelay, maxMoveDelay);
            targetY = lanes.GetLaneY(Random.Range(0, lanes.laneCount));
        }
        pos.y = Mathf.MoveTowards(pos.y, targetY, moveSpeed * Time.deltaTime);
        transform.position = pos;

        if (enemyProjectilePrefab != null && player != null && player.gameObject.activeInHierarchy && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            if (firePoints == null || firePoints.Length == 0)
            {
                FireFrom(transform);
            }
            else if (fireAllAtOnce)
            {
                foreach (Transform point in firePoints) if (point != null) FireFrom(point);
            }
            else
            {
                fireIndex = (fireIndex + 1) % firePoints.Length;
                if (firePoints[fireIndex] != null) FireFrom(firePoints[fireIndex]);
            }
        }
    }

    // Fires one shot from a point, aimed at the player and tinted with this boss's colour.
    void FireFrom(Transform point)
    {
        Vector2 dir = player.position - point.position;
        Quaternion rot = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
        GameObject shot = Instantiate(enemyProjectilePrefab, point.position, rot);
        shot.GetComponent<EnemyProjectile>().owner = gameObject;
        EffectSpawner.Tint(shot, projectileColor);
        AudioManager.Play(AudioManager.Sfx.EnemyLaser);
    }

    // Takes damage only while engaged. Death runs as a coroutine so the
    // shake/spin/explosion cutscene can play out over time.
    public void TakeHit(float damage)
    {
        if (!IsEngaged) return;
        health -= damage;
        if (health <= 0f) StartCoroutine(Die());
    }

    // Death cutscene: drifts to the centre lane over deathSettleTime while shaking and
    // spinning, popping explosions across the sprite, then one big blast and the portal.
    System.Collections.IEnumerator Die()
    {
        dying = true;
        health = 0f;
        AudioManager.StopBossMusic();
        if (Active == this) Active = null;
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>()) col.enabled = false;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        Vector3 startPos = transform.position;
        float centerY = lanes != null ? lanes.GetLaneY(lanes.laneCount / 2) : startPos.y;
        Vector3 endPos = new Vector3(Mathf.Min(startPos.x, holdX), centerY, startPos.z);
        Vector3 basePos = startPos;
        float interval = deathDuration / Mathf.Max(1, GameSettings.ScaleEffectCount(deathExplosions));
        float shake = GameSettings.ScreenShake ? shakeAmount : 0f;
        float nextExplosion = 0f;

        for (float t = 0f; t < deathDuration; t += Time.deltaTime)
        {
            basePos = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, deathSettleTime > 0f ? t / deathSettleTime : 1f));
            transform.position = basePos + (Vector3)(Random.insideUnitCircle * shake);
            transform.Rotate(0f, 0f, deathSpinSpeed * (t / deathDuration) * Time.deltaTime);
            if (t >= nextExplosion)
            {
                nextExplosion += interval;
                Bounds bounds = sr != null ? sr.bounds : new Bounds(transform.position, Vector3.one);
                Vector3 offset = new Vector3(Random.Range(-bounds.extents.x, bounds.extents.x), Random.Range(-bounds.extents.y, bounds.extents.y), 0f);
                SpawnExplosion(bounds.center + offset, deathExplosionScale);
            }
            yield return null;
        }

        basePos = endPos;
        transform.position = basePos;
        SpawnExplosion(basePos, finalExplosionScale);
        if (portalPrefab != null) Instantiate(portalPrefab, basePos, Quaternion.identity);
        Destroy(gameObject);
    }

    void SpawnExplosion(Vector3 position, float scale)
    {
        AudioManager.Play(AudioManager.Sfx.ShipExplode);
        if (destroyEffect == null) return;
        GameObject effect = Instantiate(destroyEffect, position, Quaternion.identity);
        effect.transform.localScale *= scale;
        Destroy(effect, explosionLifetime);
    }

    void OnDestroy()
    {
        if (Active == this) Active = null;
    }
}
