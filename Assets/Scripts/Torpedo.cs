using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Torpedo : MonoBehaviour
{
    public float speed = 7f;
    public float turnSpeed = 200f;
    public int damage = 5;
    public float damagePerUpgrade = 0.1f;
    public float maxTargetAngle = 25f;
    [Range(0f, 1f)] public float missChance = 0.25f;
    public float missOffset = 1.5f;
    public float lifetime = 6f;
    public GameObject hitEffect;

    private Transform target;
    private Rigidbody2D rb;
    private Camera cam;
    private Vector3 aimOffset;
    private bool homing = true;

    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        AcquireTarget();
        Destroy(gameObject, lifetime);
    }

    void AcquireTarget()
    {
        if (Boss.Active != null) target = Boss.Active.transform;
        if (target == null) target = FindClosestTurret();
        if (target == null) target = FindClosestBreakable();
        aimOffset = target != null && Random.value < missChance ? (Vector3)(Random.insideUnitCircle.normalized * missOffset) : Vector3.zero;
    }

    Transform FindClosestTurret()
    {
        Transform best = null;
        float bestDist = Mathf.Infinity;
        foreach (Turret t in FindObjectsOfType<Turret>())
        {
            if (!t.IsOnScreen || !IsInFront(t.transform)) continue;
            float d = (t.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = t.transform; }
        }
        return best;
    }

    Transform FindClosestBreakable()
    {
        Transform best = null;
        float bestDist = Mathf.Infinity;
        foreach (BreakableDropper b in FindObjectsOfType<BreakableDropper>())
        {
            Vector3 vp = cam.WorldToViewportPoint(b.transform.position);
            if (vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f) continue;
            if (!IsInFront(b.transform)) continue;
            float d = (b.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = b.transform; }
        }
        return best;
    }

    bool IsInFront(Transform t)
    {
        Vector2 dir = t.position - transform.position;
        return dir.x > 0f && Vector2.Angle(Vector2.right, dir) <= maxTargetAngle;
    }

    void Update()
    {
        if (homing && target == null)
        {
            AcquireTarget();
        }

        if (homing && target != null && target.position.x < transform.position.x)
        {
            homing = false;
            target = null;
        }

        if (homing && target != null)
        {
            Vector3 aimPoint = target.position + aimOffset;
            Vector2 dir = aimPoint - transform.position;
            if (aimOffset != Vector3.zero && dir.magnitude < 0.3f)
            {
                homing = false;
            }
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, turnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        rb.velocity = transform.up * speed;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        if (vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Turret turret = col.GetComponent<Turret>();
        BreakableDropper breakable = col.GetComponent<BreakableDropper>();
        Boss boss = col.GetComponent<Boss>();
        if (turret == null && breakable == null && boss == null) return;

        float hitDamage = damage * (1f + SaveSystem.Data.firepowerLevel * damagePerUpgrade);
        AudioManager.Play(AudioManager.Sfx.ProjectileHit);
        if (boss != null) boss.TakeHit(hitDamage);
        else if (turret != null) turret.TakeHit(Mathf.RoundToInt(hitDamage));
        else breakable.TakeHit(hitDamage);

        EffectSpawner.SpawnHit(hitEffect, col, transform.position, transform.up);
        Destroy(gameObject);
    }
}
