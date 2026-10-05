// StardustPickup
// A stardust collectible. Scatters when spawned, then drifts left; when the player gets
// close it magnetises to the ship and flies in. All pieces from one asteroid share a
// Group so the pickup sound plays once per asteroid, not per piece.
using UnityEngine;

public class StardustPickup : MonoBehaviour
{
    public int amount = 1;
    public float scrollSpeed = -2f;
    public float scatterSpeed = 2f;
    public float scatterDamping = 3f;
    public float verticalScatter = 0.5f;
    public float bobAmount = 0.1f;
    public float bobSpeed = 2f;
    public float spinSpeed = 90f;
    public float magnetRadius = 1.2f;
    public float magnetSpeed = 8f;
    public float lifetime = 20f;
    public float collectDuration = 0.25f;

    public class Group
    {
        public bool soundPlayed;
    }

    [HideInInspector] public Group group;

    private bool collecting;
    private float collectT;
    private Vector3 collectStart;
    private Vector3 startScale;
    private SpriteRenderer sr;
    private PlayerStardust collector;

    private Vector2 scatterVelocity;
    private Vector3 basePos;
    private float bobOffset;
    private bool magnetized;
    private Transform player;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        PlayerStardust ps = FindObjectOfType<PlayerStardust>();
        if (ps != null) player = ps.transform;

        Vector2 dir = Random.insideUnitCircle.normalized;
        dir.y *= verticalScatter;
        scatterVelocity = dir * scatterSpeed * Random.Range(0.5f, 1f);
        bobOffset = Random.value * Mathf.PI * 2f;
        spinSpeed *= Random.Range(0.5f, 1.5f) * (Random.value < 0.5f ? -1f : 1f);
        basePos = transform.position;
        Destroy(gameObject, lifetime);
    }

    // Scatter, bob, spin, magnet flight and off screen cleanup.
    void Update()
    {
        float dt = Time.deltaTime;

        if (collecting)
        {
            UpdateCollect(dt);
            return;
        }

        if (player != null && player.gameObject.activeInHierarchy)
        {
            if (!magnetized && (player.position - basePos).sqrMagnitude < magnetRadius * magnetRadius)
            {
                magnetized = true;
                if (group == null || !group.soundPlayed)
                {
                    if (group != null) group.soundPlayed = true;
                    AudioManager.Play(AudioManager.Sfx.StardustPickup);
                }
            }
            if (magnetized)
            {
                basePos = Vector3.MoveTowards(basePos, player.position, magnetSpeed * dt);
                transform.position = basePos;
                return;
            }
        }

        scatterVelocity = Vector2.Lerp(scatterVelocity, Vector2.zero, scatterDamping * dt);
        basePos += (Vector3)(scatterVelocity * dt);
        basePos += Vector3.right * scrollSpeed * ObjectScroller.SpeedMultiplier * dt;

        transform.position = basePos + Vector3.up * Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobAmount;
        transform.Rotate(0f, 0f, spinSpeed * dt);

        float dist = Mathf.Abs(cam.transform.position.z - basePos.z);
        if (basePos.x < cam.ViewportToWorldPoint(new Vector3(0f, 0f, dist)).x - 1f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (collecting) return;
        PlayerStardust ps = col.GetComponent<PlayerStardust>();
        if (ps == null) return;

        collecting = true;
        collector = ps;
        collectStart = transform.position;
        startScale = transform.localScale;
        sr = GetComponentInChildren<SpriteRenderer>();
        Collider2D ownCol = GetComponent<Collider2D>();
        if (ownCol != null) ownCol.enabled = false;
    }

    void UpdateCollect(float dt)
    {
        collectT += dt / Mathf.Max(0.01f, collectDuration);
        float t = Mathf.Clamp01(collectT);
        float eased = t * t;

        Vector3 target = collector != null ? collector.transform.position : collectStart;
        transform.position = Vector3.Lerp(collectStart, target, eased);
        transform.localScale = startScale * (1f - eased);
        transform.Rotate(0f, 0f, spinSpeed * 4f * dt);

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f - eased;
            sr.color = c;
        }

        if (t >= 1f)
        {
            if (collector != null && collector.isActiveAndEnabled) collector.Add(amount);
            Destroy(gameObject);
        }
    }
}
