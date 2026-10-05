// Torpedo
// The player's heavy homing shot. It only locks onto targets inside a narrow cone in
// front of the ship, gives up if its target ends up behind it, and accelerates once it
// is halfway to the target. Deals much more damage than a laser, with reduced upgrade
// scaling, and its launch sound fades out when it is destroyed.
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
    public float soundFadeTime = 0.1f;
    public float boostMultiplier = 2f;
    public float boostAcceleration = 25f;

    private float startDistance;
    private float currentSpeed;
    private bool boosted;

    [HideInInspector] public int launchSound = -1;

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
        currentSpeed = speed;
        AcquireTarget();
        Destroy(gameObject, lifetime);
    }

    // Picks the closest target: the boss first, then turrets, then asteroids/crates.
    void AcquireTarget()
    {
        if (Boss.Active != null) target = Boss.Active.transform;
        if (target == null) target = FindClosestTurret();
        if (target == null) target = FindClosestBreakable();
        startDistance = target != null ? Vector2.Distance(target.position, transform.position) : 0f;
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

    // Checks a target is ahead of the ship and inside the aiming cone.
    bool IsInFront(Transform t)
    {
        Vector2 dir = t.position - transform.position;
        return dir.x > 0f && Vector2.Angle(Vector2.right, dir) <= maxTargetAngle;
    }

    // Homing, half-distance speed boost, and flying off screen cleanup.
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

        if (!boosted && homing && target != null && startDistance > 0f && Vector2.Distance(target.position, transform.position) <= startDistance * 0.5f)
        {
            boosted = true;
        }
        currentSpeed = Mathf.MoveTowards(currentSpeed, boosted ? speed * boostMultiplier : speed, boostAcceleration * Time.deltaTime);
        rb.velocity = transform.up * currentSpeed;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        if (vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    // Cuts the launch sound short when the torpedo is destroyed, so the rocket
    // noise doesn't linger after the impact.
    void OnDestroy()
    {
        AudioManager.FadeOut(launchSound, soundFadeTime);
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
